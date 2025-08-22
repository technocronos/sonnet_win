using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TechBehaviour : MonoBehaviour
{
    public StarStoreBehaviour starStoreP;
    public StarStoreBehaviour starStoreE;

    public string side { get; set; }
    public string opp { get; set; }
    public bool effect { get; set; }

    /// <summary>
    /// 0:何も無し
    /// 100:value2 で指定したターンの終了時、固定で value1 で指定したダメージを与える必殺技。
    /// 200:value2 で指定したターンの終了時、固定で value1 で指定した回復を行う必殺技。
    /// 300:value2 で指定したターンの終了時、固定で value1 で指定した量の吸収を行う必殺技。
    /// 400:value2 で指定したターンの終了時、固定で value1 で指定した量のスター追加を行う必殺技。
    /// </summary>
    public Dictionary<string, int> CODE { get; set; } = new Dictionary<string, int>();

    public Dictionary<string, string> NAME { get; set; } = new Dictionary<string, string>();
    public Dictionary<string, string> DESC { get; set; } = new Dictionary<string, string>();

    public Dictionary<string, int> VALUE { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, int> TURN { get; set; } = new Dictionary<string, int>();

    DtechBehaviour Dtech;
    BattleBehaviour Battle;
    InfoBehaviour Info;

    Dictionary<string, int> RandNum = new Dictionary<string, int>();

    public static TechBehaviour Instance
    {
        get
        {
            return instance;
        }
    }

    private static TechBehaviour instance;

    private void Start()
    {
        instance = this;

        CODE["P"] = 0;
        CODE["E"] = 0;

        NAME["P"] = "";
        NAME["E"] = "";

        DESC["P"] = "";
        DESC["E"] = "";

        VALUE["P"] = 0;
        VALUE["E"] = 0;

        TURN["P"] = 0;
        TURN["E"] = 0;
    }

    /// <summary>
    /// 変数 notify で示されたタイミングでの必殺技処理を行う。
    /// -1(最終ターン)、-2(ランダム)のいずれかを指定できる。
    /// </summary>
    public void Notify()
    {
        switch (TURN[side])
        {
            case -1:
                TURN[side] = Battle.battle.timeupTurns;
                break;
            case -2:
                TURN[side] = RandNum[side];
                break;
        }

        switch (CODE[side])
        {
            case 100:
            case 200:
            case 300:
            case 400:
                // 最後のターンの "turnend" ならば発動。
                if (Dtech.notify == "turnend" && Info.turnValue == TURN[side])
                    effect = true;
                break;
        }
    }

    /// <summary>
    /// 初期化処理を行う。

    /// </summary>
    public void Init()
    {
        Dtech = DtechBehaviour.Instance;
        Battle = BattleBehaviour.Instance;
        Info = InfoBehaviour.Instance;

        RandNum["P"] = Battle.randomEx.Range(1, Battle.battle.timeupTurns);
        RandNum["E"] = Battle.randomEx.Range(1, Battle.battle.timeupTurns);
    }

    /// <summary>
    /// エフェクトが終了した時点でコールされる。
    /// </summary>
    public IEnumerator fire()
    {
        switch (CODE[side])
        {
            case 100:
                // ダメージ処理。
                Battle.side = opp;
                Battle.type = 4;
                Battle.value = VALUE[side];
                Battle.dir = -1;
                Battle.way = "O";
                Battle.Damage();
                break;
            case 200:
                // ダメージ回復。
                Battle.side = side;
                Battle.type = 4;
                Battle.value = VALUE[side] * -1;
                Battle.dir = -1;
                Battle.way = "O";
                Battle.Damage();
                break;
            case 300:
                // ダメージ処理。
                Battle.side = opp;
                Battle.type = 4;
                Battle.value = VALUE[side];
                Battle.dir = -1;
                Battle.way = "O";
                Battle.Damage();

                // ダメージ処理。
                Battle.side = side;
                Battle.type = 4;
                Battle.value = VALUE[side] * -1;
                Battle.dir = -1;
                Battle.way = "O";
                Battle.Damage();
                break;
            case 400:
                // スターを溜める
                for (int i = 0; i < VALUE[side]; i++)
                {
                    int pushType = Battle.randomEx.Range(1, 3);

                    if (side == "P")
                        starStoreP.Push(pushType, 1);
                    else
                        starStoreE.Push(pushType, 1);

                    yield return new WaitForSeconds(0.2f);
                }

                break;
        }

        yield return null;
    }

}
