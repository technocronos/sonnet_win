using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class RaidPointBehaviour : MonoBehaviour
{
    public Image Rotator;

    public GameObject CardNormal;
    public GameObject CardRare;
    public GameObject CardSRare;

    public Image MonsterIcon;
    public TextMeshProUGUI MonsterGetText;
    public TextMeshProUGUI MonsterNFTText;
    public TextMeshProUGUI Title;
    public delegate void OnCompleteDelegate();
    public OnCompleteDelegate CompleteHandler;

    void Start()
    {
        Title.text = Utility.getText("TEXT_TITLE_RAIDPOINT_GET");

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
        MonsterNFTText.gameObject.SetActive(false);

        switch (result.battle.result_detail.monster.rare_level)
        {
            case 1:
                CardNormal.SetActive(true);
                break;
            case 2:
                CardRare.SetActive(true);
                break;
            case 3:
                CardSRare.SetActive(true);
                break;
        }
        MonsterIcon.sprite = Utility.getAssetImage("Image/MOB/" + result.battle.result_detail.monster.graphic_id.ToString("D5"));

        MonsterGetText.text = Utility.getText("battle_text_get_raidpoint").Replace("{0}", result.battle.result_detail.get_raid_point.ToString());

        if (result.battle.result_detail.get_nft)
        {
            MonsterNFTText.gameObject.SetActive(true);
        }

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
