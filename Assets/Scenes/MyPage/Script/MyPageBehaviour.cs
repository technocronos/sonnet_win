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

public class MyPageBehaviour : BaseBehaviour
{

    public Image BG;

    public TextMeshProUGUI grade_name;
    public TextMeshProUGUI grade_pt;
    public TextMeshProUGUI raise_border;
    public TextMeshProUGUI abase_border;
    public TextMeshProUGUI param_seed;
    public Text hed_text;
    public Text acs_text;
    public Text wpn_text;
    public Text bod_text;

    public Button btn_param_seed;
    public GameObject objParamSeed;
    public GameObject ExpirePanel;

    public GameObject objEquipChange;
    public GameObject objGradeListPanel;
    public GameObject objGradeUserPanel;
    public GameObject objMemberListPanel;
    public GameObject objHistoryListPanel;
    public GameObject objBattleLogListPanel;

    public GameObject StatusPanel;
    public Transform gauge;
    public TextMeshProUGUI hp;
    public TextMeshProUGUI hp_max;

    //public GameObject batch_message;
    public GameObject batch_member;
    public Button btn_change;
    public Button btn_cloth_out;

    public TextMeshProUGUI paramup1_expire;
    public TextMeshProUGUI paramup2_expire;
    public TextMeshProUGUI paramup3_expire;

    public Button ButtonRareGet;
    public Button ButtonExpUp;
    public Button ButtonDtech;

    public GameObject ImageRareGet;
    public GameObject ImageExpUp;
    public GameObject ImageDtech;

    public AvatarBehaviour Avatar;
    private jsonConstants constants;
    public GameObject charaImage;

    public GameObject Arrow;

    public NaviController naviController;

    public Button ButtonHed;
    public Button ButtonWpn;
    public Button ButtonBod;
    public Button ButtonAcs;
    public Button ButtonClothOut;
    public Button ButtonMemberList;
    public Button ButtonHistoryList;
    public Button ButtonBattlelog;
    public Button ButtonGrade;

    Dictionary<string, Tweener> tweener = new Dictionary<string, Tweener>();
    public static MyPageBehaviour Instance
    {
        get
        {
            return instance;
        }
    }

    private static MyPageBehaviour instance;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        //BG.sprite = Utility.getAssetImage("Image/BG/bg2");

        instance = this;

        Debug.Log("MyPageBehaviour start..");
        setSafearea("MyPageCanvas");

        Header.Instance.SetTitle(Utility.getText("TEXT_MYPAGE"));

        //ナビのタッチは無効にしておく
        naviController.TouchPanel.gameObject.SetActive(false);
        naviController.gameObject.SetActive(false);
        Arrow.SetActive(false);

        //定数取得
        constants = APIConnectManager.Instance.login.constants;

        objEquipChange.SetActive(false);
        objParamSeed.SetActive(false);
        objGradeListPanel.SetActive(false);
        objGradeUserPanel.SetActive(false);
        objMemberListPanel.SetActive(false);
        objHistoryListPanel.SetActive(false);
        objBattleLogListPanel.SetActive(false);

        tweener["btn_param_seed"] = null;
        tweener["batch_member"] = null;

        //APIをたたく
        APIConnectManager.Instance.Status(onStart);

        DispatchEvent(CwEvent.SCENE_READY);
    }

    jsonStatus response { get; set; }


    void makeJson(string json)
    {
        response = JsonUtility.FromJson<jsonStatus>(json);

        Dictionary<string, object> jsonDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

        foreach (KeyValuePair<string, object> keyvalue in jsonDict)
        {
            if (keyvalue.Key == "PLAEQP")
            {
                try
                {
                    response.PLAEQP = new Dictionary<int, jsonEquip>();
                    Dictionary<int, jsonEquip> jsonDict2 = JsonConvert.DeserializeObject<Dictionary<int, jsonEquip>>(keyvalue.Value.ToString());

                    foreach (KeyValuePair<int, jsonEquip> keyvalue2 in jsonDict2)
                    {
                        if (keyvalue2.Value != null)
                        {
                            response.PLAEQP.Add(keyvalue2.Key, keyvalue2.Value);
                        }
                    }

                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                    for (int i = 1; i <= 4; i++)
                        response.PLAEQP.Add(i, null);
                }
            }
            else if (keyvalue.Key == "effectExpires")
            {
                try
                {
                    response.effectExpires = new Dictionary<int, jsonEffectExpires>();
                    Dictionary<int, jsonEffectExpires> jsonDict2 = JsonConvert.DeserializeObject<Dictionary<int, jsonEffectExpires>>(keyvalue.Value.ToString());

                    foreach (KeyValuePair<int, jsonEffectExpires> keyvalue2 in jsonDict2)
                    {
                        if (keyvalue2.Value != null)
                        {
                            response.effectExpires.Add(keyvalue2.Key, keyvalue2.Value);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                    response.effectExpires = null;
                }
            }



        }

    }


    void onStart(string json)
    {
        makeJson(json);

        //キャラ作成
        this.makeCharaAnim(response.chara.equip_info, charaImage);

        //チュートリアル中の場合
        if (Header.Instance.GetSummary().tutorial_step == constants.User_Info_Tutorial.TUTORIAL_EQUIP)
        {

            ButtonHed.interactable = false;
            ButtonWpn.interactable = false;
            ButtonBod.interactable = false;
            ButtonAcs.interactable = false;
            ButtonClothOut.interactable = false;
            ButtonMemberList.interactable = false;
            ButtonHistoryList.interactable = false;
            ButtonBattlelog.interactable = false;
            ButtonGrade.interactable = false;

            HomeApi summary = Header.Instance.GetSummary();
            summary.opening = Utility.getText("TEXT_NAVI_TUTORIAL_MYPAGE_1").Split("\n");

            summary.openingNum = summary.opening.Length;

            naviController.gameObject.SetActive(true);
            naviController.onStart(summary, null, TutorialNaviSpeakEnd);
        }

        reload();
    }

    void TutorialNaviSpeakEnd()
    {
        naviController.disappere();

        //ナビカーソルを表示する
        Arrow.SetActive(true);
        Arrow.GetComponent<ArrowBehaviour>().Show("down", 0, 65);
    }

    //---------------------------------------------------------------------------------------------------------
    /**
     * メイン処理
     */
    void reload()
    {
        //階級情報        
        grade_name.text = this.response.grade.grade_name;
        grade_pt.text = this.response.chara.grade_pt + "pt";
        raise_border.text = this.response.grade.raise_border.ToString();
        abase_border.text = this.response.grade.abase_border.ToString();

        //振り分けポイント情報
        param_seed.text = this.response.chara.param_seed + "pt"; ;

        //振り分けポイントがある場合
        if (this.response.chara.param_seed > 0)
        {
            btn_param_seed.gameObject.SetActive(true);

            //振り分けボタン
            btn_param_seed.onClick.RemoveAllListeners();
            btn_param_seed.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlaySE("se_btn");

                objParamSeed.SetActive(true);
                objParamSeed.GetComponent<ParamSeedBehaviour>().Show(restart);

            });

            //点滅
            if (tweener["btn_param_seed"] == null)
                tweener["btn_param_seed"] = btn_param_seed.transform.GetComponent<Image>().DOFade(0.0f, 1f).SetEase(Ease.InCubic).SetLoops(-1, LoopType.Yoyo);

        }
        else
        {
            btn_param_seed.gameObject.SetActive(false);
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


        //キャラ登場
        //self.CharaCanvas.pos(130, 80);
        //self.CharaCanvas.fadein();

        //未読メッセージバッチ表示
        if (Header.Instance.GetSummary().unreadCount > 0)
        {
            //batch_message.SetActive(true);
        }
        else
        {
            //batch_message.SetActive(false);
        }

        //仲間申請バッチ表示
        if (Header.Instance.GetSummary().unanswerCount > 0 || Header.Instance.GetSummary().unconfirmCount > 0)
        {
            batch_member.SetActive(true);
            //点滅
            if (tweener["batch_member"] == null)
                tweener["batch_member"] = batch_member.transform.GetComponent<Image>().DOFade(0.0f, 1f).SetEase(Ease.InCubic).SetLoops(-1, LoopType.Yoyo);
        }
        else
        {
            batch_member.SetActive(false);
        }

        setEquipInfo();
        showExpireInfo();
        showParamupItemInfo();
    }

    //装備ボタン
    public void onEquip(string eqp)
    {
        AudioManager.Instance.PlaySE("se_btn");
        objEquipChange.SetActive(true);
        objEquipChange.transform.GetComponent<EquipChangeBoxBehaviour>().Show(eqp);
    }

    //最強装備ボタン
    public void onChange()
    {
        AudioManager.Instance.PlaySE("se_btn");
        //チュートリアル中の場合
        if (Header.Instance.GetSummary().tutorial_step == constants.User_Info_Tutorial.TUTORIAL_EQUIP)
        {
            Arrow.SetActive(false);
        }
        equipChange("auto");
    }

    //全装備解除ボタン
    public void onClothOut()
    {
        AudioManager.Instance.PlaySE("se_btn");
        equipChange("release");
    }

    public void onMessage()
    {
        AudioManager.Instance.PlaySE("se_btn");
        //メッセージボタン
        //showMessageList(response.chara.user_id, "receive");
    }

    public void onMemberList()
    {
        AudioManager.Instance.PlaySE("se_btn");
        //仲間ボタン
        showMemberList(response.chara.user_id);

    }

    void showMemberList(int user_id)
    {
        objMemberListPanel.SetActive(true);
        objMemberListPanel.transform.GetComponent<MemberListBehaviour>().Show(user_id, null);
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
        showHistoryList(response.chara.user_id, Header.Instance.GetSummary().player_name);

    }
    void showHistoryList(int user_id, string player_name)
    {
        objHistoryListPanel.SetActive(true);
        objHistoryListPanel.transform.GetComponent<HistoryListBehaviour>().Show(user_id, player_name);
    }

    public void changeReload()
    {

        //マイページ画面ステータス情報取得API
        APIConnectManager.Instance.Status((string json) =>
        {
            makeJson(json);

            AudioManager.Instance.PlaySE("se_repair");

            Avatar.PlayAnimFlg("AvatarChange", () =>
            {
                this.setEquipInfo();
                this.setStatusInfo();

                //キャラ作成
                this.makeCharaAnim(response.chara.equip_info, charaImage);
            }, () =>
            {
                //チュートリアル中の場合、ナビを再表示
                if (Header.Instance.GetSummary().tutorial_step == constants.User_Info_Tutorial.TUTORIAL_EQUIP)
                {
                    HomeApi summary = Header.Instance.GetSummary();
                    summary.opening = Utility.getText("TEXT_NAVI_TUTORIAL_MYPAGE_2").Split("\n");

                    summary.openingNum = summary.opening.Length;

                    naviController.gameObject.SetActive(true);
                    naviController.onStart(summary, null, TutorialEnd);
                }
            });

        });
    }

    //---------------------------------------------------------------------------------------------------------
    /**
     * 自動装備、装備をはずす
     * "auto"/"release"  自動装備/解除
     */
    void equipChange(string func)
    {
        //APIをたたく
        APIConnectManager.Instance.EquipChange(response.chara.character_id, func, 0, 0, 0, 0, 0, (string json) =>
        {
            jsonEquipChange res = JsonUtility.FromJson<jsonEquipChange>(json);

            if (res.result == "ok")
            {
                changeReload();
            }
            else
            {
                switch (res.err_code)
                {
                    case "noitem":
                    case "not_me":
                        Main.Instance.showDialogue(Utility.getText("API_ERROR_EquipChange_" + res.err_code), null, 4);
                        break;
                    case "equipping":
                    case "maxlevel":
                    case "nomoney":
                    case "in_quest":
                        Main.Instance.showDialogue(Utility.getText("API_ERROR_EquipChange_" + res.err_code));
                        break;
                }
            }
        });
    }

    void TutorialEnd()
    {
        naviController.disappere();

        SceneController.Instance.Jump("Tutorial", (() =>
        {
            TutorialBehaviour tutorial = FindObjectOfType<TutorialBehaviour>() as TutorialBehaviour;
            tutorial.Param = new TutorialBehaviour.Parameter { TutorialStep = Header.Instance.GetSummary().tutorial_step };
        }));
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
    //---------------------------------------------------------------------------------------------------------
    /**
     * 再スタート時に呼び出す。振り分け後等に呼ばれる
     */
    void restart()
    {
        APIConnectManager.Instance.Home((string json) =>
        {
            HomeApi homeSummary = JsonUtility.FromJson<HomeApi>(json);

            Header.Instance.SetSummary(homeSummary);
            Footer.Instance.SetSummary(homeSummary);

            //APIをたたく
            APIConnectManager.Instance.Status((string json) =>
            {
                makeJson(json);

                reload();
            });
        });

    }

    /// <summary>
    /// 装備武器
    /// </summary>
    void setEquipInfo()
    {
        string evolstr = "[進化]";

        //ヘッド
        if (response.PLAEQP[3].item_id > 0)
        {
            if (response.PLAEQP[3].evolution == 1)
                response.PLAEQP[3].item_name += evolstr;

            hed_text.text = response.PLAEQP[3].item_name;
        }
        else
        {
            hed_text.text = Utility.getText("TEXT_NO_EQUIP");
        }

        //アクセサリ
        if (response.PLAEQP[4].item_id > 0)
        {
            if (response.PLAEQP[4].evolution == 1)
                response.PLAEQP[4].item_name += evolstr;

            acs_text.text = response.PLAEQP[4].item_name;
        }
        else
        {
            acs_text.text = Utility.getText("TEXT_NO_EQUIP");
        }

        //武器
        if (response.PLAEQP[1].item_id > 0)
        {
            if (response.PLAEQP[1].evolution == 1)
                response.PLAEQP[1].item_name += evolstr;

            wpn_text.text = response.PLAEQP[1].item_name;
        }
        else
        {
            wpn_text.text = Utility.getText("TEXT_NO_EQUIP");
        }

        //ボディ
        if (response.PLAEQP[2].item_id > 0)
        {
            if (response.PLAEQP[2].evolution == 1)
                response.PLAEQP[2].item_name += evolstr;

            bod_text.text = response.PLAEQP[2].item_name;
        }
        else
        {
            bod_text.text = Utility.getText("TEXT_NO_EQUIP");
        }
    }


    //---------------------------------------------------------------------------------------------------------
    /**
     * 効果を表示する
     */
    void showExpireInfo()
    {
        ExpirePanel.SetActive(true);

        ButtonRareGet.interactable = false;
        ButtonExpUp.interactable = false;
        ButtonDtech.interactable = false;

        ButtonRareGet.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "-----";
        ButtonExpUp.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "-----";
        ButtonDtech.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "-----";

        ImageExpUp.SetActive(true);
        ImageRareGet.SetActive(false);
        ImageDtech.SetActive(false);

        ImageExpUp.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "-----";
        ImageRareGet.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "-----";
        ImageDtech.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "-----";

        if (response.effectExpires != null)
        {
            var i = 0;
            foreach (KeyValuePair<int, jsonEffectExpires> keyvalue in response.effectExpires)
            {

                int type = keyvalue.Key;
                jsonEffectExpires entry = keyvalue.Value;

                if (type == constants.Character_Effect.TYPE_EXP_INCREASE)
                {
                    //発動時間
                    ButtonExpUp.interactable = true;
                    //経験値増加
                    ButtonExpUp.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = Utility.compareDate(entry.expire);
                    ImageExpUp.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = entry.effect_name + "(" + entry.value + "%)";

                    ImageExpUp.SetActive(true);
                    ImageRareGet.SetActive(false);
                    ImageDtech.SetActive(false);

                    ButtonExpUp.onClick.RemoveAllListeners();
                    ButtonExpUp.onClick.AddListener(() =>
                    {
                        AudioManager.Instance.PlaySE("se_btn");

                        ImageExpUp.SetActive(true);
                        ImageRareGet.SetActive(false);
                        ImageDtech.SetActive(false);

                    });
                }
                else if (type == constants.Character_Effect.TYPE_HP_RECOVER)
                {
                    //HP回復量増加(現在の所無し)
                }
                else if (type == constants.Character_Effect.TYPE_ATTRACT)
                {
                    ButtonRareGet.interactable = true;
                    ButtonRareGet.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = Utility.compareDate(entry.expire);
                    if (entry.value == 1)
                    {
                        ImageRareGet.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = Utility.getText("MYPAGE_ITEM_EFFECTS_TYPE_ATTRACT").Replace("{0}", constants.Item_Master.ITEM_RARE_ENCOUNT_LV1.ToString()).Replace("{1}", constants.Item_Master.ITEM_SRARE_ENCOUNT_LV1.ToString());
                    }
                    else if (entry.value == 2)
                    {
                        ImageRareGet.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = Utility.getText("MYPAGE_ITEM_EFFECTS_TYPE_ATTRACT").Replace("{0}", constants.Item_Master.ITEM_RARE_ENCOUNT_LV2.ToString()).Replace("{1}", constants.Item_Master.ITEM_SRARE_ENCOUNT_LV2.ToString());
                    }
                    else if (entry.value == 3)
                    {
                        ImageRareGet.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = Utility.getText("MYPAGE_ITEM_EFFECTS_TYPE_ATTRACT").Replace("{0}", constants.Item_Master.ITEM_RARE_ENCOUNT_LV3.ToString()).Replace("{1}", constants.Item_Master.ITEM_SRARE_ENCOUNT_LV3.ToString());
                    }

                    ImageExpUp.SetActive(false);
                    ImageRareGet.SetActive(true);
                    ImageDtech.SetActive(false);

                    ButtonRareGet.onClick.RemoveAllListeners();
                    ButtonRareGet.onClick.AddListener(() =>
                    {
                        AudioManager.Instance.PlaySE("se_btn");

                        ImageExpUp.SetActive(false);
                        ImageRareGet.SetActive(true);
                        ImageDtech.SetActive(false);
                    });
                }
                else if (type == constants.Character_Effect.TYPE_DTECH_POWUP)
                {

                    //必殺率上昇
                    ButtonDtech.interactable = true;
                    ButtonDtech.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = Utility.compareDate(entry.expire);

                    if (entry.value == 2)
                        ImageDtech.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = entry.effect_name + "(" + constants.Item_Master.ITEM_DTECH_UPPER_INVOKE + "%)" + "<br>"+ Utility.getText("MYPAGE_ITEM_EFFECTS_TYPE_DTECH_POWUP") +"(" + constants.Item_Master.ITEM_DTECH_UPPER_POWER + "%)";
                    else
                        ImageDtech.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = entry.effect_name + "(" + constants.Item_Master.ITEM_DTECH_UPPER_INVOKE + "%)";

                    ImageExpUp.SetActive(false);
                    ImageRareGet.SetActive(false);
                    ImageDtech.SetActive(true);

                    ButtonDtech.onClick.RemoveAllListeners();
                    ButtonDtech.onClick.AddListener(() =>
                    {
                        AudioManager.Instance.PlaySE("se_btn");

                        ImageExpUp.SetActive(false);
                        ImageRareGet.SetActive(false);
                        ImageDtech.SetActive(true);
                    });
                }
                i++;
            };
        }
    }

    //---------------------------------------------------------------------------------------------------------
    /**
     * ステータスUPアイテムの使用状況を表示する
     */
    void showParamupItemInfo()
    {
        paramup1_expire.text = Utility.getText("MYPAGE_ITEM_EFFECTS_PARMUP").Replace("{0}", (20 - response.paramupItemStatus.param1).ToString());
        paramup2_expire.text = Utility.getText("MYPAGE_ITEM_EFFECTS_PARMUP").Replace("{0}", (20 - response.paramupItemStatus.param2).ToString()); 
        paramup3_expire.text = Utility.getText("MYPAGE_ITEM_EFFECTS_PARMUP").Replace("{0}", (20 - response.paramupItemStatus.param3).ToString()); 
    }
}
