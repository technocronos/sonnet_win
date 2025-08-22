using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;


public class MissionBehaviour : MonoBehaviour
{
    public Image Rotator;
    public TextMeshProUGUI TextGold;
    public TextMeshProUGUI TextCurrentGold;
    public TextMeshProUGUI TextNotice;


    public delegate void OnCompleteDelegate();
    public OnCompleteDelegate CompleteHandler;


    // Start is called before the first frame update
    void Start()
    {
        //回転アニメ
        Rotator.transform.DOLocalRotate(new Vector3(0, 0, 360f), 30f, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart);
    }
    public void Show(jsonMission mission, OnCompleteDelegate _callback = null)
    {

        if (_callback != null)
            CompleteHandler += _callback;

        AudioManager.Instance.PlaySE("se_congrats");

        TextGold.text = "+" + mission.gold;
        TextCurrentGold.text = (Header.Instance.GetSummary().gold - mission.gold) + "→" + Header.Instance.GetSummary().gold;
        TextNotice.text = Utility.getText("TEXT_MISSION_NOTICE");
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
