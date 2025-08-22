using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// HP・ターン表示エリアを制御する。
/// </summary>
public class InfoBehaviour : MonoBehaviour
{
    public TextMeshProUGUI TextNameP;
    public TextMeshProUGUI TextNameE;

    public TextMeshProUGUI TextTurn;

    public int turnValue { set; get; } = 0;

    // Start is called before the first frame update
    public static InfoBehaviour Instance
    {
        get
        {
            return instance;
        }
    }

    private static InfoBehaviour instance;

    private void Start()
    {
        instance = this;
    }

    public void Init()
    {
        // プレイヤー側・相手側のHPを初期化。
        HpGaugeBehaviour.Instance.HpInfo["P"].max = BattleBehaviour.Instance.battle.hpMaxP;
        HpGaugeBehaviour.Instance.HpInfo["E"].max = BattleBehaviour.Instance.battle.hpMaxE;

        HpGaugeBehaviour.Instance.HpInfo["P"].value = BattleBehaviour.Instance.battle.hpStartP;
        HpGaugeBehaviour.Instance.HpInfo["E"].value = BattleBehaviour.Instance.battle.hpStartE;

        //HpGauge初期化
        HpGaugeBehaviour.Instance.Init();

        // ターン表示を初期化。
        TurnRefresh();
    }

    public void TurnUp()
    {
        turnValue++;
        TextTurn.text = turnValue.ToString();
    }

    public void TurnRefresh()
    {
        TextTurn.text = turnValue.ToString();
    }

    public void setAvatarName(string name, string side)
    {
        switch (side)
        {
            case "P":
                TextNameP.text = name;
                break;
            case "E":
                TextNameE.text = name;
                break;
        }
    }
}
