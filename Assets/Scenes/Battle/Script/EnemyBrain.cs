using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ターンフェーズでの相手側の戦術決定を思考するムービー
/// </summary>
public class EnemyBrain : MonoBehaviour
{

    // 統計情報初期化。
    Dictionary<int, int> StatisType { get; set; } = new Dictionary<int, int>();

    public static EnemyBrain Instance
    {
        get
        {
            return instance;
        }
    }

    private static EnemyBrain instance;

    void Start()
    {
        instance = this;
        StatisType[0] = 0;
        StatisType[1] = 0;
        StatisType[2] = 0;
        StatisType[3] = 0;
        StatisType[4] = 0;
    }


    // 敵側戦術を思考・決定する。
    // ../tactP/:type にプレイヤー側の戦術が格納されている必要がある。
    // ../tactE/:type に結果が直接格納される。
    // 思考決定の理由が ../:brainV に格納される。
    public void DecideTact()
    {
        jsonBattle battle = BattleBehaviour.Instance.battle;

        // プレイヤーの統計更新(戦術ごとの選択数)
        StatisType[TurnPhaseBehaviour.Instance.TactP.type]++;

        // 初期化。
        TurnPhaseBehaviour.Instance.TactE.type = 0;

        // 思考Lv100なら、30%の確率で後だし方式で裏をかく。
        float randValue = BattleBehaviour.Instance.randomEx.Value();

        if (battle.enemyBrainLv >= 100 && randValue < 0.30)
        {
            TurnPhaseBehaviour.Instance.TactE.type = TurnPhaseBehaviour.Instance.TactP.type - 1;

            if (TurnPhaseBehaviour.Instance.TactE.type == 0) TurnPhaseBehaviour.Instance.TactE.type = 3;
            TurnPhaseBehaviour.Instance.TactE.brain.text = "mindreading";
        }

        // まだ決まっておらず、思考Lv30以上なら偏向裏かきを試みる。
        if (TurnPhaseBehaviour.Instance.TactE.type == 0 && battle.enemyBrainLv >= 30)
            FlustPartial();

        // まだ決まっていないなら...
        if (TurnPhaseBehaviour.Instance.TactE.type == 0)
        {

            // ランダム選択を行う。
            randValue = BattleBehaviour.Instance.randomEx.Value();
            TurnPhaseBehaviour.Instance.TactE.type = (int)(randValue * 3 + 1);

            // ブレインレベル50以上なら状況補正を行う。
            if (battle.enemyBrainLv >= 50)
                Revise();
        }

        // 念のため...
        if (TurnPhaseBehaviour.Instance.TactE.type <= 0 || 4 <= TurnPhaseBehaviour.Instance.TactE.type)
            TurnPhaseBehaviour.Instance.TactE.type = 2;

        // 敵はチュートリアルではつねに慎重を選択
        if (BattleBehaviour.Instance.Param.tutorial)
            TurnPhaseBehaviour.Instance.TactE.type = 2;

    }

    // 統計を元に偏向選択しているプレイヤーの裏を掻こうとする戦術決定ルーチン。
    // 決まらない場合もある。
    // 決定結果は ../tactE/:type に直接格納される。
    void FlustPartial()
    {
        jsonBattle battle = BattleBehaviour.Instance.battle;

        // サンプル数を取得。
        int sampCnt = StatisType[1] + StatisType[2] + StatisType[3];

        // サンプル数3以上で実行する。ただし3の場合、50%の確率でパスする。
        float randValue = BattleBehaviour.Instance.randomEx.Value();
        if (sampCnt >= 4 || (sampCnt == 3 && randValue < 0.5))
        {

            // 各戦術の選択率を見ていく。
            for (int i = 1; i <= 3; i++)
            {

                // 選択率が70%超ならその裏を掻く。
                if (StatisType[i] / sampCnt > 0.7)
                {
                    TurnPhaseBehaviour.Instance.TactE.type = (i - 1 == 0) ? 3 : i - 1;
                    break;
                }
            }
        }
    }

    // ../tactE/:type に格納された戦術を、状況を見て補正する。
    void Revise()
    {
        jsonBattle battle = BattleBehaviour.Instance.battle;

        // 吸収を選択している場合に...
        if (TurnPhaseBehaviour.Instance.TactE.type == 3)
        {
            // 初期化。
            bool repick = false;

            // スピードが大きく負けているなら吸収は選択しない。
            if (battle.spdRate > 0.66)
                repick = true;

            // カードの相性を取得。有利なら吸収は選択しない。
            int adv = GetAdv();

            if (adv == +1)
                repick = true;

            // 再選定するならする。
            if (repick)
            {
                float randValue = BattleBehaviour.Instance.randomEx.Value();
                TurnPhaseBehaviour.Instance.TactE.type = (int)(randValue * 2 + 1);
            }
        }
    }

    Dictionary<int, int> Votes { get; set; } = new Dictionary<int, int>();

    // カードの相性を変数 adv に設定する。
    // +1:2枚以上有利  0:同じ  -1:2枚以上不利  -2:バラ
    int GetAdv()
    {

        // 初期化。
        Votes[0] = 0;  // 有利な数。
        Votes[1] = 0;  // 同じ数。
        Votes[2] = 0;  // 不利な数。

        // 1ペアごとに見ていく。
        for (int i = 0; i < 3; i++)
        {

            // 敵側カードとプレイヤー側のカードの属性を取得。
            int sbjCard = TurnPhaseBehaviour.Instance.Cards["E" + i].type;
            int oppCard = TurnPhaseBehaviour.Instance.Cards["P" + i].type;

            // 指定側の有利・不利を変数 adv に求める。
            // 以下の式で 0:同じ 1:有利 2:不利 になる。
            int advan = sbjCard - oppCard;
            if (advan < 0) advan += 3;

            // 該当の相性を+1;
            Votes[advan]++;
        }

        // 2票以上獲得している相性があるならそれ。
        // ない(つまり全相性1票ずつ)ならバラ。
        int adv = -2;
        for (int i = 0; i <= 2; i++)
        {
            if (Votes[i] >= 2)
                adv = i;
        }

        if (adv == 2) adv = -1;

        return adv;

    }

}
