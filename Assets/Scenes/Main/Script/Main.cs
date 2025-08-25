using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CreateWave;
using UnityEngine;
using Scenes.Common.Scripts;
using UnityEngine.UI;
using TMPro;
using AppsFlyerSDK;
using System;
using UnityEngine.Networking;
using System.Reflection;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using Steamworks;

public class Main : BaseBehaviour, IAppsFlyerConversionData
{
    public GameObject connectGuard;
    public GameObject AuthenticationKit;
    public GameObject tapGuard;
    public GameObject Confirm;
    public GameObject Dialogue;
    public PurchaseBehaviour Purchase;
    public GameObject SettingsView;
    public GameObject MessageView;
    public TextMeshProUGUI ConfirmText;
    public TextMeshProUGUI DialogueText;
    public GameObject SoundMixer;
    public DownloadGaugeBehaviour DownloadGauge;
    public GameObject EtheriumCanvas;


    public Dictionary<string, Dictionary<string, Sprite>> ImageList = new Dictionary<string, Dictionary<string, Sprite>>();
    public Dictionary<string, object> conversionDataDictionary;
    public Dictionary<string, object> attributionDataDictionary;

    public Image DialogueIcon1;
    public Image DialogueIcon2;
    public Image DialogueIcon3;

    public Image ConfirmIcon1;
    public Image ConfirmIcon2;
    public Image ConfirmIcon3;

    public delegate void EventCallback();

    private EventCallback DialogueEventCallback = null;
    private EventCallback ConfirmEventCallbackOk = null;
    private EventCallback ConfirmEventCallbackCancel = null;

    private static Main instance;

    public bool in_apply = false;

    public static Locale Locale
    {
        get
        {
            return locale;
        }
        set
        {
            locale = value;
        }
    }

    private static Locale locale;

    private AsyncOperationHandle _initializeOperation;

    // Start is called before the first frame update
    protected override void Start()
    {
        //base.Start();

        instance = this;

        AppsFlyer.initSDK(Settings.AP_DEVKEY, Settings.AP_APPID, this);
        AppsFlyer.startSDK();
        AppsFlyer.setIsDebug(Settings.IsDevelop);

#if UNITY_EDITOR
        Addressables.ClearDependencyCacheAsync("default");
        Addressables.ClearDependencyCacheAsync("bgm");
        Addressables.ClearDependencyCacheAsync("se");
#endif

        Purchase.gameObject.SetActive(true);
        Purchase.GetComponent<Canvas>().enabled = false;

        Confirm.SetActive(false);
        Dialogue.SetActive(false);
        SettingsView.SetActive(false);
        MessageView.SetActive(false);
        SoundMixer.SetActive(false);
        DownloadGauge.gameObject.SetActive(false);
        AuthenticationKit.SetActive(false);
        EtheriumCanvas.SetActive(false);

        APIConnectManager.Instance.connectObj = connectGuard;
        Application.targetFrameRate = 60;

        //プッシュ通知の設定

        //　Androidチャンネルの登録
        //LocalPushNotification.RegisterChannel(引数1,引数２,引数３);
        //引数１ Androidで使用するチャンネルID なんでもいい LocalPushNotification.AddSchedule()で使用する
        //引数2　チャンネルの名前　なんでもいい　アプリ名でも入れておく
        //引数3　通知の説明 なんでもいい　自分がわかる用に書いておくもの　
        string channelName = "Sonnet Of Wizard";
        LocalPushNotification.RegisterChannel(Settings.CHANNELID_AP_RECV, channelName, "For AP RECOVER");

        //通知のクリア
        LocalPushNotification.AllClear();

        _initializeOperation = LocalizationSettings.SelectedLocaleAsync;
        if (_initializeOperation.IsDone)
        {
            OnInitializeCompleted(_initializeOperation);
        }
        else
        {
            _initializeOperation.Completed += OnInitializeCompleted;
        }

    }

    void OnInitializeCompleted(AsyncOperationHandle handle)
    {
        Debug.Log("OnInitializeCompleted run..");
        Debug.Log(LocalizationSettings.SelectedLocale);

        int lang_select_flg = PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED);

        //preyersprefに設定されている言語を取得
        int select_lang = PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        if (lang_select_flg == 1)
        {
            var locale = LocalizationSettings.AvailableLocales.Locales[select_lang];

            Debug.Log(locale);

            if (LocalizationSettings.SelectedLocale != locale)
            {
                //食い違っている場合はpreyersprefに合わせる
                ChangeLang(select_lang);
            }
            else
            {
                Locale = LocalizationSettings.SelectedLocale;
            }

        }
        else
        {
            var locale_ja = LocalizationSettings.AvailableLocales.Locales[0];

            if (LocalizationSettings.SelectedLocale != locale_ja)
            {
                ChangeLang(1);
            }
            else
            {
                Locale = LocalizationSettings.SelectedLocale;
            }

            PlayerPrefs.SetInt(Settings.LANGUAGE_SELECTED, 1);
        }

        APIConnectManager.Instance.SteamLogin((string steamId)=> {
            SceneController.Instance.Init("Title", null, tapGuard);
        });

    }

    public static Main Instance
    {
        get
        {
            return instance;
        }
    }

    public void ShowPurchase(PurchaseBehaviour.EventCallback _eventCallback = null)
    {
        Purchase.gameObject.SetActive(true);
        Purchase.GetComponent<Canvas>().enabled = true;
        Purchase.ShowPurchase(_eventCallback);
    }

    public void TapTop()
    {
        AudioManager.Instance.StopBGM();
        AudioManager.Instance.PlaySE("se_btn");
        SceneController.Instance.Jump("Title");
    }

    public void onCloseSoundMixer()
    {
        AudioManager.Instance.PlaySE("se_btn");
        SoundMixer.SetActive(false);
    }

    public void onClickSettings()
    {
        AudioManager.Instance.PlaySE("se_btn");
        SettingsView.SetActive(true);
    }

    public void onCloseSettings()
    {
        AudioManager.Instance.PlaySE("se_btn");
        SettingsView.SetActive(false);
    }

    public void onSoundSettingsOpen()
    {
        AudioManager.Instance.PlaySE("se_btn");

        SoundMixer.SetActive(true);
    }

    public void TapHelp()
    {
        AudioManager.Instance.PlaySE("se_btn");

        SceneController.Instance.PopUp("Help");
    }

    public void TapBitCoin()
    {
        AudioManager.Instance.PlaySE("se_btn");

        SettingsView.SetActive(false);
        SceneController.Instance.Jump("BitCoin");
    }

    public void TapWalletConnect()
    {
        AudioManager.Instance.PlaySE("se_btn");

        AuthenticationKit.SetActive(true);
    }

    public void TapWalletConnectClose()
    {
        AudioManager.Instance.PlaySE("se_btn");

        AuthenticationKit.SetActive(false);
    }

    /// <summary>
    /// フッター切り替え用イベントハンドラ
    /// </summary>
    public void FooterChange(string SceneName)
    {
        Debug.Log("FooterChange run..");

        //二度押しは効かない
        if (SceneController.Instance.SceneName == SceneName)
            return;

        AudioManager.Instance.PlaySE("se_btn");

        SceneController.Instance.Jump(SceneName, (() =>
        {
            if (SceneName == "HisPage")
            {
                HisPageBehaviour _scene = FindObjectOfType<HisPageBehaviour>() as HisPageBehaviour;
                _scene.Param = new HisPageBehaviour.Parameter
                {
                    userId = Footer.Instance.getUserId(),
                };
            }
        }));
    }

    public void showDialogue(string text, EventCallback eventCallback = null, int iconNo = 1)
    {
        //AudioManager.Instance.PlaySE("se_btn");
        Dialogue.SetActive(true);

        //テキスト表示
        DialogueText.text = text;

        //icon切り替え
        switch (iconNo)
        {
            case 1:
                DialogueIcon1.enabled = true;
                DialogueIcon2.enabled = false;
                DialogueIcon3.enabled = false;
                break;
            case 2:
                DialogueIcon1.enabled = false;
                DialogueIcon2.enabled = true;
                DialogueIcon3.enabled = false;
                break;
            case 3:
                DialogueIcon1.enabled = false;
                DialogueIcon2.enabled = false;
                DialogueIcon3.enabled = true;
                break;
            case 4:
                DialogueIcon1.enabled = false;
                DialogueIcon2.enabled = false;
                DialogueIcon3.enabled = false;
                break;
        }

        //コールバック登録
        this.DialogueEventCallback = eventCallback;
    }

    public void closeDialogue()
    {
        if (this.DialogueEventCallback != null)
        {
            //コールバック
            this.DialogueEventCallback?.Invoke();
        }

        this.DialogueEventCallback = null;

        Dialogue.SetActive(false);
    }

    public void showConfirm(string text, EventCallback okEventCallback = null, EventCallback CancelEventCallback = null, int iconNo = 1)
    {
        //表示
        Confirm.SetActive(true);

        //テキスト表示
        ConfirmText.text = text;

        //icon切り替え
        switch (iconNo)
        {
            case 1:
                ConfirmIcon1.enabled = true;
                ConfirmIcon2.enabled = false;
                ConfirmIcon3.enabled = false;
                break;
            case 2:
                ConfirmIcon1.enabled = false;
                ConfirmIcon2.enabled = true;
                ConfirmIcon3.enabled = false;
                break;
            case 3:
                ConfirmIcon1.enabled = false;
                ConfirmIcon2.enabled = false;
                ConfirmIcon3.enabled = true;
                break;
            case 4:
                ConfirmIcon1.enabled = false;
                ConfirmIcon2.enabled = false;
                ConfirmIcon3.enabled = false;
                break;
        }

        //コールバック登録
        this.ConfirmEventCallbackOk = okEventCallback;
        this.ConfirmEventCallbackCancel = CancelEventCallback;
    }

    public void closeConfirm(int result)
    {
        if (result == 1)
        {
            if (this.ConfirmEventCallbackOk != null)
            {
                //コールバック
                this.ConfirmEventCallbackOk?.Invoke();
            }
        }
        else
        {
            if (this.ConfirmEventCallbackCancel != null)
            {
                //コールバック
                this.ConfirmEventCallbackCancel?.Invoke();
            }
        }
        this.ConfirmEventCallbackOk = null;
        this.ConfirmEventCallbackCancel = null;

        Confirm.SetActive(false);
    }

    /// <summary>
    /// AddressablesのPercentCompleteは正確じゃないので、内部のUnityWebRequestで進捗を表示
    /// </summary>
    public IEnumerator ShowProgress(AsyncOperationHandle downloadDependencies, string assetname)
    {
        Debug.Log("ShowProgress start.. assetname = " + assetname);

        DownloadGaugeBehaviour dlgauge = null;

        //　キャッシュされている時等はローディングキャンバスを表示しない
        if (downloadDependencies.GetDownloadStatus().Percent < 0.95f)
        {
            //ダウンロードゲージ表示
            dlgauge = Instantiate(DownloadGauge, new Vector3(0, 0, 0), Quaternion.identity);
            dlgauge.name = "DownloadGauge_" + assetname;

            dlgauge.gameObject.SetActive(true);
            dlgauge.Init(100);

            //　進捗状況を表示
            while (downloadDependencies.Status == AsyncOperationStatus.None)
            {
                dlgauge.GaugeInfo.value = downloadDependencies.GetDownloadStatus().Percent * 100;

                if(assetname == "default")
                    dlgauge.setText(Utility.getText("RESOURCE_DL_IMAGE"));
                else if(assetname == "bgm")
                    dlgauge.setText(Utility.getText("RESOURCE_DL_AUDIO"));
                else
                    dlgauge.setText(Utility.getText("RESOURCE_DL"));

                Debug.Log((downloadDependencies.GetDownloadStatus().Percent * 100).ToString("00.0") + "%");
                yield return null;
            }

            if (!downloadDependencies.IsDone)
            {
                Debug.Log("ShowProgress !downloadDependencies.IsDone..");
                yield return downloadDependencies;
            }
            else
            {
                Debug.Log("ShowProgress End..");

                dlgauge.GaugeInfo.value = 100;

                yield return new WaitForSeconds(1f);

                dlgauge.gameObject.SetActive(false);
                Destroy(dlgauge.gameObject);

                yield break;
            }

        }

        yield break;
    }

    public void ShowLoading()
    {
        Debug.Log("ShowLoading run..");

        connectGuard.SetActive(true);

        //ダウンロードゲージ表示
        DownloadGaugeBehaviour dlgauge = Instantiate(DownloadGauge, new Vector3(0, 0, 0), Quaternion.identity);
        dlgauge.name = "AudioInitGauge";

        if (GameObject.Find("DownloadGauge_audio") != null)
            GameObject.Find("DownloadGauge_audio").SetActive(false);

        if (GameObject.Find("DownloadGauge_image") != null)
            GameObject.Find("DownloadGauge_image").SetActive(false);

        dlgauge.gameObject.SetActive(true);
        dlgauge.Init(100);
        dlgauge.GaugeInfo.value = 100;

        var strtbl = LocalizationSettings.StringDatabase.GetTable("StringTable");
        dlgauge.setText(strtbl.GetEntry("TEXT_SOUND_INIT").Value);

    }

    public void HideLoading()
    {
        connectGuard.SetActive(false);
        GameObject dlgauge = GameObject.Find("AudioInitGauge");

        dlgauge.SetActive(false);
        Destroy(dlgauge);
    }

    /// <summary>
    /// リフレクションでWebRequestQueueからUnityWebRequestを取得
    /// </summary>
    private UnityWebRequest FetchUnityWebRequestForProvider(string assetname)
    {
        var libAssembly = Assembly.GetAssembly(typeof(AssetBundleProvider));
        var type = libAssembly.GetType("UnityEngine.ResourceManagement.WebRequestQueue");
        var field = type.GetField("s_ActiveRequests", BindingFlags.Static | BindingFlags.NonPublic);
        var requests = (List<UnityWebRequestAsyncOperation>)field?.GetValue(type);
        if (requests == null)
        {
            Debug.Log("ShowProgress FetchUnityWebRequestForProvider is null..");
            return null;
        }

        // 初回はカタログファイルのダウンロードが入るので、bundleファイルの通信のみ取得するようにする
        foreach (var request in requests)
        {
            //if (request.webRequest.url.EndsWith(".bundle")) return request.webRequest;
            Debug.Log("ShowProgress FetchUnityWebRequestForProvider url=" + request.webRequest.url);

            if (request.webRequest.url.EndsWith(".bundle"))
            {
                if (request.webRequest.uri.Segments[3].StartsWith(assetname))
                {
                    return request.webRequest;
                }
            }
        }

        Debug.Log("ShowProgress FetchUnityWebRequestForProvider End..");
        return null;
    }


    /// <summary>
    /// attributionData には、OneLink、deeplink に関する情報が含まれています。
    /// </summary>
    /// <param name="attributionData">返されたディープリンク データの JSON 文字列</param>
    public void onAppOpenAttribution(string attributionData)
    {
        AppsFlyer.AFLog("onAppOpenAttribution", attributionData);
        attributionDataDictionary = AppsFlyer.CallbackStringToDictionary(attributionData);

        // add direct deeplink logic here
    }

    public void onAppOpenAttributionFailure(string error)
    {
        AppsFlyer.AFLog("onAppOpenAttributionFailure", error);
    }

    /// <summary>
    /// ConversionData には、インストールに関する情報が含まれています。
    /// </summary>
    /// <param name="conversionData">返されたコンバージョン データの JSON 文字列</param>
    public void onConversionDataSuccess(string conversionData)
    {
        AppsFlyer.AFLog("onConversionDataSuccess", conversionData);
        conversionDataDictionary = AppsFlyer.CallbackStringToDictionary(conversionData);

        // add deferred deeplink logic here
    }

    public void onConversionDataFail(string error)
    {
        AppsFlyer.AFLog("onConversionDataFail", error);
    }


    public void onWalletConnect()
    {
        Debug.Log("onWalletConnect run..");
    }

    public void OnAKConnected()
    {
        //Debug.Log("OnConnected run.." + WalletConnect.Instance.name);
        AudioManager.Instance.PlaySE("se_btn");
    }
    public void OnAKDisConnected()
    {
        // Debug.Log("OnConnected run.." + WalletConnect.Instance.name);
        AudioManager.Instance.PlaySE("se_btn");
    }

    public void EtheriumCanvasShow()
    {
        AudioManager.Instance.PlaySE("se_btn");

        EtheriumCanvas.SetActive(true);
    }
    public void EtheriumCanvasClose()
    {
        AudioManager.Instance.PlaySE("se_btn");

        EtheriumCanvas.SetActive(false);
    }

    public float getParFrame()
    {
        float par_frame = Settings.PAR_FRAME;

        return par_frame;
    }


    public void ChangeLang(int lang)
    {
        Debug.Log("ChangeLang run.. lang=" + lang);

        if (lang != 0)
            lang = 1;

        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[lang];
        Locale = LocalizationSettings.SelectedLocale;

        Debug.Log(Locale);

        PlayerPrefs.SetInt(Settings.LANGUAGE_SELECTED_KEY, lang);
    }

}
