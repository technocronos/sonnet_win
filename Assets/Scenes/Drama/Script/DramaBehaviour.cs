using System.Collections;
using System.Collections.Generic;
using MyScene;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Playables;
using Scenes.Common.Scripts;

public class DramaBehaviour : BaseBehaviour
{
    public TextMeshProUGUI Line1;
    public TextMeshProUGUI Line2;
    public TextMeshProUGUI Line3;
    public TextMeshProUGUI Line4;
    public Image Speaker;
    public TextMeshProUGUI SpeakerName;
    public Image SpeakerNamePanel;
    public GameObject DramaSpeaker;
    public PlayableDirector director;

    private Image BG1;
    private Image BG2;
    private Image BG3;

    Hashtable spkeakerList = new Hashtable();

    public Image EffectPanel;
    public GameObject TouchPanel;

    public Button Select1;
    public Button Select2;
    public Button Select3;
    public Button Select4;
    public Button Select5;
    public Button Select6;

    public GameObject Content;

    string waitMode { get; set; }

    private static DramaBehaviour instance;

    string trailer = "";

    protected override void Start()
    {
        base.Start();

        Debug.Log("DramaBehaviour Start...");

        SpeakerName.text = "";
        SpeakerNamePanel.enabled = false;

        DramaSpeaker.SetActive(false);

        // エフェクト画像の初期化
        EffectPanel.gameObject.SetActive(true);
        Color efcolor = new Color(0f, 0f, 0f, 0f);
        EffectPanel.material.SetColor("_Color", efcolor);

        //コンテンツはひとまず全部非表示にしておく
        clearContent();

        DispatchEvent(CwEvent.SCENE_READY);
    }

    void OnEnable()
    {
        Debug.Log("DramaBehaviour OnEnable");
        director.stopped += OnPlayableDirectorStopped;

        director.Stop();
    }

    void clearContent()
    {
        foreach (Transform n in Content.transform)
        {
            n.gameObject.SetActive(false);
        }
    }

    void OnPlayableDirectorStopped(PlayableDirector aDirector)
    {
        if (director == aDirector)
            Debug.Log("DramaBehaviour timeline named " + aDirector.name + " is now stopped.");
    }

    void OnDisable()
    {
        director.stopped -= OnPlayableDirectorStopped;
    }

    public jsonDrama drama { get; set; } = null;
    public int dramaId { get; set; } = 0;


    int flowNo { set; get; } = 0;
    // フローとマーカーに関する情報を初期化。
    int flowLen { set; get; } = 0;        // フローエントリの数
    int markerNum { set; get; } = 0;      // マーカーの数
    bool end { get; set; } = false;

    Dictionary<int, string> flow = new Dictionary<int, string>();
    Dictionary<int, string> markerName = new Dictionary<int, string>();
    Dictionary<int, int> markerPos = new Dictionary<int, int>();

    Dictionary<int, string> marker = new Dictionary<int, string>();

    public delegate void OnCompleteDelegate(string result);
    public OnCompleteDelegate CompleteHandler;

    const int acceptKeys = 6;

    public void Show()
    {
        Debug.Log("DramaBehaviour Show.." + this.dramaId + ":" + drama.flow.Length);

        BG1 = Content.transform.Find(drama.BG1).GetComponent<Image>();
        BG2 = Content.transform.Find(drama.BG2).GetComponent<Image>();
        BG3 = Content.transform.Find(drama.BG3).GetComponent<Image>();

        // ファイルが存在するものだけ
        Sprite _bg1 = Utility.getAssetImage("Image/Drama/" + drama.BG1);
        if (_bg1 != null)
        {
            //画像を差し替えていく
            if (BG1 != null)
                BG1.GetComponent<Image>().sprite = _bg1;
        }

        Sprite _bg2 = Utility.getAssetImage("Image/Drama/" + drama.BG2);
        if (_bg2 != null)
        {
            //画像を差し替えていく
            if (BG2 != null)
                BG2.GetComponent<Image>().sprite = _bg2;
        }

        Sprite _bg3 = Utility.getAssetImage("Image/Drama/" + drama.BG3);
        if (_bg3 != null)
        {
            //画像を差し替えていく
            if (BG3 != null)
                BG3.GetComponent<Image>().sprite = _bg3;
        }

        //BG1を表示しておく
        if (BG1 != null)
            BG1.gameObject.SetActive(true);

        DramaSpeaker.SetActive(true);
        TouchPanel.SetActive(false);
        Select1.gameObject.SetActive(false);
        Select2.gameObject.SetActive(false);
        Select3.gameObject.SetActive(false);
        Select4.gameObject.SetActive(false);
        Select5.gameObject.SetActive(false);
        Select6.gameObject.SetActive(false);

        foreach (string sp in drama.speakers)
        {
            string[] arrayStr = sp.Split(new char[] { ' ' });
            spkeakerList[arrayStr[0]] = arrayStr[1];
        }

        flowCompile(drama.flow);

        StartCoroutine(Progress());

    }

    /*
     * フローを解析して準備を整える。表示もクリアして初期状態に戻す。
    */
    private void flowCompile(string[] flows)
    {
        // 表示をクリア。
        this.Clear();

        // 定義されたフローを一つずつ解析して...
        //     ・コンパイルしたフローを、flowN にセットしていく。
        //     ・マーカー名を markerNameN に、対応する位置をmarkerPosN にセットする。
        foreach (string command in flows)
        {
            // フロー解析
            string[] c = command.Split(new char[] { ' ' });
            string com = c[0];

            switch (com)
            {
                // "!PAGE" は "!WAIT" ⇒ "!CLEAR" のセットに分解する。
                case "!PAGE":
                    flow[flowLen++] = "!WAIT";
                    flow[flowLen++] = "!CLEAR";
                    break;

                // "!FULL" は "!HIDE" ⇒ "!WAIT" ⇒ "!SHOW" のセットに分解する。
                case "!FULL":
                    flow[flowLen++] = "!HIDE";
                    flow[flowLen++] = "!WAIT";
                    flow[flowLen++] = "!SHOW";
                    break;
                // "!VIBRATE" は "!DELAY" も追加する。
                case "!VIBRATE":
                    flow[flowLen++] = command;
                    flow[flowLen++] = "!DELAY 660";
                    break;
                // "!MARKER" を見つけたら...
                case "!MARKER":

                    // マーカー用変数に情報をセット。
                    markerName[markerNum] = c[1];
                    //インクリメントされてから再開するので-1
                    markerPos[markerNum] = flowLen - 1;
                    markerNum++;
                    break;

                // "!URLGO", "!URLSEL" を見つけたら...
                case "!URLGO":
                case "!URLSEL":

                    // コマンドの後に "!STOP" を追加する。
                    flow[flowLen++] = command;
                    flow[flowLen++] = "!STOP";
                    break;

                // "!FLOWEND" を見つけたらループを抜ける。
                case "!FLOWEND":
                    end = true;

                    break;
                // それ以外はそのままフローとして使う。
                default:
                    flow[flowLen++] = command;
                    break;
            }
        }
    }

    //
    // フロー処理を開始／再開する。
    // ここにgotoするまえにinitを呼んでおくこと。
    IEnumerator Progress()
    {
        while (flowNo < flowLen)
        {
            yield return StartCoroutine(this.playCommand(this.flow[flowNo]));
            flowNo++;
        }
    }


    IEnumerator playCommand(string _command)
    {
        string[] strArray = _command.Split(new char[] { ' ' });

        string command = strArray[0];
        string value = "";
        if (strArray.Length == 2)
            value = strArray[1];

        switch (command)
        {

            // 一時停止
            case "!WAIT":
                waitMode = "wait";
                yield return StartCoroutine(Wait());
                break;

            // 遅延
            case "!DELAY":
                float delayCount = int.Parse(value) / 1000;
                yield return StartCoroutine(Delay(delayCount));
                break;

            // フロージャンプ
            case "!GOTO":
                string _marker = strArray[1];
                jump(_marker);
                break;

            // ユーザ入力による選択ジャンプ
            case "!SELECT":
                //!SELECT ok retry

                string[] arg = _command.Substring(8).Split(new char[] { ' ' });

                // 数字キーが押されたとき、どのマーカに飛ぶかを擬似配列 marker に指定する。
                for (int i = 1; i <= 9; i++)
                {
                    if (i <= arg.Length)
                        marker[i] = arg[i - 1];
                    else
                        marker[i] = "";
                }

                // "select" モードで待機。
                waitMode = "select";
                yield return StartCoroutine(WaitSelect(arg.Length));
                break;

            // 表示のクリア
            case "!CLEAR":
                this.Clear();
                break;

            // 話し手切り替え(特殊)
            case "!XSPEAKER":
                if (strArray.Length >= 3)
                {
                    value = strArray[1];

                    List<string> name = new List<string>();
                    for (int i = 2; i < strArray.Length; i++)
                    {
                        name.Add(strArray[i]);
                    }

                    //話手切替
                    this.SpeakerName.text = string.Join(" ", name);
                }
                else
                {
                    //話手切替
                    this.SpeakerName.text = (string)this.spkeakerList[value];
                }

                //画像切替
                this.Speaker.sprite = Utility.getAssetImage("Image/Drama/Chara/" + value);

                if (value == "blank")
                {
                    SpeakerNamePanel.enabled = false;
                }
                else
                {
                    SpeakerNamePanel.enabled = true;
                }

                break;

            // 話し手切り替え
            case "!SPEAKER":
                this.SpeakerName.text = value;
                SpeakerNamePanel.enabled = true;
                break;

            // ウィンドウを非表示
            case "!HIDE":
                this.DramaSpeaker.SetActive(false);
                break;

            // ウィンドウを非表示から戻す(表示する)
            case "!SHOW":
                this.DramaSpeaker.SetActive(true);
                break;

            // バイブレーション
            case "!VIBRATE":
                this.vibrate();
                break;

            // 背景画像の番号をチェンジ
            case "!BGCHANGE":
                int current_bg_no = int.Parse(value);

                if (value == "1")
                {
                    if (BG1 != null)
                        BG1.gameObject.SetActive(true);
                    if (BG2 != null)
                        BG2.gameObject.SetActive(false);
                    if (BG3 != null)
                        BG3.gameObject.SetActive(false);

                }
                else if (value == "2")
                {
                    if (BG1 != null)
                        BG1.gameObject.SetActive(false);
                    if (BG2 != null)
                        BG2.gameObject.SetActive(true);
                    if (BG3 != null)
                        BG3.gameObject.SetActive(false);
                }
                else if (value == "3")
                {
                    if (BG1 != null)
                        BG1.gameObject.SetActive(false);
                    if (BG2 != null)
                        BG2.gameObject.SetActive(false);
                    if (BG3 != null)
                        BG3.gameObject.SetActive(true);
                }
                else
                {
                    if (BG1 != null)
                        BG1.gameObject.SetActive(false);
                    if (BG2 != null)
                        BG2.gameObject.SetActive(false);
                    if (BG3 != null)
                        BG3.gameObject.SetActive(false);
                }
                break;

            // 背景画像にエフェクト
            case "!BGEFFECT":
                yield return StartCoroutine(EffectFade(value));
                break;

            // 背景画像を単色塗りつぶし
            case "!BGOUT":

                //カスタムシェーダーを使っているので注意
                //RGBが0の場合は黒、1の場合は白になる。
                Color bgcolor = new Color(0.5f, 0.5f, 0.5f, 0.0f);

                switch (value)
                {
                    case "black":
                        bgcolor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
                        break;
                    case "white":
                        bgcolor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                        break;
                }

                EffectPanel.material.SetColor("_Color", bgcolor);
                break;

            // サウンドを鳴らす。
            case "!SOUND":
                AudioManager.Instance.PlayBGM(value, AudioManager.BGM_VOLUME_DEFULT);
                break;

            // 通常画面遷移
            case "!URLGO":
                waitMode = "geturl";
                string[] arg2 = _command.Split(new char[] { ' ' });
                if (arg2.Length > 1)
                    trailer = arg2[1];
                yield return StartCoroutine(Wait());
                break;

            // 選択画面遷移
            case "!URLSEL":
                waitMode = "selecturl";
                yield return StartCoroutine(Wait());
                break;

            // 停止。
            case "!STOP":
                AudioManager.Instance.StopBGM();
                break;

            // 同期(未使用)
            case "!CALL":
                //call(substring(command, 7, length(command)));
                break;

            // 通常テキスト。
            default:
                setText(_command);
                yield return StartCoroutine(Delay(0.4f));
                break;
        }

    }

    /// <summary>
    /// テキストをセットする
    /// </summary>
    int curLine { get; set; } = 1;
    private void setText(string text)
    {
        switch (this.curLine)
        {
            case 1:
                Line1.text = text;
                break;
            case 2:
                Line2.text = text;
                break;
            case 3:
                Line3.text = text;
                break;
            case 4:
                Line4.text = text;
                break;
        }

        curLine++;

    }

    /// <summary>
    /// 内容をクリアする
    /// </summary>
    private void Clear()
    {
        this.curLine = 1;
        Line1.text = "";
        Line2.text = "";
        Line3.text = "";
        Line4.text = "";
    }

    /// <summary>
    /// ユーザのボタン入力を待機する。
    /// </summary>
    /// <returns></returns>
    private IEnumerator Wait()
    {
        TouchPanel.SetActive(true);
        yield return new WaitUntil(() => TouchPanel.activeInHierarchy == false);
    }

    /// <summary>
    /// ユーザのボタン入力を待機する。2～6まで
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    private IEnumerator WaitSelect(int num)
    {
        if (num < 2 || num > 6) yield break;

        TouchPanel.SetActive(false);

        int line1Y = 700;
        int line2Y = 500;

        switch (num)
        {
            case 2:
                Select1.gameObject.SetActive(true);
                Select2.gameObject.SetActive(true);

                Select1.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(-100, line2Y, 0);
                Select2.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(100, line2Y, 0);

                Select3.gameObject.SetActive(false);
                Select4.gameObject.SetActive(false);
                Select5.gameObject.SetActive(false);
                Select6.gameObject.SetActive(false);
                break;
            case 3:
                Select1.gameObject.SetActive(true);
                Select2.gameObject.SetActive(true);
                Select3.gameObject.SetActive(true);

                Select1.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(-200, line2Y, 0);
                Select2.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, line2Y, 0);
                Select3.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(200, line2Y, 0);

                Select4.gameObject.SetActive(false);
                Select5.gameObject.SetActive(false);
                Select6.gameObject.SetActive(false);
                break;
            case 4:
                Select1.gameObject.SetActive(true);
                Select2.gameObject.SetActive(true);
                Select3.gameObject.SetActive(true);
                Select4.gameObject.SetActive(true);

                Select1.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(-200, line1Y, 0);
                Select2.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, line1Y, 0);
                Select3.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(200, line1Y, 0);
                Select4.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, line2Y, 0);

                Select5.gameObject.SetActive(false);
                Select6.gameObject.SetActive(false);
                break;
            case 5:
                Select1.gameObject.SetActive(true);
                Select2.gameObject.SetActive(true);
                Select3.gameObject.SetActive(true);
                Select4.gameObject.SetActive(true);
                Select5.gameObject.SetActive(true);

                Select1.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(-200, line1Y, 0);
                Select2.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, line1Y, 0);
                Select3.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(200, line1Y, 0);
                Select4.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(-100, line2Y, 0);
                Select5.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(100, line2Y, 0);

                Select6.gameObject.SetActive(false);
                break;
            case 6:
                Select1.gameObject.SetActive(true);
                Select2.gameObject.SetActive(true);
                Select3.gameObject.SetActive(true);
                Select4.gameObject.SetActive(true);
                Select5.gameObject.SetActive(true);
                Select6.gameObject.SetActive(true);

                Select1.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(-200, line1Y, 0);
                Select2.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, line1Y, 0);
                Select3.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(200, line1Y, 0);
                Select4.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(-200, line2Y, 0);
                Select5.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, line2Y, 0);
                Select6.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(200, line2Y, 0);


                break;
        }

        yield return new WaitUntil(() => Select1.gameObject.activeInHierarchy == false);
    }


    /// <summary>
    ///     一定時間待機する
    /// </summary>
    private IEnumerator Delay(float second)
    {
        Debug.Log("Delay start..");
        yield return new WaitForSeconds(second);
        Debug.Log("Delay end..");
    }

    void vibrate()
    {
        director.time = 1f;
        director.Play();
    }
    /// <summary>
    ///     フェードイン、アウトをeffectpanelにかける
    /// </summary>
    private IEnumerator EffectFade(string value)
    {

        //カスタムシェーダーを使っているので注意
        //RGBが0の場合は黒、1の場合は白になる。
        Color efcolor = new Color(0.0f, 0.0f, 0.0f, 0.0f);

        float whileRGB = 1.0f;
        float blackRGB = 0.0f;

        float colorRGB = 0.0f;
        float alpha_src = 0.0f;
        float alpha_dest = 0.0f;

        //初期値設定
        switch (value)
        {
            case "blackout":
                colorRGB = blackRGB;
                alpha_src = 0.0f;
                alpha_dest = 1f;
                break;
            case "blackin":
                colorRGB = blackRGB;
                alpha_src = 1f;
                alpha_dest = 0.0f;
                break;
            case "whiteout":
                colorRGB = whileRGB;
                alpha_src = 0.0f;
                alpha_dest = 1f;
                break;
            case "whitein":
                colorRGB = whileRGB;
                alpha_src = 1f;
                alpha_dest = 0.0f;
                break;
        }

        while (true)
        {
            yield return new WaitForSeconds(Main.Instance.getParFrame());

            if (alpha_src < alpha_dest)
            {
                alpha_src += 0.02f;
                if (alpha_src >= alpha_dest)
                    break;

            }
            else if (alpha_src > alpha_dest)
            {
                alpha_src -= 0.02f;
                if (alpha_src <= alpha_dest)
                    break;

            }
            else
            {
                break; // 繰り返し終了
            }

            // 画像の不透明度を下げる
            efcolor = new Color(colorRGB, colorRGB, colorRGB, alpha_src);
            EffectPanel.material.SetColor("_Color", efcolor);
        }
    }

    /// <summary>
    /// 変数 marker で指定されたマーカ位置へ、フロー処理をジャンプさせる。
    /// </summary>
    /// <param name="_marker"></param>
    void jump(string _marker)
    {
        // 設定されているマーカを一つずつ調べて、指定されたマーカを見つけたらジャンプさせる。
        for (int i = 0; i < markerNum; i++)
        {
            if (markerName[i] == _marker)
            {
                flowNo = markerPos[i];
                break;
            }
        }
    }

    public void onClick(int pushed)
    {
        try
        {
            AudioManager.Instance.PlaySE("se_btn");
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }

        // 
        // waitフレームで待機していたボタンが押されたらcallされる。
        // 変数 pushed には押下されたキーが格納されている。
        // "wait" フレームの付属ラベル。

        // waitModeで指定された動作を行う。
        switch (this.waitMode)
        {

            // フロー処理を再開する。
            case "wait":
                TouchPanel.SetActive(false);
                //this.Progress();
                break;

            // 変数 markerN(Nは整数) で指定されたマーカにジャンプする。
            case "select":

                // 対応するマーカがセットされているなら、処理する。
                if (marker[pushed] != "")
                {
                    Select1.gameObject.SetActive(false);
                    Select2.gameObject.SetActive(false);
                    Select3.gameObject.SetActive(false);
                    Select4.gameObject.SetActive(false);
                    Select5.gameObject.SetActive(false);
                    Select6.gameObject.SetActive(false);

                    string _marker = marker[pushed];
                    jump(_marker);
                    Clear();
                    //Progress();
                }
                break;

            // /:urlOnEnd へ遷移する。
            case "geturl":
                // コールバック実行
                CompleteHandler?.Invoke(trailer);
                CompleteHandler = null;
                break;

            // 押下された数字キーをURL末尾に追加して、/:urlOnEnd へgetUrl()で遷移する。
            case "selecturl":

                // 変数 acceptKeys で指定された値以下のキーならば処理する。
                if (pushed != 0 && pushed <= acceptKeys)
                {
                    // コールバック実行
                    CompleteHandler?.Invoke(pushed.ToString());
                    CompleteHandler = null;
                }
                break;
        }

    }
}
