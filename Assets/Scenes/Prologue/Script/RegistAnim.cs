using System.Collections;
using System.Collections.Generic;
using CreateWave;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
using TMPro;
using Scenes.Common.Scripts;
using System;

public class RegistAnim : MonoBehaviour
{
    public PlayableDirector director;
    public InputField InputName;
    public NaviController naviController;
    public Prologue prologue;
    public GameObject RegistPanel;
    public Image BG;

    public InputField InputInheritCode;
    public Button BtnInheritOK;
    public TextMeshProUGUI TextCaption2;

    private string inviterId;

    // Start is called before the first frame update
    void Start()
    {
        BG.sprite = Utility.getAssetImage("Image/BG/bg1");

        RegistPanel.SetActive(false);

        Debug.Log("RegistAnim Start");

        inviterId = null;

        if (Main.Instance.conversionDataDictionary != null)
        {
            foreach (KeyValuePair<string, object> item in Main.Instance.conversionDataDictionary)
            {
                //友達招待がある場合
                if (item.Key == Settings.AF_INVITE_KEY)
                {
                    //Main.Instance.showDialogue("AF_INVITE_KEY=" + item.Value.ToString());

                    inviterId = item.Value.ToString();
                    break;
                }
            }
        }

        string[] navi_speak = null;

        if (!Main.Instance.in_apply)
        {
            navi_speak = Utility.getText("TEXT_NAVI_REGIST_GAME1").Split("\n", StringSplitOptions.None);
        }
        else
        {
            //apple申請中の場合は端末引き継ぎ非表示
            navi_speak = Utility.getText("TEXT_NAVI_REGIST_GAME1_2").Split("\n", StringSplitOptions.None);
            InputInheritCode.gameObject.SetActive(false);
            BtnInheritOK.gameObject.SetActive(false);
            TextCaption2.gameObject.SetActive(false);
        }

        HomeApi homeSummary = new HomeApi();
        homeSummary.special = navi_speak;
        homeSummary.specialNum = navi_speak.Length;

        naviController.gameObject.SetActive(true);
        naviController.onStart(homeSummary, null, () =>
        {
            RegistPanel.SetActive(true);
            naviController.disappere();
        });
    }

    void OnEnable()
    {
        Debug.Log("RegistAnim OnEnable");
        director.stopped += OnPlayableDirectorStopped;

        director.Stop();
    }

    public void onBtnRegOK()
    {
        Debug.Log("RegistAnim onBtnRegOK");

        AudioManager.Instance.PlaySE("se_btn");

        if (InputName.text == "")
        {
            Main.Instance.showDialogue(Utility.getText("TEXT_NAVI_INPUT_NAME"));
        }
        else
        {
            Main.Instance.showConfirm(Utility.getText("TEXT_NAVI_CONFIRM_NAME").Replace("[NAME]", InputName.text), () =>
            {
                APIConnectManager.Instance.Regist(InputName.text, inviterId, onRegist);
            });
        }
    }


    void onRegist(string json)
    {
        Debug.Log("RegistAnim onRegist json =" + json);

        APIConnectManager.Instance.regist = JsonUtility.FromJson<jsonRegist>(json);

        jsonRegist regInfo = APIConnectManager.Instance.regist;

        if (regInfo.result == 1)
        {
            RegistPanel.SetActive(false);

            string endtext = Utility.getText("TEXT_NAVI_REGIST_END").Replace("[NAME]", InputName.text);

            string[] speak = endtext.Split("\n", StringSplitOptions.None);

            HomeApi homeSummary = new HomeApi();
            homeSummary.special = speak;
            homeSummary.specialNum = speak.Length;

            naviController.gameObject.SetActive(true);
            naviController.onStart(homeSummary, null, (() =>
            {
                naviController.disappere();
                prologue.gotoNext();
            }));

        }
        else if (regInfo.result == 2)
        {
            Main.Instance.showDialogue(Utility.getText("TEXT_NAVI_ERROR_REGIST1"));
        }
    }

    public void onBtnInheritOK()
    {
        Debug.Log("RegistAnim onBtnInheritOK");

        AudioManager.Instance.PlaySE("se_btn");

        if (InputInheritCode.text == "")
        {
            Main.Instance.showDialogue(Utility.getText("TEXT_NAVI_INPUT_HIKITSUGI"));
        }
        else
        {
            Main.Instance.showConfirm(Utility.getText("TEXT_NAVI_CONFIRM_HIKITSUGI"),
                (() => APIConnectManager.Instance.Inherit(InputInheritCode.text, onInherit)));
        }
    }

    void onInherit(string json)
    {
        Debug.Log("RegistAnim onInherit json =" + json);

        APIConnectManager.Instance.regist = JsonUtility.FromJson<jsonRegist>(json);

        jsonRegist regInfo = APIConnectManager.Instance.regist;

        if (regInfo.result == 1)
        {
            RegistPanel.SetActive(false);

            HomeApi homeSummary = new HomeApi();

            string[] speak = Utility.getText("TEXT_NAVI_FINISH_HIKITSUGI").Split("\n", StringSplitOptions.None);

            homeSummary.special = speak;
            homeSummary.specialNum = speak.Length;

            naviController.gameObject.SetActive(true);
            naviController.onStart(homeSummary, null, (() =>
            {
                naviController.disappere();
                SceneController.Instance.Jump("Home");
            }));

        }
        else if (regInfo.result == -1)
        {
            Main.Instance.showDialogue(Utility.getText("TEXT_NAVI_ERROR_HIKITSUGI_1"));
        }
        else if (regInfo.result == -2)
        {
            Main.Instance.showDialogue(Utility.getText("TEXT_NAVI_ERROR_HIKITSUGI_2"));
        }
        else if (regInfo.result == -3)
        {
            Main.Instance.showDialogue(Utility.getText("TEXT_NAVI_ERROR_HIKITSUGI_3"));
        }
    }

    void OnPlayableDirectorStopped(PlayableDirector aDirector)
    {
        if (director == aDirector)
            Debug.Log("RegistAnim named " + aDirector.name + " is now stopped.");
    }

    void OnDisable()
    {
        director.stopped -= OnPlayableDirectorStopped;
    }
}
