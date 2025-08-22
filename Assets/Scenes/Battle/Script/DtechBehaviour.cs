using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
// 必殺技を管理するムービー
public class DtechBehaviour : MonoBehaviour
{
    public TechBehaviour tech;
    public GameObject Lay;

    public string notify { get; set; }

    BattleBehaviour Battle { get; set; }

    List<string> side = new List<string>();

    bool endflg = false;

    public static DtechBehaviour Instance
    {
        get
        {
            return instance;
        }
    }

    private static DtechBehaviour instance;

    private void Start()
    {
        instance = this;

        side.Add("P");
        side.Add("E");
    }
    public void Init()
    {
        Battle = BattleBehaviour.Instance;
        tech.Init();
    }
    //
    // 変数 notify で示されたタイミングでの必殺技処理を行う。
    // notify は以下の値をとる。
    //      turnend     ターン終了時。リベンジが発生している場合はリベンジの後。
    //                  どちらかのHPが0になっている場合は発生しない。
    //                  ターン0の終了時も呼び出される。
    public void Notify()
    {
        StartCoroutine(next());
    }

    // 
    // 変数csrで示されたインデックス以降の必殺技に、変数notifyで示されたタイミングを通知する。
    IEnumerator next()
    {
        // すべての必殺技スロットに通知するまで行う。
        foreach (string s in side)
        {
            jsonDtech dtech = null;

            // 必殺技スロットを取得。
            if (s == "P")
            {
                tech.side = "P";
                tech.opp = "E";
                dtech = Battle.battle.dtech_charaP;
            }
            else if (s == "E")
            {
                tech.side = "E";
                tech.opp = "P";
                dtech = Battle.battle.dtech_charaE;
            }

            if (dtech.dtech_id != 0)
            {
                tech.CODE[s] = dtech.code_id;
                tech.NAME[s] = dtech.dtech_name;
                tech.DESC[s] = dtech.dtech_desc;

                tech.VALUE[s] = int.Parse(dtech.value1);
                tech.TURN[s] = int.Parse(dtech.value2);

                // 通知。
                tech.effect = false;
                tech.Notify();

                // 起動エフェクトを行うように指定されている場合。
                if (tech.effect)
                {
                    // エフェクトを起動。
                    endflg = false;
                    effec();

                    // エフェクト終了まで待機
                    while (!endflg)
                    {
                        yield return null;
                    }
                }
            }
        }

        // すべてのスロットに通知が終わったのなら、バトルを進める。
        prog();

        yield return null;
    }

    //
    // 必殺技エフェクトを再生する。
    public void effec()
    {
        // 技名を表示
        PreterBehaviour.Instance.Visible(true);
        PreterBehaviour.Instance.PlayAnim("plane");
        PreterBehaviour.Instance.setText(tech.NAME[tech.side]);

        // ナビを表示
        NaviBehaviour.Instance.setSide(tech.side);
        NaviBehaviour.Instance.setText(tech.DESC[tech.side]);

        if (tech.side == "P")
            NaviBehaviour.Instance.Show(2);
        else
            NaviBehaviour.Instance.Show(3);

        // 閃光をセット
        Lay.SetActive(true);
        LayBehaviour.Instance.side = tech.side;
        LayBehaviour.Instance.Show(effectWait);

    }

    void effectWait()
    {
        // 技名表示、ナビ表示をOFFに
        PreterBehaviour.Instance.Visible(false);
        NaviBehaviour.Instance.Visible(false);

        // エフェクト終了時の処理。
        StartCoroutine(effEnd());
    }

    // 
    // 変数 csr で示された必殺技に、エフェクトが終了したことを通知する。
    IEnumerator effEnd()
    {
        Lay.SetActive(false);
        yield return StartCoroutine(tech.fire());

        if (notify != "deadend" && (HpGaugeBehaviour.Instance.HpInfo["P"].value <= 0 || HpGaugeBehaviour.Instance.HpInfo["E"].value <= 0))
        {
            // "deadend" 以外のタイミングは、どちらかのHPが0になっているならメインフェーズ終了
            BattleBehaviour.Instance.MainEnd();
        }
        else if (MainPhaseBehaviour.Instance.objstarStoreP.value >= BattleBehaviour.REVENGE_REQUIRED_NUM || MainPhaseBehaviour.Instance.objstarStoreE.value >= BattleBehaviour.REVENGE_REQUIRED_NUM)
        {
            //スターが溜まっているならProgに戻ってリベンジ発動
            if (tech.side == "P")
            {
                Battle.battle.dtech_charaP = new jsonDtech();
            }
            else
            {
                Battle.battle.dtech_charaE = new jsonDtech();
            }
            MainPhaseBehaviour.Instance.Prog();
        }
        else
        {
            // それ以外は引き続きイベント通知を続ける。
            endflg = true;
        }

        yield return null;
    }

    //
    // 変数notifyで示されたタイミング通知が終わったときの処理を行う。
    public void prog()
    {
        switch (notify)
        {
            case "turnend":
                MainPhaseBehaviour.Instance.turnEnd();
                break;
        }

    }
}
