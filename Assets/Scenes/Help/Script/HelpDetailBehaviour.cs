using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MyScene;
using System;
using UnityEngine.UI;
using Scenes.Common.Scripts;
using AppsFlyerSDK;

public class HelpDetailBehaviour : BaseBehaviour, IAppsFlyerUserInvite
{
    public GameObject Content;

    public TextMeshProUGUI Title;

    public TMP_InputField InputInheritCode;

    public QRCodeEncodeController qrCodeEncodeController;
    public RawImage InviteQRCodeImage;

    jsonConstants constants;

    public class Parameter
    {
        public string id;
    }

    public Parameter Param;
    jsonHelpContents help;
    Transform helpcontents;

    // Start is called before the first frame update
    protected override void Start()
    {
        //base.Start();

        //定数取得
        constants = APIConnectManager.Instance.login.constants;

        ListClear();

        //APIをたたく
        APIConnectManager.Instance.HelpList(Param.id, onStart);

        DispatchEvent(CwEvent.SCENE_READY);
    }

    void onStart(string json)
    {
        jsonHelpDetail response = JsonUtility.FromJson<jsonHelpDetail>(json);
        help = response.help;

        Title.text = help.help_title;

        helpcontents = Content.transform.Find(Param.id);

        if (helpcontents == null)
        {
            helpcontents = Content.transform.Find("common");
        }

        if (helpcontents != null)
        {
            int i = 1;
            foreach (string body in response.help.help_body)
            {

                TextMeshProUGUI helptext = helpcontents.transform.Find("Text" + i).GetComponent<TextMeshProUGUI>();
                helptext.text = body;

                helpcontents.gameObject.SetActive(true);
                int now = (int)(Utility.GetUnixTime(System.DateTime.Now) / 1000);
                bool IN_BTC = DateTime.Parse(constants.BTC_CAMPAIGN_START_DATE) <= System.DateTime.Now && DateTime.Parse(constants.BTC_CAMPAIGN_END_DATE) > System.DateTime.Now;

                if (Param.id == "vcoin-about")
                {
                    if (IN_BTC)
                    {
                        var text = Utility.getText("TEXT_HELPDETAIL_IN_BTC1").Replace("{0}", constants.BTC_CAMPAIGN_NAME) + "\n\n\n " + Utility.getText("TEXT_HELPDETAIL_IN_BTC2").Replace("{0}", constants.BTC_CAMPAIGN_START_DATE).Replace("{1}", constants.BTC_CAMPAIGN_END_DATE) + "\n\n";
                        helptext.text = text + helptext.text;
                    }

                    if (constants.BTC_CAMPAIGN_PAYMENT_STOP)
                    {
                        var text = Utility.getText("TEXT_HELPDETAIL_END_BTC").Replace("{0}", constants.BTC_CAMPAIGN_NAME);
                        helptext.text = text + helptext.text;
                    }

                }
                else if (Param.id == "other-ranking")
                {
                    //定数置換
                    if (helptext.text.Contains("{IN_BTC_EXPLAIN}"))
                    {
                        helptext.text = helptext.text.Replace("{IN_BTC_EXPLAIN}", Utility.getText("help_master_other-ranking_IN_BTC_EXPLAIN").Replace("{0}", constants.BTC_CAMPAIGN_NAME));
                    }

                    if (helptext.text.Contains("{PRESENT_INFO}"))
                    {
                        var text = Utility.getText("help_master_other-ranking_PRESENT_INFO");
                        if (IN_BTC)
                        {
                            text = Utility.getText("help_master_other-ranking_PRESENT_INFO2");
                        }
                        helptext.text = helptext.text.Replace("{PRESENT_INFO}", text);
                    }

                    HomeApi summary = Header.Instance.GetSummary();

                    //非開催時は表示しない
                    if (summary.battle_rank_info.status != 3)
                    {
                        if (helptext.text.Contains("{PRESENT_EQUIP}"))
                        {
                            var text = Utility.getText("TEXT_GIVEN_ITEM") + "\n";

                            foreach (KeyValuePair<int, jsonRanking_Log_Prize> rankKeyValue in constants.Ranking_Log_Prize_Week)
                            {
                                if (rankKeyValue.Value.set_name != null)
                                    text += Utility.getText("TEXT_RANK").Replace("{0}", rankKeyValue.Key.ToString())  + "：" + rankKeyValue.Value.item_name + "(" + rankKeyValue.Value.set_name + ") \n";
                            }

                            helptext.text = helptext.text.Replace("{PRESENT_EQUIP}", text);
                        }
                        if (helptext.text.Contains("{PRESENT_BTC}"))
                        {
                            if (IN_BTC)
                            {
                                var text = Utility.getText("TEXT_GIVEN_BITCOIN") + "\n";

                                text +=  Utility.getText("help_master_other-ranking_IN_BTC_EXPLAIN2").Replace("{0}", constants.BTC_CAMPAIGN_NAME) + "\n" + "\n";

                                foreach (KeyValuePair<int, jsonRanking_Log_Prize> rankKeyValue in constants.Ranking_Log_Prize_Week)
                                {
                                    if (rankKeyValue.Value.btc > 0)
                                        text += Utility.getText("TEXT_RANK").Replace("{0}", rankKeyValue.Key.ToString()) + "：" + Utility.getVCoinAmount(rankKeyValue.Value.btc) + "BTC" + "\n";
                                }

                                helptext.text = helptext.text.Replace("{PRESENT_BTC}", text);
                            }
                        }
                    }
                    else
                    {
                        if (helptext.text.Contains("{PRESENT_EQUIP}"))
                        {
                            var text = Utility.getText("TEXT_GIVEN_ITEM") + "\n\n";
                            text += Utility.getText("TEXT_GIVEN_ITEM_IN_READY") + "\n" + "\n";

                            helptext.text = helptext.text.Replace("{PRESENT_EQUIP}", text);
                        }

                        if (helptext.text.Contains("{PRESENT_BTC}"))
                        {
                            if (IN_BTC)
                            {
                                var text = Utility.getText("TEXT_GIVEN_BITCOIN") + "\n\n";

                                text += Utility.getText("help_master_other-ranking_IN_BTC_EXPLAIN2").Replace("{0}", constants.BTC_CAMPAIGN_NAME) + "\n" + "\n";
                                text += Utility.getText("TEXT_GIVEN_BTC_IN_READY") + "\n" + "\n";

                                helptext.text = helptext.text.Replace("{PRESENT_BTC}", text);
                            }
                        }
                    }
                }
                else if (Param.id == "other-shoutai")
                {
                    var text = constants.Invitation_Log.INVITE_BTC + " BTC" + "\n";
                    foreach (jsonInvitation_Bonus bonus in constants.Invitation_Log.INVITE_BONUS)
                    {
                        text += bonus.item_name + " " + Utility.getText("TEXT_KOSUU").Replace("{0}", bonus.count.ToString()) + " \n";
                    }
                    helptext.text = helptext.text.Replace("{INVITE_PRIZE}", text);

                    text = constants.Invitation_Log.INVITED_BTC + " BTC" + "\n";
                    foreach (jsonInvitation_Bonus bonus in constants.Invitation_Log.ANSWER_BONUS)
                    {
                        text += bonus.item_name + " " + Utility.getText("TEXT_KOSUU").Replace("{0}", bonus.count.ToString()) + " \n";
                    }
                    helptext.text = helptext.text.Replace("{INVITED_PRIZE}", text);

                    AppsFlyerSDK.AppsFlyer.setAppInviteOneLinkID("0oGJ");

                    Dictionary<string, string> parameters = new Dictionary<string, string>();

                    //REFERRER_ID
                    parameters.Add("deep_link_value", "Main");
                    parameters.Add(Settings.AF_INVITE_KEY, Header.Instance.GetSummary().chara.user_id.ToString());
                    AppsFlyerSDK.AppsFlyer.setCustomerUserId(Header.Instance.GetSummary().chara.user_id.ToString());

                    AppsFlyerSDK.AppsFlyer.generateUserInviteLink(parameters, this);
                }

                //定数置換
                if (helptext.text.Contains("{DUEL_LIMIT_ON_DAY_RIVAL}"))
                {
                    helptext.text = helptext.text.Replace("{DUEL_LIMIT_ON_DAY_RIVAL}", constants.DUEL_LIMIT_ON_DAY_RIVAL.ToString());
                }

                if (helptext.text.Contains("{BATTLE_RANK_WEEK}"))
                {
                    helptext.text = helptext.text.Replace("{BATTLE_RANK_WEEK}", constants.BATTLE_RANK_WEEK.ToString());
                }

                if (helptext.text.Contains("{BTC_CAMPAIGN_NAME}"))
                {
                    helptext.text = helptext.text.Replace("{BTC_CAMPAIGN_NAME}", constants.BTC_CAMPAIGN_NAME.ToString());
                }

                if (helptext.text.Contains("{VCOIN_FEE}"))
                {
                    helptext.text = helptext.text.Replace("{VCOIN_FEE}", constants.VCOIN_FEE.ToString());
                }

                if (helptext.text.Contains("{VCOIN_MINIMAM}"))
                {
                    helptext.text = helptext.text.Replace("{VCOIN_MINIMAM}", constants.VCOIN_MINIMAM.ToString());
                }

                if (helptext.text.Contains("{VCOIN_MINIMAM_PAYMENT}"))
                {
                    helptext.text = helptext.text.Replace("{VCOIN_MINIMAM_PAYMENT}", constants.VCOIN_MINIMAM_PAYMENT.ToString());
                }

                //ショップボタンが有る場合
                Transform objButtonShop = helpcontents.transform.Find("ButtonShop");
                if (objButtonShop != null)
                {
                    Button ButtonShop = objButtonShop.GetComponent<Button>();
                    ButtonShop.onClick.RemoveAllListeners();
                    ButtonShop.onClick.AddListener(() =>
                    {
                        AudioManager.Instance.PlaySE("se_btn");

                        SceneController.Instance.Jump("Shop");
                        Main.Instance.SettingsView.SetActive(false);
                    });

                }

                //装備ボタンが有る場合
                Transform objButtonEquip = helpcontents.transform.Find("ButtonEquip");
                if (objButtonEquip != null)
                {
                    Button ButtonEquip = objButtonEquip.GetComponent<Button>();
                    ButtonEquip.onClick.RemoveAllListeners();
                    ButtonEquip.onClick.AddListener(() =>
                    {
                        AudioManager.Instance.PlaySE("se_btn");

                        SceneController.Instance.Jump("Equip");
                        Main.Instance.SettingsView.SetActive(false);
                    });

                }

                //対戦ボタンが有る場合
                Transform objButtonBattleList = helpcontents.transform.Find("ButtonBattleList");
                if (objButtonBattleList != null)
                {
                    Button ButtonBattleList = objButtonBattleList.GetComponent<Button>();
                    ButtonBattleList.onClick.RemoveAllListeners();
                    ButtonBattleList.onClick.AddListener(() =>
                    {
                        AudioManager.Instance.PlaySE("se_btn");

                        SceneController.Instance.Jump("BattleList");
                        Main.Instance.SettingsView.SetActive(false);
                    });
                }

                //対戦ボタンが有る場合
                Transform objButtonBattle = helpcontents.transform.Find("ButtonBattle");
                if (objButtonBattle != null)
                {
                    Button ButtonBattle = objButtonBattle.GetComponent<Button>();
                    ButtonBattle.onClick.RemoveAllListeners();
                    ButtonBattle.onClick.AddListener(() =>
                    {
                        AudioManager.Instance.PlaySE("se_btn");

                    //チュートリアル用
                    SceneController.Instance.Jump("Battle", (() =>
                        {
                            BattleBehaviour _battle = FindObjectOfType<BattleBehaviour>() as BattleBehaviour;
                            _battle.Param = new BattleBehaviour.Parameter
                            {
                                tutorial = true,
                                from = "help",
                            };
                        }));
                        Main.Instance.SettingsView.SetActive(false);
                    });
                }

                //引き継ぎコードがある場合
                Transform objInheritCode = helpcontents.transform.Find("InheritCode");
                if (objInheritCode != null)
                {
                    TMP_InputField InheritCode = objInheritCode.GetComponent<TMP_InputField>();
                    InheritCode.text = Header.Instance.GetSummary().chara.user.platform_uid;
                }
                //サポートメールアドレスがある場合
                Transform objMail = helpcontents.transform.Find("Mail");
                if (objMail != null)
                {
                    TMP_InputField MailAddress = objMail.GetComponent<TMP_InputField>();
                    MailAddress.text = Settings.SUPPORT_MAIL_ADDRESS;
                }
                //ユーザーIDがある場合
                Transform objUserId = helpcontents.transform.Find("UserId");
                if (objUserId != null)
                {
                    TMP_InputField UserId = objUserId.GetComponent<TMP_InputField>();
                    UserId.text = Header.Instance.GetSummary().chara.user_id.ToString();
                }
                //ユーザー名がある場合
                Transform objUserName = helpcontents.transform.Find("UserName");
                if (objUserName != null)
                {
                    TMP_InputField UserName = objUserName.GetComponent<TMP_InputField>();
                    UserName.text = Header.Instance.GetSummary().player_name;
                }
                i++;
            }

        }
        else
        {
            helpcontents = Content.transform.Find("common");
            TextMeshProUGUI helptext = helpcontents.transform.Find("Text").GetComponent<TextMeshProUGUI>();
            helptext.text = Utility.getText("TEXT_HELP_IN_READY");
            Content.transform.Find("common").gameObject.SetActive(true);
        }
    }

    private void OnGUI()
    {
        //高さ再設定
        if (helpcontents != null)
        {
            helpcontents.GetComponent<VerticalLayoutGroup>().childForceExpandHeight = false;
            helpcontents.GetComponent<VerticalLayoutGroup>().childForceExpandHeight = true;
        }
    }

    /// <summary>
    /// リストを全部消す
    /// </summary>
    void ListClear()
    {
        foreach (Transform n in Content.transform)
        {
            n.gameObject.SetActive(false);
        }
    }

    //引き継ぎ
    void onInherit()
    {
        AudioManager.Instance.PlaySE("se_btn");

        var inherit_code = InputInheritCode.text;

        if (inherit_code == "")
        {
            Main.Instance.showDialogue(Utility.getText("TEXT_INPUT_HIKITSUGI_CODE"), null, 4);
        }
        else
        {
            Main.Instance.showConfirm(Utility.getText("TEXT_NAVI_CONFIRM_HIKITSUGI"), () =>
            {
                APIConnectManager.Instance.Inherit(inherit_code, onInherit);
            });
        }

    }

    void onInherit(string json)
    {
        Debug.Log("RegistAnim onInherit json =" + json);

        APIConnectManager.Instance.regist = JsonUtility.FromJson<jsonRegist>(json);

        jsonRegist regInfo = APIConnectManager.Instance.regist;

        if (regInfo.result == 1)
        {
            Main.Instance.showDialogue(Utility.getText("TEXT_NAVI_FINISH_HIKITSUGI2"), () =>
            {
                SceneController.Instance.Jump("Title");
            });
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
    public void TapClose()
    {
        AudioManager.Instance.PlaySE("se_btn");

        SceneController.Instance.ClosePopUpName("HelpDetail");
    }

    public void onInviteLinkGenerated(string link)
    {
        AppsFlyer.AFLog("onInviteLinkGenerated", link);

        // エンコード完了時に呼ばれるイベント
        qrCodeEncodeController.onQREncodeFinished += encodeFinished;

        // 指定した文字列をエンコードする
        qrCodeEncodeController.Encode(link, QRCodeEncodeController.CodeMode.QR_CODE);
    }

    public void onInviteLinkGeneratedFailure(string error)
    {
        AppsFlyer.AFLog("onInviteLinkGeneratedFailure", error);

    }

    public void onOpenStoreLinkGenerated(string link)
    {
        AppsFlyer.AFLog("onOpenStoreLinkGenerated", link);
    }


    /** エンコード完了時に呼ばれる */
    void encodeFinished(Texture2D texture)
    {
        if (texture != null)
        {
            // そのまま表示
            InviteQRCodeImage.texture = texture;
        }
    }
}
