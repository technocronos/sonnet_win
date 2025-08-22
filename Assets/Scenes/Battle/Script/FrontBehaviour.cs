using DG.Tweening;
using Scenes.Common.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
// 突合時のカードの動き等を制御するオブジェクト。
public class FrontBehaviour : MonoBehaviour
{
    public delegate void OnCompleteDelegate(string name);
    public OnCompleteDelegate CompleteHandler;
    public OnCompleteDelegate CompleteHandler2;

    // 定数いろいろ

    // スターストアに向かうときの目標座標。
    private Dictionary<string, float> SS { get; set; } = new Dictionary<string, float>();
    private Dictionary<string, float> HIT { get; set; } = new Dictionary<string, float>();

    private float STEP1_GOAL_X { get; set; } = -44;          // ステップ1カード突合時におけるカードX座標目標(プレイヤー側)。
    private float STEP1_ACCEL { get; set; } = 4.8f;          // ステップ1におけるカード加速度(プレイヤー側)。
    private float STEP1_INIT_SPEED { get; set; } = -5.0f;    // ステップ1におけるカード初期速度(プレイヤー側)。
    private float STEP2_FRAMES { get; set; } = BattleBehaviour.FRAME_RATE / 4.8f; //5;           // ステップ2で何フレームでカードを元の位置に戻すか。
    private float ATT_FRAMES { get; set; } = BattleBehaviour.FRAME_RATE / 6; //4;             // カードを相手に打ち込む時、何フレームで処理するか。
    private float TEAR_FRAMES { get; set; } = BattleBehaviour.FRAME_RATE / 3.4f; //7;            // 破れたカードがスターストアに移動する時、何フレームで処理するか。
    private float ABSORB_FRAMES { get; set; } = BattleBehaviour.FRAME_RATE / 3.4f; //7;          // 吸収されたカードがスターストアに移動する時、何フレームで処理するか。


    public GameObject StarPanel;
    public GameObject StarStoreP;
    public GameObject StarStoreE;
    public GameObject starDust0;
    public GameObject starDust1;
    public GameObject starDust2;

    private Dictionary<int, GameObject> starDusts = new Dictionary<int, GameObject>();

    public CardBehaviour CardP0;
    public CardBehaviour CardP1;
    public CardBehaviour CardP2;

    public CardBehaviour CardE0;
    public CardBehaviour CardE1;
    public CardBehaviour CardE2;

    public GameObject confront;

    private Dictionary<string, CardBehaviour> Cards = new Dictionary<string, CardBehaviour>();

    public bool motion { get; set; } = false;
    private string oppSide { get; set; }
    //private string side { get; set; } = "P";

    float speed { get; set; }
    float accel { get; set; }
    float goal { get; set; }

    bool tear { get; set; }

    public int row { get; set; }
    public string side { set; get; }

    float startX { get; set; }
    float startY { get; set; }
    float destX { get; set; }
    float destY { get; set; }

    Vector3 cardpos { get; set; }

    float dustFrames { get; set; }
    bool absorb { get; set; } = false;
    int damage { get; set; }

    bool flow_sound_flg { get; set; } = false;

    int frameCount { get; set; } = 0;

    // Start is called before the first frame update
    public static FrontBehaviour Instance
    {
        get
        {
            return instance;
        }
    }

    private static FrontBehaviour instance;

    private void Start()
    {
        instance = this;

        starDusts[0] = starDust0;
        starDusts[1] = starDust1;
        starDusts[2] = starDust2;

        Cards["P" + 0] = CardP0;
        Cards["P" + 1] = CardP1;
        Cards["P" + 2] = CardP2;
        Cards["E" + 0] = CardE0;
        Cards["E" + 1] = CardE1;
        Cards["E" + 2] = CardE2;

        //スターの目的地
        SS["XP"] = StarStoreP.transform.GetComponent<RectTransform>().anchoredPosition.x;
        SS["YP"] = StarPanel.transform.GetComponent<RectTransform>().anchoredPosition.y;
        SS["XE"] = StarStoreE.transform.GetComponent<RectTransform>().anchoredPosition.x;
        SS["YE"] = StarPanel.transform.GetComponent<RectTransform>().anchoredPosition.y;

        HIT["XP"] = BattleBehaviour.HIT_XP;
        HIT["YP"] = BattleBehaviour.HIT_YP;
        HIT["XE"] = BattleBehaviour.HIT_XE;
        HIT["YE"] = BattleBehaviour.HIT_YE;

        // スターダストを非表示にする。
        for (int i = 0; i < 3; i++)
        {
            starDusts[i].SetActive(false);
        }

        //座標を中央に合わせる。
        transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
    }

    CardBehaviour card { set; get; }

    /// <summary>
    /// 突合処理を開始する。
    /// </summary>
    public void FrontStart(OnCompleteDelegate _callback, OnCompleteDelegate _callback2)
    {
        Debug.Log("FrontBehaviour FrontStart run..");

        if (_callback != null)
        {
            CompleteHandler += _callback;
        }
        if (_callback2 != null)
        {
            CompleteHandler2 += _callback2;
        }

        // 制御中であることを示すフラグをONに。
        motion = true;

        // 変数 side で指定されている側の反対を取得。
        if (side == "P")
            oppSide = "E";
        else
            oppSide = "P";

        // カードを取得。
        card = Cards[side + row];

        // 突合の成り行きを計算する。(tear, absorb, damage の各変数がセットされる。)
        this.Course();

        // 突合ステップ1として初期化。
        if (side == "P")
        {
            speed = STEP1_INIT_SPEED;
            accel = STEP1_ACCEL;
            goal = STEP1_GOAL_X;
        }
        else
        {
            speed = STEP1_INIT_SPEED * -1;
            accel = STEP1_ACCEL * -1;
            goal = STEP1_GOAL_X * -1;
        }

        this.FrontStep1();
    }

    void FrontStep1()
    {
        cardpos = card.GetComponent<RectTransform>().anchoredPosition;

        Vector3[] path = {
            new Vector3(cardpos.x, cardpos.y, 0f),
            new Vector3(goal, cardpos.y, 0f),
        };

        card.GetComponent<RectTransform>().DOAnchorPos(new Vector3(goal, cardpos.y, 0f), 0.1f).SetEase(Ease.Linear).OnComplete(() => {
            //衝突を親へ通知
            if (CompleteHandler2 != null)
            {
                CompleteHandler2?.Invoke(transform.name);
                CompleteHandler2 = null;
            }

            if (side == "P")
            {
                //衝突エフェクト
                GameObject objConfront = UnityEngine.Object.Instantiate(confront, new Vector3(0, 0, 0), Quaternion.identity, transform.parent);
                objConfront.SetActive(true);
                //ポジションを設定する
                objConfront.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, cardpos.y, 0);
                //アニメ再生
                StartCoroutine(objConfront.GetComponent<ConfrontBehaviour>().PlayAnim());
            }

            // 破れている場合は縮小を開始。
            if (tear)
            {
                StartCoroutine(card.GetComponent<CardBehaviour>().PlayAnim("CardBreak", null));
            }
            else
            {
                StartCoroutine(card.GetComponent<CardBehaviour>().PlayAnim("CardAttack" + card.type, null));
            }

            // ステップ2へ。
            frameCount = 0;
            StartCoroutine(FrontStep2());
        });
    }

    /// <summary>
    /// ステップ2の間は1フレームごとに実行される。
    /// カードを元の位置に戻す。
    /// </summary>
    /// <returns></returns>
    IEnumerator FrontStep2()
    {
        Debug.Log("FrontBehaviour FrontStep2 run..");

        while (true)
        {

            frameCount++;

            // まだステップ2の途中である場合。
            // 経過フレーム数にしたがって、カードを元の位置に戻す。
            if (frameCount <= STEP2_FRAMES)
            {
                cardpos = card.GetComponent<RectTransform>().anchoredPosition;

                // プレイヤー側における計算を行う。
                float x = STEP1_GOAL_X + ((TurnPhaseBehaviour.CARD_REGULAR_X - STEP1_GOAL_X) * (frameCount / STEP2_FRAMES));

                // 敵側を制御している場合は、プレイヤー側の鏡対象の位置に置く。    
                card.GetComponent<RectTransform>().anchoredPosition = new Vector3(x * ((side == "P") ? 1 : -1), cardpos.y, 0);

            }
            else
            {
                // ステップ2が終わったら

                // 破れているかどうかで分岐する。破れている場合。
                if (tear)
                {
                    // 次のラベルで使用するため、初期化しておく。
                    frameCount = 0;

                    // カードは非表示に。
                    card.gameObject.SetActive(false);

                    // ユニゾンされて破れているなら、そのまま終わり。
                    if (TurnPhaseBehaviour.Instance.Tacts[oppSide].type == 0)
                    {
                        waitCount = 0.05f;

                        StartCoroutine(Last());
                    }
                    else
                    {
                        // ユニゾンではないなら、スターになる。
                        Vector3 _cardv = card.transform.GetComponent<RectTransform>().anchoredPosition;

                        // スターダストのスタート地点と目標地点を格納。
                        startX = _cardv.x;
                        startY = _cardv.y;
                        destX = SS["X" + side];
                        destY = SS["Y" + side];

                        // dustラベルへ。
                        dustFrames = TEAR_FRAMES;
                        RunDust = true;
                    }

                }
                else
                {
                    // 破れていないならいったん止める。
                    Debug.Log("FrontBehaviour FrontStep2 破れていないならいったん止める..");

                    StartCoroutine(Last());

                }

                yield break;
            }

            //1フレーム進める
            yield return new WaitForSeconds(Main.Instance.getParFrame() * 2f);

        }
    }

    /// <summary>
    /// カードを設定された目標地点に移動させる。
    /// </summary>
    /// <returns></returns>
    IEnumerator Att()
    {
        Debug.Log("FrontBehaviour Att run..");

        // カードを攻撃モードに。
        StartCoroutine(card.PlayAnim("CardMinimize" + card.type, null));

        while (true)
        {
            frameCount++;

            // まだステップの途中である場合。
            if (frameCount <= ATT_FRAMES)
            {

                // 進行率を計算。
                float p = (float)frameCount / ATT_FRAMES;

                // 進行率にしたがってカードの位置を移動。
                float _x = startX + (destX - startX) * p;
                float _y = startY + (destY - startY) * p * p * p;

                card.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(_x, _y, 0);

            }
            else
            {
                // ステップが終わったら。
                AttEnd();
                yield break;
            }

            yield return new WaitForSeconds(Main.Instance.getParFrame() * 2.5f);

        }
    }

    // ステップが終わったら。
    void AttEnd()
    {
        // カードは非表示に。
        card.gameObject.SetActive(false);

        if (!motion) return;

        if (absorb)
        {
            // 吸収された場合。
            Vector3 _cardv = card.transform.GetComponent<RectTransform>().anchoredPosition;

            // スターダストのスタート地点と目標地点を格納。
            startX = _cardv.x + 10;
            startY = _cardv.y;
            destX = SS["X" + oppSide];
            destY = SS["Y" + oppSide];

            // dustラベルへ。
            frameCount = 0;
            dustFrames = ABSORB_FRAMES;

            RunDust = true;
        }
        else
        {
            // 吸収されずに打ち込めた場合。

            // ダメージ処理。
            BattleBehaviour.Instance.side = oppSide;
            BattleBehaviour.Instance.type = card.GetComponent<CardBehaviour>().type;
            BattleBehaviour.Instance.value = damage;
            BattleBehaviour.Instance.dir = row;
            BattleBehaviour.Instance.way = "N";
            BattleBehaviour.Instance.Damage();

            waitCount = 0.1f;

            StartCoroutine(Last());
        }
    }

    bool RunDust = false;

    private void FixedUpdate()
    {
        if (RunDust)
            StartCoroutine(Dust());
    }

    //
    // スター増加ステップの間、1フレームごとに実行される。
    // 設定されたスタート地点から目標地点へスターダストを移動させる。
    IEnumerator Dust()
    {
        Debug.Log("FrontBehaviour Dust run..");

        flow_sound_flg = false;

        frameCount++;

        // まだステップの途中である場合。
        if (frameCount <= dustFrames)
        {
            // スターダストは3つのムービーからなるので、一つずつ処理する。
            for (int i = 0; i < 3; i++)
            {
                if (frameCount - i > 0)
                {

                    if (flow_sound_flg == false)
                    {
                        if (frameCount == 3 && i == 1)
                        {
                            AudioManager.Instance.PlaySE("se_combo");
                            flow_sound_flg = true;
                        }
                    }

                    starDusts[i].SetActive(true);

                    // ステップ進行率を計算。
                    float p = (float)(frameCount - i) / dustFrames;

                    float _x = startX + (destX - startX) * p + i;
                    float _y = startY + (destY - startY) * p + i;

                    starDusts[i].transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(_x, _y, 0);
                }
            }
        }
        else
        {
            RunDust = false;
            DustEnd();

            yield break;
        }

        yield return new WaitForSeconds(Main.Instance.getParFrame() * 1.5f);

    }

    /// <summary>
    /// ステップが終わったら。
    /// </summary>
    void DustEnd()
    {
        // スターダストを非表示に。
        for (int i = 0; i < 3; i++)
            starDusts[i].SetActive(false);

        // カードをプッシュするスターストアを取得。
        // 破れてダストになったのか、吸収されてなったのかで決まる。
        string starStoreName = "starStore" + (tear ? side : oppSide);

        StarStoreBehaviour starStore = StarPanel.transform.Find(starStoreName).GetComponent<StarStoreBehaviour>();

        // スターストアにプッシュ。吸収なら3つプッシュする
        int pushType = card.GetComponent<CardBehaviour>().type;
        int num = tear ? 1 : 3;

        starStore.Push(pushType, num);

        waitCount = 0.1f;

        StartCoroutine(Last());
    }

    float waitCount { get; set; } = 0f;

    IEnumerator Last()
    {
        Debug.Log("FrontBehaviour Last run..");
        motion = false;

        // 一定時間待機して終了。
        yield return new WaitForSeconds(waitCount);

        if (CompleteHandler != null)
        {
            CompleteHandler?.Invoke(transform.name);
            CompleteHandler = null;
        }

        yield return null;
    }

    //
    // 突合の成り行きを算出するcallラベル。
    // 変数 side と oppSide が設定されている状態で呼ばれる。
    //
    // 結果は次の変数に格納される。
    //     tear     カードが破れる(スターになる)かどうか。
    //     absorb   吸収されるかどうか。tearがtrueなら、この値は常に false になる。
    //     damage   ダメージをいくつ与えるか。
    //              tearがtrue、あるいはabsorbがtrueなら、この値は常に 0 になる。
    void Course()
    {
        Debug.Log("FrontBehaviour Course run..");
        jsonBattle battle = BattleBehaviour.Instance.battle;

        // 双方のカードのタイプを取得。
        int cardS = Cards[side + row].type;
        int cardO = Cards[oppSide + row].type;

        // 属性の有利・不利を変数 advantage に求める。
        // 以下の式で 0:同じ 1:有利 2:不利 になる。
        int advantage = cardS - cardO;
        if (advantage < 0) advantage += 3;

        // 双方の戦術を取得。(0:ユニゾン 1:強攻 2:慎重 3:吸収)
        // 数字が大きければより防衛的な戦術を選んでいることに留意。
        int tactS = TurnPhaseBehaviour.Instance.Tacts[side].type;
        int tactO = TurnPhaseBehaviour.Instance.Tacts[oppSide].type;

        // tear フラグを求める。
        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

        switch (advantage)
        {

            // 同じ属性である場合。
            // 相手が"ユニゾン"か"強攻"、かつ、自分がそうでない場合は破れる。
            // それ以外では破れない。
            case 0:
                tear = (tactS >= 2 && tactO <= 1);
                break;

            // 有利な属性である場合、破れない。
            case 1:
                tear = false;
                break;

            // 不利な属性である場合。
            case 2:

                if (tactS == 0 && tactO != 0)
                {
                    // 自分のみがユニゾンしているなら耐える。
                    tear = false;

                }
                else if (tactS == 1 && tactO >= 2)
                {
                    // 自分が"強攻"で、相手が"慎重"か"吸収"である場合は、
                    // 相手とのLv差によって、一定確率で耐えられる。

                    // 相手とのレベル差を取得。
                    // ただし、絶対値が20以下になるようにする。(-20～+20)
                    float rate = BattleBehaviour.Instance.lv[side] - BattleBehaviour.Instance.lv[oppSide];
                    if (Mathf.Abs(rate) > 20) rate = (rate > 0 ? +1 : -1) * 20;

                    // +30して10～50にする。
                    rate = rate + 30;

                    // それを "耐えるパーセンテージ" として、破れるかどうかを決める。
                    float randValue = BattleBehaviour.Instance.randomEx.Value();
                    tear = (randValue >= rate / 100);

                }
                else
                {
                    // 戦術の条件を満たしていないなら破れる。
                    tear = true;
                }
                break;
        }

        // absorbを求める。
        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

        // カードが破れている、あるいは相手が"強攻"を選んでいるなら吸収はない。
        if (tear || tactO == 1)
        {
            absorb = false;

        }
        else
        {
            // 吸収される可能性がある場合。

            // 相手の視点で、スピード差のレートを取得(-1.0～+1.0)
            int spdRate = battle.spdRate * (oppSide == "P" ? +1 : -1);

            float rate;
            // 相手が"慎重"か"ユニゾン"なら吸収される確率は0%～18%。
            if (tactO == 2)
            {
                rate = 9 + (spdRate * 9);
            }
            // 相手が戦術"吸収"の場合は...
            else
            {
                // 自分の戦術が"強攻"なら40～100%、そうでないなら0%～18%。
                if (tactS == 1)
                    rate = 70 + (spdRate * 30);
                else
                    rate = 9 + (spdRate * 9);
            }

            // ランダムをとって、吸収できたか判断する。
            float randValue = BattleBehaviour.Instance.randomEx.Value();
            absorb = (randValue < rate / 100);

            // デバック用
            //trace("rate: " add /randGen/:value add "/" add (rate/100));
        }

        // ダメージを求める。
        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

        // カードが破れた、吸収されたなら、ダメージは 0。
        if (tear || absorb)
        {
            damage = 0;
        }
        // そうでない場合。
        else
        {
            // ダメージ計算に必要な要素を計算機にセット。
            int att = BattleBehaviour.Instance.att[cardS + side];
            int def = BattleBehaviour.Instance.def[cardS + oppSide];
            int level = BattleBehaviour.Instance.lv[side];
            int attTact = tactS;
            int defTact = tactO;

            //fscommand2("JavaScript", "showmsg", "lv_str=" add "/:Lv" add side);
            //fscommand2("JavaScript", "showmsg", "lv_value=" add eval("/:Lv" add side));

            // ダメージ計算。
            damage = (int)BattleFuncs.CalcDam(att, def, level, attTact, defTact);
        }
    }

    //
    // カードを相手に打ち込む。
    public void fire(OnCompleteDelegate _callback)
    {
        Debug.Log("FrontBehaviour fire run..");

        if (_callback != null)
        {
            CompleteHandler += _callback;
        }

        // 破れていないなら処理する。
        if (!tear)
        {
            // カードのスタート地点と目標地点を格納。
            Vector3 _cardv = card.transform.GetComponent<RectTransform>().anchoredPosition;
            startX = _cardv.x;
            startY = _cardv.y;
            destX = HIT["X" + oppSide];
            destY = HIT["Y" + oppSide];

            // "att" フレームへ。
            frameCount = 0;
            motion = true;
            StartCoroutine(Att());
        }
        else
        {
            if (CompleteHandler != null)
            {
                CompleteHandler?.Invoke(transform.name);
                CompleteHandler = null;
            }

        }

    }


}
