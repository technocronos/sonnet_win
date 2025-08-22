using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;


public class ZukanBehaviour : MonoBehaviour
{
    public Image Rotator;

    public GameObject CardNormal;
    public GameObject CardRare;
    public GameObject CardSRare;

    public TextMeshProUGUI MonserName;
    public Image MonsterIcon;
    public TextMeshProUGUI MonsterFlavorText;

    public TextMeshProUGUI Title;

    public delegate void OnCompleteDelegate();
    public OnCompleteDelegate CompleteHandler;

    void Start()
    {
        Title.text = Utility.getText("TEXT_TITLE_ZUKAN_TUIKA");

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

        CardNormal.SetActive(false);
        CardRare.SetActive(false);
        CardSRare.SetActive(false);

        switch (result.capture.rare_level)
        {
            case 1:
                CardNormal.SetActive(true);
                MonserName.textStyle = TMP_Style.NormalStyle;
                MonserName.color = ColorGet.Hex(0xFFFFFF);
                break;
            case 2:
                CardRare.SetActive(true);
                MonserName.textStyle = TMP_Style.NormalStyle;
                MonserName.color = ColorGet.Hex(0xFFFFFF);
                break;
            case 3:
                CardSRare.SetActive(true);
                MonserName.textStyle = TMP_Style.NormalStyle;
                MonserName.color = ColorGet.Hex(0x776451);
                break;
        }
        MonsterIcon.sprite = Utility.getAssetImage("Image/MOB/" + result.capture.graphic_id.ToString("D5"));

        MonserName.text = result.capture.monster_name;

        MonsterFlavorText.text = result.capture.flavor_text;

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
