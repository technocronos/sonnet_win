using DG.Tweening;
using MyScene;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scenes.Common.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;
using UnityEngine.UI;
using static SphereBehaviour;

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
    public HpBehaviour HPGaugePanel;
    public ExpDispBehaviour ExpDispPanel;
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


    public class GameState
    {
        public bool is_gamestart = false;
        public bool is_pause = false;
        public bool is_stop = false;
        public bool is_gameover = false;
    }

    public GameState gamestate = new GameState();

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
    public int TIP_SIZE { get; set; } = 110;
    public int UNIT_SIZE { get; set; } = 105;

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

        // JSONファイルからギミックを初期化
        if (!string.IsNullOrEmpty(sphere.jsonfile))
        {
            InitializeGimmicksFromJson(sphere.jsonfile, "start", "start");
        }

        Stage = StageBehaviour.Instance;
        //ステージ作成、開始
        Stage.init();

        EnvironmentBehaviour.Instance.setEnv(sphere.environment);

        actPt = sphere.actionPt;

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

    bool firegimmickflg = false;

    // 前フレームの各ユニットの位置を記録（ユニット番号 -> 位置）
    Dictionary<int, jsonUnit> lastUnitPositions = new Dictionary<int, jsonUnit>();
    
    void Update()
    {
        if (gamestate.is_gamestart && gamestate.is_gameover == false && !firegimmickflg)
        {
            // 列挙中にコレクションが変更されないように、キーのコピーを作成
            var unitKeys = new List<int>(sphere.unit.Keys);
            
            // すべてのユニットをチェック
            foreach (int unit_no in unitKeys)
            {
                if (!sphere.unit.ContainsKey(unit_no))
                    continue;
                    
                jsonUnit unit = sphere.unit[unit_no];
                if (unit == null || unit.X == -1) // X=-1は削除されたユニット
                    continue;

                // ユニットが移動したかどうかを判定
                bool hasMoved = false;
                
                if (lastUnitPositions.ContainsKey(unit_no))
                {
                    // 前フレームの位置と比較（0.5刻みのスナップを考慮して比較）
                    jsonUnit lastUnit = lastUnitPositions[unit_no];
                    float currentX = Mathf.Round(unit.X * 2f) / 2f;
                    float currentY = Mathf.Round(unit.Y * 2f) / 2f;
                    float lastX = Mathf.Round(lastUnit.X * 2f) / 2f;
                    float lastY = Mathf.Round(lastUnit.Y * 2f) / 2f;
                    
                    if (Mathf.Abs(currentX - lastX) > 0.1f || Mathf.Abs(currentY - lastY) > 0.1f)
                    {
                        hasMoved = true;
                    }
                }
                else
                {
                    // 初回はユニットを記録するだけ（移動したとみなさない）
                    hasMoved = false;
                }
                
                // 移動した場合のみギミックをチェック
                if (hasMoved)
                {
                    //過去のユニットがギミックを発動したかチェック
                    bool is_fire = CheckGimmickByUnit(lastUnitPositions[unit_no], false, true);
                    if (!is_fire)
                    {
                        // ギミックをチェック
                        CheckGimmickByUnit(unit, false);
                    }
                }
                
                // 位置を更新（jsonUnitのコピーを作成して保存）
                // 参照ではなく値のコピーを作成する必要がある
                string json = JsonUtility.ToJson(unit);
                jsonUnit unitCopy = JsonUtility.FromJson<jsonUnit>(json);
                lastUnitPositions[unit_no] = unitCopy;
            }

            // 削除されたユニットの位置情報をクリーンアップ
            var keysToRemove = new List<int>();
            foreach (var key in lastUnitPositions.Keys)
            {
                if (!sphere.unit.ContainsKey(key) || sphere.unit[key] == null || sphere.unit[key].X == -1)
                {
                    keysToRemove.Add(key);
                }
            }
            foreach (var key in keysToRemove)
            {
                lastUnitPositions.Remove(key);
            }
        }
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

    public void LeadCall(List<string> command)
    {
        mitter.lead.Clear();

        int i = 1;
        foreach(string com in command)
        {
            mitter.lead["lead" + i] = com;
            i++;
        }

        leader.flow.Clear();

        this.Lead();
    }

    private string COMMAND_UEXIT(int unitNo)
    {
        return "UEXIT " + unitNo + " collap";
    }
    private string COMMAND_UEXIT2(int unitNo)
    {
        return "UEXIT2 " + unitNo + " collap";
    }

    private string COMMAND_TRANS_FIELDEND()
    {
        return "TRANS scene=FieldEnd&sphereId=" + Param.sphereId;
    }

    private string COMMAND_TRANS_QUEST()
    {
        return "TRANS scene=Quest";
    }

    private string COMMAND_TRANS_REOPEN()
    {
        return "TRANS scene=Sphere&id=" + Param.sphereId + "&reopen=true";
    }

    private string COMMAND_DELAY(int millsec)
    {
        return "DELAY " + millsec;
    }

    private string COMMAND_DAMAG(int no, int damage)
    {
        return "DAMAG " + no + " " + damage;
    }

    private string COMMAND_DAMAG2(int no, int damage)
    {
        return "DAMAG2 " + no + " " + damage;
    }

    public void GameOver(int no, int damage)
    {
        //Time.timeScale = 0.0f;

        //Stage.objUnits.units["unit_" + no].commandkeyrecv = false;
        //Stage.act_start = true;

        List<string> command = new List<string>();
        //command.Add(COMMAND_DAMAG(no, damage));
        //command.Add(COMMAND_UEXIT(no));
        //command.Add(COMMAND_TRANS_FIELDEND());
        command.Add(COMMAND_TRANS_REOPEN());

        LeadCall(command);
    }

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
        for (int i = 1; i <= mitter.lead.Count; i++)
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
                case "DAMAG2":
                //DAMAG 001 1
                case "RECOV2":

                    // FOCUS⇒UEVNT⇒UEFCT⇒DELAY に変換する
                    leader.unitNo = lead.Split(new char[] { ' ' })[1];
                    string num = lead.Split(new char[] { ' ' })[2];
                    string effType = (lead.Split(new char[] { ' ' })[0].Contains("DAMAG")) ? "dam" : "recov";

                    if(lead.Split(new char[] { ' ' })[0] == "DAMAG" || lead.Split(new char[] { ' ' })[0] == "RECOV")
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
                    }

                    leader.flow.Add(lead);

                    break;
                // 一部ステータスの変更
                case "UVALS":
                    //UVALS 001 hp 00158

                    // USTATに変換する。
                    leader.unitNo = lead.Split(new char[] { ' ' })[1];
                    string name = lead.Split(new char[] { ' ' })[2];
                    int val = int.Parse(lead.Split(new char[] { ' ' })[3]) ;
                    UnitStatus cur = sphere.unit[int.Parse(leader.unitNo)].Status;

                    switch (name)
                    {
                        case "hp":
                            leader.flow.Add("USTAT " + leader.unitNo + " " + (cur.hp + val) + " " + cur.maxhp + " " + cur.att1 + " " + cur.att2 + " " + cur.att3 + " " + cur.def1 + " " + cur.def2 + " " + cur.def3 + " " + cur.spd + " " + cur.defX);
                            break;
                    }

                    break;

                // ユニットの退場
                case "UEXIT":
                case "UEXIT2":
                    //UEXIT 003 collap

                    // FOCUS⇒UEFCT⇒DELAY⇒UREMV に変換する
                    leader.unitNo = lead.Split(new char[] { ' ' })[1];

                    if (lead.Split(new char[] { ' ' })[0] == "UEXIT")
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
                gamestate.is_stop = true;

                if (leader.flowCsr >= leader.flow.Count && leader.transUrl != "")
                {
                    //コマンド最初が他画面遷移の場合、画面表示＋サウンド発声しない

                }
                else
                {
                    //それ以外はBGMを鳴らして画面表示
                    AudioManager.Instance.PlayBGM(sphere.bgm, AudioManager.BGM_VOLUME_DEFULT);
                    gamestate.is_gamestart = true;
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

                gamestate.is_stop = false;

                //ApDispPanel.gameObject.SetActive(true);
                firegimmickflg = false;

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
                //ApDispPanel.GetComponent<apDispBehaviour>().refInfo();
                break;

            // ユニットステータスの更新
            case "USTAT":
                //USTAT 008 0007 00092 00092 0051 0054 0051 0049 0049 0049 0047 0020

                int u_no = int.Parse(command.Split(new char[] { ' ' })[1]);
                sphere.unit[u_no].Status.level = int.Parse(command.Split(new char[] { ' ' })[2]);
                sphere.unit[u_no].Status.hp = int.Parse(command.Split(new char[] { ' ' })[3]);
                sphere.unit[u_no].Status.maxhp = int.Parse(command.Split(new char[] { ' ' })[4]);
                sphere.unit[u_no].Status.att1 = int.Parse(command.Split(new char[] { ' ' })[5]);
                sphere.unit[u_no].Status.att2 = int.Parse(command.Split(new char[] { ' ' })[6]);
                sphere.unit[u_no].Status.att3 = int.Parse(command.Split(new char[] { ' ' })[7]);
                sphere.unit[u_no].Status.def1 = int.Parse(command.Split(new char[] { ' ' })[8]);
                sphere.unit[u_no].Status.def2 = int.Parse(command.Split(new char[] { ' ' })[9]);
                sphere.unit[u_no].Status.def3 = int.Parse(command.Split(new char[] { ' ' })[10]);
                sphere.unit[u_no].Status.spd = int.Parse(command.Split(new char[] { ' ' })[11]);
                sphere.unit[u_no].Status.defX = int.Parse(command.Split(new char[] { ' ' })[12]);

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

                // コマンドが10文字以上の場合のみアイテム情報を処理
                if (command.Length > 10)
                {
                    foreach (string item_id in command.Substring(10).Split(new char[] { ' ' }))
                    {
                        if (!string.IsNullOrEmpty(item_id) && int.TryParse(item_id, out int id) && id > 0)
                        {
                            sphere.unit[unitNo].Item.Add(id);
                        }
                    }
                }
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

                // コマンドが10文字以上の場合のみ装備情報を処理
                if (command.Length > 10)
                {
                    foreach(string eqp_id in command.Substring(10).Split(new char[] { ' ' }))
                    {
                        if (!string.IsNullOrEmpty(eqp_id) && int.TryParse(eqp_id, out int id) && id > 0)
                        {
                            sphere.unit[unitNo].Eqp.Add(id);
                        }
                    }
                }
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
                sphere.unit[unitNo].Info.graphNo = int.Parse(command.Split(new char[] { ' ' })[4]);
                sphere.unit[unitNo].Info.union = int.Parse(command.Split(new char[] { ' ' })[5]);
                sphere.unit[unitNo].Info.cost = int.Parse(command.Split(new char[] { ' ' })[6]);
                sphere.unit[unitNo].Info.align = int.Parse(command.Split(new char[] { ' ' })[7]);
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
                string[] arg = command.Split(new char[] { ' ' });

                UnitBehaviour _ueve = Stage.objUnits.units["unit_" + int.Parse(arg[1])];
                _ueve.UnitEvent(int.Parse(arg[1]), arg[2], int.Parse(arg[3]));
  
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
                /*
                if (command.Split(new char[] { ' ' })[1] == "1")
                    ApDispPanel.gameObject.SetActive(true);
                else
                    ApDispPanel.gameObject.SetActive(false);
                */
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
        while (_unit.walk_stop)
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
        Stage.act_start = true;

        TouchPanel.gameObject.SetActive(true);
        TouchPanel.GetComponent<Button>().onClick.AddListener((() =>
        {
            AudioManager.Instance.PlaySE("se_btn");

            TouchPanel.GetComponent<Button>().onClick.RemoveAllListeners();
            TouchPanel.gameObject.SetActive(false);

            wait_flg = false;
            Stage.act_start = false;

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

                    jsonUnit jsonunitlist = new jsonUnit();
                    jsonUnitLocal jsonunitlistlocal = JsonUtility.FromJson<jsonUnitLocal>(keyvalue2.Value.ToString());

                    jsonunitlist.Name = jsonunitlistlocal.Name;
                    jsonunitlist.X = jsonunitlistlocal.X;
                    jsonunitlist.Y = jsonunitlistlocal.Y;
                    jsonunitlist.code = jsonunitlistlocal.code;
                    jsonunitlist.act_brain = jsonunitlistlocal.act_brain;
                    jsonunitlist.player_owner = jsonunitlistlocal.player_owner;
                    jsonunitlist.Info.graphNo = int.Parse(jsonunitlistlocal.Info.Split(new char[] { ' ' })[0]);
                    jsonunitlist.Info.union = int.Parse(jsonunitlistlocal.Info.Split(new char[] { ' ' })[1]);
                    jsonunitlist.Info.cost = int.Parse(jsonunitlistlocal.Info.Split(new char[] { ' ' })[2]);
                    jsonunitlist.Info.align = int.Parse(jsonunitlistlocal.Info.Split(new char[] { ' ' })[3]);

                    jsonunitlist.Status.level = int.Parse(jsonunitlistlocal.Status.Split(new char[] { ' ' })[0]);
                    jsonunitlist.Status.hp = int.Parse(jsonunitlistlocal.Status.Split(new char[] { ' ' })[1]);
                    jsonunitlist.Status.maxhp = int.Parse(jsonunitlistlocal.Status.Split(new char[] { ' ' })[2]);
                    jsonunitlist.Status.att1 = int.Parse(jsonunitlistlocal.Status.Split(new char[] { ' ' })[3]);
                    jsonunitlist.Status.att2 = int.Parse(jsonunitlistlocal.Status.Split(new char[] { ' ' })[4]);
                    jsonunitlist.Status.att3 = int.Parse(jsonunitlistlocal.Status.Split(new char[] { ' ' })[5]);
                    jsonunitlist.Status.def1 = int.Parse(jsonunitlistlocal.Status.Split(new char[] { ' ' })[6]);
                    jsonunitlist.Status.def2 = int.Parse(jsonunitlistlocal.Status.Split(new char[] { ' ' })[7]);
                    jsonunitlist.Status.def3 = int.Parse(jsonunitlistlocal.Status.Split(new char[] { ' ' })[8]);
                    jsonunitlist.Status.spd = int.Parse(jsonunitlistlocal.Status.Split(new char[] { ' ' })[9]);
                    jsonunitlist.Status.defX = int.Parse(jsonunitlistlocal.Status.Split(new char[] { ' ' })[10]);

                    foreach (string item_id in jsonunitlistlocal.Item.Split(new char[] { ' ' }))
                    {
                        if(int.TryParse(item_id, out var id)) { 
                            if(id > 0)
                                jsonunitlist.Item.Add(id);
                        }
                    }

                    foreach (string eqp_id in jsonunitlistlocal.Eqp.Split(new char[] { ' ' }))
                    {
                        if (int.TryParse(eqp_id, out var id))
                        {
                            if (id > 0)
                                jsonunitlist.Eqp.Add(id);
                        }
                    }

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

    public class jsonUnitLocal
    {
        public string Name;
        public float X;
        public float Y;
        public string code;
        public string act_brain;
        public int player_owner;
        public string Info;
        public string Status;
        public string Item;
        public string Eqp;
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
    public int FindUnit(float x, float y)
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

    public jsonUnit getUnit(int no)
    {
        jsonUnit _unit = null;

        // ユニットを一つずつ見ていく。
        for (int i = 1; i <= sphere.unitNum; i++)
        {

            if (sphere.unit.ContainsKey(i))
            {
                jsonUnit unitinfo = sphere.unit[i];

                // 指定された座標に存在するユニットが見つかったらその番号を戻り値にセット。X=-1は削除されたユニット
                if (unitinfo.no == no)
                {
                    _unit = unitinfo;
                    break;
                }
            }
        }

        return _unit;
    }

    public jsonUnit getUnitByCode(string code)
    {
        jsonUnit _unit = null;

        // ユニットを一つずつ見ていく。
        for (int i = 1; i <= sphere.unitNum; i++)
        {

            if (sphere.unit.ContainsKey(i))
            {
                jsonUnit unitinfo = sphere.unit[i];

                // 指定された座標に存在するユニットが見つかったらその番号を戻り値にセット。X=-1は削除されたユニット
                if (unitinfo.code == code)
                {
                    _unit = unitinfo;
                    break;
                }
            }
        }

        return _unit;
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

    //=====================================================================================================
    // ギミック初期化関連メソッド（サーバ側のinitGimmicksと関連関数を移植）
    //=====================================================================================================

    // ギミックデータを格納するDictionary
    protected Dictionary<string, JObject> gimmicks = new Dictionary<string, JObject>();

    /// <summary>
    /// ギミックを初期化する（サーバ側のinitGimmicksに相当）
    /// </summary>
    /// <param name="roomName">ルーム名</param>
    /// <param name="roomInfo">ルーム定義（JSONオブジェクト）</param>
    /// <param name="reason">理由を表す文字列。通常、クエスト開始なら "start"</param>
    protected void InitGimmicks(string roomName, JObject roomInfo, string reason)
    {
        // 初期化
        gimmicks.Clear();

        // 定義されているギミックを一つずつ処理していく
        var gimmicksNode = roomInfo["gimmicks"];
        if (gimmicksNode != null && gimmicksNode.Type == JTokenType.Object)
        {
            foreach (var gimmickPair in (JObject)gimmicksNode)
            {
                string name = gimmickPair.Key;
                JObject gimmick = gimmickPair.Value as JObject;
                if (gimmick != null)
                {
                    AddGimmick(name, gimmick, roomName, reason);
                }
            }
        }
    }

    /// <summary>
    /// 引数に指定されたギミックをこのスフィアに追加する（サーバ側のaddGimmickに相当）
    /// </summary>
    /// <param name="name">ギミック名</param>
    /// <param name="gimmick">ギミックの内容（JSONオブジェクト）</param>
    /// <param name="roomName">ルーム名</param>
    /// <param name="reason">理由を表す文字列</param>
    /// <returns>追加したのなら true、しなかったのなら false</returns>
    protected bool AddGimmick(string name, JObject gimmick, string roomName = "", string reason = "")
    {
        // "switch" キーが付いているものは最初に処理する
        if (gimmick["switch"] != null)
        {
            var switchNode = gimmick["switch"];
            var gimmickCopy = gimmick.DeepClone() as JObject;
            gimmickCopy.Remove("switch");

            // "switch" キーを一つずつ見ていく
            if (switchNode.Type == JTokenType.Array)
            {
                foreach (var caseNode in (JArray)switchNode)
                {
                    if (caseNode.Type == JTokenType.Object)
                    {
                        // "switch" キーの要素でオリジナルギミックの内容をマージしてギミックを作成
                        var merge = new JObject(gimmickCopy);
                        foreach (var prop in (JObject)caseNode)
                        {
                            merge[prop.Key] = prop.Value;
                        }

                        // それを追加してみる。追加できたのなら終了
                        if (AddGimmick(name, merge, roomName, reason))
                            return true;
                    }
                }
            }

            // ここまで来るのはいずれの "switch" キーでも追加できなかったから。このギミックは追加できない
            return false;
        }

        // "one_shot"フラグが定義されている場合は、"yet_flag" と "flag_on" に展開する
        if (gimmick["one_shot"] != null)
        {
            var oneShot = gimmick["one_shot"];
            gimmick.Remove("one_shot");
            
            if (gimmick["condition"] == null)
                gimmick["condition"] = new JObject();
            
            ((JObject)gimmick["condition"])["yet_flag"] = oneShot;
            gimmick["flag_on"] = oneShot;
        }

        // "memory_shot"フラグが定義されている場合は、"yet_memory" と "memory_on" に展開する
        if (gimmick["memory_shot"] != null)
        {
            var memoryShot = gimmick["memory_shot"];
            gimmick.Remove("memory_shot");
            
            if (gimmick["condition"] == null)
                gimmick["condition"] = new JObject();
            
            ((JObject)gimmick["condition"])["yet_memory"] = memoryShot;
            gimmick["memory_on"] = memoryShot;
        }

        // 条件に該当しない場合は追加しない
        gimmick["name"] = name;
        var condition = gimmick["condition"];
        if (!TestCondition(condition, gimmick, reason))
            return false;
        
        gimmick.Remove("name");
        gimmick.Remove("condition");

        // 置物を一緒に置くように指定されている場合は...
        if (gimmick["ornament"] != null)
        {
            // ornament キーを削除する代わりに ornNo で紐づいている置物を指すようにする
            var ornamentType = gimmick["ornament"].ToString();
            gimmick.Remove("ornament");
            
            var ornNoArray = new JArray();
            gimmick["ornNo"] = ornNoArray;

            // ギミックの設置範囲の右下座標を取得
            var pos = gimmick["pos"] as JArray;
            var rb = gimmick["rb"] as JArray ?? pos;

            if (pos != null && rb != null)
            {
                int posX = pos[0].Value<int>();
                int posY = pos[1].Value<int>();
                int rbX = rb[0].Value<int>();
                int rbY = rb[1].Value<int>();

                // 設置範囲のすべての座標に置物を置く
                // 注意: Unity側では実際の置物配置処理が必要な場合は、ここでマップシステムに通知する
                for (int y = posY; y <= rbY; y++)
                {
                    for (int x = posX; x <= rbX; x++)
                    {
                        // 置物を追加する処理（Unity側の実装に応じて変更が必要）
                        // int ornNo = map.AddOrnament(x, y, ornamentType);
                        // ornNoArray.Add(ornNo);
                    }
                }
            }
        }

        // コメントは削除
        gimmick.Remove("rem");

        // スフィアに追加
        gimmicks[name] = gimmick;

        return true;
    }

    /// <summary>
    /// 引数で指定された条件に該当しているかどうかを返す（サーバ側のtestConditionに相当）
    /// </summary>
    /// <param name="condition">条件（JSONオブジェクト）</param>
    /// <param name="owner">ギミックまたはユニット定義（JSONオブジェクト）</param>
    /// <param name="reason">理由を表す文字列</param>
    /// <returns>条件に該当している、あるいは条件がない場合は true、該当しない場合は false</returns>
    protected bool TestCondition(JToken condition, JObject owner, string reason)
    {
        // 特に条件がないなら常にtrue
        if (condition == null || condition.Type == JTokenType.Null)
            return true;

        if (condition.Type != JTokenType.Object)
            return true;

        string method = "and";

        // 指定されている条件をすべてチェック
        foreach (var condPair in (JObject)condition)
        {
            string condName = condPair.Key;
            var value = condPair.Value;

            // "yet_flag" は "!has_flag" に、"yet_memory" は "!has_memory" に変換する
            if (condName == "yet_flag")
                condName = "!has_flag";
            if (condName == "yet_memory")
                condName = "!has_memory";

            // 条件名が "!" で始まっているものは逆評価する
            bool positive = !condName.StartsWith("!");
            if (!positive)
                condName = condName.Substring(1);

            // 条件名が "|" で始まっているものはOR条件
            if (condName.StartsWith("|"))
            {
                method = "or";
                condName = condName.Substring(1);
            }

            // 判定
            bool result = JudgeCondition(condName, value, owner, reason);

            if (method == "and")
            {
                // 一つでも偽なら、条件には該当しない
                if (positive != result)
                    return false;
            }
            else
            {
                // 一つでも真なら、条件に該当
                if (positive == result)
                    return true;
            }
        }

        if (method == "and")
            // ここまで来たらすべて該当している
            return true;
        else
            // ここまで来たらすべてfalse
            return false;
    }

    /// <summary>
    /// testCondition()のヘルパ。一つ一つの条件判定を行う（サーバ側のjudgeConditionに相当）
    /// </summary>
    protected bool JudgeCondition(string name, JToken value, JObject owner, string reason)
    {
        switch (name)
        {
            // クリアしている／いない
            case "cleared":
                // Unity側ではstateから取得する必要がある
                // bool cleared = sphere.state.cleared;
                // return cleared == value.Value<bool>();
                Debug.LogWarning($"cleared condition not fully implemented: {value}");
                return false;

            // 指定の理由
            case "reason":
                if (value.Type == JTokenType.Array)
                {
                    foreach (var v in (JArray)value)
                    {
                        if (v.ToString() == reason)
                            return true;
                    }
                    return false;
                }
                return value.ToString() == reason;

            // チェックメモリがある
            case "has_memory":
                // Unity側ではstateから取得する必要がある
                // var memory = sphere.state.memory;
                // return memory != null && memory[value.ToString()] != null && memory[value.ToString()].Value<bool>();
                Debug.LogWarning($"has_memory condition not fully implemented: {value}");
                return false;

            // チェックフラグがある（PlayerPrefsで管理）
            case "has_flag":
                int flagId = value.Value<int>();
                string flagKey = $"SPHERE_FLAG_{flagId}";
                return PlayerPrefs.GetInt(flagKey, 0) == 1;

            // ミッションが存在する／しない
            case "mission":
                // Unity側ではstateから取得する必要がある
                // bool missionExists = sphere.state.mission_exists;
                // return missionExists == value.Value<bool>();
                Debug.LogWarning($"mission condition not fully implemented: {value}");
                return false;

            // 指定のユニットのどれかがいる
            case "unit":
            case "unit_exist":
                if (value.Type == JTokenType.Array)
                {
                    foreach (var codeToken in (JArray)value)
                    {
                        string code = codeToken.ToString();
                        if (getUnitByCode(code) != null)
                            return true;
                    }
                }
                else
                {
                    string code = value.ToString();
                    if (getUnitByCode(code) != null)
                        return true;
                }
                return false;

            // 指定のユニットのどれかがいない
            case "unit_nonexist":
                if (value.Type == JTokenType.Array)
                {
                    foreach (var codeToken in (JArray)value)
                    {
                        string code = codeToken.ToString();
                        if (getUnitByCode(code) == null)
                            return true;
                    }
                }
                else
                {
                    string code = value.ToString();
                    if (getUnitByCode(code) == null)
                        return true;
                }
                return false;

            // 指定のユニットのどれかがまだ生きている
            case "unit_alive":
                if (value.Type == JTokenType.Array)
                {
                    foreach (var codeToken in (JArray)value)
                    {
                        string code = codeToken.ToString();
                        var unit = getUnitByCode(code);
                        if (unit != null && unit.Status.hp > 0)
                            return true;
                    }
                }
                else
                {
                    string code = value.ToString();
                    var unit = getUnitByCode(code);
                    if (unit != null && unit.Status.hp > 0)
                        return true;
                }
                return false;

            // 指定のユニットが起動した
            case "igniter":
                if (value.Type == JTokenType.Array)
                {
                    foreach (var v in (JArray)value)
                    {
                        if (v.ToString() == reason)
                            return true;
                    }
                    return false;
                }
                return value.ToString() == reason;

            // カスタムコール（Unity側では実装が異なる可能性がある）
            case "call":
                // Unity側でのカスタムメソッド呼び出し処理が必要
                Debug.LogWarning($"call condition not implemented: {value}");
                return false;

            // その他の条件は無視
            default:
                return true;
        }
    }

    /// <summary>
    /// 通常のギミック終了処理を行わずに、指定されたギミックを削除する
    /// </summary>
    public void RemoveGimmick(string name)
    {
        gimmicks.Remove(name);
    }

    public JObject CheckTriggerGimmick(string name, jsonUnit unit = null)
    {
        // 指定のギミックがもう死んでいるなら何もしない
        if (!gimmicks.ContainsKey(name))
            return null;

        // 指定されたギミックを取得して、ギミック名を "name" キーに格納する
        JObject gimmick = gimmicks[name] as JObject;
        gimmick["name"] = name;

        // "touch" を処理（連鎖発動）
        if (gimmick["touch"] != null)
        {
            var touchArray = gimmick["touch"] as JArray;
            if (touchArray != null)
            {
                foreach (var touch in touchArray)
                {
                    TriggerGimmick(touch.ToString(), unit);
                }
            }
        }

        // "ignition" の条件を満たしていない場合は無視する
        var ignition = gimmick["ignition"];
        string unitCode = unit != null ? unit.code : "";
        if (!TestCondition(ignition, gimmick, unitCode))
            return null;

        return gimmick;
    }

    /// <summary>
    /// 引数で指定されたギミックをただちに起動する（サーバ側のtriggerGimmickに相当）
    /// </summary>
    /// <param name="name">ギミック名</param>
    /// <param name="unit">ギミックを発動させたユニットのインスタンス。ユニットによらないならnull</param>
    /// <param name="isChained">chainで呼ばれた場合はtrue（mitter.leadをクリアしない）</param>
    /// <returns>SWFへのリターンが発生したかどうか</returns>
    public bool TriggerGimmick(string name, jsonUnit unit = null, bool isChained = false)
    {
        var gimmick = CheckTriggerGimmick(name, unit);
        if (gimmick != null)
        {
            // chainで呼ばれていない場合（最初のギミック発動時）のみmitter.leadをクリア
            if (!isChained)
            {
                mitter.lead.Clear();
            }

            // 発動⇒終了
            bool swfReturn1 = FireGimmick(gimmick, unit, isChained);
            bool swfReturn2 = CloseGimmick(gimmick, unit);

            // リターン
            return swfReturn1 || swfReturn2;
        }

        return false;
    }

    /// <summary>
    /// 引数で指定されたギミックの発動を処理する（サーバ側のfireGimmickに相当）
    /// </summary>
    /// <param name="gimmick">ギミック。"name" キーにギミック名が格納されている</param>
    /// <param name="unit">ギミックを発動させたユニットのインスタンス。ユニットによらないならnull</param>
    /// <param name="isChained">chainで呼ばれた場合はtrue（mitter.leadをクリアしない）</param>
    /// <returns>SWFへのリターンが発生したかどうか</returns>
    protected bool FireGimmick(JObject gimmick, jsonUnit unit, bool isChained = false)
    {
        var typeToken = gimmick["type"];
        if (typeToken == null)
            return false;

        string type = typeToken.ToString();
        // leadIndexは既存のmitter.leadの数+1から開始（サーバ側のarray_mergeと同じ）
        int leadIndex = mitter.lead.Count + 1;

        // 基底で処理できるなら処理する
        switch (type)
        {
            // 指揮
            case "lead":
                // 埋め込みコードを置き換えて、指揮に追加
                var leadsToken = gimmick["leads"];
                if (leadsToken != null && leadsToken.Type == JTokenType.Array)
                {
                    var leadsArray = leadsToken as JArray;

                    // chainで呼ばれた場合は既存のmitter.leadに追加（サーバ側のarray_mergeと同じ）
                    foreach (var lead in leadsArray)
                    {
                        string leadStr = ReplaceEmbedCode(lead.ToString(), unit);
                        mitter.lead["lead" + leadIndex] = leadStr;
                        leadIndex++;
                    }

                    // chainで呼ばれていない場合のみLead()を呼び出す
                    // chainで呼ばれた場合は、最後のギミック発動後にまとめてLead()が呼ばれる
                    if (!isChained)
                    {
                        gimmick_call_lead(unit, leadIndex);
                    }

                }

                // SWFへ返すかどうかは設定次第
                return gimmick["swf_return"] != null && gimmick["swf_return"].Value<bool>();

            // 寸劇
            case "drama":
                // シーンを寸劇に変更（Unity側では実装が異なる可能性がある）
                // sphere.state.scene = "drama";
                // sphere.state.scene_id = gimmick["drama_id"].Value<int>();
                // sphere.state.scene_trigger = gimmick["name"].ToString();
                Debug.LogWarning($"drama type gimmick not fully implemented: {gimmick["name"]}");
                return true;

            // アイテムゲット
            case "treasure":
                // アイテム取得処理（Unity側では実装が異なる可能性がある）
                if (gimmick["item_id"] != null)
                {
                    int itemId = gimmick["item_id"].Value<int>();
                    // アイテムを取得するユニットを取得
                    // "treasure_catcher" が指定されているならそのユニット、されてないなら起動したユニット
                    jsonUnit catcher = unit;
                    if (gimmick["treasure_catcher"] != null)
                    {
                        string catcherCode = gimmick["treasure_catcher"].ToString();
                        catcher = getUnitByCode(catcherCode);
                    }
                    // アイテム取得処理（Unity側の実装に応じて変更が必要）
                    Debug.LogWarning($"treasure item_id={itemId} not fully implemented");
                }

                // ゴールド取得
                if (gimmick["gold"] != null)
                {
                    int gold = gimmick["gold"].Value<int>();
                    // ゴールド取得処理（Unity側の実装に応じて変更が必要）
                    Debug.LogWarning($"treasure gold={gold} not fully implemented");
                }

                return false;

            // マップチップの変更
            case "square_change":
                // change_posとchange_tipを取得
                var changePosToken = gimmick["change_pos"];
                var changeTipToken = gimmick["change_tip"];
                if (changePosToken == null || changeTipToken == null)
                {
                    Debug.LogError($"Gimmick type 'square_change' requires 'change_pos' and 'change_tip': {gimmick["name"]}");
                    return false;
                }

                var changePosArray = changePosToken as JArray;
                if (changePosArray == null || changePosArray.Count < 2)
                {
                    Debug.LogError($"Gimmick type 'square_change' requires 'change_pos' as array[2]: {gimmick["name"]}");
                    return false;
                }

                int changeX = changePosArray[0].Value<int>();
                int changeY = changePosArray[1].Value<int>();
                int changeTipId = changeTipToken.Value<int>();

                // change_tip（グラフィック番号）から内部チップ番号を取得
                // tipIdは Dictionary<int, string> で、キーが内部チップ番号、値がグラフィック番号
                int internalTipNo = 0;
                if (sphere.tipId != null)
                {
                    foreach (var kvp in sphere.tipId)
                    {
                        if (kvp.Value == changeTipId.ToString())
                        {
                            internalTipNo = kvp.Key;
                            break;
                        }
                    }
                }

                if (internalTipNo == 0)
                {
                    Debug.LogError($"Gimmick type 'square_change': tipId {changeTipId} not found in tipId table: {gimmick["name"]}");
                    return false;
                }

                // RPBG1コマンドを生成（サーバ側のchangeSquare()と同じ）
                mitter.lead["lead" + leadIndex] = string.Format("RPBG1 {0:D2} {1:D2} {2:D4}", changeX, changeY, internalTipNo);
                leadIndex++;

                // chainで呼ばれていない場合のみLead()を呼び出す
                if (!isChained)
                {
                    gimmick_call_lead(unit, leadIndex);
                }

                return false;

            // ユニットの登場
            case "unit":
                // ユニット定義を取得
                var unitDefine = gimmick["unit"] as JObject;
                if (unitDefine == null)
                {
                    Debug.LogError($"Gimmick type 'unit' requires 'unit' definition: {gimmick["name"]}");
                    return false;
                }

                // character_idを取得
                var characterIdToken = unitDefine["character_id"];
                if (characterIdToken == null)
                {
                    Debug.LogError($"Gimmick type 'unit' requires 'character_id': {gimmick["name"]}");
                    return false;
                }
                int characterId = characterIdToken.Value<int>();

                // 座標を取得（デフォルトは[0,0]）
                int posX = 0;
                int posY = 0;
                var posToken = unitDefine["pos"];
                if (posToken != null && posToken.Type == JTokenType.Array)
                {
                    var posArray = posToken as JArray;
                    if (posArray.Count >= 2)
                    {
                        posX = posArray[0].Value<int>();
                        posY = posArray[1].Value<int>();
                    }
                }

                // マスターデータからキャラクター情報を取得
                var charInfo = CharacterInfoModel.Rows.Find(c => c.character_id == characterId);
                if (charInfo == null)
                {
                    Debug.LogError($"Character info not found for character_id={characterId} in gimmick: {gimmick["name"]}");
                    return false;
                }

                // ユニット番号を決定（既存のユニット番号の最大値+1）
                int newUnitNo = 1;
                if (sphere.unit != null && sphere.unit.Count > 0)
                {
                    foreach (var key in sphere.unit.Keys)
                    {
                        if (key >= newUnitNo)
                            newUnitNo = key + 1;
                    }
                }

                // ユニット情報を作成
                jsonUnit newUnit = new jsonUnit();
                newUnit.no = newUnitNo;
                newUnit.X = posX;
                newUnit.Y = posY;
                // text_masterからキャラクター名を取得
                int lang = PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY, 0);
                newUnit.Name = TextMasterModel.GetCharacterName(charInfo.name_id, lang);
                if (string.IsNullOrEmpty(newUnit.Name))
                {
                    // text_masterにない場合はentryをフォールバックとして使用
                    newUnit.Name = charInfo.entry;
                }
                newUnit.code = unitDefine["code"] != null ? unitDefine["code"].ToString() : "";
                newUnit.act_brain = unitDefine["act_brain"] != null ? unitDefine["act_brain"].ToString() : "generic";
                newUnit.player_owner = 0;

                // UnitInfoを設定
                // iconプロパティからグラフィック番号を取得（サーバ側のgetUnitSpecs()と同じロジック）
                string iconName = unitDefine["icon"] != null ? unitDefine["icon"].ToString() : "shadow";
                
                // sphere.unitIconからicon名に対応するグラフィック番号を探す
                int graphNo = 0;
                if (sphere.unitIcon != null)
                {
                    foreach (var kvp in sphere.unitIcon)
                    {
                        if (kvp.Value == iconName)
                        {
                            graphNo = kvp.Key;
                            break;
                        }
                    }
                    
                    // 見つからない場合は、新しい番号を割り当て
                    if (graphNo == 0)
                    {
                        // 既存の最大値+1を取得
                        int maxGraphNo = 0;
                        foreach (var key in sphere.unitIcon.Keys)
                        {
                            if (key > maxGraphNo)
                                maxGraphNo = key;
                        }
                        graphNo = maxGraphNo + 1;
                        sphere.unitIcon[graphNo] = iconName;
                    }
                }
                else
                {
                    // unitIconが初期化されていない場合は1から開始
                    graphNo = 1;
                    if (sphere.unitIcon == null)
                    {
                        sphere.unitIcon = new Dictionary<int, string>();
                    }
                    sphere.unitIcon[graphNo] = iconName;
                }
                
                newUnit.Info.graphNo = graphNo;
                newUnit.Info.union = unitDefine["union"] != null ? unitDefine["union"].Value<int>() : 2;
                newUnit.Info.cost = unitDefine["move_pow"] != null ? unitDefine["move_pow"].Value<int>() : 4; // デフォルトは4
                newUnit.Info.align = 0;

                // UnitStatusを設定
                newUnit.Status.level = 1; // レベルは後で計算する必要があるが、とりあえず1
                newUnit.Status.hp = (int)charInfo.hp;
                newUnit.Status.maxhp = (int)charInfo.hp_max;
                newUnit.Status.att1 = charInfo.attack1;
                newUnit.Status.att2 = charInfo.attack2;
                newUnit.Status.att3 = charInfo.attack3;
                newUnit.Status.def1 = charInfo.defence1;
                newUnit.Status.def2 = charInfo.defence2;
                newUnit.Status.def3 = charInfo.defence3;
                newUnit.Status.defX = charInfo.defenceX;
                newUnit.Status.spd = charInfo.speed;

                // ユニットをsphereに追加
                sphere.unit[newUnitNo] = newUnit;

                // 指揮を生成（既存のmitter.leadに追加）
                // サーバ側の順番に合わせる: PFOCS → USTAT → UITEM → UEQIP → UADDI → IPRET → DELAY → IPRET

                // quietが指定されていない場合、フォーカスを先に追加
                bool quiet = gimmick["quiet"] != null && gimmick["quiet"].Value<bool>();
                if (!quiet)
                {
                    mitter.lead["lead" + leadIndex] = string.Format("PFOCS {0:D2} {1:D2}", posX, posY);
                    leadIndex++;
                }

                // USTAT指揮を生成
                mitter.lead["lead" + leadIndex] = string.Format("USTAT {0:D3} {1:D4} {2:D5} {3:D5} {4:D4} {5:D4} {6:D4} {7:D4} {8:D4} {9:D4} {10:D4} {11:D3}",
                    newUnitNo, newUnit.Status.level, newUnit.Status.hp, newUnit.Status.maxhp,
                    newUnit.Status.att1, newUnit.Status.att2, newUnit.Status.att3,
                    newUnit.Status.def1, newUnit.Status.def2, newUnit.Status.def3,
                    newUnit.Status.spd, newUnit.Status.defX);
                leadIndex++;

                // UITEM指揮を生成
                // unitDefineからitemsを取得（通常は空）
                string itemSpec = "";
                var itemsToken = unitDefine["items"];
                if (itemsToken != null && itemsToken.Type == JTokenType.Array)
                {
                    var itemsArray = itemsToken as JArray;
                    // 注意: Unity側ではuser_item_idからSWFアイテム番号への変換ができないため、
                    // itemsが指定されている場合は空として扱う（サーバ側で処理する必要がある）
                    // 通常、敵ユニットはitemsを持たないので、空のコマンドを送る
                }
                mitter.lead["lead" + leadIndex] = string.Format("UITEM {0:D3} {1}", newUnitNo, itemSpec).TrimEnd();
                leadIndex++;

                // UEQIP指揮を生成
                // unitDefineからsequipを取得（通常は空）
                string eqpSpec = "";
                var sequipToken = unitDefine["sequip"];
                if (sequipToken != null && sequipToken.Type == JTokenType.Array)
                {
                    var sequipArray = sequipToken as JArray;
                    // 注意: Unity側ではuser_item_idからSWFアイテム番号への変換ができないため、
                    // sequipが指定されている場合は空として扱う（サーバ側で処理する必要がある）
                    // 通常、敵ユニットはsequipを持たないので、空のコマンドを送る
                }
                mitter.lead["lead" + leadIndex] = string.Format("UEQIP {0:D3} {1}", newUnitNo, eqpSpec).TrimEnd();
                leadIndex++;

                // UADDI指揮を生成
                string infoStr = string.Format("{0:D2} {1:D2} {2:D3} {3:D2}",
                    newUnit.Info.graphNo, newUnit.Info.union, newUnit.Info.cost, newUnit.Info.align);
                mitter.lead["lead" + leadIndex] = string.Format("UADDI {0:D3} {1:D2} {2:D2} {3} {4}",
                    newUnitNo, posX, posY, infoStr, newUnit.Name);
                leadIndex++;

                // quietが指定されていない場合、解説を追加（UADDIの後）
                if (!quiet)
                {
                    mitter.lead["lead" + leadIndex] = string.Format("IPRET {0} Lv{1}が現れました", newUnit.Name, newUnit.Status.level);
                    leadIndex++;
                    mitter.lead["lead" + leadIndex] = "DELAY 800";
                    leadIndex++;
                    mitter.lead["lead" + leadIndex] = "IPRET";
                    leadIndex++;
                }

                // chainで呼ばれていない場合のみLead()を呼び出す
                if (!isChained)
                {
                    gimmick_call_lead(unit, leadIndex);
                }

                return false;

            // その他のタイプは未実装
            default:
                Debug.LogWarning($"Gimmick type '{type}' not implemented: {gimmick["name"]}");
                return false;
        }
    }

    public void gimmick_call_lead(jsonUnit unit, int leadIndex)
    {
        if (unit.code == "avatar")
            mitter.lead["lead" + leadIndex] = "COMND " + unit.no;

        // 既存の tween があれば移動を最後までやりきってから発動
        var objunit = Stage.objUnits.units["unit_" + unit.no];
        if (objunit != null && objunit.currentMoveTween != null && objunit.currentMoveTween.IsActive())
        {
            objunit.currentMoveTween.OnComplete(() =>
            {
                objunit.commandkeyrecv = true;

                firegimmickflg = true;
                leader.flow.Clear();
                Lead();

            });
        }
        else
        {
            // Lead()を呼び出して処理
            firegimmickflg = true;

            leader.flow.Clear();
            Lead();
        }
    }

    /// <summary>
    /// 埋め込みコードを置き換える（サーバ側のreplaceEmbedCodeに相当）
    /// </summary>
    /// <param name="lead">指揮文字列</param>
    /// <param name="unit">発動ユニット（null可）</param>
    /// <returns>置き換え後の指揮文字列</returns>
    protected string ReplaceEmbedCode(string lead, jsonUnit unit)
    {
        string result = lead;

        // ユニットのコードと番号の対応表を取得
        Dictionary<string, string> map = new Dictionary<string, string>();
        foreach (var unitPair in sphere.unit)
        {
            if (unitPair.Value != null && !string.IsNullOrEmpty(unitPair.Value.code))
            {
                string code = unitPair.Value.code;
                int no = unitPair.Value.no;
                map[$"%{code}%"] = no.ToString("D3");
            }
        }

        // "[NAME]" が含まれている場合、プレイヤーアバタの名前を取得
        if (result.Contains("[NAME]") && !map.ContainsKey("[NAME]"))
        {
            // Unity側ではプレイヤー名の取得方法が異なる可能性がある
            // とりあえず空文字列にしておく（必要に応じて実装）
            map["[NAME]"] = "";
        }

        // "%xxx%" と "[NAME]" の置き換え
        foreach (var kvp in map)
        {
            result = result.Replace(kvp.Key, kvp.Value);
        }

        // コマンドごとの特殊処理（RPBG1, AALGN など）は必要に応じて実装
        // 現時点では基本的な置き換えのみ

        return result;
    }

    /// <summary>
    /// 引数で指定されたギミックの終了処理を行う（サーバ側のcloseGimmickに相当）
    /// </summary>
    /// <param name="gimmick">ギミック</param>
    /// <param name="unit">ギミックを発動させたユニットのインスタンス。ユニットによらないならnull</param>
    /// <returns>SWFへのリターンが発生したかどうか</returns>
    protected bool CloseGimmick(JObject gimmick, jsonUnit unit)
    {
        bool swfReturn = false;

        // "lasting" が設定されてないならギミックを削除
        var lastingToken = gimmick["lasting"];
        int lasting = lastingToken != null ? lastingToken.Value<int>() : 1;
        if (lasting <= 1)
        {
            string gimmickName = gimmick["name"].ToString();
            RemoveGimmick(gimmickName);

            // 置物が関連付けられている場合は合わせて削除
            if (gimmick["ornNo"] != null)
            {
                var ornNoArray = gimmick["ornNo"] as JArray;
                if (ornNoArray != null)
                {
                    foreach (var ornNo in ornNoArray)
                    {
                        int ornNoInt = ornNo.Value<int>();
                        // 置物削除処理（Unity側の実装に応じて変更が必要）
                        Debug.LogWarning($"Ornament removal not fully implemented: ornNo={ornNoInt}");
                    }
                }
            }
        }
        else
        {
            // "lasting" が設定されているならカウントダウンする
            gimmick["lasting"] = lasting - 1;
        }

        // 永続フラグをONにするよう指示されている場合はONにする
        if (gimmick["flag_on"] != null)
        {
            int flagId = gimmick["flag_on"].Value<int>();
            string flagKey = $"SPHERE_FLAG_{flagId}";
            PlayerPrefs.SetInt(flagKey, 1);
            PlayerPrefs.Save();
        }

        // メモリフラグをONにするよう指示されている場合はONにする
        if (gimmick["memory_on"] != null)
        {
            int memoryId = gimmick["memory_on"].Value<int>();
            // Unity側ではstate.memoryに保存する必要がある
            // sphere.state.memory[memoryId] = true;
            Debug.LogWarning($"memory_on not fully implemented: memoryId={memoryId}");
        }

        // one_shot が設定されている場合はフラグを立てる
        if (gimmick["one_shot"] != null)
        {
            int flagId = gimmick["one_shot"].Value<int>();
            string flagKey = $"SPHERE_FLAG_{flagId}";
            PlayerPrefs.SetInt(flagKey, 1);
            PlayerPrefs.Save();
        }

        // ギミックにchainが設定されている場合は起動する
        if (gimmick["chain"] != null)
        {
            var chainToken = gimmick["chain"];
            // サーバ側では (array)$gimmick['chain'] でキャストしているので、文字列でも配列でも対応
            if (chainToken.Type == JTokenType.String)
            {
                // 文字列の場合はそのまま発動（chainで呼ばれているのでisChained=true）
                swfReturn = swfReturn || TriggerGimmick(chainToken.ToString(), unit, true);
            }
            else if (chainToken.Type == JTokenType.Array)
            {
                // 配列の場合は各要素を発動（chainで呼ばれているのでisChained=true）
                var chainArray = chainToken as JArray;
                foreach (var chain in chainArray)
                {
                    swfReturn = swfReturn || TriggerGimmick(chain.ToString(), unit, true);
                }
            }
        }

        // ギミックにchain_delayedが設定されている場合はイベントをキューに
        if (gimmick["chain_delayed"] != null)
        {
            var chainDelayedArray = gimmick["chain_delayed"] as JArray;
            if (chainDelayedArray != null)
            {
                foreach (var chain in chainDelayedArray)
                {
                    // イベントキューにpush（Unity側の実装に応じて変更が必要）
                    Debug.LogWarning($"chain_delayed not fully implemented: {chain}");
                }
            }
        }

        // リターン
        return swfReturn;
    }

    /// <summary>
    /// 指定のギミックの指定のプロパティの値を変更する
    /// </summary>
    public void ModifyGimmick(string gimName, string propName, JToken value)
    {
        if (gimmicks.ContainsKey(gimName))
        {
            gimmicks[gimName][propName] = value;
        }
    }

    /// <summary>
    /// JSONファイルからフィールド定義を取得し、ルーム情報を取得する
    /// </summary>
    protected JObject GetRoomDefinition(JObject fieldData, string roomName)
    {
        var rooms = fieldData["rooms"] as JObject;
        if (rooms == null)
            return null;

        var room = rooms[roomName];
        if (room == null)
            return null;

        // キーにルーム情報が入っているならそれを返す
        if (room.Type == JTokenType.Object)
            return room as JObject;

        // それ以外なら、それは別定義ファイルのIDと解釈して読み込む
        // Unity側では別ファイルの読み込み処理が必要
        Debug.LogWarning($"Room definition file reference not implemented: {room}");
        return null;
    }

    /// <summary>
    /// JSONファイルを初期化する（onStartで呼び出す）
    /// </summary>
    /// <param name="jsonFileContent">JSONファイルの内容（文字列）</param>
    /// <param name="roomName">初期ルーム名（通常は "start"）</param>
    /// <param name="reason">理由（通常は "start"）</param>
    public void InitializeGimmicksFromJson(string jsonFileContent, string roomName = "start", string reason = "start")
    {
        try
        {
            // JSONをパース
            JObject fieldData = JObject.Parse(jsonFileContent);
            
            // ルーム定義を取得
            JObject roomInfo = GetRoomDefinition(fieldData, roomName);
            if (roomInfo == null)
            {
                Debug.LogError($"Room '{roomName}' not found in field data");
                return;
            }

            // グローバル値をマージ
            if (fieldData["global_gimmicks"] != null)
            {
                var globalGimmicks = fieldData["global_gimmicks"] as JObject;
                if (roomInfo["gimmicks"] == null)
                    roomInfo["gimmicks"] = new JObject();
                
                var roomGimmicks = roomInfo["gimmicks"] as JObject;
                foreach (var gimmickPair in globalGimmicks)
                {
                    roomGimmicks[gimmickPair.Key] = gimmickPair.Value;
                }
            }

            // ギミックを初期化
            InitGimmicks(roomName, roomInfo, reason);
            
            Debug.Log($"Gimmicks initialized for room '{roomName}'");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize gimmicks from JSON: {e.Message}");
        }
    }

    /// <summary>
    /// ユニットの進入によるギミック発動がないかチェックする（サーバ側のcheckGimmickByUnitに相当）
    /// </summary>
    /// <param name="unit">進入しているかもしれないユニットのインスタンス</param>
    /// <param name="stayPos">ユニットが移動を行わず留まっているならtrue</param>
    protected bool CheckGimmickByUnit(jsonUnit unit, bool stayPos = false, bool check = false)
    {

        bool is_fire = false;

        if (unit == null)
            return false;

        // 対象ユニットがいる座標にあるギミックのインデックスをすべて取得
        Vector2Int unitPos = new Vector2Int((int)unit.X, (int)unit.Y);
        List<string> gimmickNames = FindGimmicksOnPosition(unitPos);

        // 対象ユニットかどうか確認していく
        foreach (string name in gimmickNames)
        {
            if (!gimmicks.ContainsKey(name))
                continue;

            JObject gimmick = gimmicks[name] as JObject;
            if (gimmick == null)
                continue;

            // ユニットが留まっている場合は、"always" フラグがないギミックは無視する
            if (stayPos && (gimmick["always"] == null || !gimmick["always"].Value<bool>()))
                continue;

            // 対象ユニットでないなら次へ
            var triggerToken = gimmick["trigger"];
            string trigger = triggerToken != null ? triggerToken.ToString() : "";

            bool isTarget = false;
            switch (trigger)
            {
                case "hero":
                    if (unit.code == "avatar")
                        isTarget = true;
                    break;
                case "player":
                    if (unit.player_owner == 1)
                        isTarget = true;
                    break;
                case "all":
                    isTarget = true;
                    break;
                case "unit_into":
                    if (gimmick["unit_into"] != null)
                    {
                        var unitIntoArray = gimmick["unit_into"] as JArray;
                        if (unitIntoArray != null)
                        {
                            foreach (var codeToken in unitIntoArray)
                            {
                                if (codeToken.ToString() == unit.code)
                                {
                                    isTarget = true;
                                    break;
                                }
                            }
                        }
                    }
                    break;
                default:
                    // triggerが指定されていない場合はスキップ
                    break;
            }

            if (!isTarget)
                continue;

            if (check)
            {
                if (CheckTriggerGimmick(name, unit) != null)
                    is_fire = true;
            }
            else
            {
                // ギミックを発動
                TriggerGimmick(name, unit);
                is_fire = true;
            }
        }

        return is_fire;
    }

    /// <summary>
    /// 指定された座標に位置しているギミックの名前を返す（サーバ側のmap->findOnに相当）
    /// </summary>
    /// <param name="point">座標</param>
    /// <returns>ギミック名のリスト</returns>
    protected List<string> FindGimmicksOnPosition(Vector2Int point)
    {
        List<string> result = new List<string>();

        // 検査対象を一つずつ見ていく
        foreach (var gimmickPair in gimmicks)
        {
            string name = gimmickPair.Key;
            JObject gimmick = gimmickPair.Value as JObject;
            if (gimmick == null)
                continue;

            // キー"pos" を持っていないものは無視
            var posToken = gimmick["pos"];
            if (posToken == null)
                continue;

            // 対象に当たっているならヒット
            if (IsHit(point, gimmick))
                result.Add(name);
        }

        return result;
    }

    /// <summary>
    /// 指定された座標が、指定された対象物に当たっているかどうかを返す（サーバ側のSphereMap::isHitに相当）
    /// </summary>
    /// <param name="point">調べたい座標</param>
    /// <param name="aim">対象物（ギミック）。少なくともキー "pos" を持っていること。"rb", "mask" を持っている場合はそれも加味される</param>
    /// <returns>当たっているなら true、当たっていないなら false</returns>
    protected bool IsHit(Vector2Int point, JObject aim)
    {
        var posToken = aim["pos"];
        if (posToken == null)
            return false;

        var posArray = posToken as JArray;
        if (posArray == null || posArray.Count < 2)
            return false;

        int posX = posArray[0].Value<int>();
        int posY = posArray[1].Value<int>();

        // キー "rb" がないなら1点のみで当たり判定する
        var rbToken = aim["rb"];
        if (rbToken == null)
        {
            return (point.x == posX && point.y == posY);
        }

        var rbArray = rbToken as JArray;
        if (rbArray == null || rbArray.Count < 2)
            return (point.x == posX && point.y == posY);

        int rbX = rbArray[0].Value<int>();
        int rbY = rbArray[1].Value<int>();

        // 対象よりも左・上に位置しているなら当たってない
        if (point.x < posX || point.y < posY)
            return false;

        // 対象よりも右・下に位置しているなら当たってない
        if (rbX < point.x || rbY < point.y)
            return false;

        // キー "mask" を持っている場合の処理（現時点では未実装）
        // Unity側では必要に応じて実装

        return true;
    }

}
