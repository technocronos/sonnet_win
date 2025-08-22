using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ContinueConfirmBehaviour : MonoBehaviour
{
    public TextMeshProUGUI Text;
    public GameObject BuyItemPhaseObj;

    public int mode { set; get; }

    public static ContinueConfirmBehaviour Instance
    {
        get
        {
            return instance;
        }
    }

    private static ContinueConfirmBehaviour instance;

    private void Start()
    {
        instance = this;
    }

    public void setText(string _text)
    {
        Text.text = _text;
    }

    /// <summary>
    /// 
    /// </summary>
    public void onOK()
    {
        AudioManager.Instance.PlaySE("se_btn");
        switch (mode)
        {
            case 1:
                ContinuePhaseBehaviour.Instance.ContinuePhaseStart();
                break;
            case 2:
                BuyItemPhaseObj.SetActive(true);
                BuyItemPhaseBehaviour.Instance.BuyItemPhaseStart();
                break;
        }
    }

    public void onCancel()
    {
        AudioManager.Instance.PlaySE("se_btn");

        PreterBehaviour.Instance.Visible(false);
        NaviBehaviour.Instance.Visible(false);

        StartCoroutine(BattleBehaviour.Instance.Close());

        transform.gameObject.SetActive(false);

    }

}
