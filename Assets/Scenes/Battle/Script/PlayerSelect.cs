using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSelect : MonoBehaviour
{
    public static PlayerSelect Instance
    {
        get
        {
            return instance;
        }
    }

    private static PlayerSelect instance;

    void Start()
    {
        instance = this;
    }

    public string reasontext { get; set; } = "";

    // ユーザの戦術選択を開始する。
    public IEnumerator Init()
    {
        jsonBattle battle = BattleBehaviour.Instance.battle;

        if (BattleBehaviour.Instance.auto_flg)
        {
            player_brain();
        }
        else
        {

            // ナビゲータのセリフを戦術選択時のものにセット。
            NaviBehaviour.Instance.setText(Utility.getText("BATTLE_NAV_SEL_TACT"));
            NaviBehaviour.Instance.setSide("P");
            NaviBehaviour.Instance.Show(1);

            // 解説に数字キーと戦術の対応を表すメッセージをセット＆点滅。
            PreterBehaviour.Instance.Visible(true);
            PreterBehaviour.Instance.PlayAnim("blink_s");
        }

        yield return null;
    }

    // ユーザが戦術を決定したときにcallされるラベル。
    // 何を選択したかはすでに TurnPhaseBehaviour.Instance.TactP.type にセットされている。
    public void Decided()
    {
        jsonBattle battle = BattleBehaviour.Instance.battle;

        //戦術選択セリフ（マニュアルの場合）
        if (TurnPhaseBehaviour.Instance.TactP.type == 1)
            reasontext = battle.MANUAL_SERIFU_STRONG_ATTACK;
        else if (TurnPhaseBehaviour.Instance.TactP.type == 2)
            reasontext = battle.MANUAL_SERIFU_PRUDENCE;
        else if (TurnPhaseBehaviour.Instance.TactP.type == 3)
            reasontext = battle.MANUAL_SERIFU_ABSORPTION;

        // ナビゲータを非表示に。
        NaviBehaviour.Instance.Visible(false);

        // 解説を非表示に。
        PreterBehaviour.Instance.Visible(false);

        // ボタンを表示しないフレームに飛ぶ。
        //gotoAndStop("end");

        // 親の戦術決定用ラベルをcallする。
        StartCoroutine(TurnPhaseBehaviour.Instance.Decided());
    }

    Dictionary<int, int> StatisType { get; set; } = new Dictionary<int, int>();

    // ターンフェーズでの相手側の戦術決定を思考するムービー
    void player_brain()
    {
        // 統計情報初期化。
        StatisType[0] = 0;
        StatisType[1] = 0;
        StatisType[2] = 0;
        StatisType[3] = 0;
        StatisType[4] = 0;

        //戦術選択理由初期化
        reasontext = "";

        DecideTact();
    }

    // 敵側戦術を思考・決定する。
    // TurnPhaseBehaviour.Instance.TactP.type にプレイヤー側の戦術が格納されている必要がある。
    // TurnPhaseBehaviour.Instance.TactE.type に結果が直接格納される。
    // 思考決定の理由が ../:brainV に格納される。
    void DecideTact()
    {
        jsonBattle battle = BattleBehaviour.Instance.battle;

        // プレイヤーの統計更新(戦術ごとの選択数)
        StatisType[TurnPhaseBehaviour.Instance.TactE.type]++;

        // 初期化。
        TurnPhaseBehaviour.Instance.TactP.type = 0;

        // 思考Lv100なら、30%の確率で後だし方式で裏をかく。
        float randValue = BattleBehaviour.Instance.randomEx.Value();
        if (battle.playerBrainLv >= 100 && randValue < 0.30)
        {
            TurnPhaseBehaviour.Instance.TactP.type = TurnPhaseBehaviour.Instance.TactE.type - 1;
            if (TurnPhaseBehaviour.Instance.TactP.type == 0) TurnPhaseBehaviour.Instance.TactP.type = 3;
            TurnPhaseBehaviour.Instance.TactP.brain.text = "mindreading";
            reasontext = battle.AUTO_SERIFU_MIND_READING;
        }

        // まだ決まっておらず、思考Lv30以上なら偏向裏かきを試みる。
        if (TurnPhaseBehaviour.Instance.TactP.type == 0 && battle.playerBrainLv >= 30)
            FlustPartial();

        // まだ決まっていないなら...
        if (TurnPhaseBehaviour.Instance.TactP.type == 0)
        {
            // ランダム選択を行う。
            randValue = BattleBehaviour.Instance.randomEx.Value();
            TurnPhaseBehaviour.Instance.TactP.type = (int)(randValue * 3 + 1);
            if (TurnPhaseBehaviour.Instance.TactP.type == 1)
                reasontext = battle.AUTO_SERIFU_STRONG_ATTACK;

            else if (TurnPhaseBehaviour.Instance.TactP.type == 2)
                reasontext = battle.AUTO_SERIFU_PRUDENCE;

            else if (TurnPhaseBehaviour.Instance.TactP.type == 3)
                reasontext = battle.AUTO_SERIFU_ABSORPTION;

            // ブレインレベル50以上なら状況補正を行う。
            if (battle.playerBrainLv >= 50)
                Revise();
        }

        // 念のため...
        if (TurnPhaseBehaviour.Instance.TactP.type <= 0 || 4 <= TurnPhaseBehaviour.Instance.TactP.type)
            TurnPhaseBehaviour.Instance.TactP.type = 2;


        // ボタンを表示しないフレームに飛ぶ。
        //gotoAndStop("end");

        // 親の戦術決定用ラベルをcallする。
        StartCoroutine(TurnPhaseBehaviour.Instance.Decided());

    }

    // 統計を元に偏向選択しているプレイヤーの裏を掻こうとする戦術決定ルーチン。
    // 決まらない場合もある。
    // 決定結果は TurnPhaseBehaviour.Instance.TactE.type に直接格納される。
    void FlustPartial()
    {
        jsonBattle battle = BattleBehaviour.Instance.battle;

        // サンプル数を取得。
        int sampCnt = StatisType[1] + StatisType[2] + StatisType[3];

        // サンプル数3以上で実行する。ただし3の場合、50%の確率でパスする。
        float randValue = BattleBehaviour.Instance.randomEx.Value();
        if (sampCnt >= 4 || (sampCnt == 3 && randValue < 0.5))
        {

            string str = "";
            // 各戦術の選択率を見ていく。
            for (int i = 1; i <= 3; i++)
            {
                // 選択率が70%超ならその裏を掻く。
                if (StatisType[i] / sampCnt > 0.7)
                {
                    TurnPhaseBehaviour.Instance.TactP.type = (i - 1 == 0) ? 3 : i - 1;
                    if (i == 1)
                        str = Utility.getText("TEXT_TACTICS_1");
                    else if (i == 2)
                        str = Utility.getText("TEXT_TACTICS_2");
                    else
                        str = Utility.getText("TEXT_TACTICS_3");

                    reasontext = Utility.getText("TEXT_TACTICS_CHANGE1").Replace("{0}", str);
                    break;
                }
            }
        }
    }

    // TurnPhaseBehaviour.Instance.TactP.type に格納された戦術を、状況を見て補正する。
    void Revise()
    {
        jsonBattle battle = BattleBehaviour.Instance.battle;

        // 吸収を選択している場合に...
        if (TurnPhaseBehaviour.Instance.TactP.type == 3)
        {

            // 初期化。
            bool repick = false;

            // スピードが大きく負けているなら吸収は選択しない。
            if (battle.spdRate < -0.66)
            {
                repick = true;
                reasontext = battle.AUTO_SERIFU_NO_ABSORPTION_1;
            }

            // カードの相性を取得。有利なら吸収は選択しない。
            int adv = getAdv();

            if (adv == +1)
            {
                repick = true;
                reasontext = battle.AUTO_SERIFU_NO_ABSORPTION_2;
            }

            // 再選定するならする。
            if (repick)
            {
                float randValue = BattleBehaviour.Instance.randomEx.Value();
                TurnPhaseBehaviour.Instance.TactP.type = (int)(randValue * 2 + 1);
                if (TurnPhaseBehaviour.Instance.TactP.type == 1)
                    reasontext = reasontext + battle.AUTO_SERIFU_STRONG_ATTACK_DESIDE;
                else
                    reasontext = reasontext + battle.AUTO_SERIFU_PRUDENCE_DESIDE;
            }
        }
    }


    Dictionary<int, int> Votes { get; set; } = new Dictionary<int, int>();


    // カードの相性を変数 adv に設定する。
    // +1:2枚以上有利  0:同じ  -1:2枚以上不利  -2:バラ
    int getAdv()
    {
        // 初期化。
        Votes[0] = 0;  // 有利な数。
        Votes[1] = 0;  // 同じ数。
        Votes[2] = 0;  // 不利な数。

        // 1ペアごとに見ていく。
        for (int i = 0; i < 3; i++)
        {

            // 敵側カードとプレイヤー側のカードの属性を取得。
            int sbjCard = TurnPhaseBehaviour.Instance.Cards["P" + i].type;
            int oppCard = TurnPhaseBehaviour.Instance.Cards["E" + i].type;

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
