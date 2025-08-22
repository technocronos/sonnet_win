using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

/// <summary>
/// BGMとSEの管理をするマネージャ。シングルトン。
/// </summary>
public class AudioManager : SingletonMonoBehaviour<AudioManager>
{

    //ボリューム保存用のkeyとデフォルト値
    private const string BGM_VOLUME_KEY = "BGM_VOLUME_KEY";
    private const string SE_VOLUME_KEY = "SE_VOLUME_KEY";
    public const float BGM_VOLUME_DEFULT = 8f;
    public const float SE_VOLUME_DEFULT = 0.5f;

    //オーディオファイルのパス
    private const string BGM_PATH = "Audio/BGM";
    private const string SE_PATH = "Audio/SE";

    //BGMがフェードするのにかかる時間
    public const float BGM_FADE_SPEED_RATE_HIGH = 0.9f;
    public const float BGM_FADE_SPEED_RATE_LOW = 0.3f;
    private float _bgmFadeSpeedRate = BGM_FADE_SPEED_RATE_HIGH;

    //次流すBGM名、SE名
    private string _nextBGMName;
    private string _nextSEName;
    private float _nextSEVol = SE_VOLUME_DEFULT;

    //BGMをフェードアウト中か
    private bool _isFadeOut = false;

    //BGM用、SE用に分けてオーディオソースを持つ
    private AudioSource _bgmSource;
    private int BGM_SOURCE_NUM = 1;

    private List<AudioSource> _seSourceList;
    private int SE_SOURCE_NUM = 10;

    //全AudioClipを保持
    public Dictionary<string, AudioClip> _bgmDic, _seDic;

    private string _last_se_name = "";
    private long _last_se_time;

    private bool initialized = false;

    //=================================================================================
    //初期化
    //=================================================================================
    private void Awake()
    {
        Debug.Log("AudioManager Awake()");

        if (this != Instance)
        {
            Destroy(this);
            return;
        }

        DontDestroyOnLoad(this.gameObject);
    }

    public void Init()
    {
        Debug.Log("AudioManager Init()");

        if (initialized)
            return;

        //リソースフォルダから全SE&BGMのファイルを読み込みセット
        _bgmDic = new Dictionary<string, AudioClip>();
        _seDic = new Dictionary<string, AudioClip>();

        Addressables.LoadAssetsAsync<AudioClip>("bgm", null).Completed += handle =>
        {
            //m_SpriteHandle = handle;
            if (handle.Result == null)
            {
                Debug.Log("BGM Load Error");
                //ここでリソースが取れてないようなら進行不能
                GameObject MessageCanvas = Main.Instance.MessageView;
                MessageCanvas.SetActive(true);
                // 通信エラーチェック
                MessageCanvas.GetComponent<MessageBehaviour>().Open("通信エラーです。リソースがダウンロードできませんでした。一度アプリを閉じて通信環境のいい所でリトライしてください", false);
                return;
            }
            IList<AudioClip> bgmList = handle.Result;

            foreach (AudioClip bgm in bgmList)
            {
                _bgmDic.Add(bgm.name, bgm);
            }

            //Addressables.Release(handle);
        };

        Addressables.LoadAssetsAsync<AudioClip>("se", null).Completed += handle =>
        {
            //m_SpriteHandle = handle;
            if (handle.Result == null)
            {
                Debug.Log("SE Load Error");
                //ここでリソースが取れてないようなら進行不能
                GameObject MessageCanvas = Main.Instance.MessageView;
                MessageCanvas.SetActive(true);
                // 通信エラーチェック
                MessageCanvas.GetComponent<MessageBehaviour>().Open("通信エラーです。リソースがダウンロードできませんでした。一度アプリを閉じて通信環境のいい所でリトライしてください", false);
                return;
            }
            IList<AudioClip> seList = handle.Result;

            foreach (AudioClip se in seList)
            {
                _seDic.Add(se.name, se);
            }

            //Addressables.Release(handle);
        };


        //Debug.Log("BGM_SOURCE_NUM=" + BGM_SOURCE_NUM);
        //Debug.Log("SE_SOURCE_NUM=" + SE_SOURCE_NUM);

        //オーディオリスナーおよびオーディオソースをSE+BGMの分,作成
        gameObject.AddComponent<AudioListener>();
        for (int i = 0; i < SE_SOURCE_NUM + BGM_SOURCE_NUM; i++)
        {
            gameObject.AddComponent<AudioSource>();
        }

        //作成したオーディオソースを取得して各変数に設定、ボリュームも設定
        AudioSource[] audioSourceArray = GetComponents<AudioSource>();
        _seSourceList = new List<AudioSource>();

        for (int i = 0; i < audioSourceArray.Length; i++)
        {
            audioSourceArray[i].playOnAwake = false;

            if (i == 0)
            {
                audioSourceArray[i].loop = true;
                _bgmSource = audioSourceArray[i];
                _bgmSource.volume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, BGM_VOLUME_DEFULT);
            }
            else
            {
                _seSourceList.Add(audioSourceArray[i]);
                audioSourceArray[i].volume = PlayerPrefs.GetFloat(SE_VOLUME_KEY, SE_VOLUME_DEFULT);
            }

        }

        initialized = true;

        Debug.Log("AudioManager Init() end..");
    }

    //=================================================================================
    //SE
    //=================================================================================

    /// <summary>
    /// 指定したファイル名のSEを流す。第二引数のdelayに指定した時間だけ再生までの間隔を空ける
    /// </summary>
    public void PlaySE(string seName, float delay = 0.0f)
    {

        if (!_seDic.ContainsKey(seName))
        {
            Debug.Log(seName + "という名前のSEがありません");
            return;
        }

        if (_last_se_name != seName)
        {
            //前回鳴らしたSEが違う場合
            _last_se_name = seName;
            _last_se_time = Utility.GetUnixTime(System.DateTime.Now);
        }
        else
        {
            //前回鳴らしたSEと同じ場合
            long now = Utility.GetUnixTime(System.DateTime.Now);

            //Debug.Log("同じSEです _last_se_time=" + _last_se_time + " now=" + now + " 誤差=" + (now - _last_se_time));

            if ((now - _last_se_time) <= 150)
            {
                //Debug.Log("同じSEで同じタイミングです。抑制します");
                return;
            }
            _last_se_time = Utility.GetUnixTime(System.DateTime.Now);
        }


        _nextSEName = seName;
        _nextSEVol = PlayerPrefs.GetFloat(SE_VOLUME_KEY, SE_VOLUME_DEFULT);

        //_nextSEVol = volume;

        //Invoke("DelayPlaySE", delay);

        DelayPlaySE(_nextSEName, _nextSEVol);
    }

    private void DelayPlaySE(string se_name, float se_vol)
    {
        foreach (AudioSource seSource in _seSourceList)
        {
            if (!seSource.isPlaying)
            {
                seSource.volume = se_vol;
                seSource.PlayOneShot(_seDic[se_name] as AudioClip);

                PlayerPrefs.SetFloat(SE_VOLUME_KEY, se_vol);
                return;
            }
        }
    }

    //=================================================================================
    //BGM
    //=================================================================================

    /// <summary>
    /// 指定したファイル名のBGMを流す。ただし既に流れている場合は前の曲をフェードアウトさせてから。
    /// 第二引数のfadeSpeedRateに指定した割合でフェードアウトするスピードが変わる
    /// </summary>
    public void PlayBGM(string bgmName, float fadeSpeedRate = BGM_FADE_SPEED_RATE_HIGH)
    {
        if (!_bgmDic.ContainsKey(bgmName))
        {
            Debug.Log(bgmName + "という名前のBGMがありません");
            return;
        }

        //現在BGMが流れていない時はそのまま流す
        if (!_bgmSource.isPlaying)
        {
            _nextBGMName = "";
            _bgmSource.volume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, BGM_VOLUME_DEFULT);
            _bgmSource.clip = _bgmDic[bgmName] as AudioClip;
            _bgmSource.Play();
        }
        //違うBGMが流れている時は、流れているBGMをフェードアウトさせてから次を流す。同じBGMが流れている時はスルー
        else if (_bgmSource.clip.name != bgmName)
        {
            _nextBGMName = bgmName;
            FadeOutBGM(fadeSpeedRate);
        }
    }

    /// <summary>
    /// BGMをすぐに止める
    /// </summary>
    public void StopBGM()
    {
        _bgmSource.Stop();
    }

    /// <summary>
    /// 現在流れている曲をフェードアウトさせる
    /// fadeSpeedRateに指定した割合でフェードアウトするスピードが変わる
    /// </summary>
    public void FadeOutBGM(float fadeSpeedRate = BGM_FADE_SPEED_RATE_LOW)
    {
        _bgmFadeSpeedRate = fadeSpeedRate;
        _isFadeOut = true;
    }

    private void Update()
    {
        if (!_isFadeOut)
        {
            return;
        }

        //徐々にボリュームを下げていき、ボリュームが0になったらボリュームを戻し次の曲を流す
        _bgmSource.volume -= Time.deltaTime * _bgmFadeSpeedRate;
        if (_bgmSource.volume <= 0)
        {
            _bgmSource.Stop();
            _bgmSource.volume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, BGM_VOLUME_DEFULT);
            _isFadeOut = false;

            if (!string.IsNullOrEmpty(_nextBGMName))
            {
                PlayBGM(_nextBGMName);
            }
        }

    }


    public float getBGMVol()
    {
        return PlayerPrefs.GetFloat(BGM_VOLUME_KEY, BGM_VOLUME_DEFULT);
    }
    public float getSEVol()
    {
        return PlayerPrefs.GetFloat(SE_VOLUME_KEY, SE_VOLUME_DEFULT);
    }

    //=================================================================================
    //音量変更
    //=================================================================================

    /// <summary>
    /// BGMとSEのボリュームを別々に変更&保存
    /// </summary>
    public void ChangeVolume(float BGMVolume, float SEVolume)
    {
        _bgmSource.volume = BGMVolume;

        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, BGMVolume);

        foreach (AudioSource seSource in _seSourceList)
        {
            seSource.volume = SEVolume;
        }

        PlayerPrefs.SetFloat(SE_VOLUME_KEY, SEVolume);
    }

    //=================================================================================
    //BGM音量変更
    //=================================================================================

    /// <summary>
    /// BGMボリュームを変更&保存
    /// </summary>
    public void ChangeBgmVolume(float BGMVolume)
    {
        _bgmSource.volume = BGMVolume;

        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, BGMVolume);

        PlayerPrefs.Save();
    }

    //=================================================================================
    //SE音量変更
    //=================================================================================

    /// <summary>
    /// SEのボリュームを変更&保存
    /// </summary>
    public void ChangeSEVolume(float SEVolume)
    {
        foreach (AudioSource seSource in _seSourceList)
        {
            seSource.volume = SEVolume;
        }

        PlayerPrefs.SetFloat(SE_VOLUME_KEY, SEVolume);

        PlayerPrefs.Save();
    }
}
