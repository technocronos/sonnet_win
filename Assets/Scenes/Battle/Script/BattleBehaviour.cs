using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Playables;
using Newtonsoft.Json;

public class BattleBehaviour : BaseBehaviour
{
    public PlayableDirector open_director;

    public GameObject TouchPanel;

    public GameObject ConfirmPhaseObj;
    public GameObject OpenPhaseObj;
    public GameObject MainPhaseObj;
    public GameObject ResultPhaseObj;

    public DamShowBehaviour DamShow0;
    public DamShowBehaviour DamShow1;
    public DamShowBehaviour DamShow2;
    public DamShowBehaviour DamShow3;
    public DamShowBehaviour DamShow4;
    public DamShowBehaviour DamShow5;

    Dictionary<int, DamShowBehaviour> DamShow { get; set; } = new Dictionary<int, DamShowBehaviour>();

    public GameObject PreterObj;
    public Animator PreterAnim;
    public TextMeshProUGUI PreterText;

    public GameObject OPCharaP;
    public GameObject CharaP;
    public GameObject OPCharaE;
    public GameObject CharaE;
    public GameObject EDCharaP;
    public Image GradeCharaP;

    public GameObject ButtonAuto;
    public GameObject TopAuto;

    public GameObject ContinueConfirmObj;
    public GameObject NaviObj;
    public TextMeshProUGUI NaviText;

    public GameObject BuyItemPhaseObj;

    public GameObject BG;
    public GameObject Explain;

    public class Parameter
    {
        public int battleId;
        public string firstscene;
        public int repaireId;
        public bool tutorial;
        public string from;
    }

    public Parameter Param;

    public static BattleBehaviour Instance
    {
        get
        {
            return instance;
        }
    }

    private static BattleBehaviour instance;

    /// <summary>
    // フレームレート
    /// </summary>
    public static int FRAME_RATE { get; set; } = 60;

    // ステージの幅・高さ
    public static float STAGE_WIDTH { get; set; } = 750;
    public static float STAGE_HEIGHT { get; set; } = 2000;

    public static float BOTTOM_POS { get; set; } = -947.8125f;

    // プレイヤーキャラ、相手キャラの攻撃点の座標
    public static float HIT_XP { get; set; } = -246f;
    public static float HIT_YP { get; set; } = -575f;
    public static float HIT_XE { get; set; } = HIT_XP * -1;
    public static float HIT_YE { get; set; } = HIT_YP;

    // リベンジに必要なスターの数
    public static int REVENGE_REQUIRED_NUM = 10;

    public Dictionary<string, string> NAV_REV = new Dictionary<string, string>();

    //復元用キー
    public static string TURN = "BATTLE_KEY_TURN";
    public static string HP_P = "BATTLE_KEY_HP_P";
    public static string HP_E = "BATTLE_KEY_HP_E";
    public static string STAR_TYPES_P = "BATTLE_KEY_STAR_TYPES_P";
    public static string STAR_TYPES_E = "BATTLE_KEY_STAR_TYPES_E";

    public static string STAT_REV_CNT_P = "BATTLE_KEY_STAT_REV_CNT_P";
    public static string STAT_REV_CNT_E = "BATTLE_KEY_STAT_REV_CNT_E";

    public static string STAT = "BATTLE_KEY_STAT";
    public static string STAT_TACT = "BATTLE_KEY_STAT_TACT";

    public static string STAT_ATT_N = "BATTLE_KEY_STAT_ATT_N";
    public static string STAT_ATT_R = "BATTLE_KEY_STAT_ATT_R";
    public static string STAT_DAM_N = "BATTLE_KEY_STAT_DAM_N";
    public static string STAT_DAM_R = "BATTLE_KEY_STAT_DAM_R";
    public static string STAT_DAM_O = "BATTLE_KEY_STAT_DAM_O";
    public static string STAT_HIT_N = "BATTLE_KEY_STAT_HIT_N";
    public static string STAT_HIT_R = "BATTLE_KEY_STAT_HIT_R";
    public static string STAT_HIT_O = "BATTLE_KEY_STAT_HIT_O";

    public int nextDamNo { set; get; }
    public bool auto_flg { set; get; }

    public jsonBattle battle { get; set; } = null;

    public NaviBehaviour Navi { get; set; }
    public PreterBehaviour Preter { get; set; }

    public ConfirmPhaseBehaviour ConfirmPhase { get; set; }
    public OpenPhaseBehaviour OpenPhase { get; set; }
    public MainPhaseBehaviour MainPhase { get; set; }
    public ContinueConfirmBehaviour ContinueConfirm { get; set; }
    public ClosePhaseBehaviour ClosePhase { get; set; }
    public ResultPhaseBehaviour ResultPhase { get; set; }

    public ContinuePhaseBehaviour ContinuePhase { get; set; }
    public BuyItemPhaseBehaviour BuyItemPhase { get; set; }


    public string nextSeq { get; set; } = null;

    public Dictionary<string, int> att { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, int> def { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, int> lv { get; set; } = new Dictionary<string, int>();

    public Dictionary<string, int> statTact { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, int> statRevCnt { get; set; } = new Dictionary<string, int>();

    public Dictionary<string, StatInfo> stat { get; set; } = new Dictionary<string, StatInfo>();

    public class StatInfo
    {
        public Dictionary<string, int> attCnt;
        public Dictionary<string, int> hitCnt;
        public Dictionary<string, int> dam;
    }

    public RandomEx randomEx { get; set; }

    public jsonConstants constants { get; set; }

    public bool restore { get; set; } = false;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        instance = this;
        //セーフエリア対応
        setSafearea("BattleCanvas");

        //背景はセーフエリア対応戻す
        //Vector3 hcv = BG.transform.localPosition;
        //BG.transform.localPosition = new Vector3(hcv.x, hcv.y + Screen.safeArea.y, hcv.z);

        //openPhaseはセーフエリア対応戻す
        //hcv = OpenPhaseObj.transform.localPosition;
        //OpenPhaseObj.transform.localPosition = new Vector3(hcv.x, hcv.y + Screen.safeArea.y, hcv.z);

        //hcv = OpenPhaseObj.transform.Find("Panel").localPosition;
        //OpenPhaseObj.transform.Find("Panel").localPosition = new Vector3(hcv.x, hcv.y - Screen.safeArea.y, hcv.z);

        //ResultPhaseはセーフエリア対応戻す
        Vector3 hcv = ResultPhaseObj.transform.localPosition;
        float safeAreaH = Screen.safeArea.y;

#if UNITY_ANDROID
        safeAreaH = Screen.height - Screen.safeArea.yMax;
#endif

        ResultPhaseObj.transform.localPosition = new Vector3(hcv.x, hcv.y + safeAreaH, hcv.z);

        //定数取得
        constants = APIConnectManager.Instance.login.constants;

        //ステージの大きさを入れておく
        STAGE_WIDTH = transform.Find("BattleCanvas").GetComponent<RectTransform>().rect.width;
        STAGE_HEIGHT = transform.Find("BattleCanvas").GetComponent<RectTransform>().rect.height;

        PreterObj.SetActive(true);
        NaviObj.SetActive(true);
        ButtonAuto.SetActive(true);
        ContinueConfirmObj.SetActive(true);
        BuyItemPhaseObj.SetActive(true);

        ConfirmPhaseObj.SetActive(true);
        OpenPhaseObj.SetActive(true);
        MainPhaseObj.SetActive(true);
        ResultPhaseObj.SetActive(true);

        TouchPanel.SetActive(false);

        open_director.Stop();
        Explain.SetActive(false);

        //APIをたたく
        if (!Param.tutorial)
            APIConnectManager.Instance.Battle(Param.battleId, Param.firstscene, Param.repaireId, onStart);
        else
            APIConnectManager.Instance.TutorialBattle(null, Param.from, onTutorialStart);
    }
    private void getBattleInfo(string json)
    {
        battle = JsonUtility.FromJson<jsonBattle>(json);

        //バトルを途中から復元するかどうか判断する
        int turn = PlayerPrefsUtility.Load(BattleBehaviour.TURN, 0);

        //ターンが進んでいる場合のみ復元する。チュートリアルとユーザーバトルは無条件に復元無し。
        if (!BattleBehaviour.Instance.Param.tutorial && BattleBehaviour.Instance.battle.tournament_id == constants.Tournament_Master.TOUR_QUEST && turn > 0)
            restore = true;

        if (restore)
        {
            //ターン復元
            InfoBehaviour.Instance.turnValue = PlayerPrefsUtility.Load(BattleBehaviour.TURN, 0);

            //HPを復元
            battle.hpStartP = PlayerPrefsUtility.Load(BattleBehaviour.HP_P, 0);
            battle.hpStartE = PlayerPrefsUtility.Load(BattleBehaviour.HP_E, 0);

            //スター復元
            Dictionary<int, int> star_p = PlayerPrefsUtility.LoadDict<int, int>(BattleBehaviour.STAR_TYPES_P);
            foreach (KeyValuePair<int, int> star in star_p)
            {
                MainPhaseBehaviour.Instance.objstarStoreP.Push(star.Value, 1);
            }

            Dictionary<int, int> star_e = PlayerPrefsUtility.LoadDict<int, int>(BattleBehaviour.STAR_TYPES_E);
            foreach (KeyValuePair<int, int> star in star_e)
            {
                MainPhaseBehaviour.Instance.objstarStoreE.Push(star.Value, 1);
            }

            //リベンジ回数復元
            battle.statRevCntP = PlayerPrefsUtility.Load(BattleBehaviour.STAT_REV_CNT_P, 0);
            battle.statRevCntE = PlayerPrefsUtility.Load(BattleBehaviour.STAT_REV_CNT_E, 0);

            //攻撃統計値復元
            Dictionary<string, int> DictstatTact = PlayerPrefsUtility.LoadDict<string, int>(BattleBehaviour.STAT_TACT);

            battle.statTactP0 = DictstatTact["P0"];
            battle.statTactP1 = DictstatTact["P1"];
            battle.statTactP2 = DictstatTact["P2"];
            battle.statTactP3 = DictstatTact["P3"];

            battle.statTactE0 = DictstatTact["E0"];
            battle.statTactE1 = DictstatTact["E1"];
            battle.statTactE2 = DictstatTact["E2"];
            battle.statTactE3 = DictstatTact["E3"];

            Dictionary<string, int> DictStatAttN = PlayerPrefsUtility.LoadDict<string, int>(BattleBehaviour.STAT_ATT_N);
            Dictionary<string, int> DictStatAttR = PlayerPrefsUtility.LoadDict<string, int>(BattleBehaviour.STAT_ATT_R);

            //attCnt代入
            battle.statNattCntP = DictStatAttN["P"];
            battle.statNattCntE = DictStatAttN["E"];
            battle.statRattCntP = DictStatAttR["P"];
            battle.statRattCntE = DictStatAttR["E"];

            Dictionary<string, int> DictStatDamN = PlayerPrefsUtility.LoadDict<string, int>(BattleBehaviour.STAT_DAM_N);
            Dictionary<string, int> DictStatDamR = PlayerPrefsUtility.LoadDict<string, int>(BattleBehaviour.STAT_DAM_R);
            Dictionary<string, int> DictStatDamO = PlayerPrefsUtility.LoadDict<string, int>(BattleBehaviour.STAT_DAM_O);

            //dam代入
            battle.statNdamP = DictStatDamN["P"];
            battle.statNdamE = DictStatDamN["E"];
            battle.statRdamP = DictStatDamR["P"];
            battle.statRdamE = DictStatDamR["E"];
            battle.statOdamP = DictStatDamO["P"];
            battle.statOdamE = DictStatDamO["E"];

            Dictionary<string, int> DictStatHitN = PlayerPrefsUtility.LoadDict<string, int>(BattleBehaviour.STAT_HIT_N);
            Dictionary<string, int> DictStatHitR = PlayerPrefsUtility.LoadDict<string, int>(BattleBehaviour.STAT_HIT_R);
            Dictionary<string, int> DictStatHitO = PlayerPrefsUtility.LoadDict<string, int>(BattleBehaviour.STAT_HIT_O);

            //hitCnt代入
            battle.statNhitCntP = DictStatHitN["P"];
            battle.statNhitCntE = DictStatHitN["E"];
            battle.statRhitCntP = DictStatHitR["P"];
            battle.statRhitCntE = DictStatHitR["E"];
            battle.statRhitCntP = DictStatHitO["P"];
            battle.statRhitCntE = DictStatHitO["E"];

        }
        else
        {
            InfoBehaviour.Instance.turnValue = 0;
        }

        this.initStat();

        NAV_REV["P0"] = Utility.getText("BATTLE_NAV_REV_P0");
        NAV_REV["P1"] = Utility.getText("BATTLE_NAV_REV_P1");
        NAV_REV["P2"] = Utility.getText("BATTLE_NAV_REV_P2");
        NAV_REV["P3"] = Utility.getText("BATTLE_NAV_REV_P3");
        NAV_REV["P4"] = Utility.getText("BATTLE_NAV_REV_P4");
        NAV_REV["P5"] = Utility.getText("BATTLE_NAV_REV_P5");
        NAV_REV["P6"] = Utility.getText("BATTLE_NAV_REV_P6");
        NAV_REV["E0"] = Utility.getText("BATTLE_NAV_REV_E0");
        NAV_REV["E1"] = Utility.getText("BATTLE_NAV_REV_E1");
        NAV_REV["E2"] = Utility.getText("BATTLE_NAV_REV_E2");
        NAV_REV["E3"] = Utility.getText("BATTLE_NAV_REV_E3");
        NAV_REV["E4"] = Utility.getText("BATTLE_NAV_REV_E4");
        NAV_REV["E5"] = Utility.getText("BATTLE_NAV_REV_E5");
        NAV_REV["E6"] = Utility.getText("BATTLE_NAV_REV_E6");

        Dictionary<string, object> jsonDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

        foreach (KeyValuePair<string, object> keyvalue in jsonDict)
        {
            if (keyvalue.Key == "card")
            {
                battle.card = new Dictionary<string, Dictionary<string, int[]>>();
                Dictionary<string, object> jsonDict2 = JsonConvert.DeserializeObject<Dictionary<string, object>>(keyvalue.Value.ToString());

                foreach (KeyValuePair<string, object> keyvalue2 in jsonDict2)
                {
                    if (keyvalue2.Value != null)
                    {
                        Dictionary<string, int[]> d = new Dictionary<string, int[]>();
                        Dictionary<string, int[]> jsonDict3 = JsonConvert.DeserializeObject<Dictionary<string, int[]>>(keyvalue2.Value.ToString());
                        foreach (KeyValuePair<string, int[]> keyvalue3 in jsonDict3)
                        {
                            d.Add(keyvalue3.Key, keyvalue3.Value);
                        }
                        battle.card.Add(keyvalue2.Key, d);
                    }
                    else
                    {
                        battle.card.Add(keyvalue2.Key, null);
                    }
                }
            }
        }

    }

    /// <summary>
    /// 統計値を初期化する
    /// </summary>
    public void initStat()
    {
        // 乱数の初期化
        // 固定のシード値を設定する。
        randomEx = new RandomEx(battle.randomSeed);

        att["1P"] = battle.att1P;
        att["2P"] = battle.att2P;
        att["3P"] = battle.att3P;
        att["1E"] = battle.att1E;
        att["2E"] = battle.att2E;
        att["3E"] = battle.att3E;

        def["1P"] = battle.def1P;
        def["2P"] = battle.def2P;
        def["3P"] = battle.def3P;
        def["1E"] = battle.def1E;
        def["2E"] = battle.def2E;
        def["3E"] = battle.def3E;

        statTact["P0"] = battle.statTactP0;
        statTact["P1"] = battle.statTactP1;
        statTact["P2"] = battle.statTactP2;
        statTact["P3"] = battle.statTactP3;

        statTact["E0"] = battle.statTactE0;
        statTact["E1"] = battle.statTactE1;
        statTact["E2"] = battle.statTactE2;
        statTact["E3"] = battle.statTactE3;

        statRevCnt["P"] = battle.statRevCntP;
        statRevCnt["E"] = battle.statRevCntE;

        //stat初期化
        stat["N"] = new StatInfo();
        stat["R"] = new StatInfo();
        stat["O"] = new StatInfo();

        //attCnt初期化
        stat["N"].attCnt = new Dictionary<string, int>();
        stat["R"].attCnt = new Dictionary<string, int>();

        //attCnt代入
        stat["N"].attCnt["P"] = battle.statNattCntP;
        stat["N"].attCnt["E"] = battle.statNattCntE;
        stat["R"].attCnt["P"] = battle.statRattCntP;
        stat["R"].attCnt["E"] = battle.statRattCntE;

        //dam初期化
        stat["N"].dam = new Dictionary<string, int>();
        stat["R"].dam = new Dictionary<string, int>();
        stat["O"].dam = new Dictionary<string, int>();

        //dam代入
        stat["N"].dam["P"] = battle.statNdamP;
        stat["N"].dam["E"] = battle.statNdamE;
        stat["R"].dam["P"] = battle.statRdamP;
        stat["R"].dam["E"] = battle.statRdamE;
        stat["O"].dam["P"] = battle.statOdamP;
        stat["O"].dam["E"] = battle.statOdamE;

        //hitCnt初期化
        stat["N"].hitCnt = new Dictionary<string, int>();
        stat["R"].hitCnt = new Dictionary<string, int>();
        stat["O"].hitCnt = new Dictionary<string, int>();

        //hitCnt代入
        stat["N"].hitCnt["P"] = battle.statNhitCntP;
        stat["N"].hitCnt["E"] = battle.statNhitCntE;
        stat["R"].hitCnt["P"] = battle.statRhitCntP;
        stat["R"].hitCnt["E"] = battle.statRhitCntE;
        stat["O"].hitCnt["P"] = battle.statRhitCntP;
        stat["O"].hitCnt["E"] = battle.statRhitCntE;
    }

    void onStart(string json)
    {
        //API結果受け取り
        getBattleInfo(json);

        lv["P"] = battle.LvP;
        lv["E"] = battle.LvE;

        Navi = NaviBehaviour.Instance;
        Preter = PreterBehaviour.Instance;
        ConfirmPhase = ConfirmPhaseBehaviour.Instance;
        OpenPhase = OpenPhaseBehaviour.Instance;
        MainPhase = MainPhaseBehaviour.Instance;
        ContinueConfirm = ContinueConfirmBehaviour.Instance;
        ClosePhase = ClosePhaseBehaviour.Instance;
        ResultPhase = ResultPhaseBehaviour.Instance;

        // 背景を初期状態に。
        this.ShowBg("Init");
        BG.transform.Find("Norm").GetComponent<Image>().sprite = Utility.getAssetImage("Image/BattleBg/" + battle.battle_bg);

        // ナビゲータのセリフを開始時のものにセット。
        this.setNaviPos("bottom");
        Navi.setText(battle.navSerif_open);
        Navi.setSide("P");
        Navi.Show(1);

        // 解説を表示
        Preter.setPos("center");
        Preter.setText(Utility.getText("TEXT_MESSAGE_BATTLE_TAP_START"));
        Preter.PlayAnim("blink");
        Preter.Visible(true);

        //スピーカーの画像を入れ替え
        Navi.setIcon(2, battle.speaker_charaP);
        Navi.setIcon(3, battle.speaker_charaE);

        //キャラグラ差し替え
        this.makeCharaAnim(battle.equip_infoP, OPCharaP);
        this.makeCharaAnim(battle.equip_infoE, OPCharaE);

        this.makeCharaAnim(battle.equip_infoP, CharaP);
        this.makeCharaAnim(battle.equip_infoE, CharaE);

        this.makeCharaAnim(battle.equip_infoP, EDCharaP);
        this.makeCharaUI(battle.equip_infoP, GradeCharaP);

        // 各オブジェクトを非表示に。
        ConfirmPhaseObj.SetActive(false);
        OpenPhaseObj.SetActive(false);
        MainPhaseObj.SetActive(false);
        ResultPhaseObj.SetActive(false);

        ButtonAuto.SetActive(false);
        ContinueConfirmObj.SetActive(false);
        BuyItemPhaseObj.SetActive(false);

        // ダメージディスプレイヤーの初期化。
        nextDamNo = 0;

        DamShow[0] = DamShow0;
        DamShow[1] = DamShow1;
        DamShow[2] = DamShow2;
        DamShow[3] = DamShow3;
        DamShow[4] = DamShow4;
        DamShow[5] = DamShow5;

        //最初はオートモードはOFF
        auto_flg = false;

        if (battle.firstscene != "")
        {

            // サーバへの確認リクエストを出したあと、そのレスポンスがあるまで待機する。
            if (battle.firstscene == "result")
            {
                APIConnectManager.Instance.BattleResult(battle.battle_id, null, Param.repaireId, null, ((string json) =>
                {
                    jsonBattleResult launcher = ClosePhaseBehaviour.Instance.JsonToClass(json);
                    // エラーコードが返っている場合はその表示を行う。
                    switch (launcher.result)
                    {
                        case "ok":
                            // エラーないならresultへ
                            MainPhaseObj.SetActive(false);
                            ButtonAuto.SetActive(false);
                            Preter.gameObject.SetActive(false);
                            Navi.gameObject.SetActive(false);

                            ResultPhaseObj.SetActive(true);
                            ResultPhase.Init(launcher);
                            break;
                    }
                }));
            }
            else
            {
                // 背景を初期状態に。
                this.ShowBg("Norm");

                // メインのアバタを表示
                CharaP.gameObject.SetActive(true);
                CharaE.gameObject.SetActive(true);
            }

            battle.firstscene = "";
        }
        else
        {
            // 確認フェーズ制御オブジェクトを表示＆スタート。
            ConfirmPhaseObj.SetActive(true);
            ConfirmPhase.Init();

            nextSeq = "transmit";
            TouchPanel.SetActive(true);

        }

        DispatchEvent(CwEvent.SCENE_READY);
    }

    /// <summary>
    /// サーバへの開始時通達が終わったらcallされるラベル。
    /// </summary>
    public void ConfEnd()
    {
        Debug.Log("confirmEnd run..");

        // サーバへの確認が問題ないならば開始する。
        if (ConfirmPhaseBehaviour.Instance.transmitter.result == "ok")
        {
            this.Open();
        }

    }


    /// <summary>
    /// バトル開始時のエフェクト再生を行う。
    /// </summary>
    void Open()
    {
        open_director.stopped += OnPlayableDirectorStopped;

        ConfirmPhaseObj.SetActive(false);
        Preter.Visible(false);
        Navi.Visible(false);

        // 背景を元に戻す。
        this.ShowBg("Norm");

        if (battle.hpStartP > 0)
        {
            // メインのアバタを非表示
            CharaP.gameObject.SetActive(false);
            CharaE.gameObject.SetActive(false);

            OpenPhaseObj.GetComponent<OpenPhaseBehaviour>().TextNameP.text = battle.nameP;
            OpenPhaseObj.GetComponent<OpenPhaseBehaviour>().TextNameE.text = battle.nameE;

            OpenPhaseObj.SetActive(true);
            ButtonAuto.SetActive(true);

            //Info初期化
            InfoBehaviour.Instance.Init();

            //BGMスタート。
            AudioManager.Instance.PlayBGM(battle.bgm_sound, AudioManager.BGM_VOLUME_DEFULT);
            // 開始フェーズ制御オブジェクトを表示＆スタート。
            // 開始フェーズが終わったら "OnPlayableDirectorStopped" がcallされるので、それまで待つ。
            open_director.Play();
        }
        else
        {
            //自分のHPが0の場合はいきなりcontinueに飛ばす
            //復旧処理の実装にて戻りで必要
            MainPhaseObj.SetActive(true);
            ButtonAuto.SetActive(true);

            // メインのアバタを表示
            CharaP.gameObject.SetActive(true);
            CharaE.gameObject.SetActive(true);

            // HP表示の動作を開始。
            InfoBehaviour.Instance.Init();
            HpGaugeBehaviour.Instance.HpStart();

            StartCoroutine(this.Contiune());
        }
    }

    //
    // バトル開始時の確認処理を行う。
    // サーバにバトル開始の通達を行い、そのレスポンスをチェックする。
    void onTutorialStart(string json)
    {
        //API結果受け取り
        getBattleInfo(json);

        lv["P"] = battle.LvP;
        lv["E"] = battle.LvE;

        Navi = NaviBehaviour.Instance;
        Preter = PreterBehaviour.Instance;
        ConfirmPhase = ConfirmPhaseBehaviour.Instance;
        OpenPhase = OpenPhaseBehaviour.Instance;
        MainPhase = MainPhaseBehaviour.Instance;
        ContinueConfirm = ContinueConfirmBehaviour.Instance;
        ClosePhase = ClosePhaseBehaviour.Instance;
        ResultPhase = ResultPhaseBehaviour.Instance;

        // 背景を初期状態に。
        this.ShowBg("Init");
        BG.transform.Find("Norm").GetComponent<Image>().sprite = Utility.getAssetImage("Image/BattleBg/forest");

        //interpreter._visible = true;
        //interpreter.text = "";

        //navigator._visible = true;
        //navigator.text = "読み込み中なのだ・・";

        // プレイヤー名、相手の名前を初期化。
        InfoBehaviour.Instance.setAvatarName(battle.nameP, "P");
        InfoBehaviour.Instance.setAvatarName(battle.nameE, "E");

        //キャラグラ差し替え
        string[] formationP = new string[] { "PLA", "11001", "12001", "13001", "14001" };
        this.makeCharaAnim(formationP, CharaP);

        string[] formationE = new string[] { "MOB", "1100" };
        this.makeCharaAnim(formationE, CharaE);

        // 各オブジェクトを非表示に。
        ConfirmPhaseObj.SetActive(false);
        OpenPhaseObj.SetActive(false);
        MainPhaseObj.SetActive(false);
        ResultPhaseObj.SetActive(false);

        //ButtonAuto.SetActive(false);
        ContinueConfirmObj.SetActive(false);
        BuyItemPhaseObj.SetActive(false);

        // ダメージディスプレイヤーの初期化。
        nextDamNo = 0;

        DamShow[0] = DamShow0;
        DamShow[1] = DamShow1;
        DamShow[2] = DamShow2;
        DamShow[3] = DamShow3;
        DamShow[4] = DamShow4;
        DamShow[5] = DamShow5;

        //最初はオートモードはOFF
        auto_flg = false;

        // チュートリアルバトルは確認ナシ。
        //gotoAndPlay("open");

        this.onTutorialOpen();

        DispatchEvent(CwEvent.SCENE_READY);
    }

    //
    // バトル開始時のエフェクト再生を行う。
    void onTutorialOpen()
    {

        //BGMスタート。
        AudioManager.Instance.PlayBGM("bgm_battle", AudioManager.BGM_VOLUME_DEFULT);

        // 背景を元に戻す。
        this.ShowBg("Norm");

        // HP表示の動作を開始。
        InfoBehaviour.Instance.Init();
        HpGaugeBehaviour.Instance.HpStart();

        // メインのアバタを表示
        BattleBehaviour.Instance.CharaP.gameObject.SetActive(true);
        BattleBehaviour.Instance.CharaE.gameObject.SetActive(true);

        // チュートリアルバトルは開始エフェクトないが、代わりにナビの
        // 独壇が入る
        BattleBehaviour.Instance.setNaviPos("top");
        Navi.Visible(true);


        MainPhaseObj.SetActive(true);
        Preter.setPos("main");
        Preter.Visible(true);

        //Dtech初期化
        DtechBehaviour.Instance.Init();

        //ナビにしゃべらせてから進行
        StartCoroutine(naviSpeaks(battle.tutOpen, MainPhaseBehaviour.Instance.Prog));

    }

    public delegate void OnCompleteDelegate();
    public OnCompleteDelegate CompleteHandler;

    /// <summary>
    /// ナビの連続セリフを表示する
    /// </summary>
    bool navitouchflg = false;
    public IEnumerator naviSpeaks(string[] speaks, OnCompleteDelegate _callback = null)
    {

        if (_callback != null)
            CompleteHandler += _callback;

        if (speaks != null)
        {
            NaviObj.SetActive(true);

            nextSeq = "navispeak";
            Navi.setSide("P");
            Navi.Show(1);

            //説明パネル表示
            Explain.SetActive(true);

            foreach (string speak in speaks)
            {
                if (speak.Contains("COMMAND:"))
                {
                    string command = speak.Split(':')[1];
                    string action = command.Split(' ')[0];
                    string image = command.Split(' ')[1];

                    Transform target = Explain.transform.Find(image);

                    if (action == "SHOW")
                    {
                        if (image.Contains("Arrow"))
                        {
                            //⇒の場合は点滅
                            Vector3 pos = target.GetComponent<RectTransform>().anchoredPosition;
                            string align = command.Split(' ')[2];

                            target.GetComponent<ArrowBehaviour>().Show(align, pos.x, pos.y);
                        }

                        target.gameObject.SetActive(true);
                    }
                    else if (action == "HIDE")
                    {
                        target.gameObject.SetActive(false);
                    }

                }
                else
                {
                    navitouchflg = false;
                    //タッチパネル表示
                    TouchPanel.SetActive(true);
                    //セリフを設定
                    Navi.setText(speak);

                    //タッチされるまで待機
                    while (!navitouchflg)
                    {
                        yield return null;
                    }
                }
            }

            TouchPanel.SetActive(false);
            NaviObj.SetActive(false);
        }

        if (CompleteHandler != null)
        {
            CompleteHandler?.Invoke();
            CompleteHandler = null;
        }
    }

    public void Main()
    {
        OpenPhaseObj.SetActive(false);
        MainPhaseObj.SetActive(true);
        ButtonAuto.SetActive(true);

        Preter.setPos("main");
        //Preter.Visible(true);

        BattleBehaviour.Instance.setNaviPos("top");
        Navi.Visible(true);

        //Dtech初期化
        DtechBehaviour.Instance.Init();

        //MainPhaseBehaviour.Instance.revengeTest();
        MainPhaseBehaviour.Instance.Prog();
    }

    //
    // バトルメインフェーズが終了したらcallされる。
    public void MainEnd()
    {
        StartCoroutine(this.Contiune());
    }


    void OnPlayableDirectorStopped(PlayableDirector aDirector)
    {
        if (open_director == aDirector)
        {
            Debug.Log("PlayableDirector named " + aDirector.name + " is now stopped.");

            this.Main();
        }
    }

    string battleResult;

    IEnumerator Contiune()
    {
        Debug.Log("BattleBehaviour Contiune run..");

        // 勝ったのか、負けたのか、引き分けたのか、タイムアップしたのかを取得。
        if (HpGaugeBehaviour.Instance.HpInfo["P"].value <= 0 && HpGaugeBehaviour.Instance.HpInfo["E"].value <= 0)
        {
            battleResult = "draw";
        }
        else if (HpGaugeBehaviour.Instance.HpInfo["P"].value <= 0)
        {
            battleResult = "lose";
        }
        else if (HpGaugeBehaviour.Instance.HpInfo["E"].value <= 0)
        {
            battleResult = "win";
            StartCoroutine(MainPhaseBehaviour.Instance.Avatar["P"].PlayAnim("AvatarWin"));
        }
        else
        {
            battleResult = "timeup";
        }

        //BGMを止めてバトル終了ジングルを流す
        AudioManager.Instance.StopBGM();

        if (battleResult == "lose")
            AudioManager.Instance.PlaySE("se_retire");
        else if (battleResult == "win")
            AudioManager.Instance.PlaySE("se_win");
        else
            AudioManager.Instance.PlaySE("se_battle_end");

        if (battleResult == "lose" && battle.continueError == 0)
        {
            // プレイヤー名、相手の名前を設定。
            InfoBehaviour.Instance.setAvatarName(battle.nameP, "P");
            InfoBehaviour.Instance.setAvatarName(battle.nameE, "E");

            if (battle.continueItemCnt > 0)
            {
                // 終了フェーズ制御オブジェクトを表示＆スタート。
                this.continueConfirm();
            }
            else
            {
                this.continueBuy();
            }
        }
        else
        {
            float waitCount = 0.5f;

            // 一定時間待機して終了。
            yield return new WaitForSeconds(waitCount);

            StartCoroutine(this.Close());
        }

    }

    public void continueConfirm()
    {
        ContinueConfirmBehaviour.Instance.mode = 1;
        ContinueConfirmObj.SetActive(true);

        //メッセージ出力
        string text = Utility.getText("BATTLE_CONTINUE_CONFIRM")  + Utility.getText("BATTLE_RERAISE_COUNT").Replace("{0}", battle.continueItemName).Replace("{1}", battle.continueItemCnt.ToString());
        ContinueConfirm.setText(text);

        //mode = battleResult;

        // ナビゲータのセリフをセット。
        this.setNaviPos("bottom");
        Navi.setText(this.getNaviSpeak(battleResult));
        Navi.setSide("P");
        Navi.Show(1);
    }

    void continueBuy()
    {
        ContinueConfirmBehaviour.Instance.mode = 2;
        ContinueConfirmObj.SetActive(true);

        //メッセージ出力
        string text = Utility.getText("BATTLE_CONTINUE_BUY_CONFIRM").Replace("{0}", battle.continueItemName).Replace("{1}", battle.continueItemName);
        ContinueConfirm.setText(text);

        BuyItemPhaseBehaviour.Instance.mode = battleResult;

        // ナビゲータのセリフをセット。
        this.setNaviPos("bottom");
        Navi.setText(this.getNaviSpeak(battleResult));
        Navi.setSide("P");
        Navi.Show(1);
    }

    /// <summary>
    /// バトル終了時のエフェクト再生＆終了処理を行う。
    /// </summary>
    public IEnumerator Close()
    {
        Debug.Log("BattleBehaviour Close run..");

        // 終了フェーズ制御オブジェクトを表示＆スタート。
        ClosePhase.mode = battleResult;
        ClosePhase.ClosePhaseStart();

        yield return null;
    }

    void OnDisable()
    {
        open_director.stopped -= OnPlayableDirectorStopped;
    }

    public void ShowBg(string bgname)
    {
        BG.transform.Find("Init").gameObject.SetActive(false);
        BG.transform.Find("Rev").gameObject.SetActive(false);
        BG.transform.Find("Norm").gameObject.SetActive(false);

        BG.transform.Find(bgname).gameObject.SetActive(true);
    }

    public void onTouch()
    {
        if (nextSeq != null)
        {
            AudioManager.Instance.PlaySE("se_btn");
            TouchPanel.SetActive(false);
            switch (nextSeq)
            {
                case "navispeak":
                    navitouchflg = true;
                    break;
                case "transmit":
                    ConfirmPhase.Transmit();
                    break;
                case "result":
                    MainPhaseObj.SetActive(false);
                    ButtonAuto.SetActive(false);
                    Preter.gameObject.SetActive(false);
                    Navi.gameObject.SetActive(false);

                    ResultPhaseObj.SetActive(true);
                    ResultPhase.Init(ClosePhase.launcher);

                    break;
                case "tutorialend":
                    APIConnectManager.Instance.TutorialBattle("1", Param.from, ((string json) =>
                    {
                        jsonTutlrialBattle tutobattle = JsonUtility.FromJson<jsonTutlrialBattle>(json);
                        if (tutobattle.result == "ok")
                        {
                            ResultPhase.Trans(tutobattle.urlOnEnd);
                        }
                    }));
                    break;
            }
        }
    }

    public void onAutoTouch()
    {
        //チュートリアル中は効かない
        if (Param.tutorial)
            return;

        AudioManager.Instance.PlaySE("se_btn");

        auto_flg = (!auto_flg);
        if (auto_flg)
        {
            ButtonAuto.transform.Find("BtnAuto").gameObject.SetActive(true);
            ButtonAuto.transform.Find("BtnManual").gameObject.SetActive(false);
            MainPhase.transform.Find("Info/Turn/BtnAuto").gameObject.SetActive(true);
            MainPhase.transform.Find("Info/Turn/BtnManual").gameObject.SetActive(false);
        }
        else
        {
            ButtonAuto.transform.Find("BtnAuto").gameObject.SetActive(false);
            ButtonAuto.transform.Find("BtnManual").gameObject.SetActive(true);
            MainPhase.transform.Find("Info/Turn/BtnAuto").gameObject.SetActive(false);
            MainPhase.transform.Find("Info/Turn/BtnManual").gameObject.SetActive(true);
        }
    }

    public string side { get; set; }
    public int type { get; set; }
    public int value { get; set; }
    public int dir { get; set; }
    public string way { get; set; }

    //
    // 指定された側に、指定されたダメージ／回復が発生した場合の処理を行う
    // callラベル。
    // 以下の値を変数で指定する。
    //     side     どちらに発生したのか"P"か"E"。
    //     type     どの属性でダメージが与えられたのか。1, 2, 3 のいずれか。
    //              必殺技などでいずれでもないならば 4 を指定する。
    //     value    ダメージの値。回復の場合はマイナスで指定する。
    //     dir      ダメージディスプレイヤーの飛び出る方向。0,1,2のいずれか。
    //              オマカセなら-1を指定する。
    //     way      通常攻撃によるダメージなら "N"、リベンジによるダメージなら "R" を指定する。
    //              必殺技などでいずれでもないならば "O" を指定する。
    public void Damage()
    {
        bool flg = false;

        // 回復の場合は（敵が死んでようが）つねに行う
        if (value < 0)
        {
            flg = true;
        }
        else
        {
            // ダメージの場合はHPがちゃんと残っているなら行う。
            if (HpGaugeBehaviour.Instance.HpInfo["P"].value > 0 && HpGaugeBehaviour.Instance.HpInfo["E"].value > 0)
                flg = true;
        }

        if (flg)
        {

            // 飛び出る方向がオマカセになっているなら決定しておく。
            if (dir == -1)
                dir = nextDamNo % 3;

            // 回復の場合は type=5 とする。
            if (value < 0)
                type = 5;

            // ダメージディスプレイヤーに値をセットして再生。
            DamShow[nextDamNo].gameObject.SetActive(true);
            DamShow[nextDamNo].value = Mathf.Abs(value);
            DamShow[nextDamNo].side = side;
            DamShow[nextDamNo].type = type;
            DamShow[nextDamNo].dir = dir;
            DamShow[nextDamNo].DamShowStart();

            // 次のダメージディスプレイヤーをセットしておく。
            nextDamNo++;
            nextDamNo %= 6;

            // ダメージOR回復が発生しているなら...
            if (value != 0)
            {

                // 回復時、オーバーリカバーの分は削って考える。
                if (value < 0 && HpGaugeBehaviour.Instance.HpInfo[side].max < HpGaugeBehaviour.Instance.HpInfo[side].value - value)
                {
                    value = (int)(HpGaugeBehaviour.Instance.HpInfo[side].value - HpGaugeBehaviour.Instance.HpInfo[side].max);
                }

                // HPゲージの調整
                HpGaugeBehaviour.Instance.HpInfo[side].value -= value;

                if (HpGaugeBehaviour.Instance.HpInfo[side].value < 0)
                {
                    HpGaugeBehaviour.Instance.HpInfo[side].value = 0;
                }

                if (HpGaugeBehaviour.Instance.HpInfo[side].max < HpGaugeBehaviour.Instance.HpInfo[side].value)
                {
                    HpGaugeBehaviour.Instance.HpInfo[side].value = (int)HpGaugeBehaviour.Instance.HpInfo[side].max;
                }

                // 変数 side で指定された側のアバターにエフェクトを発生させる。
                // HPが0になったら死亡エフェクト。そうでないならダメージエフェクト。
                if (HpGaugeBehaviour.Instance.HpInfo[side].value <= 0)
                {
                    AudioManager.Instance.PlaySE("se_explosionshort");

                    StartCoroutine(MainPhaseBehaviour.Instance.Avatar[side].PlayAnim("AvatarDeath"));
                }
                else
                {
                    if (type <= 4)
                    {
                        if (type == 4)
                        {
                            AudioManager.Instance.PlaySE("se_damage");
                            //AudioManager.Instance.PlaySE("se_damage");
                        }
                        else
                        {
                            AudioManager.Instance.PlaySE("se_hit");
                        }

                        StartCoroutine(MainPhaseBehaviour.Instance.Avatar[side].PlayAnim("AvatarDamage" + type));
                    }
                }
            }

            // 戦闘統計更新
            if (way != "")
            {
                string opp = (side == "P") ? "E" : "P";    // ダメージを与えた側を取得。
                stat[way].hitCnt[opp]++;
                stat[way].dam[opp] += value;
            }
        }

    }

    public void setNaviPos(string pos)
    {
        Vector3 navi_pos = Navi.transform.GetComponent<RectTransform>().anchoredPosition;
        Vector3 newpos;
        switch (pos)
        {
            case "top":
                newpos = new Vector3(navi_pos.x, -200, 0);
                Navi.setPos(newpos);
                break;
            case "bottom":
                newpos = new Vector3(navi_pos.x, BOTTOM_POS, 0);
                Navi.setPos(newpos);
                break;
        }
    }

    public string getNaviSpeak(string mode)
    {
        string text = "";
        switch (mode)
        {
            case "open":
                text = battle.navSerif_open;
                break;
            case "win":
                text = battle.navSerif_win;
                break;
            case "lose":
                text = battle.navSerif_lose;
                break;
            case "draw":
                text = battle.navSerif_draw;
                break;
            case "timeup":
                text = battle.navSerif_timeup;
                break;
        }
        return text;
    }

    /// <summary>
    /// バトル情報をPlayerPrefに保持する
    /// </summary>
    public void SaveStat()
    {
        if (!BattleBehaviour.Instance.Param.tutorial && BattleBehaviour.Instance.battle.tournament_id == constants.Tournament_Master.TOUR_QUEST)
        {
            PlayerPrefsUtility.Save(BattleBehaviour.TURN, InfoBehaviour.Instance.turnValue);
            PlayerPrefsUtility.Save(BattleBehaviour.HP_P, HpGaugeBehaviour.Instance.HpInfo["P"].value);
            PlayerPrefsUtility.Save(BattleBehaviour.HP_E, HpGaugeBehaviour.Instance.HpInfo["E"].value);
            PlayerPrefsUtility.SaveDict<int, int>(BattleBehaviour.STAR_TYPES_P, MainPhaseBehaviour.Instance.objstarStoreP.starTypes);
            PlayerPrefsUtility.SaveDict<int, int>(BattleBehaviour.STAR_TYPES_E, MainPhaseBehaviour.Instance.objstarStoreE.starTypes);

            PlayerPrefsUtility.Save(BattleBehaviour.STAT_REV_CNT_P, BattleBehaviour.Instance.statRevCnt["P"]);
            PlayerPrefsUtility.Save(BattleBehaviour.STAT_REV_CNT_E, BattleBehaviour.Instance.statRevCnt["E"]);
            PlayerPrefsUtility.SaveDict<string, int>(BattleBehaviour.STAT_TACT, BattleBehaviour.Instance.statTact);

            PlayerPrefsUtility.SaveDict<string, int>(BattleBehaviour.STAT_ATT_N, BattleBehaviour.Instance.stat["N"].attCnt);
            PlayerPrefsUtility.SaveDict<string, int>(BattleBehaviour.STAT_ATT_R, BattleBehaviour.Instance.stat["R"].attCnt);

            PlayerPrefsUtility.SaveDict<string, int>(BattleBehaviour.STAT_DAM_N, BattleBehaviour.Instance.stat["N"].dam);
            PlayerPrefsUtility.SaveDict<string, int>(BattleBehaviour.STAT_DAM_R, BattleBehaviour.Instance.stat["R"].dam);
            PlayerPrefsUtility.SaveDict<string, int>(BattleBehaviour.STAT_DAM_O, BattleBehaviour.Instance.stat["O"].dam);

            PlayerPrefsUtility.SaveDict<string, int>(BattleBehaviour.STAT_HIT_N, BattleBehaviour.Instance.stat["N"].hitCnt);
            PlayerPrefsUtility.SaveDict<string, int>(BattleBehaviour.STAT_HIT_R, BattleBehaviour.Instance.stat["R"].hitCnt);
            PlayerPrefsUtility.SaveDict<string, int>(BattleBehaviour.STAT_HIT_O, BattleBehaviour.Instance.stat["O"].hitCnt);

        }
    }

    /// <summary>
    /// PlayerPrefに保持したバトル情報をクリアする
    /// </summary>
    public void ClearStat()
    {
        if (!BattleBehaviour.Instance.Param.tutorial && BattleBehaviour.Instance.battle.tournament_id == constants.Tournament_Master.TOUR_QUEST)
        {
            PlayerPrefsUtility.Save(BattleBehaviour.TURN, 0);
            PlayerPrefsUtility.Save(BattleBehaviour.HP_P, 0);
            PlayerPrefsUtility.Save(BattleBehaviour.HP_E, 0);
            PlayerPrefsUtility.SaveDict<int, int>(BattleBehaviour.STAR_TYPES_P, new Dictionary<int, int>());
            PlayerPrefsUtility.SaveDict<int, int>(BattleBehaviour.STAR_TYPES_E, new Dictionary<int, int>());
            PlayerPrefsUtility.Save(BattleBehaviour.STAT_REV_CNT_P, 0);
            PlayerPrefsUtility.Save(BattleBehaviour.STAT_REV_CNT_E, 0);
            PlayerPrefsUtility.SaveDict<string, int>(BattleBehaviour.STAT_TACT, new Dictionary<string, int>());

            PlayerPrefsUtility.SaveDict<string, int>(BattleBehaviour.STAT_ATT_N, new Dictionary<string, int>());
            PlayerPrefsUtility.SaveDict<string, int>(BattleBehaviour.STAT_ATT_R, new Dictionary<string, int>());

            PlayerPrefsUtility.SaveDict<string, int>(BattleBehaviour.STAT_DAM_N, new Dictionary<string, int>());
            PlayerPrefsUtility.SaveDict<string, int>(BattleBehaviour.STAT_DAM_R, new Dictionary<string, int>());
            PlayerPrefsUtility.SaveDict<string, int>(BattleBehaviour.STAT_DAM_O, new Dictionary<string, int>());

            PlayerPrefsUtility.SaveDict<string, int>(BattleBehaviour.STAT_HIT_N, new Dictionary<string, int>());
            PlayerPrefsUtility.SaveDict<string, int>(BattleBehaviour.STAT_HIT_R, new Dictionary<string, int>());
            PlayerPrefsUtility.SaveDict<string, int>(BattleBehaviour.STAT_HIT_O, new Dictionary<string, int>());
        }
    }

}
