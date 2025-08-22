using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OpenPhaseBehaviour : MonoBehaviour
{

    public TextMeshProUGUI TextNameP;
    public TextMeshProUGUI TextNameE;

    // Start is called before the first frame update
    public static OpenPhaseBehaviour Instance
    {
        get
        {
            return instance;
        }
    }

    private static OpenPhaseBehaviour instance;

    private void Start()
    {
        instance = this;
    }

    public void startOpenPhase2()
    {
        Debug.Log("OpenPhaseBehaviour startOpenPhase2 run..");
    }

    public void onAvatarSetFinish()
    {
        Debug.Log("OpenPhaseBehaviour onAvatarSetFinish run..");

        // HP表示の動作を開始。
        MainPhaseBehaviour.Instance.setInfoVisible(true);
        HpGaugeBehaviour.Instance.HpStart();

        // メインのアバタを表示
        BattleBehaviour.Instance.CharaP.gameObject.SetActive(true);
        BattleBehaviour.Instance.CharaE.gameObject.SetActive(true);

        // プレイヤー名、相手の名前を初期化。
        InfoBehaviour.Instance.setAvatarName(BattleBehaviour.Instance.battle.nameP, "P");
        InfoBehaviour.Instance.setAvatarName(BattleBehaviour.Instance.battle.nameE, "E");

    }

}
