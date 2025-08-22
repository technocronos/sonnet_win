using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CreateWave;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class ResultPhaseBehaviour : MonoBehaviour
{
    public GameObject ListWEP;
    public GameObject ListBOD;
    public GameObject ListHED;
    public GameObject ListACS;
    public GameObject ListSUM;
    public GameObject ListNone;

    public GameObject Content;

    public AvatarBehaviour Avatar;

    public TextMeshProUGUI match_length;
    public TextMeshProUGUI total_hurtP;
    public TextMeshProUGUI total_hurtE;
    public TextMeshProUGUI normal_hurtP;
    public TextMeshProUGUI normal_hurtE;
    public TextMeshProUGUI normal_hitsP;
    public TextMeshProUGUI normal_hitsE;
    public TextMeshProUGUI tact0P;
    public TextMeshProUGUI tact0E;
    public TextMeshProUGUI revenge_hurtP;
    public TextMeshProUGUI revenge_hurtE;
    public TextMeshProUGUI revenge_countP;
    public TextMeshProUGUI revenge_countE;
    public TextMeshProUGUI revenge_hitsP;
    public TextMeshProUGUI revenge_hitsE;

    public Toggle Tab_WEP;
    public Toggle Tab_SUM;

    public TextMeshProUGUI ExpCaptionText;
    public TextMeshProUGUI GoldCaptionText;
    public TextMeshProUGUI TextGold;
    public TextMeshProUGUI GradeCaptionText;

    Dictionary<string, Tweener> tweener = new Dictionary<string, Tweener>();

    jsonConstants constants;

    public static ResultPhaseBehaviour Instance
    {
        get
        {
            return instance;
        }
    }

    private static ResultPhaseBehaviour instance;


    // 
    // 終了フェーズの制御を行う。
    private void Start()
    {
        instance = this;

        ExpCaptionText.text = Utility.getText("TEXT_EXP");
        GoldCaptionText.text = Utility.getText("TEXT_GOLD");
        TextGold.text = Utility.getText("TEXT_HAS_GOLD");
        GradeCaptionText.text = Utility.getText("TEXT_GRADE_POINT");

    }

    private bool capture_flg { get; set; } = false;
    private bool item_flg { get; set; } = false;

    string category { get; set; } = "EQP";

    public jsonBattleResult response { get; set; }

    public void Init(jsonBattleResult _response)
    {
        response = _response;

        StartCoroutine(Avatar.PlayAnim("AvatarAppear", 2));

        tweener[ListWEP.name] = null;
        tweener[ListBOD.name] = null;
        tweener[ListHED.name] = null;
        tweener[ListACS.name] = null;

        //定数取得
        constants = APIConnectManager.Instance.login.constants;

        this.Reload();
    }

    void Reload()
    {
        if (this.category == "EQP")
        {
            ListWEP.SetActive(true);
            ListBOD.SetActive(true);
            ListHED.SetActive(true);
            ListACS.SetActive(true);
            ListSUM.SetActive(false);
        }
        else
        {
            ListWEP.SetActive(false);
            ListBOD.SetActive(false);
            ListHED.SetActive(false);
            ListACS.SetActive(false);
            ListSUM.SetActive(true);
        }

        if (response.capture != null)
        {
            capture_flg = true;
        }

        if (response.battleresult.gain.uitem != null)
        {
            if (response.battleresult.gain.uitem.Length > 0)
            {
                item_flg = true;
            }
        }

        //タイトル表示
        transform.Find("Caption/win").gameObject.SetActive(false);
        transform.Find("Caption/lose").gameObject.SetActive(false);
        transform.Find("Caption/timeup").gameObject.SetActive(false);

        transform.Find("Caption/" + response.battle.bias_status).gameObject.SetActive(true);

        //if (response.battle.bias_status == "win")
        //    $("#chara_spot").show();

        //ユーザー対戦とクエストで出口を分ける
        if (response.battle.tournament_id == 1)
        {
            //バトル
            transform.Find("ButtonLeft/TextButton").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_GOTO_RIVAL_LIST");
            transform.Find("ButtonRight/TextButton").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_GOTO_RIVAL_PAGE");
        }
        else
        {
            //クエスト
            transform.Find("ButtonLeft/TextButton").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_GOTO_MAIN");
            transform.Find("ButtonRight/TextButton").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_GOTO_FIELD");
        }

        if (response.battle.bias_user_id != -1)
        {
            //追加マグナ
            transform.Find("SidePanel/GoldPanel/TextGain").GetComponent<TextMeshProUGUI>().text = "+" + response.battle.bias_result.gain.gold;
            //現在のマグナ
            transform.Find("SidePanel/GoldPanel/TextHave").GetComponent<TextMeshProUGUI>().text = (response.battle.bias_result.gain.gold + response.current.gold).ToString();

            //追加階級ポイント
            transform.Find("SidePanel/GradePanel/TextGain").GetComponent<TextMeshProUGUI>().text = "+" + response.battle.bias_result.gain.grade_nominal;
            //現在のポイント
            if (response.battle.bias_result.gain.grade < response.battle.bias_result.gain.grade_nominal)
            {
                transform.Find("SidePanel/GradePanel/TextHave").GetComponent<TextMeshProUGUI>().text = "[MAX]";
            }
            else
            {
                transform.Find("SidePanel/GradePanel/TextHave").GetComponent<TextMeshProUGUI>().text = response.current.grade_pt.ToString();
            }

            //ゲージ更新
            transform.Find("SidePanel/ExpDisp").GetComponent<ExpDispBehaviour>().show(response.battle.bias_result.gain.exp, response.current.exp["relative_exp"], response.current.exp["relative_next"]);
        }
        else
        {
            //追加マグナ
            transform.Find("SidePanel/GoldPanel/TextGain").GetComponent<TextMeshProUGUI>().text = "---";
            //現在のマグナ
            transform.Find("SidePanel/GoldPanel/TextHave").GetComponent<TextMeshProUGUI>().text = "---";

            //追加階級ポイント
            transform.Find("SidePanel/GradePanel/TextGain").GetComponent<TextMeshProUGUI>().text = "---";
            //現在のポイント
            transform.Find("SidePanel/GradePanel/TextHave").GetComponent<TextMeshProUGUI>().text = "---";
            //ゲージ更新
            transform.Find("SidePanel/ExpDisp").GetComponent<ExpDispBehaviour>().show(0, 0, 0);
        }


        //サマリー
        match_length.text = response.battle.result_detail.match_length + Utility.getText("TEXT_BATTLE_SUMMARY_KAISU");
        total_hurtP.text = response.battle.bias_result.summary.total_hurt.ToString();
        total_hurtE.text = response.battle.rival_result.summary.total_hurt.ToString();

        normal_hurtP.text = response.battle.bias_result.summary.normal_hurt.ToString();
        normal_hurtE.text = response.battle.rival_result.summary.normal_hurt.ToString();

        normal_hitsP.text = response.battle.bias_result.summary.normal_hits.ToString();
        normal_hitsE.text = response.battle.rival_result.summary.normal_hits.ToString();

        tact0P.text = response.battle.bias_result.summary.tact0.ToString();
        tact0E.text = response.battle.rival_result.summary.tact0.ToString();

        revenge_hurtP.text = response.battle.bias_result.summary.revenge_hurt.ToString();
        revenge_hurtE.text = response.battle.rival_result.summary.revenge_hurt.ToString();

        revenge_countP.text = response.battle.bias_result.summary.revenge_count.ToString();
        revenge_countE.text = response.battle.rival_result.summary.revenge_count.ToString();

        if (response.battle.bias_result.summary.revenge_attacks > 0)
        {
            revenge_hitsP.text = Mathf.Floor((response.battle.bias_result.summary.revenge_hits / response.battle.bias_result.summary.revenge_attacks * 100)) + "%";
        }
        else
        {
            revenge_hitsP.text = "---%";
        }
        if (response.battle.rival_result.summary.revenge_attacks > 0)
        {
            revenge_hitsE.text = Mathf.Floor((response.battle.rival_result.summary.revenge_hits / response.battle.rival_result.summary.revenge_attacks * 100)) + "%";
        }
        else
        {
            revenge_hitsE.text = "---%";
        }

        if (response.battle.bias_user_id != -1)
        {
            // エントリ要素を一つずつ作成・表示していく。
            for (int i = 1; i <= 4; i++)
            {
                GameObject borad = null;
                switch (i)
                {
                    case 1:
                        borad = ListWEP;
                        break;
                    case 2:
                        borad = ListBOD;
                        break;
                    case 3:
                        borad = ListHED;
                        break;
                    case 4:
                        borad = ListACS;
                        break;
                }

                // パケットをリストに表示。
                jsonEquip after = response.battleresult.equip.after != null ? (response.battleresult.equip.after[i] != null ? response.battleresult.equip.after[i] : null) : null;
                jsonEquip before = response.battleresult.equip.before != null ? (response.battleresult.equip.before[i] != null ? response.battleresult.equip.before[i] : null) : null;

                setupEntryBoard(after, before, borad);
            }

        }

        onLoaded();
    }


    //---------------------------------------------------------------------------------------------------------
    /**
     * 引数に指定されたエントリを、指定されたボードに表示するときに呼ばれる。
     */
    void setupEntryBoard(jsonEquip after, jsonEquip before, GameObject borad)
    {

        borad.transform.Find("Flame/Broken/IconDurable/Text").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_EQUIP_BROKEN");
        borad.transform.Find("Flame/None/Text").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_NO_EQUIP");

        //装備名
        if (after != null)
        {
            if (after.evolution == 1)
                borad.transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = after.item_name + "<color=\"red\" >["+Utility.getText("TEXT_EQUIP_EVOLUTION") +"]</color>";
            else
                borad.transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = after.item_name;

        }
        else
        {
            borad.transform.Find("ItemName").gameObject.SetActive(false);
        }

        //装備アイコン
        if (before.item_id != 0)
        {
            borad.transform.Find("ItemIcon").GetComponent<Image>().sprite = Utility.getAssetImage(Utility.getItemIconURL(before.item_id));
        }
        else
        {
            borad.transform.Find("ItemIcon").gameObject.SetActive(false);
        }

        borad.transform.Find("lv_icon").gameObject.SetActive(false);
        borad.transform.Find("lv_max_icon").gameObject.SetActive(false);
        borad.transform.Find("lv_max_icon2").gameObject.SetActive(false);
        borad.transform.Find("lv_evol_panel").gameObject.SetActive(false);

        borad.transform.Find("TextRepaire").gameObject.SetActive(false);

        if (before.level != after.level)
        {
            //Lvアップ
            borad.transform.Find("TextLv").GetComponent<TextMeshProUGUI>().textStyle = TMP_Style.NormalStyle;
            borad.transform.Find("TextLv").GetComponent<TextMeshProUGUI>().color = ColorGet.Hex(0xffa4aa);
            borad.transform.Find("lv_icon").gameObject.SetActive(true);

            borad.transform.Find("Flame/Normal").gameObject.SetActive(false);
            borad.transform.Find("Flame/LeveUp").gameObject.SetActive(true);
            borad.transform.Find("Flame/Broken").gameObject.SetActive(false);
            borad.transform.Find("Flame/None").gameObject.SetActive(false);

        }

        if (after.max_level > 0 && after.max_level <= after.level)
        {
            //MAXレベル
            borad.transform.Find("TextLv").GetComponent<TextMeshProUGUI>().textStyle = TMP_Style.NormalStyle;
            borad.transform.Find("TextLv").GetComponent<TextMeshProUGUI>().color = ColorGet.Hex(0xffa4aa);
            borad.transform.Find("lv_max_icon").gameObject.SetActive(true);
        }

        if (after.evolution == 1)
        {
            //進化
            borad.transform.Find("TextLv").GetComponent<TextMeshProUGUI>().textStyle = TMP_Style.NormalStyle;
            borad.transform.Find("TextLv").GetComponent<TextMeshProUGUI>().color = ColorGet.Hex(0xffffff);
            borad.transform.Find("lv_evol_panel").gameObject.SetActive(true);
        }

        //レベル
        borad.transform.Find("TextLv").GetComponent<TextMeshProUGUI>().text = after.level.ToString();

        //耐久値の変化
        if (before.durable_count != constants.Item_Master.INFINITE_DURABILITY)
        {
            string durable_count = before.durable_count.ToString();
            if (before.durable_count != after.durable_count)
            {
                durable_count = durable_count + "⇒" + after.durable_count;
            }

            //修理して戻ってきている場合は、その旨表示する
            if (after.repaire != 0)
            {
                durable_count = durable_count + "⇒" + after.repaire;
                borad.transform.Find("TextRepaire").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_BATTLE_REPAIRE_DO");
                borad.transform.Find("TextRepaire").gameObject.SetActive(true);
            }

            borad.transform.Find("TextDurable").GetComponent<TextMeshProUGUI>().text = durable_count;
        }
        else
        {
            borad.transform.Find("TextDurable").GetComponent<TextMeshProUGUI>().text = "∞";
        }

        //修理ボタン
        if (after.repaire_useto != null)
        {
            //表示
            borad.transform.Find("btn_repaire").gameObject.SetActive(true);

            //点滅
            if (tweener[borad.name] == null)
                tweener[borad.name] = borad.transform.Find("btn_repaire").GetComponent<Image>().DOFade(0.0f, 1f).SetEase(Ease.InCubic).SetLoops(-1, LoopType.Yoyo);

            //イベントハンドラ登録
            borad.transform.Find("btn_repaire").GetComponent<Button>().onClick.RemoveAllListeners();
            borad.transform.Find("btn_repaire").GetComponent<Button>().onClick.AddListener((() =>
            {
                AudioManager.Instance.PlaySE("se_btn");

                Trans(after.urlOnRepaire);
            }));
        }
        else
        {
            borad.transform.Find("btn_repaire").gameObject.SetActive(false);

        }

        if (before.user_item_id == 0)
        {
            borad.transform.Find("Flame/Normal").gameObject.SetActive(false);
            borad.transform.Find("Flame/LeveUp").gameObject.SetActive(false);
            borad.transform.Find("Flame/Broken").gameObject.SetActive(false);
            borad.transform.Find("Flame/None").gameObject.SetActive(true);
        }
        else
        {
            borad.transform.Find("Flame/Normal").gameObject.SetActive(true);
            borad.transform.Find("Flame/LeveUp").gameObject.SetActive(false);
            borad.transform.Find("Flame/Broken").gameObject.SetActive(false);
            borad.transform.Find("Flame/None").gameObject.SetActive(false);

            borad.transform.Find("StatusPanel/att1").GetComponent<TextMeshProUGUI>().text = after.attack1.ToString();
            borad.transform.Find("StatusPanel/att2").GetComponent<TextMeshProUGUI>().text = after.attack2.ToString();
            borad.transform.Find("StatusPanel/att3").GetComponent<TextMeshProUGUI>().text = after.attack3.ToString();
            borad.transform.Find("StatusPanel/spd").GetComponent<TextMeshProUGUI>().text = after.speed.ToString();
            borad.transform.Find("StatusPanel/def1").GetComponent<TextMeshProUGUI>().text = after.defence1.ToString();
            borad.transform.Find("StatusPanel/def2").GetComponent<TextMeshProUGUI>().text = after.defence2.ToString();
            borad.transform.Find("StatusPanel/def3").GetComponent<TextMeshProUGUI>().text = after.defence3.ToString();
            borad.transform.Find("StatusPanel/defX").GetComponent<TextMeshProUGUI>().text = after.defenceX.ToString();

            if (after.attack1 != before.attack1)
                borad.transform.Find("StatusPanel/atk1icon").gameObject.SetActive(false);

            if (after.attack2 != before.attack2)
                borad.transform.Find("StatusPanel/atk2icon").gameObject.SetActive(false);

            if (after.attack3 != before.attack3)
                borad.transform.Find("StatusPanel/atk3icon").gameObject.SetActive(false);

            if (after.speed != before.speed)
                borad.transform.Find("StatusPanel/speedicon").gameObject.SetActive(false);

            if (after.defence1 != before.defence1)
                borad.transform.Find("StatusPanel/def1icon").gameObject.SetActive(false);

            if (after.defence2 != before.defence2)
                borad.transform.Find("StatusPanel/def2icon").gameObject.SetActive(false);

            if (after.defence3 != before.defence3)
                borad.transform.Find("StatusPanel/def3icon").gameObject.SetActive(false);

            if (after.defenceX != before.defenceX)
                borad.transform.Find("StatusPanel/defXicon").gameObject.SetActive(false);
        }

        //この戦闘で壊れた場合
        if (after.user_item_id == 0 && before.user_item_id != 0)
        {
            borad.transform.Find("Flame/Normal").gameObject.SetActive(false);
            borad.transform.Find("Flame/LeveUp").gameObject.SetActive(false);
            borad.transform.Find("Flame/Broken").gameObject.SetActive(true);
            borad.transform.Find("Flame/None").gameObject.SetActive(false);

            borad.transform.Find("StatusPanel").gameObject.SetActive(false);
            borad.transform.Find("TextDurable").gameObject.SetActive(false);
            borad.transform.Find("lv_icon").gameObject.SetActive(false);
            borad.transform.Find("broken_icon").gameObject.SetActive(true);

        }

        //壊れている場合
        if (before.level == 0)
        {
            borad.transform.Find("lv_icon").gameObject.SetActive(false);
            borad.transform.Find("TextLv").gameObject.SetActive(false);

            borad.transform.Find("StatusPanel").gameObject.SetActive(false);
            borad.transform.Find("TextDurable").gameObject.SetActive(false);
        }

    }


    //---------------------------------------------------------------------------------------------------------
    /*
     * すべて表示し終わった時のイベントハンドラ
     *
    */
    public void onLoaded()
    {
        if (response.battle.result_detail.get_vcoin > 0)
        {
            transform.Find("Vcoin").gameObject.SetActive(true);
            transform.Find("Vcoin").GetComponent<VcoinBehaviour>().Show(response, onLoaded);
            response.battle.result_detail.get_vcoin = 0;
        }
        else if (response.battle.result_detail.get_raid_point > 0)
        {
            transform.Find("RaidPoint").gameObject.SetActive(true);
            transform.Find("RaidPoint").GetComponent<RaidPointBehaviour>().Show(response, onLoaded);
            response.battle.result_detail.get_raid_point = 0;
            response.battle.result_detail.get_nft = false;
        }
        else if (response.levelup)
        {
            transform.Find("LevelUp").gameObject.SetActive(true);
            transform.Find("LevelUp").GetComponent<LevelUPBehaviour>().Show(response, onLoaded);
            response.levelup = false;
        }
        else if (response.gradeup)
        {
            transform.Find("GradeUp").gameObject.SetActive(true);
            transform.Find("GradeUp").GetComponent<GradeUpBehaviour>().Show(response, onLoaded);
            response.gradeup = false;
        }
        else if (response.capture_flg == true)
        {
            transform.Find("Zukan").gameObject.SetActive(true);
            transform.Find("Zukan").GetComponent<ZukanBehaviour>().Show(response, onLoaded);
            response.capture_flg = false;
        }
        else if (response.item_flg == true)
        {
            transform.Find("ItemGet").gameObject.SetActive(true);
            transform.Find("ItemGet").GetComponent<ItemGetBehaviour>().Show(response, onLoaded);
            response.item_flg = false;
        }

        if (response.battle.bias_user_id == -1)
        {
            Tab_SUM.isOn = true;
            Tab_WEP.interactable = false;
            this.changeTab("SUM");
        }
    }

    public void changeTab(string category)
    {
        if (this.category == category)
            return;

        AudioManager.Instance.PlaySE("se_btn");

        this.category = category;

        this.Reload();
    }

    //左ボタンクリック時イベントハンドラ
    public void onLeftButton()
    {
        AudioManager.Instance.PlaySE("se_btn");

        if (response.battle.tournament_id == 1)
            Trans(response.urlOnRivalList);
        else
            Trans(response.urlOnHome);
    }

    //右ボタンクリック時イベントハンドラ
    public void onRightButton()
    {
        AudioManager.Instance.PlaySE("se_btn");

        if (response.battle.tournament_id == 1)
            Trans(response.urlOnHisPage);
        else
            Trans(response.urlOnSphere);
    }


    //
    //画面遷移をする
    //
    public void Trans(string url)
    {

        AudioManager.Instance.StopBGM();

        Dictionary<string, string> transUrl = new Dictionary<string, string>();
        transUrl = Utility.ParseUrl(url);

        switch (transUrl["scene"])
        {
            case "Sphere":
                SceneController.Instance.Jump("Sphere", (() =>
                {
                    SphereBehaviour _sphere = FindObjectOfType<SphereBehaviour>() as SphereBehaviour;
                    _sphere.Param = new SphereBehaviour.Parameter
                    {
                        sphereId = int.Parse(transUrl["id"]),
                    };
                }));
                break;
            case "FieldEnd":
                SceneController.Instance.Jump("FieldEnd", (() =>
                {
                    FieldEndBehaviour _fieldend = FindObjectOfType<FieldEndBehaviour>() as FieldEndBehaviour;
                    _fieldend.Param = new FieldEndBehaviour.Parameter
                    {
                        sphereId = int.Parse(transUrl["sphereId"]),
                    };
                }));
                break;
            case "HisPage":
                SceneController.Instance.Jump("HisPage", (() =>
                {
                    HisPageBehaviour _hispage = FindObjectOfType<HisPageBehaviour>() as HisPageBehaviour;
                    _hispage.Param = new HisPageBehaviour.Parameter
                    {
                        userId = int.Parse(transUrl["his_user_id"]),
                    };
                }));
                break;
            case "Suggest":
                SceneController.Instance.Jump("Suggest", (() =>
                {
                    SuggestBehaviour _suggest = FindObjectOfType<SuggestBehaviour>() as SuggestBehaviour;
                    _suggest.Param = new SuggestBehaviour.Parameter
                    {
                        type = transUrl["type"],
                        targetId = transUrl.ContainsKey("targetId") ? transUrl["targetId"] : null,
                        backto = transUrl["backto"],
                        useto = transUrl.ContainsKey("useto") ? transUrl["useto"] : null,
                    };
                }));
                break;
            case "Tutorial":
                SceneController.Instance.Jump("Tutorial", (() =>
                {
                    TutorialBehaviour tutorial = FindObjectOfType<TutorialBehaviour>() as TutorialBehaviour;
                    tutorial.Param = new TutorialBehaviour.Parameter { TutorialStep = int.Parse(transUrl["tutorial_step"]) };
                }));
                break;
            default:
                SceneController.Instance.Jump(transUrl["scene"]);
                break;
        }
    }

}
