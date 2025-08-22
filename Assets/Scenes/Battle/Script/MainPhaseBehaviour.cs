using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MainPhaseBehaviour : MonoBehaviour
{

    public AvatarBehaviour AvatarP;
    public AvatarBehaviour AvatarE;
    public GameObject Revenge;
    public GameObject TurnPhase;

    public Dictionary<string, AvatarBehaviour> Avatar = new Dictionary<string, AvatarBehaviour>();

    public GameObject Info;
    public GameObject starStoreP;
    public GameObject starStoreE;

    public Dictionary<string, GameObject> starStore = new Dictionary<string, GameObject>();

    // Start is called before the first frame update
    public static MainPhaseBehaviour Instance
    {
        get
        {
            return instance;
        }
    }

    private static MainPhaseBehaviour instance;

    public StarStoreBehaviour objstarStoreP { get; set; }
    public StarStoreBehaviour objstarStoreE { get; set; }

    private void Start()
    {
        instance = this;
        TurnPhase.SetActive(true);

        starStore["P"] = starStoreP;
        starStore["E"] = starStoreE;

        Avatar["P"] = AvatarP;
        Avatar["E"] = AvatarE;

        objstarStoreP = starStoreP.GetComponent<StarStoreBehaviour>();
        objstarStoreE = starStoreE.GetComponent<StarStoreBehaviour>();

        objstarStoreP.setText(0);
        objstarStoreE.setText(0);

    }

    public void setInfoVisible(bool _visible)
    {
        Info.SetActive(_visible);
    }

    public void revengeTest()
    {
        // リベンジテスト用。
        objstarStoreP.Push(1, 10);
        objstarStoreE.Push(1, 10);
    }

    public void StarPush(string side, int pushtype, int num)
    {
        if (side == "P")
            objstarStoreP.Push(pushtype, num);
        else
            objstarStoreE.Push(pushtype, num);

    }


    /// <summary>
    // メインフェーズを進行させるために、サブフェーズを呼び出す。
    // スターが規定数以上集まったならリベンジフェーズを起動。
    /// </summary>
    public void Prog()
    {
        if (objstarStoreP.value >= BattleBehaviour.REVENGE_REQUIRED_NUM || objstarStoreE.value >= BattleBehaviour.REVENGE_REQUIRED_NUM)
        {

            // どちらが仕掛けるのかを割り出す。両方規定数以上ならスピードの速い方。
            if (objstarStoreP.value >= BattleBehaviour.REVENGE_REQUIRED_NUM && objstarStoreE.value >= BattleBehaviour.REVENGE_REQUIRED_NUM)
            {
                RevengePhaseBehaviour.Instance.side = (BattleBehaviour.Instance.battle.spdRate >= 0.0) ? "P" : "E";
            }
            else
            {
                RevengePhaseBehaviour.Instance.side = (objstarStoreP.value >= BattleBehaviour.REVENGE_REQUIRED_NUM) ? "P" : "E";
            }

            // 統計値を更新。
            switch (RevengePhaseBehaviour.Instance.side)
            {
                case "P":
                    BattleBehaviour.Instance.statRevCnt["P"]++;
                    break;
                case "E":
                    BattleBehaviour.Instance.statRevCnt["E"]++;
                    break;
            }

            // リベンジフェーズを起動。
            Revenge.SetActive(true);
            RevengePhaseBehaviour.Instance.RevengeStart();

        }
        else
        {
            // スターが規定数集まっていない場合。

            // 必殺技管理にターンの終了を通知。これにより turnEnd がcallされる。
            DtechBehaviour.Instance.notify = "turnend";
            DtechBehaviour.Instance.Notify();

        }

    }

    // 進行用のサブフェーズが終了したら呼び出される。
    public void ProgEnd()
    {
        //PlayerPrefに保存
        BattleBehaviour.Instance.SaveStat();

        // どちらかのHPが0になっているなら親の "mainEnd" ラベルをコール。
        if (HpGaugeBehaviour.Instance.HpInfo["P"].value <= 0 || HpGaugeBehaviour.Instance.HpInfo["E"].value <= 0)
        {
            BattleBehaviour.Instance.MainEnd();
        }
        else
        {
            // まだ終わっていないならさらに進行。
            this.Prog();
        }
    }

    // 必殺技の処理も含めてターンが終了したときにcallされる。
    public void turnEnd()
    {
        // 既にタイムアップターンを迎えたならば終了。
        if (InfoBehaviour.Instance.turnValue >= BattleBehaviour.Instance.battle.timeupTurns)
        {
            Debug.Log("turnend1");
            BattleBehaviour.Instance.MainEnd();
        }
        else
        {
            // まだタイムアップでないならば   

            // ターン数アップ
            InfoBehaviour.Instance.TurnUp();

            // ターンフェーズを起動。
            Debug.Log("goto turnPhase");
            TurnPhaseBehaviour.Instance.TurnStart();
        }
    }

    /// <summary>
    /// 戦術マニュアル選択
    /// 1:強攻 2：慎重 3:吸収
    /// </summary>
    /// <param name="type"></param>
    public void onTackButton(int type)
    {
        AudioManager.Instance.PlaySE("se_btn");

        TurnPhaseBehaviour.Instance.TactP.type = type;

        PlayerSelect.Instance.Decided();
    }

}
