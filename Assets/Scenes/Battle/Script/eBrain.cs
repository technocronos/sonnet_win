using Scenes.Common.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
// 敵がリベンジを発動した場合のプレイヤー側思考を行うムービー。
public class eBrain : MonoBehaviour
{
    // ブレインLv0の場合の、ビーム発射までの反射時間。
    double REFLEX_SECS { get; set; }
    int reflexCount { get; set; }
    public int launcher { get; set; }
    int target { get; set; }
    int targetNo { get; set; }
    int chargeCount { get; set; }
    int igniteFrames { get; set; }
    int nextFireFrames { get; set; }

    public static eBrain Instance
    {
        get
        {
            return instance;
        }
    }

    private static eBrain instance;

    void Start()
    {
        instance = this;
    }


    //
    // 思考を開始すべきタイミングでgotoされる。
    public void run()
    {
        Debug.Log("eBrain run run..");
        // ビームを発射してから次のビームを発射できるまでの
        // フレーム数(つまりビームのモーション全体の長さ)を取得。
        BeamBehaviour.Instance.SetSide("E");
        float result = BeamBehaviour.Instance.mtnFrms();

        nextFireFrames = (int)(Mathf.Ceil(result));

        // ビームを発射してから、起爆反応が生ずるまでのフレーム数。
        igniteFrames = (int)(Mathf.Ceil(result * 0.8f));

        // カウンタ初期化。
        reflexCount = 0;    // ターゲットを設定してからビームを発射するまでの待機カウント
        chargeCount = 0;    // 次のビームを発射できるようになるまでの予測カウント

        ActRunStep2 = true;
    }

    bool ActRunStep2 = false;

    private void FixedUpdate()
    {
        StartCoroutine(RunStep2());
    }

    //
    // 思考中は毎フレーム実行される。
    IEnumerator RunStep2()
    {
        if (ActRunStep2)
        {
            Debug.Log("eBrain RunStep2 run..");
            yield return new WaitForSeconds(Settings.PAR_FRAME);

            if (RevengePhaseBehaviour.Instance.stopFlg)
            {
                // リベンジフェーズが終了を迎えているなら停止。
                yield break;
            }
            else
            {
                // まだ終了していないなら処理する。

                // 次に迎撃可能になるまでの時間を更新。
                if (chargeCount > 0) chargeCount--;

                // 反射待ちの場合に以下を行う。
                if (reflexCount > 0)
                {

                    // 通常中断判定。
                    // ブレインLv50以上、かつ、ターゲットがすでに発射準備でなくなった場合。
                    if (BattleBehaviour.Instance.battle.enemyBrainLv >= 50 && RevengePhaseBehaviour.Instance.Launch[target].fireWait < 0)
                    {

                        // 迎撃準備はキャンセル。
                        reflexCount = 0;

                        // 新たなターゲットを検索する。
                        this.search();

                        // 反射カウントダウン。0になったら。
                    }
                    else if (--reflexCount == 0)
                    {

                        // ターゲットの発射台に向けてビーム発射。
                        StartCoroutine(RevengePhaseBehaviour.Instance.intercept(targetNo));

                        // 次にビームを発射できるまでのフレーム数をリセット。
                        chargeCount = nextFireFrames;

                        // 新たなターゲットを検索する。
                        this.search();
                    }
                }
            }
        }
    }

    //
    // 発射筒がセットされたときにcallされるラベル。
    // 変数 launcher にセットされた発射筒へのパスが格納されている。
    public void notify()
    {

        Debug.Log("eBrain notify run..");
        // まだターゲッティングしていないなら処理する。
        if (reflexCount <= 0)
        {

            // ブレインLv100以上ならエスパースルー判定。
            // 発射までの時間が、次に迎撃可能な時間を下回っているものはスルー。
            if (!(BattleBehaviour.Instance.battle.enemyBrainLv >= 100 && RevengePhaseBehaviour.Instance.Launch[launcher].fireWait < chargeCount + igniteFrames))
            {

                // エスパースルーしないならターゲットを設定。
                target = launcher;
                setTarget();
            }
        }

    }

    /// <summary>
    // 変数targetで示された発射筒をターゲットとして設定する。
    // 迎撃ビームを打ち込む発射台番号を保持する。
    /// </summary>
    void setTarget()
    {
        Debug.Log("eBrain setTarget run..");

        // ブレインLv0の場合の、ビーム発射までの反射時間。
        REFLEX_SECS = 1.0 * (100 - BattleBehaviour.Instance.battle.enemyBrainLv) / 100;

        targetNo = RevengePhaseBehaviour.Instance.Launch[target].padNo;

        // 何フレーム後に迎撃するかを設定。
        reflexCount = (int)(REFLEX_SECS * (BattleBehaviour.FRAME_RATE / 2)) + 1;

        // ただし、次にビーム発射可能になるフレーム数より小さい場合は補正する。
        if (reflexCount < chargeCount)
            reflexCount = chargeCount + (int)(reflexCount / 2);
    }

    /// <summary>
    // 発射準備している発射筒を探して、ターゲットを設定する。
    // ただし、変数 target で示されている発射筒は除外する。
    /// </summary>
    void search()
    {

        Debug.Log("eBrain search run..");

        // 親ムービーの発射筒をすべてスキャン。
        for (int i = 0; i < RevengePhaseBehaviour.Instance.LAUNCHER_NUM; i++)
        {

            // とりあえずパスを取得。
            int lnc = i;

            // 変数 target で示されている発射筒は無視。
            if (lnc == target)
                continue;

            // 発射準備していない発射筒は無視。
            if (RevengePhaseBehaviour.Instance.Launch[lnc].fireWait < 0)
                continue;

            // ブレインLv100以上ならエスパースルー判定。
            // 発射までの時間が、次に迎撃可能な時間を下回っているものはスルー。
            if (BattleBehaviour.Instance.battle.enemyBrainLv >= 100 && (RevengePhaseBehaviour.Instance.Launch[lnc].fireWait < chargeCount + igniteFrames))
            {
                continue;
            }

            //発射筒に番号がセットされていない
            if (RevengePhaseBehaviour.Instance.Launch[lnc].padNo == 0)
                continue;

            // ここまでくればそれは発射準備している。
            target = lnc;
            setTarget();
            break;
        }

    }

}
