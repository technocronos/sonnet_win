using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class LevelUPBehaviour : MonoBehaviour
{
    public GameObject objParamSeed;
    public GameObject objStatusGet;
    public Button ButtonParamSeed;

    public Image Rotator;

    public TextMeshProUGUI TextLevel;

    public TextMeshProUGUI att1;
    public TextMeshProUGUI att2;
    public TextMeshProUGUI att3;
    public TextMeshProUGUI def1;
    public TextMeshProUGUI def2;
    public TextMeshProUGUI def3;
    public TextMeshProUGUI spd;
    public TextMeshProUGUI hp_max;
    public TextMeshProUGUI param_seed;

    public Image att1Icon;
    public Image att2Icon;
    public Image att3Icon;
    public Image def1Icon;
    public Image def2Icon;
    public Image def3Icon;
    public Image spdIcon;
    public Image hp_maxIcon;

    public TextMeshProUGUI CaptionTitleText;
    public TextMeshProUGUI CaptionStatus;
    public TextMeshProUGUI CaptionGet;

    public GameObject StatusIcons;

    private jsonBattleResult list;

    public delegate void OnCompleteDelegate();
    public OnCompleteDelegate CompleteHandler;

    public void Show(jsonBattleResult _list, OnCompleteDelegate _callback)
    {
        if (_callback != null)
            CompleteHandler += _callback;

        AudioManager.Instance.PlaySE("se_congrats");

        list = _list;

        jsonBRCharaInfo before = list.ready;
        jsonChara after = list.battleresult.character;

        CaptionTitleText.text = Utility.getText("TEXT_TITLE_LEVELUP");
        //CaptionStatus.text = Utility.getText("TEXT_STATUS_POINT");
        //CaptionGet.text = Utility.getText("TEXT_GET");

        StatusIcons.transform.Find("TextAtt1").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_STATUS_ATT1");
        StatusIcons.transform.Find("TextAtt2").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_STATUS_ATT2");
        StatusIcons.transform.Find("TextAtt3").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_STATUS_ATT3");
        StatusIcons.transform.Find("TextDef1").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_STATUS_DEF1");
        StatusIcons.transform.Find("TextDef2").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_STATUS_DEF2");
        StatusIcons.transform.Find("TextDef3").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_STATUS_DEF3");
        StatusIcons.transform.Find("TextSpd").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_STATUS_SPEED");
        StatusIcons.transform.Find("TextMaxHp").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_STATUS_MAXHP");

        StatusIcons.transform.Find("TextAtt1UP").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_UP");
        StatusIcons.transform.Find("TextAtt2UP").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_UP");
        StatusIcons.transform.Find("TextAtt3UP").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_UP");
        StatusIcons.transform.Find("TextDef1UP").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_UP");
        StatusIcons.transform.Find("TextDef2UP").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_UP");
        StatusIcons.transform.Find("TextDef3UP").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_UP");
        StatusIcons.transform.Find("TextSpdUP").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_UP");
        StatusIcons.transform.Find("TextMaxHpUP").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_UP");


        int _level = after.level;

        int _att1 = after.attack1 - before.attack1;
        int _att2 = after.attack2 - before.attack2;
        int _att3 = after.attack3 - before.attack3;

        int _def1 = after.defence1 - before.defence1;
        int _def2 = after.defence2 - before.defence2;
        int _def3 = after.defence3 - before.defence3;
        //int _defX = after.defenceX - before.defenceX;

        int _spd = after.speed - before.speed;

        float _hp_max = after.hp_max - before.hp_max;
        int _param_seed = after.param_seed - before.param_seed;

        att1.text = _att1.ToString();
        if (_att1 == 0) att1Icon.gameObject.SetActive(false);

        att2.text = _att2.ToString();
        if (_att2 == 0) att2Icon.gameObject.SetActive(false);

        att3.text = _att3.ToString();
        if (_att3 == 0) att3Icon.gameObject.SetActive(false);

        def1.text = _def1.ToString();
        if (_def1 == 0) def1Icon.gameObject.SetActive(false);

        def2.text = _def2.ToString();
        if (_def2 == 0) def2Icon.gameObject.SetActive(false);

        def3.text = _def3.ToString();
        if (_def3 == 0) def3Icon.gameObject.SetActive(false);

        spd.text = _spd.ToString();
        if (_spd == 0) spdIcon.gameObject.SetActive(false);

        hp_max.text = _hp_max.ToString();
        if (_hp_max == 0) hp_maxIcon.gameObject.SetActive(false);

        param_seed.text = "+" + _param_seed;

        if (_param_seed > 0)
        {
            objStatusGet.SetActive(true);
            ButtonParamSeed.gameObject.SetActive(true);
        }
        else
        {
            objStatusGet.SetActive(false);
            ButtonParamSeed.gameObject.SetActive(false);
        }

        //レベル
        TextLevel.text = Utility.getText("BATTLT_LEVELUP").Replace("{0}", _level.ToString());

        //回転アニメ
        Rotator.transform.DOLocalRotate(new Vector3(0, 0, 360f), 30f, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart);

    }

    public void onClickParamSeed()
    {
        AudioManager.Instance.PlaySE("se_btn");

        objParamSeed.SetActive(true);
        objParamSeed.GetComponent<ParamSeedBehaviour>().Show(onClose);
    }

    public void onClose()
    {
        AudioManager.Instance.PlaySE("se_btn");

        transform.gameObject.SetActive(false);

        if (CompleteHandler != null)
        {
            CompleteHandler?.Invoke();
            CompleteHandler = null;
        }
    }
}
