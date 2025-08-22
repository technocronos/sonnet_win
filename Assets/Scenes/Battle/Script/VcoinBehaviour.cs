using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class VcoinBehaviour : MonoBehaviour
{
    public Image Rotator;
    public TextMeshProUGUI TextVCoin;

    public TextMeshProUGUI Title;

    public delegate void OnCompleteDelegate();
    public OnCompleteDelegate CompleteHandler;

    void Start()
    {
        Title.text = Utility.getText("TEXT_TITLE_CRYPTOGET");

        //回転アニメ
        Rotator.transform.DOLocalRotate(new Vector3(0, 0, 360f), 30f, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart);
    }


    public void Show(jsonBattleResult result, OnCompleteDelegate _callback)
    {

        if (_callback != null)
            CompleteHandler += _callback;

        AudioManager.Instance.PlaySE("se_congrats");

        float getCoin = result.battle.result_detail.get_vcoin;

        TextVCoin.text = Utility.getText("TEXT_GET_BTC").Replace("{0}", (decimal.Parse(getCoin.ToString(), System.Globalization.NumberStyles.AllowExponent | System.Globalization.NumberStyles.AllowDecimalPoint)).ToString());
    }

    public void onClose()
    {
        AudioManager.Instance.PlaySE("se_btn");

        if (CompleteHandler != null)
        {
            CompleteHandler?.Invoke();
            CompleteHandler = null;
        }

        transform.gameObject.SetActive(false);
    }
}
