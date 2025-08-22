using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ParamSeedBehaviour : MonoBehaviour
{

    private List<string> list = new List<string> { "att1", "att2", "att3", "def1", "def2", "def3", "spd", "hp" };
    private Dictionary<string, int> param_seed = new Dictionary<string, int>();

    private jsonChara status;

    public delegate void OnCompleteDelegate();
    public OnCompleteDelegate CompleteHandler;

    public void Show(OnCompleteDelegate _callback = null)
    {
        if (_callback != null)
            CompleteHandler += _callback;

        this.param_seed["att1"] = 0;
        this.param_seed["att2"] = 0;
        this.param_seed["att3"] = 0;
        this.param_seed["def1"] = 0;
        this.param_seed["def2"] = 0;
        this.param_seed["def3"] = 0;
        this.param_seed["spd"] = 0;
        this.param_seed["hp"] = 0;
        this.param_seed["total"] = 0;

        //APIをたたく
        APIConnectManager.Instance.Status(onStart);
    }

    void onStart(string json)
    {
        jsonStatus response = JsonUtility.FromJson<jsonStatus>(json);
        status = response.chara;
        
        transform.Find("Navi/Flame/TextNavi").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_NAV_STATUS_POINT");

        transform.Find("MainPanel/status_icons/TextAtt1").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_STATUS_ATT1");
        transform.Find("MainPanel/status_icons/TextAtt2").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_STATUS_ATT2");
        transform.Find("MainPanel/status_icons/TextAtt3").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_STATUS_ATT3");
        transform.Find("MainPanel/status_icons/TextDef1").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_STATUS_DEF1");
        transform.Find("MainPanel/status_icons/TextDef2").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_STATUS_DEF2");
        transform.Find("MainPanel/status_icons/TextDef3").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_STATUS_DEF3");
        transform.Find("MainPanel/status_icons/TextSpd").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_STATUS_SPEED");
        transform.Find("MainPanel/status_icons/TextMaxHp").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_STATUS_MAXHP");

        transform.Find("MainPanel/status_icons/TextNotice").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_NAV_STATUS_POINT_NOTICE");

        transform.Find("MainPanel/curr_att1").GetComponent<TextMeshProUGUI>().text = status.attack1.ToString();
        transform.Find("MainPanel/curr_att2").GetComponent<TextMeshProUGUI>().text = status.attack2.ToString();
        transform.Find("MainPanel/curr_att3").GetComponent<TextMeshProUGUI>().text = status.attack3.ToString();
        transform.Find("MainPanel/curr_spd").GetComponent<TextMeshProUGUI>().text = status.speed.ToString();
        transform.Find("MainPanel/curr_def1").GetComponent<TextMeshProUGUI>().text = status.defence1.ToString();
        transform.Find("MainPanel/curr_def2").GetComponent<TextMeshProUGUI>().text = status.defence2.ToString();
        transform.Find("MainPanel/curr_def3").GetComponent<TextMeshProUGUI>().text = status.defence3.ToString();
        transform.Find("MainPanel/curr_hp").GetComponent<TextMeshProUGUI>().text = status.hp_max.ToString();

        transform.Find("MainPanel/att1_add").GetComponent<TextMeshProUGUI>().text = "0";
        transform.Find("MainPanel/att2_add").GetComponent<TextMeshProUGUI>().text = "0";
        transform.Find("MainPanel/att3_add").GetComponent<TextMeshProUGUI>().text = "0";
        transform.Find("MainPanel/def1_add").GetComponent<TextMeshProUGUI>().text = "0";
        transform.Find("MainPanel/def2_add").GetComponent<TextMeshProUGUI>().text = "0";
        transform.Find("MainPanel/def3_add").GetComponent<TextMeshProUGUI>().text = "0";
        transform.Find("MainPanel/spd_add").GetComponent<TextMeshProUGUI>().text = "0";
        transform.Find("MainPanel/hp_add").GetComponent<TextMeshProUGUI>().text = "0";

        transform.Find("CurrentStatusPanel/TextCurrentStatus").GetComponent<TextMeshProUGUI>().text = status.param_seed + " pt";
        transform.Find("CurrentStatusPanel/CaptionCurrentStatus").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_STATUS_POINT_LONG");

        foreach (string value in list)
        {
            Button btnUp = transform.Find("MainPanel/" + value + "_btn_up").GetComponent<Button>();
            btnUp.interactable = true;

            // ボタンがクリックされたときのハンドラを登録
            btnUp.onClick.RemoveAllListeners();
            btnUp.onClick.AddListener((() =>
            {
                if (this.param_seed["total"] < status.param_seed)
                {
                    AudioManager.Instance.PlaySE("se_btn");

                    int add_point = 1;
                    if (value == "hp")
                        add_point = 7;

                    this.param_seed[value]++;
                    transform.Find("MainPanel/" + value + "_add").GetComponent<TextMeshProUGUI>().text = this.param_seed[value].ToString();

                    if (this.param_seed[value] == status.param_seed)
                        transform.Find("MainPanel/" + value + "_btn_up").GetComponent<Button>().interactable = false;

                    if (this.param_seed[value] >= 1)
                        transform.Find("MainPanel/" + value + "_btn_down").GetComponent<Button>().interactable = true;

                    TextMeshProUGUI curr = transform.Find("MainPanel/" + "curr_" + value).GetComponent<TextMeshProUGUI>();
                    curr.text = (int.Parse(curr.text) + add_point).ToString();

                    this.param_seed["total"]++;
                    transform.Find("CurrentStatusPanel/TextCurrentStatus").GetComponent<TextMeshProUGUI>().text = (status.param_seed - this.param_seed["total"]) + " pt";

                    if (this.param_seed["total"] > 0)
                    {
                        transform.Find("ButtonOk").GetComponent<Button>().interactable = true;
                    }

                    if ((status.param_seed - this.param_seed["total"]) == 0)
                    {
                        foreach (string value2 in list)
                        {
                            transform.Find("MainPanel/" + value2 + "_btn_up").GetComponent<Button>().interactable = false;
                        }
                    }
                }

            }));

            Button btnDown = transform.Find("MainPanel/" + value + "_btn_down").GetComponent<Button>();
            btnDown.interactable = false;

            //↓ボタンクリック時イベントハンドラ
            btnDown.onClick.RemoveAllListeners();
            btnDown.onClick.AddListener((() =>
            {
                if (this.param_seed["total"] >= 1)
                {

                    if (int.Parse(transform.Find("MainPanel/" + value + "_add").GetComponent<TextMeshProUGUI>().text) <= 0)
                        return;

                    int add_point = 1;
                    if (value == "hp")
                        add_point = 7;

                    AudioManager.Instance.PlaySE("se_btn");
                    this.param_seed[value]--;

                    transform.Find("MainPanel/" + value + "_add").GetComponent<TextMeshProUGUI>().text = this.param_seed[value].ToString();

                    if (this.param_seed[value] == 0)
                        transform.Find("MainPanel/" + value + "_btn_down").GetComponent<Button>().interactable = false;

                    if (this.param_seed[value] < status.param_seed)
                        transform.Find("MainPanel/" + value + "_btn_up").GetComponent<Button>().interactable = true;

                    TextMeshProUGUI curr = transform.Find("MainPanel/" + "curr_" + value).GetComponent<TextMeshProUGUI>();
                    curr.text = (int.Parse(curr.text) - add_point).ToString();

                    this.param_seed["total"]--;
                    transform.Find("CurrentStatusPanel/TextCurrentStatus").GetComponent<TextMeshProUGUI>().text = (status.param_seed - this.param_seed["total"]) + " pt";

                    if (this.param_seed["total"] == 0)
                    {
                        transform.Find("ButtonOk").GetComponent<Button>().interactable = true;
                    }

                    if ((status.param_seed - this.param_seed["total"]) > 0)
                    {
                        foreach (string value2 in list)
                        {
                            transform.Find("MainPanel/" + value2 + "_btn_up").GetComponent<Button>().interactable = true;
                        }
                    }
                }
            }));
        }
    }

    public void onClickParamSeed()
    {
        if (this.param_seed["total"] == 0)
            return;

        AudioManager.Instance.PlaySE("se_btn");

        var add_att1 = transform.Find("MainPanel/att1_add").GetComponent<TextMeshProUGUI>().text;
        var add_att2 = transform.Find("MainPanel/att2_add").GetComponent<TextMeshProUGUI>().text;
        var add_att3 = transform.Find("MainPanel/att3_add").GetComponent<TextMeshProUGUI>().text;
        var add_def1 = transform.Find("MainPanel/def1_add").GetComponent<TextMeshProUGUI>().text;
        var add_def2 = transform.Find("MainPanel/def2_add").GetComponent<TextMeshProUGUI>().text;
        var add_def3 = transform.Find("MainPanel/def3_add").GetComponent<TextMeshProUGUI>().text;
        var add_spd = transform.Find("MainPanel/spd_add").GetComponent<TextMeshProUGUI>().text;
        var add_hp = transform.Find("MainPanel/hp_add").GetComponent<TextMeshProUGUI>().text;

        APIConnectManager.Instance.ParamUp(null, add_att1, add_att2, add_att3, add_def1, add_def2, add_def3, add_spd, add_hp, onEnd);

    }

    void onEnd(string json)
    {
        jsonParamUp response = JsonUtility.FromJson<jsonParamUp>(json);

        Debug.Log(response);

        if (response.result.Equals("ok"))
        {
            transform.gameObject.SetActive(false);
            if (CompleteHandler != null)
            {
                CompleteHandler?.Invoke();
                CompleteHandler = null;
            }
        }
        else 
        {
            Main.Instance.showDialogue(Utility.getText("API_ERROR_ParamUp_" + response.result));
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
