using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using MyScene;
using Scenes.Common.Scripts;

public class HisPageBehaviour : BaseBehaviour
{
    public Image BG;

    public TextMeshProUGUI TextName;
    public TextMeshProUGUI TextGrade;
    public TextMeshProUGUI TextLv;
    public TextMeshProUGUI TextRelation;
    public TextMeshProUGUI TextHistory;

    public Text hed_text;
    public Text acs_text;
    public Text wpn_text;
    public Text bod_text;

    public GameObject objGradeListPanel;
    public GameObject objGradeUserPanel;
    public GameObject objMemberListPanel;
    public GameObject objHistoryListPanel;
    public GameObject objBattleLogListPanel;

    public GameObject StatusPanel;
    public Transform gauge;
    public TextMeshProUGUI hp;
    public TextMeshProUGUI hp_max;

    public GameObject CharaImage;

    public Button ButtonMemberList;
    public Button ButtonHistoryList;
    public Button ButtonBattlelog;
    public Button ButtonGrade;
    public Button ButtonMemberApply;
    public Button ButtonMemberRemove;
    public Button ButtonMemberInApply;
    public Button ButtonRival;
    public Button ButtonBattle;

    public GameObject Arrow;

    public NaviController naviController;

    private jsonConstants constants;

    public class Parameter
    {
        public int userId;
    }

    public Parameter Param;

    public static HisPageBehaviour Instance
    {
        get
        {
            return instance;
        }
    }

    private static HisPageBehaviour instance;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        BG.sprite = Utility.getAssetImage("Image/BG/bg2");

        instance = this;

        Debug.Log("HisPageBehaviour start.. userId=" + Param.userId);
        setSafearea("HisPageCanvas");

        Header.Instance.SetTitle(Utility.getText("TEXT_HISPAGE"));

        //このページに来た場合は他人のページフッター
        Footer.Instance.setUserId(Param.userId);

        //定数取得
        constants = APIConnectManager.Instance.login.constants;

        objGradeListPanel.SetActive(false);
        objGradeUserPanel.SetActive(false);
        objMemberListPanel.SetActive(false);
        objHistoryListPanel.SetActive(false);
        objBattleLogListPanel.SetActive(false);

        //ナビのタッチは無効にしておく
        naviController.TouchPanel.gameObject.SetActive(false);
        naviController.gameObject.SetActive(false);
        Arrow.SetActive(false);

        //APIをたたく
        APIConnectManager.Instance.HisPage(Param.userId, onStart);

        AudioManager.Instance.PlayBGM("bgm_menu", AudioManager.BGM_VOLUME_DEFULT);
        DispatchEvent(CwEvent.SCENE_READY);
    }

    jsonHisPage response { get; set; }


    jsonHisPage makeJson(string json)
    {
        jsonHisPage response = JsonUtility.FromJson<jsonHisPage>(json);

        Dictionary<string, object> jsonDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

        foreach (KeyValuePair<string, object> keyvalue in jsonDict)
        {
            if (keyvalue.Key == "chara")
            {
                Dictionary<string, object> jsonDict2 = JsonConvert.DeserializeObject<Dictionary<string, object>>(keyvalue.Value.ToString());

                foreach (KeyValuePair<string, object> keyvalue2 in jsonDict2)
                {
                    if (keyvalue2.Key == "equip")
                    {

                        try
                        {
                            response.chara.equip = new Dictionary<int, jsonEquip>();
                            Dictionary<int, jsonEquip> jsonDict3 = JsonConvert.DeserializeObject<Dictionary<int, jsonEquip>>(keyvalue2.Value.ToString());

                            foreach (KeyValuePair<int, jsonEquip> keyvalue3 in jsonDict3)
                            {
                                if (keyvalue3.Value != null)
                                {
                                    response.chara.equip.Add(keyvalue3.Key, keyvalue3.Value);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.Log(e.Message);
                            response.chara.equip = null;
                        }

                    }
                }


            }
        }

        return response;
    }


    void onStart(string json)
    {
        response = makeJson(json);

        reload();
    }

    //---------------------------------------------------------------------------------------------------------
    /**
     * メイン処理
     */
    void reload()
    {

        //ユーザー名
        TextName.text = response.chara.player_name;
        //キャラ作成
        Main.Instance.makeCharaAnim(response.chara.equip_info, CharaImage);

        //階級情報        
        TextGrade.text = this.response.chara.grade_name;
        TextLv.text = response.chara.level.ToString();
        TextHistory.text = Utility.getText("TEXT_SEISEKI").Replace("{0}", response.ctour.win.ToString()).Replace("{1}", response.ctour.lose.ToString()).Replace("{2}", response.ctour.draw.ToString());

        if (response.isMember)
        {
            //仲間解除ボタンを出す
            ButtonMemberApply.gameObject.SetActive(false);
            ButtonMemberRemove.gameObject.SetActive(true);
            ButtonMemberInApply.gameObject.SetActive(false);
            TextRelation.text = Utility.getText("TEXT_IS_FRIEND");
        }
        else if (response.isApproaching)
        {
            //申請中ボタンを出す
            ButtonMemberApply.gameObject.SetActive(false);
            ButtonMemberRemove.gameObject.SetActive(false);
            ButtonMemberInApply.gameObject.SetActive(true);
            TextRelation.text = Utility.getText("TEXT_IN_APPLY");
        }
        else
        {
            //友達申請ボタンを出す
            ButtonMemberApply.gameObject.SetActive(true);
            ButtonMemberRemove.gameObject.SetActive(false);
            ButtonMemberInApply.gameObject.SetActive(false);
            TextRelation.text = Utility.getText("TEXT_NOT_FRIEND");
        }

        setStatusInfo();

        // HPゲージを更新。
        int _hp = this.response.chara.hp;
        float _hp_max = this.response.chara.hp_max;
        float hp_val = Mathf.Min(_hp, _hp_max);

        float gauge_width = gauge.transform.GetComponent<RectTransform>().rect.width; ;

        int posx = (int)(((hp_val * 1.0f) / _hp_max) * gauge_width);
        gauge.transform.localPosition = new Vector3(posx - gauge_width, 0, 0);

        hp.text = _hp.ToString();
        hp_max.text = _hp_max.ToString();

        setEquipInfo();

        //回数制限
        if (response.canBattle == "count_rival")
        {

            ButtonBattle.interactable = false;

        }

        int TUTORIAL_HISPAGE = PlayerPrefs.GetInt(Settings.TUTORIAL_HISPAGE, 0);

        //チュートリアル
        if (TUTORIAL_HISPAGE == 0)
        {

            HomeApi summary = Header.Instance.GetSummary();
            summary.opening = Utility.getText("TEXT_NAVI_TUTORIAL_HISPAGE_1").Split("\n");

            summary.openingNum = summary.opening.Length;

            naviController.gameObject.SetActive(true);
            naviController.onStart(summary, null, () =>
            {
                naviController.disappere();
                //二度と表示しない
                PlayerPrefs.SetInt(Settings.TUTORIAL_HISPAGE, 1);
            });
        }


    }


    public void onMemberApply()
    {
        if (response.isApproaching == true)
            return;

        AudioManager.Instance.PlaySE("se_btn");

        string text = "";
        string approach = "0";
        string dissolve = "0";

        if (response.isMember)
        {
            text = Utility.getText("HISPAGE_FIREND_LIFT_CONFIRM");
            approach = "0";
            dissolve = "1";
        }
        else if (response.isApproaching == false)
        {
            text = Utility.getText("HISPAGE_FIREND_APPLY_CONFIRM");
            approach = "1";
            dissolve = "0";
        }

        Main.Instance.showConfirm(text, () =>
        {
            AudioManager.Instance.PlaySE("se_btn");

            //APIをたたく
            APIConnectManager.Instance.Approach(Param.userId, approach, dissolve, (string json) =>
            {
                jsonApproachResult res = JsonUtility.FromJson<jsonApproachResult>(json);

                var text = "";
                if (res.result == "ok")
                {
                    if (approach == "1")
                    {
                        text = Utility.getText("HISPAGE_FIREND_APPLY_RESULT");
                    }
                    else if (dissolve == "1")
                    {
                        text = Utility.getText("HISPAGE_FIREND_LIFT_RESULT");

                        //ヘッダーを更新する
                        HomeApi summary = Header.Instance.GetSummary();
                        summary.member.current -= 1;
                        Header.Instance.SetSummary(summary);
                    }
                }
                else if (res.result == "error")
                {
                    if (res.err_code == "recipient_limit")
                    {
                        text = Utility.getText("API_ERROR_Approach_" + res.err_code);
                    }
                    else if (res.err_code == "inviter_limit")
                    {
                        text = Utility.getText("API_ERROR_Approach_" + res.err_code);
                    }
                    else if (res.err_code == "cross_request")
                    {
                        text = Utility.getText("API_ERROR_Approach_" + res.err_code);
                    }
                    else if (res.err_code == "member_already")
                    {
                        text = Utility.getText("API_ERROR_Approach_" + res.err_code);
                    }
                    else if (res.err_code == "self_request")
                    {
                        text = Utility.getText("API_ERROR_Approach_" + res.err_code);
                    }
                    else
                    {
                        text = Utility.getText("API_ERROR_Approach_unknown");
                    }
                }

                Main.Instance.showDialogue(text, () =>
                {
                    //APIをたたく
                    APIConnectManager.Instance.HisPage(Param.userId, onStart);
                });

            });
        });

    }

    public void onBattleConfirm()
    {
        AudioManager.Instance.PlaySE("se_btn");

        SceneController.Instance.Jump("BattleConfirm", () =>
        {
            BattleConfirmBehaviour scene = FindObjectOfType<BattleConfirmBehaviour>() as BattleConfirmBehaviour;
            scene.Param = new BattleConfirmBehaviour.Parameter
            {
                rivalId = response.chara.character_id,
                BackTo = "scene=HisPage&his_user_id=" + Param.userId,
            };
        });
    }
    public void onBattleList()
    {
        AudioManager.Instance.PlaySE("se_btn");

        SceneController.Instance.Jump("BattleList");
    }

    public void onMemberList()
    {
        AudioManager.Instance.PlaySE("se_btn");
        //仲間ボタン
        showMemberList(response.chara.user_id, response.chara.player_name);

    }

    void showMemberList(int user_id, string player_name)
    {
        objMemberListPanel.SetActive(true);
        objMemberListPanel.transform.GetComponent<MemberListBehaviour>().Show(user_id, player_name);
    }


    public void onBattleLog()
    {
        AudioManager.Instance.PlaySE("se_btn");
        //戦歴ボタン
        showBattlelogList(response.chara.character_id);
    }
    void showBattlelogList(int character_id)
    {
        objBattleLogListPanel.SetActive(true);
        objBattleLogListPanel.transform.GetComponent<BattleLogListBehaviour>().Show(character_id);
    }
    public void onHistoryList()
    {
        AudioManager.Instance.PlaySE("se_btn");
        //履歴ボタン
        showHistoryList(response.chara.user_id, response.chara.player_name);

    }
    void showHistoryList(int user_id, string player_name)
    {
        objHistoryListPanel.SetActive(true);
        objHistoryListPanel.transform.GetComponent<HistoryListBehaviour>().Show(user_id, player_name);
    }


    public void onGradeList()
    {
        AudioManager.Instance.PlaySE("se_btn");

        objGradeListPanel.SetActive(true);
        objGradeListPanel.transform.GetComponent<GradeListBehaviour>().Show();
    }

    //---------------------------------------------------------------------------------------------------------
    /*
     * ステータス情報を更新する
     *
    */
    void setStatusInfo()
    {
        //ステータス情報
        StatusPanel.transform.Find("att1").GetComponent<TextMeshProUGUI>().text = response.chara.total_attack1.ToString();
        StatusPanel.transform.Find("att2").GetComponent<TextMeshProUGUI>().text = response.chara.total_attack2.ToString();
        StatusPanel.transform.Find("att3").GetComponent<TextMeshProUGUI>().text = response.chara.total_attack3.ToString();
        StatusPanel.transform.Find("spd").GetComponent<TextMeshProUGUI>().text = response.chara.total_speed.ToString();

        StatusPanel.transform.Find("def1").GetComponent<TextMeshProUGUI>().text = response.chara.total_defence1.ToString();
        StatusPanel.transform.Find("def2").GetComponent<TextMeshProUGUI>().text = response.chara.total_defence2.ToString();
        StatusPanel.transform.Find("def3").GetComponent<TextMeshProUGUI>().text = response.chara.total_defence3.ToString();
        StatusPanel.transform.Find("defX").GetComponent<TextMeshProUGUI>().text = response.chara.total_defenceX.ToString();
    }

    /// <summary>
    /// 装備武器
    /// </summary>
    void setEquipInfo()
    {
        if (response.chara.equip == null)
        {
            hed_text.text = Utility.getText("TEXT_NO_EQUIP");
            acs_text.text = Utility.getText("TEXT_NO_EQUIP");
            wpn_text.text = Utility.getText("TEXT_NO_EQUIP");
            bod_text.text = Utility.getText("TEXT_NO_EQUIP");
            return;
        }

        string evolstr = "[" + Utility.getText("TEXT_EQUIP_EVOLUTION") +  "]";

        //ヘッド
        if (response.chara.equip.ContainsKey(3))
        {
            if (response.chara.equip[3].evolution == 1)
                response.chara.equip[3].item_name += evolstr;

            hed_text.text = response.chara.equip[3].item_name;
        }
        else
        {
            hed_text.text = Utility.getText("TEXT_NO_EQUIP");
        }

        //アクセサリ
        if (response.chara.equip.ContainsKey(4))
        {
            if (response.chara.equip[4].evolution == 1)
                response.chara.equip[4].item_name += evolstr;

            acs_text.text = response.chara.equip[4].item_name;
        }
        else
        {
            acs_text.text = Utility.getText("TEXT_NO_EQUIP");
        }

        //武器
        if (response.chara.equip.ContainsKey(1))
        {
            if (response.chara.equip[1].evolution == 1)
                response.chara.equip[1].item_name += evolstr;

            wpn_text.text = response.chara.equip[1].item_name;
        }
        else
        {
            wpn_text.text = Utility.getText("TEXT_NO_EQUIP");
        }

        //ボディ
        if (response.chara.equip.ContainsKey(2))
        {
            if (response.chara.equip[2].evolution == 1)
                response.chara.equip[2].item_name += evolstr;

            bod_text.text = response.chara.equip[2].item_name;
        }
        else
        {
            bod_text.text = Utility.getText("TEXT_NO_EQUIP");
        }
    }


}
