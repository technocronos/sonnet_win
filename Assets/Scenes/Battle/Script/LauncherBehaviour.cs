using Scenes.Common.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
// リベンジフェーズでの一つのカードの登場から発射までを制御する。
public class LauncherBehaviour : MonoBehaviour
{

    public CardBehaviour card;
    public ExplodeBehaviour explode;
    private Dictionary<string, float> HIT = new Dictionary<string, float>();

    public string side { get; set; }
    public string oppSide { get; set; }
    public int padNo { get; set; }
    public int restCount { get; set; }
    public bool explodeFlg { get; set; } = false;
    public bool fireFlg { get; set; } = false;
    public int fireWait { get; set; }
    float startX { get; set; }
    float startY { get; set; }
    float destX { get; set; }
    float destY { get; set; }
    int fireFrames { get; set; }
    double damage { get; set; }
    int frameCount { get; set; }

    MainPhaseBehaviour MainPhase { get; set; }
    BattleBehaviour Battle { get; set; }
    RevengePhaseBehaviour Revenge { get; set; }

    // カード登場から打ち出しまでの最大秒数(敵側発動時)。 2.5から変更
    float FIRE_MAX_SECS = (float)3.2 * (BattleBehaviour.FRAME_RATE / (BattleBehaviour.FRAME_RATE / 2));

    // 打ち出し時、X軸におけるカードの1フレーム当たりのスピード。
    int SPEED = 1200 / BattleBehaviour.FRAME_RATE;

    //
    // 発射筒の動作を最初から行うタイミングでcallされる。
    // 以下の変数がセットされている。
    //     side     リベンジを発動した側。"P"か"E"。
    //     oppSide  sideと反対の側。
    //     padNo    発射台番号。
    // その他の必要な情報は親から取得する。
    public void LaunchStart()
    {
        Debug.Log("LauncherBehaviour LaunchStart run..");

        MainPhase = MainPhaseBehaviour.Instance;
        Battle = BattleBehaviour.Instance;
        Revenge = RevengePhaseBehaviour.Instance;

        HIT["XP"] = BattleBehaviour.HIT_XP;
        HIT["YP"] = BattleBehaviour.HIT_YP;
        HIT["XE"] = BattleBehaviour.HIT_XE;
        HIT["YE"] = BattleBehaviour.HIT_YE;

        // 発動した側のスターストアへのパスを取得。
        // スターストアからカードを一枚取り出す。
        StarStoreBehaviour store = MainPhase.starStore[side].GetComponent<StarStoreBehaviour>();

        store.Pop();

        if (store.popType == 0)
        {
            // ありえないけど、もうないならすぐに終了

            int no = int.Parse(transform.name.Substring(6, 1));
            StartCoroutine(Revenge.launchFin(no));
            end();

        }
        else
        {
            // ちゃんと取り出せた場合。

            // 戦闘統計の更新。
            Battle.stat["R"].attCnt[side]++;

            // 出現位置セット
            Vector3 _launch = Revenge.getCirclePos(padNo);
            transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(_launch.x, _launch.y, 0);

            // カードの初期化。
            card.type = store.popType;
            card.CardInit();

            card.gameObject.SetActive(true);

            // メインステップ2で参照する割り込みフラグを初期化。
            explodeFlg = false;
            fireFlg = false;


            // 発射までの最大待機秒数を取得。
            // プレイヤー側発動時は2倍にする。
            float maxSecs;
            if (Battle.auto_flg == false)
                maxSecs = FIRE_MAX_SECS * (side == "P" ? 4 : 1);
            else
                maxSecs = FIRE_MAX_SECS;

            // 発射までの待機カウントをセット。
            // 変数fireWaitはeBrainも参照しているので、うかつに挙動を変えないこと。
            float randValue = BattleBehaviour.Instance.randomEx.Value();
            fireWait = (int)(maxSecs * (BattleBehaviour.FRAME_RATE / 2) * randValue) + 1;

            StartCoroutine(card.PlayAnim("CardAppear2", main));
        }
    }

    //
    // ステップ1。
    // カードの登場エフェクトが終わるまで待機。
    void main(string cardname)
    {

        Debug.Log("LauncherBehaviour main run..");

        if (Revenge.stopFlg)
        {
            // 親ムービーから中断命令が出ていないか監視しておく。
            end();
        }
        else
        {
            // カードの登場エフェクトが終わったらステップ2へ。
            StartCoroutine(mainStep2());
        }
    }

    //
    // ステップ2。
    // 発射まで待機＆割り込みの処理を行う。
    IEnumerator mainStep2()
    {

        Debug.Log("LauncherBehaviour mainStep2 run..");

        while (true)
        {
            yield return new WaitForSeconds(Main.Instance.getParFrame());

            Vector3 _launch = transform.GetComponent<RectTransform>().anchoredPosition;
            Rect _square = Revenge.Square.GetComponent<RectTransform>().rect;

            if (Revenge.stopFlg)
            {
                // 親ムービーから中断命令が出ていないか監視しておく。
                end();
                yield break;
            }
            else if (explodeFlg)
            {
                // 割り込みの爆破フラグがONになったら。

                // 発射準備フェーズは終わったことを示す。
                fireWait = -1;

                // 爆破ムービーをスタート。
                explode.ExprodeStart(mainStep3);

                yield break;
            }
            else if (fireFlg || --fireWait < 0)
            {
                // 割り込みの発射フラグがONになる、あるいは待機カウントが0になったら。

                // 発射準備フェーズは終わったことを示す。
                fireWait = -1;

                // 現在の位置をスタート地点として保持。
                startX = _launch.x;
                startY = _launch.y;

                // リベンジの非発動側を到達座標としてセットする。
                destX = HIT["X" + oppSide];
                destY = HIT["Y" + oppSide];

                // startX と destX の差を見て、打ち出しにかける時間を計算する。
                fireFrames = (int)(Mathf.Abs(destX - startX) / SPEED);

                // カードを攻撃モードに。
                StartCoroutine(card.PlayAnim("CardMinimize" + card.type, null));

                // ダメージを計算しておく。
                this.calcDam();

                // ステップ4へ。
                frameCount = 0;
                runMainStep4 = true;

                yield break;

            }
        }
    }

    //
    // ステップ3。
    // カードが途中で爆破された場合に、爆破ムービーが終わるまで待機する。
    void mainStep3()
    {

        Debug.Log("LauncherBehaviour mainStep3 run..");

        card.gameObject.SetActive(false);

        if (Revenge.stopFlg)
        {
            // 親ムービーから中断命令が出ていないか監視しておく。
            end();
        }
        else
        {
            // 爆破ムービーが終わったら、この発射動作は終了。
            int no = int.Parse(transform.name.Substring(6, 1));
            StartCoroutine(Revenge.launchFin(no));
        }
    }

    bool runMainStep4 = false;

    private void FixedUpdate()
    {
        if (runMainStep4)
            StartCoroutine(mainStep4());
    }

    //
    // ステップ4。
    // カードの打ち出しポイントから着弾ポイントまでの移動を行う。
    IEnumerator mainStep4()
    {
        frameCount++;

        // まだ移動の途中である場合。
        if (frameCount <= fireFrames)
        {

            // 進行率を計算。
            float p = (float)frameCount / fireFrames;

            // 進行率にしたがって位置を移動。
            float _x = startX + (destX - startX) * p;
            float _y = startY + (destY - startY) * p * p * p;

            transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(_x, _y, 0);
        }
        else
        {
            // 移動が完了したら。
            Debug.Log("LauncherBehaviour mainStep4 run..");

            // ダメージ処理。
            Battle.side = oppSide;
            Battle.type = card.type;
            Battle.value = (int)damage;
            Battle.dir = -1;
            Battle.way = "R";
            Battle.Damage();

            // 停止処理。
            end();

            // 親ムービーの完了ラベルをcall。
            int no = int.Parse(transform.name.Substring(6, 1));
            StartCoroutine(Revenge.launchFin(no));

            runMainStep4 = false;
            yield break;
        }

        yield return new WaitForSeconds(Main.Instance.getParFrame());
    }

    //
    // 活動を停止したときにcallされるラベル。
    void end()
    {

        Debug.Log("LauncherBehaviour end run..");

        card.gameObject.SetActive(false);
        fireWait = -1;
    }

    //
    // リベンジのダメージを計算して変数 damage にセットする。
    void calcDam()
    {
        Debug.Log("LauncherBehaviour calcDam run..");

        // 双方の該当属性の攻撃力を取得。非発動側から75%、発動側から25%を取得して攻撃力とする。        
        int att = (int)(Battle.att[card.type + oppSide] * 0.75 + Battle.att[card.type + side] * 0.25);

        // 非発動側の該当属性の守備力を取得してセット。
        int def = Battle.def[card.type + oppSide];

        // 発動側のレベルを取得してセット。
        int level = Battle.lv[side];

        // その他をセット。
        int attTact = -1;
        int defTact = -1;

        // ダメージ計算。
        damage = BattleFuncs.CalcDam(att, def, level, attTact, defTact);
    }

}
