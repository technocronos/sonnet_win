using MyScene;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.Rendering.Universal;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D.Animation;
using UnityEngine.UI;

public class BaseBehaviour : EventDispatcher
{
    public bool show_header = false;

    private AsyncOperationHandle<IList<Sprite>> m_SpriteHandle;

    protected virtual void Awake()
    {
        //https://engineering.enish.jp/?p=1115
        // EventSystem シングルトンインスタンスが存在しない場合、
        // EventSystem を動的に生成する
        if (EventSystem.current == null)
        {
            var instance = new GameObject("EventSystem");
            EventSystem.current = instance.AddComponent<EventSystem>();
            instance.AddComponent<StandaloneInputModule>();
        }
    }

    protected virtual void Start()
    {
        Debug.Log("BaseBehaviour start run.. this.name=" + this.name);

        Camera maincamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        Camera childcamera = GameObject.Find(this.name + " Camera").GetComponent<Camera>();

        var cameraData = childcamera.GetUniversalAdditionalCameraData();
        cameraData.cameraStack.Add(maincamera);

        if (show_header)
        {
            Header.Instance.gameObject.SetActive(true);
            Footer.Instance.gameObject.SetActive(true);
        }
        else
        {
            Header.Instance.gameObject.SetActive(false);
            Footer.Instance.gameObject.SetActive(false);
        }

        //IsUpdateAssets();

    }

    /// <summary>
    /// リソースの更新があるか判断してある場合はTITLE画面へ遷移させる
    /// </summary>
    public async void IsUpdateAssets()
    {
        string ResourceDlScene = "Title";

        //タイトルの場合はリターン
        if (SceneController.Instance.SceneName == ResourceDlScene) return;

        Debug.Log("BaseBehaviour IsUpdateAssets start...");

        string text = "リソースの更新がありました。TOPへ移動してリソースをダウンロードしてください。";

        string label = "default";

        //Check the download size
        AsyncOperationHandle<long> getDownloadSize = Addressables.GetDownloadSizeAsync(label);
        await getDownloadSize.Task;

        Debug.Log("BaseBehaviour IsUpdateAssets getDownloadSize.Result =" + getDownloadSize.Result + " label=" + label);

        //If the download size is greater than 0, download all the dependencies.
        if (getDownloadSize.Result > 0)
        {
            Debug.Log("BaseBehaviour IsUpdateAssets image update...");
            Main.Instance.showDialogue(text, () =>
            {
                SceneController.Instance.Jump(ResourceDlScene);
            }, 4);
            return;
        }

        //オーディオファイルDL
        //ラベルはbgmとseに分けてるが結局audioというグループにしてるので
        label = "bgm";

        AsyncOperationHandle<long> getDownloadSizebgm = Addressables.GetDownloadSizeAsync(label);
        await getDownloadSizebgm.Task;

        Debug.Log("BaseBehaviour IsUpdateAssets getDownloadSize.Result =" + getDownloadSize.Result + " label=" + label);

        //If the download size is greater than 0, download all the dependencies.
        if (getDownloadSizebgm.Result > 0)
        {
            Debug.Log("BaseBehaviour IsUpdateAssets audio update...");
            Main.Instance.showDialogue(text, () =>
            {
                SceneController.Instance.Jump(ResourceDlScene);
            }, 4);
            return;
        }

    }

    public IEnumerator GetTexture(Image img, string url, string substitute = null)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);

            img.sprite = Utility.getAssetImage(substitute);
        }
        else
        {
            Texture2D myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;

            //Texture2DをSpriteに変換
            Sprite sprite = Sprite.Create(myTexture, new Rect(0, 0, myTexture.width, myTexture.height), Vector2.zero);

            img.sprite = sprite;
        }
    }

    public void setSafearea(string canvasname)
    {
        Canvas HomeCanvas = GameObject.Find(canvasname).GetComponent<Canvas>();
        Vector3 hcv = HomeCanvas.transform.localPosition;

        float safeAreaH = Screen.safeArea.y;

#if UNITY_ANDROID
        safeAreaH = Screen.height - Screen.safeArea.yMax;
#endif

        HomeCanvas.transform.localPosition = new Vector3(hcv.x, hcv.y - safeAreaH, hcv.z);

        GameObject objMainCanvas = GameObject.Find(canvasname + "Main");
        if (objMainCanvas != null)
        {
            Canvas MainCanvas = objMainCanvas.GetComponent<Canvas>();
            Vector3 hcv2 = MainCanvas.transform.localPosition;
            MainCanvas.transform.localPosition = new Vector3(hcv2.x, hcv2.y - safeAreaH, hcv2.z);
        }
    }

    public void RequestReview()
    {
#if UNITY_ANDROID
        StartCoroutine(RequestReviewAndroid());
#elif UNITY_IOS
        UnityEngine.iOS.Device.RequestStoreReview();
#endif
    }

    private IEnumerator RequestReviewAndroid()
    {
#if UNITY_ANDROID
        var reviewManager = new Google.Play.Review.ReviewManager();
        var requestFlowOperation = reviewManager.RequestReviewFlow();
        yield return requestFlowOperation;
        if (requestFlowOperation.Error != Google.Play.Review.ReviewErrorCode.NoError)
        {
            // Log error. For example, using requestFlowOperation.Error.ToString().
            yield break;
        }
        var playReviewInfo = requestFlowOperation.GetResult();
        var launchFlowOperation = reviewManager.LaunchReviewFlow(playReviewInfo);
        yield return launchFlowOperation;
        playReviewInfo = null; // Reset the object
        if (launchFlowOperation.Error != Google.Play.Review.ReviewErrorCode.NoError)
        {
            // Log error. For example, using requestFlowOperation.Error.ToString().
            yield break;
        }
#else
        yield break;
#endif
    }


    //-----------------------------------------------------------------------------------------------------
    /**
     * 指定された画像構成でのレイヤ構造を下から順に配列で返す。
     *
     * @param array     getFormation() の戻り値。
     * @return array    レイヤ画像のパスを格納している序数配列。
     *                  下レイヤから順番に格納されている。
     */
    /*
    public void makeChara(string[] formation, Image Source)
    {
        //Transform Parent = Source.transform.parent;

        // 構成の最初の要素は race の値なので取り出しておく。
        string race = formation[0];

        // レイヤ構造を下から順にファイル名のみで作成。
        List<string> tmplayers = new List<string>();
        switch (race)
        {

            // race:PLA の場合。この場合、配列 $formation の要素には次の部位のアイテムIDが格納されている。
            //     1:武器、2:体、3:頭、4:盾
            case "PLA":
                tmplayers.Add(makeFileName(formation[3]) + "_1");
                tmplayers.Add(makeFileName(formation[2]) + "_1");
                tmplayers.Add(makeFileName(formation[1]));
                tmplayers.Add(makeFileName(formation[2]));
                tmplayers.Add(makeFileName(formation[4]) + "_1");
                tmplayers.Add(makeFileName(formation[3]) + "_2");
                tmplayers.Add(makeFileName(formation[4]) + "_2");
                break;

            // race:MOB の場合はレイヤは一つしかない。
            case "MOB":
                tmplayers.Add(makeFileName(formation[1]));
                break;
        }

        List<Sprite> layers = new List<Sprite>();

        Vector2Int output = new Vector2Int(400, 500);
        Texture2D texture = new Texture2D(output.x, output.y);
        Color32[] texColors = new Color32[output.x * output.y];

        int i = 0;
        foreach (Color32 color in texColors)
        {
            texColors[i] = Color.clear;
            i++;
        }

        texture.SetPixels32(texColors);
        texture.Apply();

        // 構成された各レイヤを見て...
        foreach (string layer in tmplayers)
        {

            // ファイルが存在するものだけ
            Sprite changeimg = Utility.getAssetImage("Image/" + race + "/" + layer);
            if (changeimg != null)
            {
                Texture2D combineTexture = changeimg.texture;

                for (int x = 0; x < combineTexture.width; x++)
                {
                    for (int y = 0; y < combineTexture.height; y++)
                    {
                        var color = combineTexture.GetPixel(x, y);
                        texture.SetPixel(x, y, Color.Lerp(
                            texture.GetPixel(x, y),
                            color,
                            color.a
                            ));
                    }
                }
            }
        }

        texture.Apply();

        Sprite sprite = Sprite.Create(
            texture: texture,
            rect: new Rect(0, 0, texture.width, texture.height),
            pivot: new Vector2(0.5f, 0.5f)
        );

        Source.sprite = sprite;

        //Source.enabled = false;
    }
    */


    private string makeFileName(string filename)
    {
        string text = "";

        if (filename != "")
            text = int.Parse(filename).ToString("D5");

        return text;
    }

    public void makeCharaUI(string[] formation, Image CharaImage)
    {
        // 構成の最初の要素は race の値なので取り出しておく。
        string race = formation[0];

        // レイヤ構造を下から順にファイル名のみで作成。
        List<string> tmplayers = new List<string>();
        switch (race)
        {

            // race:PLA の場合。この場合、配列 $formation の要素には次の部位のアイテムIDが格納されている。
            //     1:武器、2:体、3:頭、4:盾
            case "PLA":
                tmplayers.Add(makeFileName(formation[3]) + "_1");
                tmplayers.Add(makeFileName(formation[2]) + "_1");
                tmplayers.Add(makeFileName(formation[1]));
                tmplayers.Add(makeFileName(formation[2]));
                tmplayers.Add(makeFileName(formation[4]) + "_1");
                tmplayers.Add(makeFileName(formation[3]) + "_2");
                tmplayers.Add(makeFileName(formation[4]) + "_2");
                break;

            // race:MOB の場合はレイヤは一つしかない。
            case "MOB":
                tmplayers.Add(makeFileName(formation[1]));
                tmplayers.Add("");
                tmplayers.Add("");
                tmplayers.Add("");
                tmplayers.Add("");
                tmplayers.Add("");
                tmplayers.Add("");
                break;
        }

        int i = 1;
        // 構成された各レイヤを見て...
        foreach (string layer in tmplayers)
        {
            // ファイルが存在するものだけ
            Sprite changeimg = Utility.getAssetImage("Image/" + race + "/" + layer);
            Image img = CharaImage.transform.Find("CharaImage" + i).GetComponent<Image>();

            if (changeimg != null)
            {
                img.enabled = true;
                img.sprite = changeimg;
                img.gameObject.SetActive(true);
            }
            else
            {
                img.enabled = false;
            }

            i++;
        }

        CharaImage.sprite = Utility.getAssetImage("Image/transparent");
    }

    public void makeChara(string[] formation, Image CharaImage)
    {
        // 構成の最初の要素は race の値なので取り出しておく。
        string race = formation[0];

        // レイヤ構造を下から順にファイル名のみで作成。
        List<string> tmplayers = new List<string>();
        switch (race)
        {
            // race:PLA の場合。この場合、配列 $formation の要素には次の部位のアイテムIDが格納されている。
            //     1:武器、2:体、3:頭、4:盾
            case "PLA":
                tmplayers.Add(makeFileName(formation[3]) + "_1");
                tmplayers.Add(makeFileName(formation[2]) + "_1");
                tmplayers.Add(makeFileName(formation[1]));
                tmplayers.Add(makeFileName(formation[2]));
                tmplayers.Add(makeFileName(formation[4]) + "_1");
                tmplayers.Add(makeFileName(formation[3]) + "_2");
                tmplayers.Add(makeFileName(formation[4]) + "_2");
                break;

            // race:MOB の場合はレイヤは一つしかない。
            case "MOB":
                tmplayers.Add(makeFileName(formation[1]));
                tmplayers.Add("");
                tmplayers.Add("");
                tmplayers.Add("");
                tmplayers.Add("");
                tmplayers.Add("");
                tmplayers.Add("");
                break;
        }

        int i = 1;
        // 構成された各レイヤを見て...
        foreach (string layer in tmplayers)
        {
            // ファイルが存在するものだけ
            Sprite changeimg = Utility.getAssetImage("Image/" + race + "/" + layer);
            Transform CharaImageObj = CharaImage.transform.Find("CharaImage" + i);

            if (changeimg != null)
            {
                ParticleSystemRenderer avatar_renderer = CharaImageObj.GetComponent<ParticleSystemRenderer>();

                avatar_renderer.material.SetTexture("_MainTex", changeimg.texture);
                CharaImageObj.gameObject.SetActive(true);
            }
            else
            {
                CharaImageObj.gameObject.SetActive(false);
            }

            i++;
        }

        CharaImage.enabled = false;
    }

    public void makeCharaAnim(string[] formation, GameObject CharaImage)
    {
        // 構成の最初の要素は race の値なので取り出しておく。
        string race = formation[0];

        // レイヤ構造を下から順にファイル名のみで作成。
        List<string> tmplayers = new List<string>();
        switch (race)
        {
            // race:PLA の場合。この場合、配列 $formation の要素には次の部位のアイテムIDが格納されている。
            //     1:武器、2:体、3:頭、4:盾
            case "PLA":
                tmplayers.Add(makeFileName(formation[3]) + "_1");
                tmplayers.Add(makeFileName(formation[2]) + "_1");
                tmplayers.Add(makeFileName(formation[1]));
                tmplayers.Add(makeFileName(formation[2]));
                tmplayers.Add(makeFileName(formation[4]) + "_1");
                tmplayers.Add(makeFileName(formation[3]) + "_2");
                tmplayers.Add(makeFileName(formation[4]) + "_2");
                break;

            // race:MOB の場合はレイヤは一つしかない。
            case "MOB":
                tmplayers.Add(makeFileName(formation[1]));
                tmplayers.Add("");
                tmplayers.Add("");
                tmplayers.Add("");
                tmplayers.Add("");
                tmplayers.Add("");
                tmplayers.Add("");
                break;
        }

        int i = 1;
        // 構成された各レイヤを見て...
        foreach (string layer in tmplayers)
        {
            // ファイルが存在するものだけ
            Sprite changeimg = Utility.getAssetImage("Image/" + race + "/" + layer);
            Transform CharaImageObj = CharaImage.transform.Find("CharaImage" + i);

            if (changeimg != null)
            {
                CharaImageObj.gameObject.SetActive(true);

                SpriteResolver resolver = CharaImageObj.GetComponent<SpriteResolver>();
                resolver.SetCategoryAndLabel("Layer" + i, race + "_" + layer);
            }
            else
            {
                CharaImageObj.gameObject.SetActive(false);
            }
            i++;
        }
    }

    //プッシュ通知の設定
    public void SettingPush(string channelId, string channelName, string explain, string title, string body, int sendtime)
    {
        //　Androidチャンネルの登録
        //LocalPushNotification.RegisterChannel(引数1,引数２,引数３);
        //引数１ Androidで使用するチャンネルID なんでもいい LocalPushNotification.AddSchedule()で使用する
        //引数2　チャンネルの名前　なんでもいい　アプリ名でも入れておく
        //引数3　通知の説明 なんでもいい　自分がわかる用に書いておくもの　
        LocalPushNotification.RegisterChannel(channelId, channelName, explain);

        //通知のクリア
        LocalPushNotification.AllClear();

        // プッシュ通知の登録
        //LocalPushNotification.AddSchedule(引数１,引数2,引数3,引数4,引数5);
        //引数１ プッシュ通知のタイトル
        //引数2　通知メッセージ
        //引数3　表示するバッジの数(バッジ数はiOSのみ適用の様子 Androidで数値を入れても問題無い)
        //引数4　何秒後に表示させるか？
        //引数5　Androidで使用するチャンネルID　「Androidチャンネルの登録」で登録したチャンネルIDと合わせておく
        //注意　iOSは45秒経過後からしかプッシュ通知が表示されない        
        LocalPushNotification.AddSchedule(title, body, 1, sendtime, channelId);
    }

    protected virtual void OnDestroy()
    {
        if (m_SpriteHandle.IsValid()) Addressables.Release(m_SpriteHandle);
        DestroyListener();
    }
}
