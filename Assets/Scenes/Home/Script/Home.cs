using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MyScene;
using Scenes.Common.Scripts;
using UnityEngine.Localization.Settings;

public class Home : BaseBehaviour
{
    public GameObject Avatar;
    public Image CharaImage;

    public GameObject AvatarAnim;
    public GameObject CharaImageAnim;

    public TextMeshProUGUI player_name;
    public Button QuestButton;
    public Button BattleButton;

    public GameObject infoFrame;
    public GameObject infoContent;
    public GameObject info;
    public ScrollRect infoScrollRect;
    public GameObject infoView;
    public GameObject SallyQuestPanel;
    public TextMeshProUGUI infoViewDetail;
    public TextMeshProUGUI infoTitle;
    public NaviController naviController;
    public GameObject ConfirmWin;
    public GameObject infoLoading;

    public GameObject RaidPalel;
    public TextMeshProUGUI RaidButtonText;

    public GameObject Arrow;

    public Sprite btn_battle_en_disable;
    public Sprite quest_disable_en;
    
    public Image BG;

    public GameObject BannarPanel;
    public Button Bannar;
    public TextMeshProUGUI BannarText;

    private int _page = 0;
    private bool _isUpdate = true;
    private int _totalPage = 0;

    private jsonConstants constants;

    HomeApi homeSummary = null;

    private Sequence seq;
    private GameObject objInfoLoading;

    

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        //BG.sprite = Utility.getAssetImage("Image/BG/bg1");

        //safearea対応
        setSafearea("HomeCanvas");
        //定数取得
        constants = APIConnectManager.Instance.login.constants;

        //このページに来た場合は他人のページフッターはリセット
        Footer.Instance.setUserId(0);

        //ナビのタッチは無効にしておく
        naviController.TouchPanel.gameObject.SetActive(false);
        //実行中クエストパネルまだ非表示
        SallyQuestPanel.SetActive(false);
        //確認はまだ非表示
        ConfirmWin.SetActive(false);

        info.SetActive(false);
        infoFrame.SetActive(false);
        infoLoading.SetActive(false);

        RaidPalel.SetActive(false);

        Arrow.SetActive(false);
        BannarPanel.SetActive(false);

        int _lang = PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        if(_lang != 0) {
            SpriteState BattleState = new SpriteState();
            BattleState.disabledSprite = btn_battle_en_disable;
            BattleButton.spriteState = BattleState;

            SpriteState QuestState = new SpriteState();
            QuestState.disabledSprite = quest_disable_en;
            QuestButton.spriteState = QuestState;
        }

        //homeAPIをたたく
        APIConnectManager.Instance.Home(onLoaded);
    }

    private void onLoaded(string json)
    {
        //API結果受け取り
        homeSummary = JsonUtility.FromJson<HomeApi>(json);

        // ユーザが以下のチュートリアルステップである場合はTutorialアクションに飛ばす。
        // オープニング
        // バトルチュートリアル
        // バトル後
        // ショップ前
        // チュートリアル完了直後
        if (homeSummary.tutorial_step == constants.User_Info_Tutorial.TUTORIAL_MORNING || homeSummary.tutorial_step == constants.User_Info_Tutorial.TUTORIAL_BATTLE ||
            homeSummary.tutorial_step == constants.User_Info_Tutorial.TUTORIAL_AFTERBATTLE || homeSummary.tutorial_step == constants.User_Info_Tutorial.TUTORIAL_PRESHOP ||
            homeSummary.tutorial_step == constants.User_Info_Tutorial.TUTORIAL_LAST)
        {
            SceneController.Instance.Jump("Tutorial", (() =>
            {
                TutorialBehaviour tutorial = FindObjectOfType<TutorialBehaviour>() as TutorialBehaviour;
                tutorial.Param = new TutorialBehaviour.Parameter { TutorialStep = homeSummary.tutorial_step };
            }));

            return;
        }


        //ヘッダー・フッターに情報を渡す
        Header.Instance.SetTitle(Utility.getText("TITLE_HOME"));

        if (homeSummary.start_speak1 == "")
        {
            if (homeSummary.history.Length > 0)
            {
                //homeSummary.start_speak1 = "仲間の履歴なのだ。\n" + "『" + homeSummary.history[0].player_name + "さん：" + Utility.getHistoryText(homeSummary.history[0]) + "』";
            }
            else
            {
                homeSummary.start_speak1 = Utility.getText("TEXT_NAVI_HOME_DOSOMETHING");
            }
        }

        //キャラ作成
        this.makeCharaAnim(homeSummary.chara.equip_info, CharaImageAnim);

        /*
        //キャラ登場
        Vector3 cv = Avatar.transform.localPosition;
        Avatar.transform.localPosition = new Vector3(cv.x - 500, cv.y, cv.z);
        Avatar.transform.DOLocalMove(new Vector3(cv.x, cv.y, cv.z), 1.5f).SetEase(Ease.OutCubic);

        //影
        Avatar.transform.Find("Shadow").gameObject.SetActive(true);
        */

        Vector3 cv = AvatarAnim.transform.localPosition;
        AvatarAnim.transform.localPosition = new Vector3(cv.x - 500, cv.y, cv.z);
        AvatarAnim.transform.DOLocalMove(new Vector3(cv.x, cv.y, cv.z), 1.5f).SetEase(Ease.OutCubic);

        //影
        AvatarAnim.transform.Find("Shadow").gameObject.SetActive(true);

        //キャラ名
        player_name.text = homeSummary.player_name;

        infoView.SetActive(false);

        //クエストボタン非活性
        if (homeSummary.menu6State == "disable")
        {
            QuestButton.interactable = false;
        }

        //バトルボタン非活性
        if (homeSummary.menu7State == "disable")
        {
            BattleButton.interactable = false;
        }

        //naviメッセージがある場合
        naviController.onStart(homeSummary, null, onSpeakAfter);

        //実行中クエストがある場合
        if (homeSummary.sally_quest.quest_id != 0)
        {
            //$("#bannar_panel").hide();

            SallyQuestPanel.SetActive(true);
            SallyQuestPanel.transform.Find("caption/sally_quest_name").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_QUEST_STARTED").Replace("{0}", homeSummary.sally_quest.quest_name); 

            Button questDo = SallyQuestPanel.transform.Find("btn_panel/quest_do").GetComponent<Button>();
            Button questGiveup = SallyQuestPanel.transform.Find("btn_panel/quest_giveup").GetComponent<Button>();

            //クエスト実行ボタンイベントハンドラ
            questDo.onClick.AddListener((() =>
            {
                AudioManager.Instance.PlaySE("se_btn");
                this.showQuestConfirm(homeSummary.sally_quest, homeSummary.sally_quest.quest_id);
            }));

            if (homeSummary.tutorial_step >= constants.User_Info_Tutorial.TUTORIAL_END)
            {
                //クエストやめボタンイベントハンドラ
                questGiveup.onClick.AddListener((() =>
                {
                    AudioManager.Instance.PlaySE("se_btn");
                    this.showGiveup(homeSummary.sally_quest);
                }));
            }
            else
            {
                questGiveup.enabled = false;
            }

        }
        else
        {
            SallyQuestPanel.SetActive(false);
        }


        if (homeSummary.tutorial_step >= constants.User_Info_Tutorial.TUTORIAL_END)
        {

            if (homeSummary.raid_dungeon.status > constants.Raid_Dungeon.NONE)
            {
                RaidPalel.SetActive(true);
                RaidButtonText.DOFade(0.0f, 1f).SetEase(Ease.InCubic).SetLoops(-1, LoopType.Yoyo);

                string f = "yyyy-MM-dd HH:mm:ss";

                if (homeSummary.raid_dungeon.status == constants.Raid_Dungeon.READY)
                {
                    DateTime start_date = DateTime.ParseExact(homeSummary.raid_dungeon.start_at, f, null);

                    RaidPalel.transform.Find("BannarText").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_HOME_START_DATE").Replace("{0}", start_date.ToString("MM/dd"));
                }
                else if (homeSummary.raid_dungeon.status == constants.Raid_Dungeon.START)
                {
                    DateTime end_date = DateTime.ParseExact(homeSummary.raid_dungeon.end_at, f, null);

                    RaidPalel.transform.Find("BannarText").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_HOME_RAID_STARTED").Replace("{0}", end_date.ToString("MM/dd"));
                }
                else if (homeSummary.raid_dungeon.status == constants.Raid_Dungeon.SUCCESS)
                {
                    RaidPalel.transform.Find("BannarText").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_HOME_RAID_CLEAR");
                }
                else if (homeSummary.raid_dungeon.status == constants.Raid_Dungeon.FAILURE)
                {
                    RaidPalel.transform.Find("BannarText").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_HOME_RAID_FAIL");
                }

                RaidPalel.transform.Find("Bannar").GetComponent<Button>().onClick.AddListener(() =>
            {
                AudioManager.Instance.PlaySE("se_btn");

                SceneController.Instance.Jump("RaidInfo", () => { });
            });
            }


            //バナー
            /*
            if (homeSummary.freeGacha)
            {
                BannarPanel.SetActive(true);
                Bannar.GetComponent<Image>().sprite = Utility.getAssetImage("Image/Gacha/09997");
                BannarText.text = "現在、1日1回無料ガチャが引けます";

                Bannar.onClick.AddListener(() =>
                {

                    //二度押しは効かない
                    if (SceneController.Instance.SceneName == "GachaDetail")
                        return;

                    AudioManager.Instance.PlaySE("se_btn");

                    jsonGachaContents contents = new jsonGachaContents();

                    contents.gacha_id = 9997;
                    contents.gacha_name = "一日一回無料ガチャ";

                    APIConnectManager.Instance.Gacha((string json) =>
                    {
                        jsonGacha gacha_list = JsonUtility.FromJson<jsonGacha>(json);
                        SceneController.Instance.Jump("GachaDetail", (() =>
                        {
                            GachaDetailBehaviour _dacha_detail = FindObjectOfType<GachaDetailBehaviour>() as GachaDetailBehaviour;
                            _dacha_detail.Param = new GachaDetailBehaviour.Parameter
                            {
                                entry = contents,
                                ticketCount = gacha_list.ticketCount,
                                freeGacha = gacha_list.freeGacha,
                            };
                        }));
                    });
                });
            }
            else
            */

            if (homeSummary.battle_rank_info.status == 1 || homeSummary.battle_rank_info.status == 4)
            {
                BannarPanel.SetActive(true);
                string url = Settings.Domain + "://" + Settings.Host + "/" + "img/parts/sp/bannar/b_q_battle_event.png";
                if (Main.Locale != LocalizationSettings.AvailableLocales.Locales[0])
                {
                    url = Settings.Domain + "://" + Settings.Host + "/" + "img/parts/sp/bannar/b_q_battle_event_en.png";
                }

                StartCoroutine(GetTexture(Bannar.GetComponent<Image>(), url));

                if (homeSummary.battle_rank_info.status == 1)
                {
                    BannarText.text = Utility.getText("TEXT_HOME_BATTLEEVENT_STARTED");
                }
                else if (homeSummary.battle_rank_info.status == 4)
                {
                    DateTime start_date = Utility.GetDateTime(homeSummary.battle_rank_info.start_date);
                    BannarText.text = Utility.getText("TEXT_HOME_START_DATE").Replace("{0}", start_date.ToString("MM/dd")); 
                }

                Bannar.onClick.AddListener(() =>
                {
                    //二度押しは効かない
                    if (SceneController.Instance.SceneName == "BattleEvent")
                        return;

                    AudioManager.Instance.PlaySE("se_btn");

                    SceneController.Instance.PopUp("BattleEvent");
                });
            }
            else if (homeSummary.bannar.quest != null && homeSummary.bannar.quest.Length > 0)
            {
                //バナーがある場合はそれを表示する
                BannarPanel.SetActive(true);

                if (homeSummary.bannar.quest[0].quest_id == 99999 && homeSummary.raid_dungeon.status == constants.Raid_Dungeon.START)
                {
                    string url = Settings.Domain + "://" + Settings.Host + "/" + "img/parts/sp/bannar/b_q_" + homeSummary.bannar.quest[0].quest_id + "_2" + ".png";
                    if (Main.Locale != LocalizationSettings.AvailableLocales.Locales[0])
                    {
                        url = Settings.Domain + "://" + Settings.Host + "/" + "img/parts/sp/bannar/b_q_" + homeSummary.bannar.quest[0].quest_id + "_2" + "_en.png";
                    }
                    StartCoroutine(GetTexture(Bannar.GetComponent<Image>(), url));
                }
                else
                {
                    string url = Settings.Domain + "://" + Settings.Host + "/" + "img/parts/sp/bannar/b_q_" + homeSummary.bannar.quest[0].quest_id + ".png";

                    if (Main.Locale != LocalizationSettings.AvailableLocales.Locales[0])
                    {
                        url = Settings.Domain + "://" + Settings.Host + "/" + "img/parts/sp/bannar/b_q_" + homeSummary.bannar.quest[0].quest_id + "_en.png";
                    }

                    StartCoroutine(GetTexture(Bannar.GetComponent<Image>(), url));
                }
                BannarText.text = homeSummary.bannar.explain;

                Bannar.onClick.AddListener(() =>
                {
                    AudioManager.Instance.PlaySE("se_btn");

                    showQuestConfirm(homeSummary.bannar.quest[0], homeSummary.sally_quest.quest_id);
                });
            }
        }

        DispatchEvent(CwEvent.SCENE_READY);

        Header.Instance.SetSummary(homeSummary);
        Footer.Instance.SetSummary(homeSummary);


        AudioManager.Instance.PlayBGM("bgm_menu", AudioManager.BGM_VOLUME_DEFULT);
    }

    public void onQuestClick()
    {
        AudioManager.Instance.PlaySE("se_btn");

        SceneController.Instance.Jump("Quest",(() =>
        {
            Quest _q = FindObjectOfType<Quest>() as Quest;
            _q.Param = new Quest.Parameter
            {
                panel = "QuestList"
            };
        }));
    }

    public void onBattleClick()
    {
        AudioManager.Instance.PlaySE("se_btn");

        SceneController.Instance.Jump("BattleList");
    }

    void onSpeakAfter()
    {
        //チュートリアル中なら・・
        if (homeSummary.tutorial_step == constants.User_Info_Tutorial.TUTORIAL_MAINMENU)
        {
            //ナビカーソルを表示する
            Arrow.SetActive(true);
            Arrow.GetComponent<ArrowBehaviour>().Show("down", 0, 25);

            //クエストボタン活性
            QuestButton.interactable = true;
        }


    }

    public void TapInfo()
    {
        AudioManager.Instance.PlaySE("se_btn");

        foreach (Transform n in infoContent.transform)
        {
            if (n.name != infoFrame.name && n.name != infoLoading.name)
                GameObject.Destroy(n.gameObject);
        }

        infoView.SetActive(false);
        infoTitle.text = Utility.getText("TITLE_INFO");
        _page = 1;
        APIConnectManager.Instance.Info(_page - 1, OnInfo);
    }

    /// <summary>
    ///お知らせを押したとき 
    /// </summary>
    private void OnInfo(string json)
    {

        infoScrollRect.verticalNormalizedPosition = 1.0f;
        InfoApi infoApi = JsonUtility.FromJson<InfoApi>(json);

        info.SetActive(true);

        _totalPage = infoApi.totalPages;

        int i = 0;
        foreach (InfoResultSet infoResultSet in infoApi.resultset)
        {
            GameObject objInfo = UnityEngine.Object.Instantiate(infoFrame, new Vector3(0, 0, 0), Quaternion.identity, infoContent.transform);
            objInfo.name = "infoFrame" + i;
            objInfo.transform.localPosition = new Vector3(0, 0, 0);

            PageFrame pageFrame = objInfo.GetComponent<PageFrame>();
            pageFrame.OnSet(infoResultSet);
            pageFrame.AddEventListener(CwEvent.INFO_CLICK, InfoClick);

            objInfo.SetActive(true);

            i++;
        }

        infoLoading.SetActive(false);

    }

    /// <summary>
    ///お知らせのそれぞれの詳細ボタンを押したとき 
    /// </summary>
    private void InfoClick(GameObject eventGamObject, string eventname)
    {
        AudioManager.Instance.PlaySE("se_btn");

        PageFrame pageFrame = eventGamObject.GetComponent<PageFrame>();
        infoView.SetActive(true);

        if (Main.Locale != LocalizationSettings.AvailableLocales.Locales[0])
        {
            infoTitle.text = pageFrame.infoResultSet.title_en;
            infoViewDetail.text = pageFrame.infoResultSet.body_en;
        }
        else
        {
            infoTitle.text = pageFrame.infoResultSet.title;
            infoViewDetail.text = pageFrame.infoResultSet.body;
        }

        //infoViewDetail.rectTransform.sizeDelta = new Vector2(infoViewDetail.preferredWidth, infoViewDetail.preferredHeight);

        Debug.Log(pageFrame.infoResultSet.body);
    }
    /// <summary>
    /// スクロールで追加読み込み
    /// </summary>
    public void InfoScroll()
    {
        if (infoScrollRect.verticalNormalizedPosition < -0.02f)
        {
            //Debug.Log(infoScrollRect.verticalNormalizedPosition);
            if (_isUpdate)
            {
                _isUpdate = false;
                if (_page < _totalPage)
                {
                    _page++;

                    //loadingを出す
                    objInfoLoading = UnityEngine.Object.Instantiate(infoLoading, new Vector3(0, 0, 0), Quaternion.identity, infoContent.transform);
                    objInfoLoading.name = "infoLoading" + _page;
                    objInfoLoading.transform.localPosition = new Vector3(0, 0, 0);

                    objInfoLoading.SetActive(true);

                    StartCoroutine(gotoNext());

                }
            }
        }
    }

    IEnumerator gotoNext()
    {
        float delayCount = 1.5f;

        yield return new WaitForSeconds(delayCount);

        APIConnectManager.Instance.Info(_page - 1, (string json) =>
        {
            OnInfo(json);

            foreach (Transform n in infoContent.transform)
            {
                if (n.name == objInfoLoading.name)
                    GameObject.Destroy(n.gameObject);
            }

            _isUpdate = true;
        });
    }

    public void TapInfoClose()
    {
        AudioManager.Instance.PlaySE("se_btn");

        if (infoView.activeSelf)
        {
            infoView.SetActive(false);
            infoTitle.text = Utility.getText("TITLE_INFO");
        }
        else
        {
            info.SetActive(false);
        }

    }

    private void showQuestConfirm(jsonQuestList entry, int sally_quest_id)
    {

        ConfirmWin.SetActive(true);
        //クエスト名
        ConfirmWin.transform.Find("QuestTitle").GetComponent<TextMeshProUGUI>().text = entry.quest_name;

        //フレーバーテキスト
        ConfirmWin.transform.Find("flavor_text").GetComponent<TextMeshProUGUI>().text = entry.flavor_text;

        if (entry.quest_id == homeSummary.raid_dungeon.quest_id && homeSummary.raid_dungeon.status == constants.Raid_Dungeon.START)
        {
            ConfirmWin.transform.Find("flavor_text").GetComponent<TextMeshProUGUI>().text = entry.flavor_text + "\n\n" + Utility.getText("TEXT_HOME_RAID_STARTED2");
        }

        //クエストタイプ
        switch (entry.type)
        {
            case "FLD":
                ConfirmWin.transform.Find("quest_type").GetComponent<TextMeshProUGUI>().text = Utility.getText("CAPTION_FIELD");
                break;
            default:
                ConfirmWin.transform.Find("quest_type").GetComponent<TextMeshProUGUI>().text = Utility.getText("CAPTION_EVENT");
                break;
        }

        //推奨レベル
        if (entry.preferred_level != "" && entry.preferred_level != null)
            ConfirmWin.transform.Find("preferred_Image/preferred_level").GetComponent<TextMeshProUGUI>().text = entry.preferred_level;
        else
            ConfirmWin.transform.Find("preferred_Image/preferred_level").GetComponent<TextMeshProUGUI>().text = "---";

        //消費AP
        ConfirmWin.transform.Find("consume_pt").GetComponent<TextMeshProUGUI>().text = entry.consume_pt + "pt";

        //確認文言
        ConfirmWin.transform.Find("navispeak/navitext").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_HOME_QUEST_CONFIRM_DO");

        //ステータス作成
        var status = entry.status;

        //実行中クエの場合
        if (sally_quest_id != 0 && entry.quest_id == sally_quest_id)
        {
            status = 4;
        }

        Sprite status_icon1 = Utility.getAssetImage(Utility.getStatusIcon(status));
        Image statusIcon = ConfirmWin.transform.Find("Status").GetComponent<Image>();
        statusIcon.sprite = status_icon1;

        if (status == 1 || status == 4)
        {
            Sprite status_icon2 = Utility.getAssetImage(Utility.getStatusIcon(status, "_2"));

            //Sequenceを宣言する
            seq = DOTween.Sequence();
            seq.Append(DOVirtual.DelayedCall(0.8f, () => statusIcon.sprite = status_icon1));
            seq.Append(DOVirtual.DelayedCall(0.8f, () => statusIcon.sprite = status_icon2));
            seq.SetLoops(-1, LoopType.Restart);//無限ループする
        }

        Button btn_ok = ConfirmWin.transform.Find("BtnOk").GetComponent<Button>();
        Button btn_cancel = ConfirmWin.transform.Find("BtnCancel").GetComponent<Button>();

        //実行中クエがあり、それでない場合
        if (sally_quest_id != 0 && entry.quest_id != sally_quest_id)
        {
            ConfirmWin.transform.Find("navispeak/navitext").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_HOME_QUEST_OTHER_DO");
            btn_ok.interactable = false;
        }
        else
        {
            btn_ok.interactable = true;

            // OKボタンがクリックされたときのハンドラを登録
            btn_ok.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlaySE("se_btn");

                //実行中クエがあり、それの場合
                if (sally_quest_id != 0 && entry.quest_id == sally_quest_id)
                {
                    //二度押しは効かない
                    if (SceneController.Instance.SceneName == "Sphere")
                        return;

                    SceneController.Instance.Jump("Sphere", (() =>
                    {
                        SphereBehaviour _sphere = FindObjectOfType<SphereBehaviour>() as SphereBehaviour;
                        _sphere.Param = new SphereBehaviour.Parameter
                        {
                            sphereId = homeSummary.chara.sally_sphere,
                            reopen = "resume",
                        };

                        AudioManager.Instance.StopBGM();
                    }));
                }
                else
                {
                    if (entry.type == "FLD")
                    {
                        //二度押しは効かない
                        if (SceneController.Instance.SceneName == "Ready")
                            return;

                        //準備画面へ
                        SceneController.Instance.Jump("Ready", (() =>
                            {
                                ReadyBehaviour ready = FindObjectOfType<ReadyBehaviour>() as ReadyBehaviour;
                                ready.Param = new ReadyBehaviour.Parameter
                                {
                                    questId = entry.quest_id,
                                    placeId = entry.place_id,
                                    consume_pt = entry.consume_pt,
                                    FromScene = "Quest",
                                };
                                AudioManager.Instance.StopBGM();
                            }));

                    }
                    else
                    {
                        //二度押しは効かない
                        if (SceneController.Instance.SceneName == "Terminable")
                            return;

                        //ドラマの場合は直接遷移
                        AudioManager.Instance.StopBGM();
                        SceneController.Instance.Jump("Terminable");
                    }
                }

                ConfirmWin.SetActive(false);
            });
        }

        // cancelボタンがクリックされたときのハンドラを登録
        btn_cancel.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlaySE("se_btn");
            ConfirmWin.SetActive(false);
        });
    }

    //---------------------------------------------------------------------------------------------------------
    /**
     * クエスト実行確認ポップアップを立ち上げる
     */
    void showGiveup(jsonQuestList entry)
    {
        string text = Utility.getText("TEXT_HOME_QUEST_CONFIRM_GIVEUP").Replace("{0}", entry.quest_name);

        Main.Instance.showConfirm(text, (() =>
        {
            //二度押しは効かない
            if (SceneController.Instance.SceneName == "FieldEnd")
                return;

            AudioManager.Instance.PlaySE("se_btn");

            //ギブアップをする
            APIConnectManager.Instance.FieldReopen("1", ((string json) =>
                {
                    //API結果受け取り
                    jsonFieldReopen results = JsonUtility.FromJson<jsonFieldReopen>(json);
                    if (results.result == "ok")
                    {
                        SceneController.Instance.Jump("FieldEnd", (() =>
                            {
                                FieldEndBehaviour _fieldend = FindObjectOfType<FieldEndBehaviour>() as FieldEndBehaviour;
                                _fieldend.Param = new FieldEndBehaviour.Parameter
                                {
                                    sphereId = homeSummary.chara.sally_sphere,
                                };
                            }));
                    }
                }));
        }));
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}
