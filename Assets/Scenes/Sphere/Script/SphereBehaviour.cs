using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using CreateWave;
using Scenes.Common.Scripts;
using DG.Tweening;

public class SphereBehaviour : BaseBehaviour
{

    /*
     //-------------------------------------------------------------------------
     * public 
     */
    public TextMeshProUGUI PreterText;
    public GameObject Preter;
    public GameObject OkCancel;
    public GameObject Line;
    public GameObject TouchPanel;
    public GameObject Story;
    public TextMeshProUGUI StoryText;
    public GameObject StagePanel;
    public GameObject ApDispPanel;
    public GameObject InfoW;
    public BteffXBehaviour bteffX;

    public GameObject units;
    public GameObject tip0_0;
    public UeveBehaviour ueve;
    public GameObject cursor;
    public GameObject marker;
    public GameObject sphere_bg;

    public GameObject RaidMonstar;

    public Button raidDisp;
    public TextMeshProUGUI RaidButtonText;

    jsonConstants constants;

    public class Parameter
    {
        public int sphereId;
        public string reopen;
    }

    public Parameter Param;

    public static SphereBehaviour Instance
    {
        get
        {
            return instance;
        }
    }

    private static SphereBehaviour instance;

    //システム文言
    public string ERROR_RELOAD { get; set; }

    public string ERROR_NO_ACTIONPT { get; set; } 

    public string TRANS_OTHER_SCENE { get; set; } 

    public string SHOWWND_PUSH_BUTTON { get; set; } 

    public string SHOWWND_IN_TRANS { get; set; } 

    public string SHOWWND_FAIL_SEND_CMD { get; set; } 

    public string SHOWWND_FAIL_TRANS { get; set; } 

    public string SHOWWND_RELOAD_FOR_LIMIT { get; set; } 

    public string STR_CONFIRM_CHANGE_EQP { get; set; } 

    public string _STRING_MENU { get; set; } 
    public string _STRING_CANCEL { get; set; } 
    public string _STRING_ENTER { get; set; }
    public string _STRING_DETAIL { get; set; } 

    // ボタンのフォーカスマークを消す。
    bool _focusRect { get; set; } = false;

    // ステージの大きさ。330だが下はコマンド入力画面
    public float STAGE_WID { get; set; } = 1920;

    public float STAGE_HEI { get; set; } = 1080;

    public int STAGE_MARGIN { get; set; } = 2;

    public int TOP_MARGIN { get; set; } = 2;

    public int BOTTOM_MARGIN { get; set; } = 2;

    // マップチップの大きさ
    public int TIP_SIZE { get; set; } = 75;
    public int UNIT_SIZE { get; set; } = 72;

    // 行動ptの最大値
    public int ACTPT_MAX { get; set; } = 100;

    public int actPt { get; set; } = 0;

    //public int EASY_MODE { get; set; }
    //public bool EASY_MODE_CHANGE { get; set; } = true;


    //mitter
    jsonSphereCommand mitter { get; set; } = new jsonSphereCommand();

    //sphere構造体
    public jsonSphere sphere { get; set; } = null;

    StageBehaviour Stage;
    UserBehaviour User;
    EnvironmentBehaviour Environment;

    MapTip objMapTip;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        Debug.Log("SphereBehaviour start.." + Param.sphereId);
        setSafearea("SphereCanvas");

        instance = this;

        ERROR_RELOAD  = Utility.getText("SPHERE_ERROR_RELOAD");

        ERROR_NO_ACTIONPT  = Utility.getText("SPHERE_ERROR_NO_ACTIONPT");

        TRANS_OTHER_SCENE  = Utility.getText("SPHERE_TRANS_OTHER_SCENE");

        SHOWWND_PUSH_BUTTON  = Utility.getText("TEXT_MESSAGE_BATTLE_TAP_SCREEN");

        SHOWWND_IN_TRANS  = Utility.getText("SPHERE_SHOWWND_IN_TRANS");

        SHOWWND_FAIL_SEND_CMD  = Utility.getText("SPHERE_SHOWWND_FAIL_SEND_CMD");

        SHOWWND_FAIL_TRANS  = Utility.getText("SPHERE_SHOWWND_FAIL_TRANS");

        SHOWWND_RELOAD_FOR_LIMIT  = Utility.getText("SPHERE_SHOWWND_RELOAD_FOR_LIMIT");

         STR_CONFIRM_CHANGE_EQP = Utility.getText("SPHERE_STR_CONFIRM_CHANGE_EQP");

        _STRING_MENU = Utility.getText("SPHERE_STRING_MENU");
        _STRING_CANCEL = Utility.getText("SPHERE_STRING_CANCEL");
        _STRING_ENTER  = Utility.getText("SPHERE_STRING_ENTER");
        _STRING_DETAIL = Utility.getText("SPHERE_STRING_DETAIL");

        //定数取得
        constants = APIConnectManager.Instance.login.constants;

        //状態を初期化
        Preter.SetActive(false);
        TouchPanel.SetActive(false);
        Story.SetActive(false);
        InfoW.SetActive(false);
        OkCancel.SetActive(false);
        raidDisp.gameObject.SetActive(false);
        RaidMonstar.SetActive(false);

        Vector3 hcv = ApDispPanel.transform.localPosition;
        Vector3 hcv_r = raidDisp.transform.localPosition;

        float safeAreaH = Screen.safeArea.y;

#if UNITY_ANDROID
        safeAreaH = Screen.height - Screen.safeArea.yMax;
#endif

        ApDispPanel.transform.localPosition = new Vector3(hcv.x, hcv.y - safeAreaH, hcv.z);
        raidDisp.transform.localPosition = new Vector3(hcv_r.x, hcv_r.y - safeAreaH, hcv_r.z);

        mitter.lead = new Dictionary<string, string>();

        objMapTip = new MapTip();

        //ステージの大きさを入れておく
        STAGE_WID = StagePanel.GetComponent<RectTransform>().rect.width;
        STAGE_HEI = StagePanel.GetComponent<RectTransform>().rect.height;

        //APIをたたく
        APIConnectManager.Instance.Sphere(Param.sphereId, Param.reopen, onStart);

    }

    void onStart(string json)
    {
        this.JsonToClass(json);

        Stage = StageBehaviour.Instance;
        //ステージ作成、開始
        Stage.init();

        EnvironmentBehaviour.Instance.setEnv(sphere.environment);

        actPt = sphere.actionPt;

        /*
        EASY_MODE = sphere.EASY_MODE;

        //元々easymodeのクエストは変更できない
        if (EASY_MODE == 1)
            EASY_MODE_CHANGE = false;

        //easymodeが変更できるクエストの場合
        if (EASY_MODE_CHANGE)
        {
            //preyerprefから保存しているsphereidを得る
            int easymode_sphere_id = PlayerPrefs.GetInt(Settings.EASYMODE_SPHEREID, 0);
            //sphereidが一致したら
            if (easymode_sphere_id == Param.sphereId)
            {
                //設定を戻す 
                EASY_MODE = PlayerPrefs.GetInt(Settings.EASYMODE_SPHERE);
            }
            else
            {
                PlayerPrefs.SetInt(Settings.EASYMODE_SPHEREID, Param.sphereId);
                PlayerPrefs.SetInt(Settings.EASYMODE_SPHERE, EASY_MODE);
            }
        }
        */


        ApDispPanel.GetComponent<apDispBehaviour>().init();

        if (sphere.raid_dungeon.status == constants.Raid_Dungeon.START)
        {
            raidDisp.gameObject.SetActive(true);
            RaidButtonText.DOFade(0.0f, 1f).SetEase(Ease.InCubic).SetLoops(-1, LoopType.Yoyo);
            raidDisp.onClick.RemoveAllListeners();
            raidDisp.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlaySE("se_btn");

                RaidMonstar.SetActive(true);
                RaidMonstar.GetComponent<RaidMonstarBehaviour>().init(sphere.raid_dungeon.id);
            });
        }

        User = UserBehaviour.Instance;

        if (sphere.readonly_flg == 1)
        {
            this.user();
        }
        else
        {
            this.reopen();
        }
    }

    void reopen()
    {
        // ロード時の指揮がないならば、サーバーに問い合わせるフローに入る。
        if (sphere.preLd == null)
        {
            // 解説ウィンドウを表示。タップ待ち。
            this.showPreter(SHOWWND_PUSH_BUTTON, "center", "rExe");
        }
        // ロード時の指揮があるなら...
        else
        {
            // サーバー通信ムービーに指揮を転送して・・
            int i;
            for (i = 0; i < sphere.preLd.Length; i++)
                mitter.lead["lead" + (i + 1)] = sphere.preLd[i];

            mitter.leadNum = i;

            // 指揮再生を行う。
            this.Lead();

            // 次の進捗確認で問い合わせを行うように、ロード時指揮をクリアしておく。
            sphere.preLd = null;
        }
    }

    // 解説ウィンドウを表示。タップ待ち。
    public void showPreter(string _text, string place = "center", string _event = null)
    {

        PreterText.text = _text;
        Preter.SetActive(true);

        switch (place)
        {
            case "top":
                Preter.transform.localPosition = new Vector3(0, (STAGE_HEI / 2) - 130, 0);
                break;
            case "center":
                Preter.transform.localPosition = new Vector3(0, 0, 0);
                break;
        }

        if (_event != null)
        {
            TouchPanel.SetActive(true);
            TouchPanel.GetComponent<Button>().onClick.AddListener((() =>
            {
                TouchPanel.SetActive(false);
                onTap(_event);
            }));
        }
    }

    public void onTap(string scene)
    {
        AudioManager.Instance.PlaySE("se_btn");

        //リスナーを削除しておく
        TouchPanel.GetComponent<Button>().onClick.RemoveAllListeners();

        Dictionary<string, string> varVal = new Dictionary<string, string>();

        switch (scene)
        {
            case "rExe":
                leader.transUrl = "scene=Sphere&reopen=continue&id=" + Param.sphereId;
                this.Trans();

                Preter.SetActive(false);
                break;
        }
    }



    void showStory(string _text)
    {
        StoryText.text = _text;
        Story.SetActive(true);

    }

    //
    // サーバーからの指揮内容を再生するためのムービー
    //
    // 指揮一覧
    //     ※) nnnは任意桁数の数字、NNNは固定桁数の数字、xxxは任意文字数の半角文字列、
    //         XXXは固定文字数の半角文字列、あああ は任意文字数の自由文字列を表す。
    //
    //     表示
    //         IPRET あああ             解説の表示／非表示。引数は解説内容。
    //                                  解説は四角ウィンドウで画面上部に表示される。
    //                                  解説内容をカラ文字にすると解説を非表示にする。
    //                                  表示幅は半角31まで
    //         NOTIF あああ             注意を表示してボタンが押されるまで待機。引数は注意内容。
    //                                  注意は四角ウィンドウで画面中央に表示される。
    //                                  表示幅は半角31まで
    //         LINES NNN あああ         セリフを表示してボタンが押されるまで待機。引数はユニット番号、セリフ内容
    //                                  セリフはユニットにカーソルを合わせたあと、セリフ用の
    //                                  ウィンドウで表示される。
    //         SPEAK NNN あああ あああ  LINESと同じだが、話し手を指定できる。
    //                                  引数は、ユニット番号、話し手、セリフ内容。
    //                                  仕様上、話し手にスペースを含めることはできない。
    //         EFFEC recv NNNN...       指定の座標でエフェクトを再生する。種類、X1,Y1、X2,Y2、...
    //                                  たとえば (2,3), (4,5) に回復エフェクトを出すなら
    //                                      EFFEC recv 02030405
    //                                  とする。
    //         DAMAG NNN nnn            ダメージの表示。引数はユニット番号、ダメージ量
    //         RECOV NNN nnn            回復の表示。引数はユニット番号、回復量
    //         FOCUS NNN                指定のユニットへフォーカス合わせ。
    //         PFOCS NN NN              指定の座標へフォーカス合わせ。
    //         VIBRA NN                 画面に振動をかける。数字は振動幅。00で振動を止める。
    //
    //     データの更新
    //         ACTPT nnn                行動ptの更新
    //         UMOVE NNN nnn            ユニットの移動。引数はユニット番号。移動方向
    //         USTAT NNN xxx            ユニットステータスの更新。引数はユニット番号、ステータス内容
    //         UVALS NNN XX nnn         ユニットステータスの特定の値を更新。引数はユニット番号、ステータス名、値。
    //                                  ステータス名に指定できるのは、今のところ"hp"のみ。
    //                                  値は USTAT と同じ桁数にゼロ詰めしておくこと。
    //         UITEM NNN xxx            ユニットアイテム情報の更新。引数はユニット番号、アイテム内容
    //         UEQIP NNN xxx            ユニット装備の更新。引数はユニット番号、アイテム内容
    //         UEXIT NNN xxx            ユニットの退場。引数はユニット番号。退場の仕方
    //         UADDI NNN NN NN NN NN NNN あああ
    //                                  ユニットの追加。引数はユニット番号、X座標、Y座標、グラフィック番号、ユニットの所属番号、移動力、名前。
    //                                  このコマンドの前に、USTAT, UITEM, UEQIP で該当の情報をセットしておくこと。
    //         ITEMD NNN xxx            アイテム情報の追加／更新。引数はアイテム番号、アイテムデータ。
    //         ORNAM NNN xxx            置物の更新。引数は置物番号、置物内容。
    //         RPMAT NN nnn             横一行分の敷物の変更。引数は行番号、敷物の状態
    //         RPBG1 NN NN NN           地形を一箇所変更。引数はX座標、Y座標、変更後のチップ番号
    //         REVIS nnn                リビジョン番号の更新
    //
    //     その他
    //         COMND                    ユーザに、現在のユニットの行動を入力させる。
    //         DELAY nnn                指定のミリ秒数遅延
    //         WAIT                     ユーザがボタンを押すまで待機
    //         ERROR xxx                コマンド送信にエラーがあるため、リロードの必要アリ。
    //                                  xxxはエラー理由。以下のいずれか。
    //                                      actpt   行動ptが足りない
    //                                      rev     リビジョンが古い
    //                                      move    移動先がおかしい
    //                                      item    行動がおかしい
    //                                      use     行動対象座標がおかしい
    //                                      sys     その他
    //         TRANS xxx                指定のURLへ画面遷移
    //         NOOP                     特になにもないことを表すダミーコマンド。
    //
    //     内部で使用
    //         IMOVE NNN N              指定のユニットを一つ移動
    //         MESCT あああ             画面中央に四角ウィンドウでメッセージを表示
    //         LSPKR NNN あああ         セリフウィンドウのスピーカー欄を変更する
    //                                  NNNに有効な番号がセットされているならその番号のユニット名を、
    //                                  無効な番号なら後続のテキストをそのまま使う。
    //         LTEXT NNN あああ         指定のユニットの真上にセリフウィンドウを表示して、
    //                                  ユーザがボタンを押すまで待機。
    //         UEVNT NNN xxx nnn        指定のユニットにxxxのタイプのユニットイベントを表示する。
    //                                  数字情報が必要な場合はnnnの部分に指定する。
    //                                  ダメージエフェクトや回復エフェクトの表示に使用する。
    //         UEFCT NNN xxx            指定のユニットにxxxのエフェクトを行わせる。
    //         UREMV NNN                指定のユニットを削除
    //         APDSW 0/1                行動ptゲージの表示／非表示。
    public void Mitter(string json)
    {
        Debug.Log(json);

        mitter = JsonUtility.FromJson<jsonSphereCommand>(json);

        Dictionary<string, object> jsonDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        foreach (KeyValuePair<string, object> keyvalue in jsonDict)
        {
            if (keyvalue.Key == "lead")
            {
                mitter.lead = JsonConvert.DeserializeObject<Dictionary<string, string>>(keyvalue.Value.ToString());
            }
        }

        leader.flow.Clear();

        this.Lead();
    }


    public class Leader
    {
        public int flowCsr = 0;        // 次に実行するフローの位置(フローカーソル)
        public string transSound = ""; //画面遷移時のサウンド指定
        public string transText = "";
        public string transUrl = "";      // フロー終了時、画面遷移するかどうか。するならそのURL
        public bool userComm = false;   // フロー終了時、ユーザコマンドを待機するかどうか
        public int commUnit;
        public string unitNo;
        public List<string> flow = new List<string>();
    }

    public Leader leader = new Leader();

    //
    // 受信した指揮の再生を開始する。
    void Lead()
    {
        // フローを初期化。
        leader.flowCsr = 0;
        leader.transSound = "";
        leader.transText = "";
        leader.transUrl = "";
        leader.userComm = false;

        //フリックスタートフラグ初期化（stageのact関数が動く）
        User.flick_flg = false;
        //指揮再生時はタップは無効
        User.tap_flg = false;

        // 送信機のレスポンスにある指揮内容を一つずつ見て、内部コマンド(フロー)に分解していく。
        for (int i = 1; i <= mitter.leadNum; i++)
        {
            // 指揮取得。
            string lead = mitter.lead["lead" + i];

            Debug.Log("TRANS lead=" + lead);

            // フローに変換
            switch (lead.Split(new char[] { ' ' })[0])
            {

                // 注意の表示
                case "NOTIF":
                    //NOTIF 難易度調整
                    leader.flow.Add("MESCT " + lead.Substring(6));
                    leader.flow.Add("WAIT");
                    leader.flow.Add("MESCT");
                    break;

                // セリフの表示
                case "LINES":
                //LINES 001 なんか出た・・
                case "SPEAK":
                    //SPEAK 001 もじょ そうみたいなのだ
                    string spNo;
                    string speaker;
                    string text;

                    leader.unitNo = lead.Split(new char[] { ' ' })[1];

                    // LINES か SPEAK かで、話し手とテキストの取得方法が異なる。
                    if (lead.Split(new char[] { ' ' })[0] == "LINES")
                    {
                        spNo = leader.unitNo;
                        speaker = "";
                        text = lead.Substring(10);
                    }
                    else
                    {
                        spNo = "000";
                        speaker = lead.Split(new char[] { ' ' })[2];
                        text = lead.Substring(11 + speaker.Length);

                        //コードならtext_masterから取得してspeaker翻訳
                        if (speaker.Contains("%"))
                        {
                            speaker = Utility.getText("sphere_speaker_" + speaker.Replace("%", ""));
                        }
                    }

                    leader.flow.Add("FOCUS " + leader.unitNo);
                    leader.flow.Add("LSPKR " + spNo + " " + speaker);
                    leader.flow.Add("LTEXT " + leader.unitNo + " " + text);
                    leader.flow.Add("LTEXT");
                    break;

                // 粗筋の表示
                case "STORY":
                    //STORY あれがアトランティスの主？
                    leader.flow.Add("STORY " + lead.Substring(6));
                    leader.flow.Add("WAIT");
                    leader.flow.Add("STORY");

                    break;

                // ダメージ／回復の表示
                case "DAMAG":
                //DAMAG 001 1
                case "RECOV":
                    //RECOV 001 1000

                    // FOCUS⇒UEVNT⇒UEFCT⇒DELAY に変換する
                    leader.unitNo = lead.Split(new char[] { ' ' })[1];
                    string num = lead.Split(new char[] { ' ' })[2];
                    string effType = (lead.Split(new char[] { ' ' })[0] == "DAMAG") ? "dam" : "recov";

                    leader.flow.Add("FOCUS " + leader.unitNo);
                    leader.flow.Add("UEVNT " + leader.unitNo + " " + effType + " " + num);
                    leader.flow.Add("UEFCT " + leader.unitNo + " " + effType);
                    break;

                // ユニットの向き変更
                case "UALGN":
                    //UALGN 001 0
                    leader.unitNo = lead.Split(new char[] { ' ' })[1];
                    string align = lead.Split(new char[] { ' ' })[2];

                    leader.flow.Add("IALGN " + leader.unitNo + " " + align);

                    break;
                // ユニットの移動
                case "UMOVE":
                    //UMOVE 003 2222

                    leader.unitNo = lead.Split(new char[] { ' ' })[1];
                    string move = lead.Split(new char[] { ' ' })[2];

                    leader.flow.Add("FOCUS " + leader.unitNo);

                    for (int j = 0; j < move.Length; j++)
                    {
                        var align2 = "";

                        if (move.Substring(j, 1) == "2")
                            align2 = "3";
                        else if (move.Substring(j, 1) == "4")
                            align2 = "1";
                        else if (move.Substring(j, 1) == "6")
                            align2 = "2";
                        else if (move.Substring(j, 1) == "8")
                            align2 = "0";

                        leader.flow.Add("IALGN " + leader.unitNo + " " + align2);
                        leader.flow.Add("IMOVE " + leader.unitNo + " " + move.Substring(j, 1));
                        leader.flow.Add("DELAY 200");
                    }
                    break;

                //ユニット追加の場合、もしjsonに無い場合はunitをnewしておく
                case "USTAT":

                    int u_no = int.Parse(lead.Split(new char[] { ' ' })[1]);

                    if (!sphere.unit.ContainsKey(u_no))
                    {
                        sphere.unit[u_no] = new jsonUnit();
                        sphere.unit[u_no].Status = lead.Substring(10);
                    }

                    leader.flow.Add(lead);

                    break;
                // 一部ステータスの変更
                case "UVALS":
                    //UVALS 001 hp 00158

                    // USTATに変換する。
                    leader.unitNo = lead.Split(new char[] { ' ' })[1];
                    string name = lead.Split(new char[] { ' ' })[2];
                    string val = lead.Split(new char[] { ' ' })[3];
                    string cur = sphere.unit[int.Parse(leader.unitNo)].Status;

                    switch (name)
                    {
                        case "hp":
                            leader.flow.Add("USTAT " + leader.unitNo + " " + cur.Substring(0, 5) + val + " " + cur.Substring(11));
                            break;
                    }

                    break;

                // ユニットの退場
                case "UEXIT":
                    //UEXIT 003 collap

                    // FOCUS⇒UEFCT⇒DELAY⇒UREMV に変換する
                    leader.unitNo = lead.Split(new char[] { ' ' })[1];
                    leader.flow.Add("FOCUS " + leader.unitNo);
                    leader.flow.Add("UEFCT " + leader.unitNo + " " + lead.Split(new char[] { ' ' })[2]);
                    leader.flow.Add("UREMV " + leader.unitNo);

                    break;

                //サウンド（画面遷移時のサウンドのみ指定）
                case "SOUND":
                    //SOUND se_zazaza

                    leader.transSound = lead.Split(new char[] { ' ' })[1];
                    break;

                //SEサウンド
                case "SEPLY":
                    //SEPLY se_gotoquest

                    string sfx = lead.Split(new char[] { ' ' })[1];
                    leader.flow.Add("SEPLY " + sfx);
                    break;

                case "BGMPL":
                    //BGMPL bgm_op

                    string bgm = lead.Split(new char[] { ' ' })[1];
                    leader.flow.Add("BGMPL " + bgm);
                    break;

                //bttleエフェクト
                case "BTEFF":
                    //BTEFF 001 001

                    string com = lead.Split(new char[] { ' ' })[1];
                    leader.flow.Add("BTEFF " + com);
                    break;

                case "ARROW":
                    //ARROW SHOW OR ARROW HIDE

                    string mode = lead.Split(new char[] { ' ' })[1];
                    leader.flow.Add("ARROW " + mode);
                    break;
                // 画面遷移
                case "TRANS":

                    // フロー終了時における制御になるので、フローそのものには組み込まない。
                    leader.transUrl = lead.Split(new char[] { ' ' })[1];

                    break;

                // エラー
                case "ERROR":

                    // エラーコードを取得。
                    string code = lead.Split(new char[] { ' ' })[1];

                    // 行動pt不足の場合はそれ用のページへ。
                    if (code == "actpt")
                    {
                        leader.transText = ERROR_NO_ACTIONPT;
                        leader.transUrl = sphere.apShortUrl;
                    }
                    else
                    {
                        // その他の場合はリロードを行う。
                        leader.transText = ERROR_RELOAD + code;
                        leader.transUrl = sphere.reloadUrl;
                    }
                    break;

                // NOOP は無視する。
                case "NOOP":
                    break;

                // その他はそのままフローに追加する。
                default:
                    leader.flow.Add(lead);
                    break;
            }
        }

        // フローを処理していく。
        StartCoroutine(this.Prog());
    }

    // フロー処理を開始／再開する。
    IEnumerator Prog()
    {
        string command = "";

        while (true)
        {
            //フローの初回だけ処理
            if (leader.flowCsr == 1)
            {
                if (leader.flowCsr >= leader.flow.Count && leader.transUrl != "")
                {
                    //コマンド最初が他画面遷移の場合、画面表示＋サウンド発声しない

                }
                else
                {
                    //それ以外はBGMを鳴らして画面表示
                    AudioManager.Instance.PlayBGM(sphere.bgm, AudioManager.BGM_VOLUME_DEFULT);
                    DispatchEvent(CwEvent.SCENE_READY);
                }
            }


            // フローの末尾に到達したら...
            if (leader.flowCsr >= leader.flow.Count)
            {
                // 画面センタリングマージンを戻す
                Stage.lef = STAGE_MARGIN;
                Stage.top = TOP_MARGIN;
                Stage.rig = STAGE_MARGIN;
                Stage.bot = BOTTOM_MARGIN;
                if (leader.transUrl != "")
                {
                    // フローに "TRANS" が含まれていたなら画面遷移
                    if (leader.transText != "")
                        this.showPreter(leader.transText);
                    this.Trans();
                }
                else if (leader.userComm)
                {
                    // フローに "COMND" が含まれていたならユーザ入力受付フェーズへ。
                    this.user();
                }
                else
                {
                    // "COMND" が含まれていなかったなら進捗確認フェーズへ。
                    this.reopen();
                }

                ApDispPanel.gameObject.SetActive(true);

                // このムービーはstopする。
                yield break;
            }

            // フローカーソルが示しているフローの内容を取得。
            // 取得したらフローカーソルを一つ進めておく。
            command = leader.flow[leader.flowCsr];
            leader.flowCsr++;

            // フローを一つ処理する。処理されるまで待つ。
            yield return StartCoroutine(this.Proc(command));
        }
    }

    /// <summary>
    // 変数 command で指定されたフロー内容を処理する。
    // call後、変数 goto には、フロー処理をいったん停止して、フレーム移動するべきかどうかが格納される。
    // 移動するべきであるときはそのフレーム名が、フロー処理を続行すべきであるときはカラ文字が入る。
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    IEnumerator Proc(string command)
    {
        string _goto = "";
        int unitNo = 0;

        Debug.Log(command);

        switch (command.Split(new char[] { ' ' })[0])
        {
            // 上部ウィンドウでメッセージの表示
            case "IPRET":
                //IPRET ゴブリン Lv5が現れました
                string txt = (command.Split(new char[] { ' ' }).Length > 2) ? command.Substring(6) : "";
                if (txt != "")
                {
                    this.showPreter(txt, "top");
                }
                else
                {
                    Preter.SetActive(false);
                }
                break;

            // 粗筋を表示。
            case "STORY":
                //STORY 難易度調整 lv1
                if (command.Split(new char[] { ' ' }).Length > 1)
                {
                    this.showStory(command.Substring(6));
                }
                else
                {
                    Story.SetActive(false);
                }

                break;

            // マスエフェクトの再生
            case "EFFEC":
                //EFFEC recv 1005

                // エフェクトタイプをセット。
                string type = command.Split(new char[] { ' ' })[1];

                // 座標の設定を取得。
                string poses = command.Split(new char[] { ' ' })[2];

                Stage.objEffects.num = poses.Length / 4;
                for (int i = 0; i < Stage.objEffects.num; i++)
                {
                    Stage.objEffects.posX[i] = int.Parse(poses.Substring(0 + i * 4, 2));
                    Stage.objEffects.posY[i] = int.Parse(poses.Substring(2 + i * 4, 2));
                }

                // 再生
                Stage.objEffects.go(type);

                // 再生が終わるまで待機
                _goto = "effect_watch";
                break;
            // BATTLEエフェクトの再生
            case "BTEFF":
                //BTEFF 002 3
                //unitNo = int.Parse(command.Split(new char[] { ' ' })[1]);
                //int aign = (command.Split(new char[] { ' ' }).Length > 2) ? int.Parse(command.Split(new char[] { ' ' })[2]) : 0;

                //float btlX = sphere.unit[unitNo].X;
                //float btlY = sphere.unit[unitNo].Y;

                //bteffX.setPos(btlX, btlY, aign);
                // 再生
                //StartCoroutine(bteffX.PlayAnim("bteffX"));

                bteffX.Play();

                break;

            // ユニット、座標へのフォーカス
            case "FOCUS":
            //FOCUS 002
            case "PFOCS":
                //PFOCS 15 09

                // フォーカス
                if (command.Split(new char[] { ' ' })[0] == "FOCUS")
                {
                    Stage.unitNo = int.Parse(command.Split(new char[] { ' ' })[1]);
                    Stage.focus();
                }
                else
                {
                    Stage.moveX = int.Parse(command.Split(new char[] { ' ' })[1]);
                    Stage.moveY = int.Parse(command.Split(new char[] { ' ' })[2]);
                    Stage.moveCsr();
                }

                // 画面センタリングの修正
                Stage.lef = 4;
                Stage.top = 4;
                Stage.rig = 4;
                Stage.bot = 4;

                Stage.center();

                // 画面スクロールが終わるまで待機
                _goto = "act_watch";
                break;

            // 振動
            case "VIBRA":
                //VIBRA 03

                Stage.vib = int.Parse(command.Split(new char[] { ' ' })[1]);
                break;

            case "ENVCG":
                //ENVCG FF7A7A

                string changeColor = command.Split(new char[] { ' ' })[1];
                EnvironmentBehaviour.Instance.changeColor(changeColor);

                break;
            // 行動ptの変更
            case "ACTPT":
                //ACTPT 60
                actPt = int.Parse(command.Split(new char[] { ' ' })[1]);
                ApDispPanel.GetComponent<apDispBehaviour>().refInfo();
                break;

            // ユニットステータスの更新
            case "USTAT":
                //USTAT 008 0007 00092 00092 0051 0054 0051 0049 0049 0049 0047 0020

                int u_no = int.Parse(command.Split(new char[] { ' ' })[1]);

                sphere.unit[u_no].Status = command.Substring(10);
                break;

            // ユニットアイテム情報の更新
            case "UITEM":
                //UITEM 008 007 007 007 007 007 007 007
                unitNo = int.Parse(command.Split(new char[] { ' ' })[1]);

                //無い場合は作成しておく。
                if (!sphere.unit.ContainsKey(unitNo))
                {
                    jsonUnit u = new jsonUnit();
                    sphere.unit[unitNo] = u;
                }

                sphere.unit[unitNo].Item = command.Substring(10);
                break;

            // ユニット装備の更新
            case "UEQIP":
                //UEQIP 008
                unitNo = int.Parse(command.Split(new char[] { ' ' })[1]);

                //無い場合は作成しておく。
                if (!sphere.unit.ContainsKey(unitNo))
                {
                    jsonUnit u = new jsonUnit();
                    sphere.unit[unitNo] = u;
                }

                sphere.unit[unitNo].Eqp = command.Substring(10);
                break;

            // ユニットの追加
            case "UADDI":
                //UADDI 008 07 15 01 02 040 00 オークキング
                unitNo = int.Parse(command.Split(new char[] { ' ' })[1]);

                //無い場合は作成しておく。
                if (!sphere.unit.ContainsKey(unitNo))
                {
                    jsonUnit u = new jsonUnit();
                    sphere.unit[unitNo] = u;
                }


                // ユニット定義を追加。
                sphere.unit[unitNo].X = int.Parse(command.Split(new char[] { ' ' })[2]);
                sphere.unit[unitNo].Y = int.Parse(command.Split(new char[] { ' ' })[3]);
                sphere.unit[unitNo].Info = command.Substring(16, 12);
                sphere.unit[unitNo].Name = command.Split(new char[] { ' ' })[8];

                // ユニット総数を設定しなおす。
                if (sphere.unitNum < unitNo)
                    sphere.unitNum = unitNo;

                AudioManager.Instance.PlaySE("se_pallet_fall");

                // ユニットを作成。
                Stage.unitNo = unitNo;
                Stage.objUnits.createUnits(unitNo);
                break;

            // アイテム情報の更新
            case "ITEMD":
                //ITEMD 007 eqp noeff 00 00 桃色ぱれお

                sphere.item[int.Parse(command.Split(new char[] { ' ' })[1])] = command.Substring(10);
                break;

            // 置物の更新
            case "ORNAM":
                //ORNAM 004

                // データの更新
                int ornNo = int.Parse(command.Split(new char[] { ' ' })[1]);
                sphere.orn[ornNo] = (command.Split(new char[] { ' ' }).Length > 2) ? command.Substring(10) : "";

                // 反映
                Stage.objOrnaments.refInfo(ornNo);
                break;

            // 敷物の更新
            case "RPMAT":
                //RPMAT 03 0000000111110000

                // データの更新
                int lineNo = int.Parse(command.Split(new char[] { ' ' })[1]);
                sphere.mat[lineNo] = command.Substring(9);

                // 反映
                Stage.objMapTip.line(lineNo);

                // すべての置物の表示状態を更新する。
                Stage.objOrnaments.ShowAll();
                break;

            // 背景を一箇所変更
            case "RPBG1":
                //RPBG1 16 06 0009

                // 引数を取得。
                int x = int.Parse(command.Split(new char[] { ' ' })[1]);
                int y = int.Parse(command.Split(new char[] { ' ' })[2]);
                string tip = command.Split(new char[] { ' ' })[3];

                // 反映
                Stage.RepBg(x, y, tip);
                break;

            // リビジョン変更
            case "REVIS":
                //REVIS 17

                sphere.revision = int.Parse(command.Split(new char[] { ' ' })[1]);
                break;

            // 遅延
            case "DELAY":
                //DELAY 200
                int delaytime = int.Parse(command.Split(new char[] { ' ' })[1]);
                delayCount = (float)(delaytime) / 1000;

                _goto = "delay";
                break;

            // 待機
            case "WAIT":
                _goto = "wait";
                break;

            // ユニット向き
            case "IALGN":
                unitNo = int.Parse(command.Split(new char[] { ' ' })[1]);

                int align = int.Parse(command.Split(new char[] { ' ' })[2]);
                Stage.unitNo = unitNo;
                Stage.objUnits.units["unit_" + unitNo.ToString()].setAlign(align);

                break;
            // ユニット移動
            case "IMOVE":
                AudioManager.Instance.PlaySE("se_pallet_rotate");

                // 引数を取得。
                unitNo = int.Parse(command.Split(new char[] { ' ' })[1]);
                int dir = int.Parse(command.Split(new char[] { ' ' })[2]);

                // X,Y軸における移動量を取得。
                int moveX = ((dir - 1) % 3) - 1;
                int moveY = ((dir - 1) / 3) - 1;

                // 移動。
                sphere.unit[unitNo].X += moveX;
                sphere.unit[unitNo].Y += moveY;

                Stage.unitNo = unitNo;
                Stage.objUnits.units["unit_" + unitNo.ToString()].setPos(true);

                break;

            // 真ん中ウィンドウでメッセージの表示
            case "MESCT":
                if (command.Split(new char[] { ' ' }).Length > 1)
                {
                    txt = command.Substring(6);
                    this.showPreter(txt, "center");
                }
                else
                {
                    Preter.SetActive(false);
                }
                break;

            // セリフウィンドウの話し手を変更
            case "LSPKR":

                unitNo = int.Parse(command.Split(new char[] { ' ' })[1]);
                string speaker = "";

                // ユニット番号があるならユニット名を、ないなら番号の後ろのテキストをそのまま使う。
                if (unitNo != 0)
                {
                    speaker = sphere.unit[unitNo].Name;
                }
                else
                {
                    speaker = command.Substring(10);

                }

                Line.GetComponent<LineBehaviour>().setSpeaker(speaker);
                break;

            // セリフウィンドウの表示
            case "LTEXT":

                if (command.Split(new char[] { ' ' }).Length == 1)
                {
                    //LTEXTのみの場合
                    Line.GetComponent<LineBehaviour>().hide();
                }
                else
                {
                    string text = command.Substring(10);
                    unitNo = int.Parse(command.Split(new char[] { ' ' })[1]);

                    UnitBehaviour _u = Stage.objUnits.units["unit_" + unitNo];

                    Line.GetComponent<LineBehaviour>().show(text, _u.transform.GetComponent<RectTransform>().anchoredPosition.x, _u.transform.GetComponent<RectTransform>().anchoredPosition.y);
                    Line.SetActive(true);
                    Line.GetComponent<RectTransform>().SetAsLastSibling();
                    _goto = "wait";
                }

                break;

            // ユニットにxxxのタイプのユニットイベントを表示する。
            case "UEVNT":
                //複製する
                UeveBehaviour _ueve = UnityEngine.Object.Instantiate(ueve, new Vector3(0, 0, 0), Quaternion.identity, Stage.transform);
                _ueve.transform.localPosition = new Vector3(0, 0, 0);

                string[] arg = command.Split(new char[] { ' ' });
                _ueve.Play(int.Parse(arg[1]), arg[2], arg[3]);
                break;

            // ユニットにエフェクトを再生させる。
            case "UEFCT":

                // 対象ユニットのムービーへのパスを取得。
                int _no = int.Parse(command.Split(new char[] { ' ' })[1]);

                string path = "/stage/units/no" + _no;
                UnitBehaviour _unit = Stage.objUnits.units["unit_" + _no];

                // 指定のエフェクトを再生させる。
                StartCoroutine(_unit.setEffects(command.Split(new char[] { ' ' })[2]));

                yield return StartCoroutine(wait_effec(_unit));

                break;

            // SE再生
            case "SEPLY":
                string sfx = command.Split(new char[] { ' ' })[1];
                AudioManager.Instance.PlaySE(sfx);

                break;

            // BGM再生
            case "BGMPL":
                string bgm = command.Split(new char[] { ' ' })[1];
                AudioManager.Instance.PlayBGM(bgm);

                break;
            // ユニットの削除
            case "UREMV":
                Stage.unitNo = int.Parse(command.Split(new char[] { ' ' })[1]);
                Stage.objUnits.remove(Stage.unitNo);
                break;

            // 行動ptゲージの表示/非表示
            case "APDSW":
                if (command.Split(new char[] { ' ' })[1] == "1")
                    ApDispPanel.gameObject.SetActive(true);
                else
                    ApDispPanel.gameObject.SetActive(false);

                break;
            // アイテムボタンの矢印表示
            case "ARROW":
                if (command.Split(new char[] { ' ' })[1] == "SHOW")
                {
                    User.Arrow.SetActive(true);
                    Vector3 pos = User.Arrow.GetComponent<RectTransform>().anchoredPosition;
                    User.Arrow.GetComponent<ArrowBehaviour>().Show("down", pos.x, pos.y);
                }
                else
                {
                    User.Arrow.SetActive(false);
                }

                break;

            // ユーザのコマンド入力
            case "COMND":

                leader.commUnit = int.Parse(command.Split(new char[] { ' ' })[1]);

                // フラグを立てて覚えておく。
                leader.userComm = true;

                break;
        }

        //gotoに関数が指定されてる場合、関数を実行し終わるまで待つ。
        if (_goto != "")
            yield return StartCoroutine(_goto);

    }

    float delayCount { get; set; }
    bool wait_flg { get; set; }

    /// <summary>
    /// effect_playが終わるまで待つ。
    /// </summary>
    /// <returns></returns>
    IEnumerator effect_watch()
    {

        while (Stage.objEffects.effect_play)
        {
            Debug.Log("effect_watch run...");
            yield return null;
        }

        Debug.Log("effect_watch end...");
    }

    IEnumerator act_watch()
    {

        //ステージスクロールが終わるまで待機
        while (Stage.scrolling)
        {
            Debug.Log("act_watch run...");
            yield return null;
        }

        Debug.Log("act_watch end...");
    }

    IEnumerator wait_effec(UnitBehaviour _unit)
    {
        //ステージスクロールが終わるまで待機
        while (_unit.stop)
        {
            Debug.Log("wait_effec run...");
            yield return null;
        }

        Debug.Log("wait_effec end...");
    }

    /// <summary>
    /// delayCount秒待つ
    /// </summary>
    /// <returns></returns>
    IEnumerator delay()
    {
        // 一定時間待機
        yield return new WaitForSeconds(delayCount);
    }

    /// <summary>
    /// タップされるまで待つ
    /// </summary>
    /// <returns></returns>
    IEnumerator wait()
    {
        Debug.Log("WAIT run...");

        wait_flg = true;

        TouchPanel.gameObject.SetActive(true);
        TouchPanel.GetComponent<Button>().onClick.AddListener((() =>
        {
            AudioManager.Instance.PlaySE("se_btn");

            TouchPanel.GetComponent<Button>().onClick.RemoveAllListeners();
            TouchPanel.gameObject.SetActive(false);

            wait_flg = false;
        }));

        DispatchEvent(CwEvent.SCENE_READY);

        //wait_flgがfalseになるまで待機
        while (wait_flg)
        {
            yield return null;
        }

    }

    //
    // 変数 command で指定されたコマンド文字列を解析して、引数を擬似配列 arg に格納する。
    // また、検出した引数の数を変数 argNum に格納する。
    private List<string> parse(string command)
    {

        // 初期化。
        int argNum = 0;
        List<string> arg = new List<string>();


        // command の文字列を一文字ずつ見ていく。
        // 現在何文字目を見ているかを変数 pos で管理する。
        int pos = 1;
        while (true)
        {

            // 一文字抽出。
            string _char = command.Substring(pos, 1);
            pos++;

            // 文字列が終了しているならループを抜ける。
            if (_char == "")
                break;

            // スペースなら、引数の終了として判断する。
            if (_char == " ")
            {
                argNum++;
                arg[argNum] = "";

                // スペース以外は引数を構成する文字列と解釈する。
            }
            else
            {
                arg[argNum] = arg[argNum] + _char;
            }
        }

        return arg;
    }

    /*
     * jsonで受け取った情報をjsonSphereクラスに格納する
     */
    private void JsonToClass(string json)
    {
        //API結果受け取り
        sphere = JsonUtility.FromJson<jsonSphere>(json);

        Dictionary<string, object> jsonDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

        foreach (KeyValuePair<string, object> keyvalue in jsonDict)
        {
            if (keyvalue.Key == "unit")
            {
                Dictionary<int, jsonUnit> units = new Dictionary<int, jsonUnit>();

                Dictionary<string, object> dict1 = JsonConvert.DeserializeObject<Dictionary<string, object>>(keyvalue.Value.ToString());
                foreach (KeyValuePair<string, object> keyvalue2 in dict1)
                {
                    Debug.Log(keyvalue2.Value);

                    jsonUnit jsonunitlist;
                    jsonunitlist = JsonUtility.FromJson<jsonUnit>(keyvalue2.Value.ToString());

                    units.Add(int.Parse(keyvalue2.Key), jsonunitlist);
                }

                sphere.unit = units;
            }
            else if (keyvalue.Key == "unitIcon")
            {
                sphere.unitIcon = JsonConvert.DeserializeObject<Dictionary<int, string>>(keyvalue.Value.ToString());
            }
            else if (keyvalue.Key == "item")
            {
                try
                {
                    sphere.item = JsonConvert.DeserializeObject<Dictionary<int, string>>(keyvalue.Value.ToString());
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
                sphere.item[999] = "itm damag 01 00 " + Utility.getText("SPHERE_STR_CMD_ATACK");
            }
            else if (keyvalue.Key == "orn")
            {
                try
                {
                    sphere.orn = JsonConvert.DeserializeObject<Dictionary<int, string>>(keyvalue.Value.ToString());
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }
            else if (keyvalue.Key == "tip")
            {
                sphere.tip = JsonConvert.DeserializeObject<Dictionary<int, string>>(keyvalue.Value.ToString());

            }
            else if (keyvalue.Key == "tipId")
            {

                sphere.tipId = JsonConvert.DeserializeObject<Dictionary<int, string>>(keyvalue.Value.ToString());
            }
        }
    }


    //----------------------------------------------------------------------
    // ユーザ入力を処理するフェーズを表す。
    //----------------------------------------------------------------------
    void user()
    {
        // コマンドユニットにカーソルを合わせる。
        Debug.Log("user run..");

        //User作成、開始
        User.commUnit = leader.commUnit;
        InfoW.SetActive(true);
        StartCoroutine(User.init());

        Stage.unitNo = leader.commUnit;
        Stage.focus();

    }

    //
    //画面遷移をする
    //
    public void Trans()
    {
        if (leader.transSound != "")
        {
            AudioManager.Instance.PlaySE(leader.transSound);
        }

        AudioManager.Instance.StopBGM();

        Dictionary<string, string> transUrl = new Dictionary<string, string>();
        transUrl = Utility.ParseUrl(leader.transUrl);

        switch (transUrl["scene"])
        {
            case "Sphere":
                SceneController.Instance.Jump("Sphere", (() =>
                {
                    SphereBehaviour _sphere = FindObjectOfType<SphereBehaviour>() as SphereBehaviour;
                    _sphere.Param = new SphereBehaviour.Parameter
                    {
                        sphereId = int.Parse(transUrl["id"]),
                        reopen = transUrl.ContainsKey("reopen") ? transUrl["reopen"] : null,
                    };
                }));
                break;
            case "FieldEnd":
                SceneController.Instance.Jump("FieldEnd", (() =>
                {
                    FieldEndBehaviour _fieldend = FindObjectOfType<FieldEndBehaviour>() as FieldEndBehaviour;
                    _fieldend.Param = new FieldEndBehaviour.Parameter
                    {
                        sphereId = int.Parse(transUrl["sphereId"]),
                    };
                }));
                break;
            case "FieldDrama":
                SceneController.Instance.Jump("FieldDrama", (() =>
                {
                    FieldDramaBehaviour _fielddrama = FindObjectOfType<FieldDramaBehaviour>() as FieldDramaBehaviour;
                    _fielddrama.Param = new FieldDramaBehaviour.Parameter
                    {
                        sphereId = int.Parse(transUrl["sphereId"]),
                    };
                }));
                break;
            case "Battle":
                SceneController.Instance.Jump("Battle", (() =>
                {
                    BattleBehaviour _battle = FindObjectOfType<BattleBehaviour>() as BattleBehaviour;
                    _battle.Param = new BattleBehaviour.Parameter
                    {
                        battleId = int.Parse(transUrl["battleId"]),
                    };
                }));
                break;
            case "Suggest":
                SceneController.Instance.Jump("Suggest", (() =>
                {
                    SuggestBehaviour _suggest = FindObjectOfType<SuggestBehaviour>() as SuggestBehaviour;
                    _suggest.Param = new SuggestBehaviour.Parameter
                    {
                        type = transUrl["type"],
                        targetId = transUrl.ContainsKey("targetId") ? transUrl["targetId"] : null,
                        backto = transUrl["backto"],
                        useto = transUrl.ContainsKey("useto") ? transUrl["useto"] : null,
                    };
                }));
                break;
            default:
                SceneController.Instance.Jump(transUrl["scene"]);
                break;
        }
    }

    // 
    // 変数 x, y で示された座標に存在するユニットの番号を返す。
    // 引数)
    //     x    チップ単位の座標。
    //     y    
    // 戻り値)
    //     unitNo   指定された場所に存在するユニット番号
    //              見つからなかった場合は 0。
    public int FindUnit(int x, int y)
    {
        int unitNo = 0;

        // ユニットを一つずつ見ていく。
        for (int i = 1; i <= sphere.unitNum; i++)
        {

            if (sphere.unit.ContainsKey(i))
            {
                jsonUnit unitinfo = sphere.unit[i];

                // 指定された座標に存在するユニットが見つかったらその番号を戻り値にセット。X=-1は削除されたユニット
                if (unitinfo.X != -1 && unitinfo.X == x && unitinfo.Y == y)
                {
                    unitNo = i;
                    break;
                }
            }
        }

        return unitNo;
    }

    //
    // 指定されたポイントからポイントへ一定スピードで動くときの計算を行う。
    // 引数)
    //     src      現在の位置
    //     dest     目標点
    //     speed    スピード
    // 戻り値)
    //     src      次の位置
    public int propMove(int dest, int _src, int speed)
    {

        int dir;
        int destDir;

        // 正方向に動くのか、負方向に動くのかを取得。
        if (dest > _src)
            dir = +1;
        else
            dir = -1;

        // 移動。
        int src = _src + (dir * speed);

        // 移動の結果、方向が変わったなら、目標点をすぎたということ。補正する。
        if (dest > src)
            destDir = +1;
        else
            destDir = -1;

        if (dir != destDir)
            src = dest;


        return src;
    }

}
