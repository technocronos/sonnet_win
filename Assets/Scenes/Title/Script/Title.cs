using System.Collections;
using System.Collections.Generic;
using MyScene;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System;
using Newtonsoft.Json;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Networking;
using System.Reflection;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.Rendering.Universal;
using Scenes.Common.Scripts;

public class Title : BaseBehaviour
{

    string next_scene = null;
    int dramaId = 0;
    int TutorialStep = 0;
    jsonConstants constants;
    jsonLogin LoginInfo;

    public class Parameter
    {
        public bool clearCache;
    }

    public Parameter Param;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        Debug.Log("Title Start start...");

        transform.Find("ButtonStart").gameObject.SetActive(false);
        DispatchEvent(CwEvent.SCENE_READY);

        APIConnectManager.Instance.Login(Show);
    }

    private IEnumerator LoadAssets()
    {
        Debug.Log("Title LoadAssetsLabel start...");
        /*
        string label = "default";

        //Check the download size
        AsyncOperationHandle<long> getDownloadSize = Addressables.GetDownloadSizeAsync(label);
        if (!getDownloadSize.IsDone)
        {
            yield return getDownloadSize;
        }

        //If the download size is greater than 0, download all the dependencies.
        AsyncOperationHandle downloadDependencies = Addressables.DownloadDependenciesAsync(label);
        yield return StartCoroutine(Main.Instance.ShowProgress(downloadDependencies, label));


        label = "bgm";

        AsyncOperationHandle<long> getDownloadSize_bgm = Addressables.GetDownloadSizeAsync(label);
        if (!getDownloadSize_bgm.IsDone)
        {
            yield return getDownloadSize_bgm;
        }

        //If the download size is greater than 0, download all the dependencies.
        AsyncOperationHandle downloadDependencies_bgm = Addressables.DownloadDependenciesAsync(label);
        yield return StartCoroutine(Main.Instance.ShowProgress(downloadDependencies_bgm, label));
        */

        AudioManager.Instance.Init();

        yield return StartCoroutine(reload());
    }

    IEnumerator reload()
    {
        Main.Instance.ShowLoading();
        yield return StartCoroutine(wait());
        Main.Instance.HideLoading();

        transform.Find("ButtonStart").gameObject.SetActive(true);
        transform.Find("ButtonStart").GetComponent<Image>().DOFade(0.0f, 1f).SetEase(Ease.InCubic).SetLoops(-1, LoopType.Yoyo);

        //未登録
        if (LoginInfo.regist == 0)
        {
            next_scene = "Prologue";
        }
        //登録済み
        else if (LoginInfo.regist == 1)
        {
            next_scene = "Quest";
        }
        // エラー等で、ユーザレコードは出来てるのに、キャラクターレコードが出来てない場合。
        else if (LoginInfo.regist == 2)
        {
            next_scene = "Prologue";
        }
        // まだチュートリアル中
        else if (LoginInfo.regist == 3)
        {
            this.TutorialStep = LoginInfo.tutorial_step;
            this.next_scene = LoginInfo.nextscene;
            if (LoginInfo.dramaId > 0)
                this.dramaId = LoginInfo.dramaId;
        }
    }

    public void OnStart()
    {
        if(AudioManager.Instance._seDic == null)
        {
            Debug.Log("AudioManager initialized ng");
            return;
        }

        if (!AudioManager.Instance._seDic.ContainsKey("se_btn"))
        {
            Debug.Log("AudioManager initialized ng");
            return;
        }

        Debug.Log("AudioManager initialized ok");

        /*
         //チュートリアルテスト用
        SceneController.Instance.Jump("Battle", (() => {
            BattleBehaviour _battle = FindObjectOfType<BattleBehaviour>() as BattleBehaviour;
            _battle.Param = new BattleBehaviour.Parameter
            {
                tutorial = true,
                from = "help",
            };
        }));
        return;
        */
        /*
        SceneController.Instance.Jump("GachaResult", (() =>
        {
            GachaResultBehaviour _gacha_result = FindObjectOfType<GachaResultBehaviour>() as GachaResultBehaviour;
            _gacha_result.Param = new GachaResultBehaviour.Parameter
            {
                //2fYg7mXatHnPvJl8eKX4kp2DXMJJlIfD
                //oCigtG92FDNZLAv5CFMOJAzds2Bl6VKq
                dataId = "oCigtG92FDNZLAv5CFMOJAzds2Bl6VKq",
            };
        }));
        return;
        */
        /*
        //ドラマテスト用
        SceneController.Instance.Jump("ShowDrama", (() =>
        {
            ShowDramaBehaviour _drama = FindObjectOfType<ShowDramaBehaviour>() as ShowDramaBehaviour;
            _drama.Param = new ShowDramaBehaviour.Parameter
            {
                DramaId = 9801001,
                endTo = "scene=Title"
            };
        }));
        return;
        */
        /*
        //テスト用
        SceneController.Instance.Jump("Test");
        return;
        */

        Debug.Log("Title OnStart start... next_scene=" + next_scene);

        if (next_scene != null)
        {
            AudioManager.Instance.PlaySE("se_btn");

            if (this.TutorialStep == constants.User_Info_Tutorial.TUTORIAL_BATTLE)
            {
                SceneController.Instance.Jump("Tutorial", (() =>
                {
                    TutorialBehaviour tutorial = FindObjectOfType<TutorialBehaviour>() as TutorialBehaviour;
                    tutorial.Param = new TutorialBehaviour.Parameter { TutorialStep = this.TutorialStep };
                }));
            }
            else
            {
                SceneController.Instance.Jump(next_scene, (() =>
                {
                    if (next_scene == "Tutorial")
                    {
                        TutorialBehaviour tutorial = FindObjectOfType<TutorialBehaviour>() as TutorialBehaviour;
                        tutorial.Param = new TutorialBehaviour.Parameter { TutorialStep = this.TutorialStep };
                    }else if(next_scene == "Quest")
                    {
                        Quest _q = FindObjectOfType<Quest>() as Quest;
                        _q.Param = new Quest.Parameter
                        {
                            panel = "QuestList"
                        };
                    }
                }));
            }
        }
    }


    jsonLogin makeJson(string json)
    {
        jsonLogin response = JsonUtility.FromJson<jsonLogin>(json);

        Dictionary<string, object> jsonDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

        foreach (KeyValuePair<string, object> keyvalue in jsonDict)
        {
            if (keyvalue.Key == "constants")
            {

                Dictionary<string, object> jsonDict2 = JsonConvert.DeserializeObject<Dictionary<string, object>>(keyvalue.Value.ToString());

                foreach (KeyValuePair<string, object> keyvalue2 in jsonDict2)
                {
                    if (keyvalue2.Key == "Ranking_Log_Prize_Week")
                    {

                        try
                        {
                            response.constants.Ranking_Log_Prize_Week = new Dictionary<int, jsonRanking_Log_Prize>();
                            Dictionary<int, jsonRanking_Log_Prize> jsonDict3 = JsonConvert.DeserializeObject<Dictionary<int, jsonRanking_Log_Prize>>(keyvalue2.Value.ToString());

                            foreach (KeyValuePair<int, jsonRanking_Log_Prize> keyvalue3 in jsonDict3)
                            {
                                response.constants.Ranking_Log_Prize_Week.Add(keyvalue3.Key, keyvalue3.Value);
                            }

                        }
                        catch (Exception e)
                        {
                            Debug.Log(e.Message);
                        }
                    }

                }
            }
        }
        return response;
    }


    // 表示する。
    public void Show(string json)
    {
        Debug.Log("Title Show start...");

        APIConnectManager.Instance.login = makeJson(json);

        LoginInfo = APIConnectManager.Instance.login;

        //定数取得
        constants = APIConnectManager.Instance.login.constants;

        if (LoginInfo.result == "ok")
        {
            //まだ送信してない場合は、コンバージョンデータ送信
            if (LoginInfo.appsflyer == false)
            {
                if (Main.Instance.conversionDataDictionary != null)
                {
                    APIConnectManager.Instance.Appsflyer(SaveLoadManager.Instance.UserID, Main.Instance.conversionDataDictionary, (string json) =>
                        {
                            jsonAppsflyer response = JsonUtility.FromJson<jsonAppsflyer>(json);
                            Debug.Log(response);
                        });
                }
            }

            // Canvas を有効にする
            Debug.Log("canvasTitleBehaviour show ok...");

            APIConnectManager.Instance.constants = LoginInfo.constants;

            //ログインAPIが正常ならリソースDLを行う
            StartCoroutine(LoadAssets());

        }
        else if (LoginInfo.result == "error")
        {
            //申請中の場合は接続先は常にテストに切り替え
            if (LoginInfo.err_code == "error_in_apply")
            {
                Main.Instance.in_apply = true;

                Settings.Host = "test.native.sonnet.crns-game.net";
                Settings.RsourceHost = "test.native.sonnet.crns-game.net";

                APIConnectManager.Instance.Login(Show);
            }
        }
    }



    IEnumerator wait()
    {
        //サウンド読み込みが終わるまで待機
        while (!AudioManager.Instance._seDic.ContainsKey("se_btn"))
        {
            Debug.Log("AudioManager initialize..");
            yield return null;
        }

        Debug.Log("AudioManager initialized ok");
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}
