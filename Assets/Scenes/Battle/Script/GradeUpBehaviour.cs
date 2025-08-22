using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public class GradeUpBehaviour : MonoBehaviour
{

    public Image Rotator;
    public TextMeshProUGUI TextGrade;
    public TextMeshProUGUI TextDtechName;
    public TextMeshProUGUI Title;

    public GradeUpEffectsBehaviour GradeUpEffects;

    public delegate void OnCompleteDelegate();
    public OnCompleteDelegate CompleteHandler;

    void Start()
    {
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

        Title.text = Utility.getText("TEXT_TITLE_GRADE_UP");

        //階級
        TextGrade.text = Utility.getText("TEXT_GRADE_UP").Replace("{0}", result.grade.grade_name);
        TextDtechName.text = Utility.getText("TEXT_GRADE_UP_DTECH").Replace("{0}", result.grade.dtech.dtech_name);

        //演出開始
        GradeUpEffects.PlayAnim("hemisphere");

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
