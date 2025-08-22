using Scenes.Common.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//
// 3枚のカードを出し合って突合させる1ターンを構成するムービー
public class TurnPhaseBehaviour : MonoBehaviour
{
    public TactBehaviour TactP;
    public TactBehaviour TactE;

    public Dictionary<string, TactBehaviour> Tacts = new Dictionary<string, TactBehaviour>();

    public FrontBehaviour FrontP0;
    public FrontBehaviour FrontP1;
    public FrontBehaviour FrontP2;

    public FrontBehaviour FrontE0;
    public FrontBehaviour FrontE1;
    public FrontBehaviour FrontE2;

    public Dictionary<string, FrontBehaviour> Fronts = new Dictionary<string, FrontBehaviour>();

    public CardBehaviour CardP0;
    public CardBehaviour CardP1;
    public CardBehaviour CardP2;

    public CardBehaviour CardE0;
    public CardBehaviour CardE1;
    public CardBehaviour CardE2;

    public Dictionary<string, CardBehaviour> Cards = new Dictionary<string, CardBehaviour>();

    public PowerBehaviour Power0;
    public PowerBehaviour Power1;
    public PowerBehaviour Power2;

    public Dictionary<int, PowerBehaviour> Powers = new Dictionary<int, PowerBehaviour>();

    // カードが出現するときに、次のカードが前のカードより何フレーム
    // 遅れて登場するか。
    private int CARD_APPEAR_DELAY = (int)(0.5 * (BattleBehaviour.FRAME_RATE / 2));

    // カードの幅・高さ
    public static int CARD_WIDTH = 90;
    public static int CARD_HEIGHT = 90;

    // 一番上のカードの標準時座標(プレイヤー側)。
    public static float CARD_REGULAR_X = -95f;
    public static float CARD_REGULAR_Y = -383f;

    public int type { get; set; }
    public int frameCounter { get; set; } = 0;

    // Start is called before the first frame update
    public static TurnPhaseBehaviour Instance
    {
        get
        {
            return instance;
        }
    }

    private static TurnPhaseBehaviour instance;

    private void Start()
    {
        instance = this;

        //カードをDictionaryに追加
        Cards["P" + 0] = CardP0;
        Cards["P" + 1] = CardP1;
        Cards["P" + 2] = CardP2;
        Cards["E" + 0] = CardE0;
        Cards["E" + 1] = CardE1;
        Cards["E" + 2] = CardE2;

        //非表示にしておく
        /*
        foreach (KeyValuePair<string, CardBehaviour> _card in Cards)
        {
            _card.Value.transform.Find("Card").GetComponent<ParticleSystemRenderer>().enabled = false;
        }
        */
        Tacts["P"] = TactP;
        Tacts["E"] = TactE;

        Powers[0] = Power0;
        Powers[1] = Power1;
        Powers[2] = Power2;

        foreach (KeyValuePair<int, PowerBehaviour> _p in Powers)
        {
            _p.Value.gameObject.SetActive(false);
        }
        Fronts["P0"] = FrontP0;
        Fronts["P1"] = FrontP1;
        Fronts["P2"] = FrontP2;
        Fronts["E0"] = FrontE0;
        Fronts["E1"] = FrontE1;
        Fronts["E2"] = FrontE2;

        // 戦術表示を非表示に。
        TactP.gameObject.SetActive(false);
        TactE.gameObject.SetActive(false);

        // 突合制御オブジェクトの初期化。
        for (int i = 0; i < 3; i++)
        {
            Fronts["P" + i].row = i;
            Fronts["P" + i].side = "P";
            Fronts["E" + i].row = i;
            Fronts["E" + i].side = "E";
        }
    }


    jsonBattle battle { set; get; }

    //
    // 1ターンの開始。
    public void TurnStart()
    {
        Debug.Log("TurnPhaseBehaviour TurnStart run..");
        battle = BattleBehaviour.Instance.battle;

        // プレイヤー側と相手側双方に対して行う。
        for (int m = 1; m <= 2; m++)
        {

            // どちらに対して行うのかを決定。
            string side = (m == 1) ? "P" : "E";

            // 3枚のカードを決定する。
            for (int i = 0; i < 3; i++)
            {
                if (battle.card[side] != null && battle.card[side].ContainsKey(InfoBehaviour.Instance.turnValue.ToString()))
                {
                    //出来レースの場合
                    Cards[side + i].type = battle.card[side][InfoBehaviour.Instance.turnValue.ToString()][i];
                }
                else
                {
                    // カードタイプを決定。
                    BattleFuncs.Card(side);
                    // カードを初期化
                    Cards[side + i].type = BattleFuncs.type;
                }

                Cards[side + i].reverse = false;
            }

            // ユニゾンになる場合で...
            if (Cards[side + 0].type == Cards[side + 1].type && Cards[side + 0].type == Cards[side + 2].type)
            {

                // すでに全ターンの半分でユニゾンになっているなら、最後のカードを別のものに変える。
                if (BattleBehaviour.Instance.statTact[side + 0] >= battle.timeupTurns / 2)
                {
                    Cards[side + 2].type--;

                    if (Cards[side + 2].type == 0)
                        Cards[side + 2].type = 3;
                }
            }

            // 3枚のカードを初期化する。
            for (int i = 0; i < 3; i++)
                Cards[side + i].CardInit();
        }

        // カードの出現タイミングを制御するためのフレームカウンターを初期化。
        frameCounter = 0;

        StartCoroutine(TurnStart2());

    }

    /// <summary>
    /// 出現カードを格納する
    /// </summary>
    int appearCount { get; set; }

    IEnumerator TurnStart2()
    {
        Debug.Log("TurnPhaseBehaviour TurnStart2 run..");

        //クリア
        appearCount = 0;
        int appearNo = 0;
        float wait = 0.4f;

        while (true)
        {
            // 出現モーションが終了しているなら次のパート(selectTact)へ。
            if (appearCount == 6)
            {
                StartCoroutine(this.selTact());
                yield break;
            }
            else
            {
                // まだの場合。
                if(appearNo == 0) { 
                    CardAppear(appearNo++);

                    yield return new WaitForSeconds(wait);

                    CardAppear(appearNo++);

                    yield return new WaitForSeconds(wait);

                    CardAppear(appearNo++);
                }
            }

            yield return new WaitForSeconds(Main.Instance.getParFrame());

        }
    }

    void CardAppear(int appearNo)
    {
        AudioManager.Instance.PlaySE("se_hover");

        //カードを表示しておくがカードのイメージはanimationの方でアクティブにする
        Cards["P" + appearNo].gameObject.SetActive(true);

        //カード位置移動
        float _xP = CARD_REGULAR_X;
        float _yP = CARD_REGULAR_Y - (CARD_HEIGHT + 50.625f) * appearNo;
        Cards["P" + appearNo].GetComponent<RectTransform>().anchoredPosition = new Vector3(_xP, _yP, 0);

        //カードを表示しておくがカードのイメージはanimationの方でアクティブにする
        Cards["E" + appearNo].gameObject.SetActive(true);

        //カード位置移動
        float _xE = _xP * -1;
        float _yE = _yP;
        Cards["E" + appearNo].GetComponent<RectTransform>().anchoredPosition = new Vector3(_xE, _yE, 0);

        //アニメ再生
        StartCoroutine(Cards["P" + appearNo].PlayAnim("CardAppear", CardAppearEnd));
        StartCoroutine(Cards["E" + appearNo].PlayAnim("CardAppear", CardAppearEnd));
    }

    //カード出現アニメ終了コールバック
    void CardAppearEnd(string cardname)
    {
        Debug.Log("TurnPhaseBehaviour CardAppearEnd cardname=" + cardname);
        appearCount++;
    }

    bool unizonP { get; set; } = false;
    bool unizonE { get; set; } = false;

    /// <summary>
    /// 相性表示を行うと同時に、戦術選択を開始。
    /// </summary>
    /// <returns></returns>
    IEnumerator selTact()
    {
        Debug.Log("TurnPhaseBehaviour selTact run..");

        //power表示
        foreach (KeyValuePair<int, PowerBehaviour> _p in Powers)
        {
            _p.Value.gameObject.SetActive(true);
        }


        if (BattleBehaviour.Instance.auto_flg)
            NaviBehaviour.Instance.Visible(false);

        // 相性表示を一つずつセットしていく。
        for (int i = 0; i < 3; i++)
        {
            // プレイヤー側の有利・不利を変数 advantage に求める。
            // 以下の式で 0:同じ 1:有利 2:不利 になる。
            int advantage = Cards["P" + i].type - Cards["E" + i].type;

            if (advantage < 0) advantage += 3;

            string label = "";

            // 相性にしたがって、相性表示のラベルを取得。
            switch (advantage)
            {
                case 0: label = "eq"; break;
                case 1: label = "gt"; break;
                case 2: label = "lt"; break;
            }

            // 取得したラベルにgoto。
            Powers[i].PlayAnim(label);
        }

        // プレイヤー側、相手側がユニゾンになっているかを取得。
        unizonP = (CardP0.type == CardP1.type && CardP0.type == CardP2.type);
        unizonE = (CardE0.type == CardE1.type && CardE0.type == CardE2.type);

        // プレイヤー側戦術決定
        //-------------------------------------------------------------------
        if (unizonP)
        {
            // ユニゾンなっているなら「ユニゾン」
            TactP.type = 0;

            // 戦術選択の理由セット。
            PlayerSelect.Instance.reasontext = battle.AUTO_SERIFU_UNISON;

        }
        else if (unizonE)
        {
            // 自分がユニゾンでないが、相手がユニゾンなら「慎重」
            TactP.type = 2;

            // 戦術選択の理由セット。
            PlayerSelect.Instance.reasontext = battle.AUTO_SERIFU_UNISONED;

        }
        else
        {
            // 双方ユニゾンでない場合はとりあえず-1にして、後続コードに任せる。
            TactP.type = -1;
        }

        // 後は...
        //-------------------------------------------------------------------

        // プレイヤー側戦術が決まっていない場合。
        if (TactP.type == -1)
        {
            //チュートリアルの場合で一回目のターンの時はナビがしゃべる
            if (BattleBehaviour.Instance.Param.tutorial && InfoBehaviour.Instance.turnValue == 1)
            {
                StartCoroutine(BattleBehaviour.Instance.naviSpeaks(battle.tutTurn, (() =>
                {
                    // 戦術選択用ムービーを再生。戦術選択が完了するまで待つ。
                    StartCoroutine(PlayerSelect.Instance.Init());
                })));
            }
            else
            {
                // 戦術選択用ムービーを再生。戦術選択が完了するまで待つ。
                StartCoroutine(PlayerSelect.Instance.Init());
            }
        }
        else
        {
            // 決まっているなら、戦術決定ラベルをcall。
            StartCoroutine(this.Decided());
        }

        yield return null;
    }

    //
    // プレイヤー側の戦術が決まったらcallされるラベル。

    // 相手側戦術決定
    //-------------------------------------------------------------------
    public IEnumerator Decided()
    {
        Debug.Log("TurnPhaseBehaviour Decided run..");

        // 戦術決定の理由を初期化。
        Tacts["P"].brain.text = "";
        Tacts["E"].brain.text = "";

        // ユニゾンなっているなら「ユニゾン」
        if (unizonE)
        {
            TactE.type = 0;

            // 自分がユニゾンでないが、プレイヤーがユニゾンなら「慎重」
        }
        else if (unizonP)
        {
            TactE.type = 2;

            // 双方ユニゾンでないなら思考ルーチンに決めてもらう。
        }
        else
        {
            EnemyBrain.Instance.DecideTact();
        }

        // 統計値更新。
        BattleBehaviour.Instance.statTact["P" + TactP.type]++;
        BattleBehaviour.Instance.statTact["E" + TactE.type]++;

        // 戦術表示へ。
        StartCoroutine(ShowTact());

        yield return null;
    }

    /// <summary>
    /// 戦術表示＆ユニゾンエフェクト部分を再生する。
    /// </summary>
    /// <returns></returns>
    IEnumerator ShowTact()
    {
        Debug.Log("TurnPhaseBehaviour ShowTact run..");

        // プレイヤーサイドと相手サイドで行う。
        for (int m = 0; m < 2; m++)
        {
            string side = (m == 0) ? "P" : "E";

            // 戦術を点滅で表示
            Tacts[side].TackInit();
            Tacts[side].gameObject.SetActive(true);
            Tacts[side].Blink();

            // ユニゾンである場合は、カードにユニゾンエフェクトを再生させる。
            if (Tacts[side].type == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    StartCoroutine(Cards[side + i].PlayAnim("CardUnizon", null));
                }
            }
        }

        // 戦術選択の理由セット。
        NaviBehaviour.Instance.setText(PlayerSelect.Instance.reasontext);
        NaviBehaviour.Instance.setSide("P");
        NaviBehaviour.Instance.Show(2);

        // 突合に移るまでのウェイトを設定。
        float waitCount = 0.8f;
        yield return new WaitForSeconds(waitCount);

        // 点滅表示していた戦術を戻す。
        TactP.Normal();
        TactE.Normal();

        // 突合用のフレームへ。
        StartCoroutine(Front());
    }

    int frontNo { get; set; }
    int _front_count { get; set; }
    int _front_conf_count { get; set; }

    /// <summary>
    /// カードの突合を開始する。
    /// 何番のカードの突合処理を行うのかは frontNo に格納されている。
    /// </summary>
    /// <returns></returns>
    IEnumerator Front()
    {
        Debug.Log("TurnPhaseBehaviour Front run..");

        //power非表示
        foreach (KeyValuePair<int, PowerBehaviour> _p in Powers)
        {
            _p.Value.gameObject.SetActive(false);
        }

        _front_count = 0;
        _front_conf_count = 0;

        // 突合番号を初期化。
        frontNo = 0;

        // 突合制御オブジェクトに開始命令を出す。
        for (int i = 0; i < 3; i++)
        {
            Fronts["P" + i].FrontStart(FrontEnd, FrontConfrict);
            Fronts["E" + i].FrontStart(FrontEnd, FrontConfrict);
        }

        yield return null;
    }

    /// <summary>
    /// 衝突音を鳴らす
    /// </summary>
    /// <param name="name"></param>
    void FrontConfrict(string name)
    {
        _front_conf_count++;

        if (_front_conf_count == 1)
        {
            AudioManager.Instance.PlaySE("se_airassaultdown");
        }
    }

    /// <summary>
    /// FrontStart終了コールバック。
    /// </summary>
    /// <param name="name"></param>
    void FrontEnd(string name)
    {
        Debug.Log("TurnPhaseBehaviour FrontEnd name = " + name);
        _front_count++;

        if (_front_count == 6)
        {
            // 突合用のフレームへ。
            StartCoroutine(Front2());
        }
    }

    /// <summary>
    /// 突合第二フェーズ。1ペアずつ攻撃させていく。
    /// </summary>
    /// <returns></returns>
    IEnumerator Front2()
    {
        Debug.Log("TurnPhaseBehaviour Front2 run..");
        while (true)
        {
            if (frontNo >= 3 || HpGaugeBehaviour.Instance.HpInfo["P"].value <= 0 || HpGaugeBehaviour.Instance.HpInfo["E"].value <= 0)
            {
                // 3ペアの突合が終わった、あるいはどちらかのHPがなくなったなら、待機フェーズへ。

                // まだ打ち出してないものは終了状態にする。
                for (; frontNo < 3; frontNo++)
                {
                    Fronts["P" + frontNo].motion = false;
                    Fronts["E" + frontNo].motion = false;
                }

                // 待機フレームへ。
                StartCoroutine(Front3());
                yield break;
            }
            else
            {
                // まだ終わっていないなら、指定ペアの突合を行う。

                // 戦闘統計更新
                BattleBehaviour.Instance.stat["N"].attCnt["P"]++;
                BattleBehaviour.Instance.stat["N"].attCnt["E"]++;

                // 突合制御オブジェクトに第二フェーズ開始の命令を出す。
                Fronts["P" + frontNo].fire(null);
                Fronts["E" + frontNo].fire(null);

                // 一定時間待機
                float wait = 0.36f;
                yield return new WaitForSeconds(wait);

                frontNo++;
            }

            yield return new WaitForSeconds(Main.Instance.getParFrame());
        }
    }

    bool end { set; get; } = true;

    /// <summary>
    /// すべての突合処理が完了するまで待つ。
    /// </summary>
    /// <returns></returns>
    IEnumerator Front3()
    {
        Debug.Log("TurnPhaseBehaviour Front3 run..");

        while (true)
        {
            end = true;

            // すべての突合第一フェーズが終わったかチェック。
            for (int i = 0; i < 3; i++)
            {
                if (Fronts["P" + i].motion || Fronts["E" + i].motion)
                {
                    end = false;
                    break;
                }
            }

            // すべて終わったのならこのフェーズは終了。
            if (end)
            {
                // 戦術表示を非表示に。
                TactP.gameObject.SetActive(false);
                TactE.gameObject.SetActive(false);

                //
                // ターン終了時、ナビに喋らせる必要がある場合にココにくる。

                //チュートリアルの場合でターン終了の時はナビがしゃべる
                if (BattleBehaviour.Instance.Param.tutorial)
                {
                    string[] speak = null;
                    if (InfoBehaviour.Instance.turnValue == 1)
                    {
                        speak = battle.tutUni;
                    }
                    else
                    {
                        speak = battle.tutStar;
                    }

                    StartCoroutine(BattleBehaviour.Instance.naviSpeaks(speak, (() =>
                    {
                        // 親ムービーに通知して、終了。
                        Debug.Log("tuenphase call progend...");
                        if (InfoBehaviour.Instance.turnValue != 1)
                        {

                            // スターの説明中、最後のメッセージでスターを追加して、
                            // リベンジの条件を満たすようにする。
                            while (MainPhaseBehaviour.Instance.objstarStoreP.value < BattleBehaviour.REVENGE_REQUIRED_NUM)
                            {
                                int pushType = (MainPhaseBehaviour.Instance.objstarStoreP.value % 3) + 1;
                                MainPhaseBehaviour.Instance.StarPush("P", pushType, 1);
                            }
                        }

                        MainPhaseBehaviour.Instance.ProgEnd();
                    })));
                }
                else
                {
                    // 親ムービーに通知して、終了。
                    Debug.Log("tuenphase call progend...");
                    MainPhaseBehaviour.Instance.ProgEnd();
                }

                yield break;
            }

            yield return new WaitForSeconds(Main.Instance.getParFrame());
        }
    }

}
