using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TactBehaviour : MonoBehaviour
{

    public Image Tact1;
    public Image Tact2;
    public Image Tact3;
    public Image Tact4;

    public TextMeshProUGUI brain;

    private Sequence seq { get; set; }
    public int type { get; set; }

    private int tacktype { get; set; }

    public static TactBehaviour Instance
    {
        get
        {
            return instance;
        }
    }

    private static TactBehaviour instance;

    private void Start()
    {
        instance = this;
        brain.text = "";
    }

    //
    // 変数 type に設定された値にしたがって、戦術表示グラフィックを変更する。
    public void TackInit()
    {
        tacktype = type;

        if (tacktype == 0)
            tacktype = 4;

        Tact1.gameObject.SetActive(false);
        Tact2.gameObject.SetActive(false);
        Tact3.gameObject.SetActive(false);
        Tact4.gameObject.SetActive(false);

        switch (tacktype)
        {
            case 1:
                Tact1.gameObject.SetActive(true);
                break;
            case 2:
                Tact2.gameObject.SetActive(true);
                break;
            case 3:
                Tact3.gameObject.SetActive(true);
                break;
            case 4:
                Tact4.gameObject.SetActive(true);
                break;
        }
    }

    public void Blink()
    {

        Image TackImage = null;
        Image TackImageOn = null;

        switch (tacktype)
        {
            case 1:
                TackImage = Tact1;
                TackImageOn = Tact1.transform.Find("Image").GetComponent<Image>();
                break;
            case 2:
                TackImage = Tact2;
                TackImageOn = Tact2.transform.Find("Image").GetComponent<Image>();
                break;
            case 3:
                TackImage = Tact3;
                TackImageOn = Tact3.transform.Find("Image").GetComponent<Image>();
                break;
            case 4:
                TackImage = Tact4;
                TackImageOn = Tact4.transform.Find("Image").GetComponent<Image>();
                break;
        }

        //Sequenceを宣言する
        seq = DOTween.Sequence();
        seq.Append(DOVirtual.DelayedCall(0.05f, () => TackImageOn.enabled = true));
        seq.Append(DOVirtual.DelayedCall(0.05f, () => TackImageOn.enabled = false));
        seq.SetLoops(-1, LoopType.Restart);//無限ループする
    }

    public void BlinkStop()
    {
        seq.Kill();
    }

    public void Normal()
    {
        BlinkStop();

        Image TackImage = null;
        Image TackImageOn = null;

        switch (tacktype)
        {
            case 1:
                TackImage = Tact1;
                TackImageOn = Tact1.transform.Find("Image").GetComponent<Image>();
                break;
            case 2:
                TackImage = Tact2;
                TackImageOn = Tact2.transform.Find("Image").GetComponent<Image>();
                break;
            case 3:
                TackImage = Tact3;
                TackImageOn = Tact3.transform.Find("Image").GetComponent<Image>();
                break;
            case 4:
                TackImage = Tact4;
                TackImageOn = Tact4.transform.Find("Image").GetComponent<Image>();
                break;
        }

        TackImageOn.enabled = true;

    }

}
