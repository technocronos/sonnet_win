using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CreateWave;
using TMPro;
using UnityEngine.UI;
using Scenes.Common.Scripts;
using UnityEngine.Localization.Settings;

public class BattleListBehaviour : BaseBehaviour
{
    public GameObject ListNone;
    public GameObject ListUser;
    public GameObject Content;
    public GameObject ListLoading;
    public Image Bannar;
    public GameObject objGradeListPanel;
    public GameObject objGradeUserPanel;
    public Image BG;

    jsonChara[] list { get; set; }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        BG.sprite = Utility.getAssetImage("Image/BG/bg2");

        Debug.Log("BattleListBehaviour start..");
        setSafearea("BattleListCanvas");

        Header.Instance.SetTitle(Utility.getText("TEXT_BATTLELIST"));

        objGradeListPanel.SetActive(false);
        objGradeUserPanel.SetActive(false);

        ListClear();

        //APIをたたく
        APIConnectManager.Instance.RivalList(reload);

        AudioManager.Instance.PlayBGM("bgm_menu", AudioManager.BGM_VOLUME_DEFULT);
        DispatchEvent(CwEvent.SCENE_READY);
    }

    jsonRivalList jsonParse(string json)
    {
        jsonRivalList response = JsonUtility.FromJson<jsonRivalList>(json);

        return response;
    }
    void reload(string json)
    {
        jsonRivalList response = jsonParse(json);

        //バナーをステータスによって切り分ける
        string url = Settings.Domain + "://" + Settings.Host + "/" + "img/parts/sp/bannar/b_q_battle_event_b_" + Header.Instance.GetSummary().battle_rank_info.status + ".png";
        if (Main.Locale != LocalizationSettings.AvailableLocales.Locales[0])
        {
            url = Settings.Domain + "://" + Settings.Host + "/" + "img/parts/sp/bannar/b_q_battle_event_b_" + Header.Instance.GetSummary().battle_rank_info.status + "_en.png";
        }
        StartCoroutine(GetTexture(Bannar, url));

        list = response.rivalList;

        // GETアイテムが一つもなかったら...
        if (list.Length == 0)
        {
            // その旨のパネルを表示。
            ListNone.SetActive(true);
            Content.SetActive(false);

            // 処理はここまで。
            return;
        }

        int i = 0;
        foreach (jsonChara entry in list)
        {
            GameObject board = null;

            // リストを複製
            board = UnityEngine.Object.Instantiate(ListUser, new Vector3(0, 0, 0), Quaternion.identity, Content.transform);
            board.name = "ListUser" + i;

            setupEntryBoard(entry, board);

            board.SetActive(true);
            i++;
        }

    }
    //---------------------------------------------------------------------------------------------------------
    /**
     * 引数に指定されたエントリを、指定されたボードに表示するときに呼ばれる。
     */
    void setupEntryBoard(jsonChara entry, GameObject board)
    {

        //ユーザー名
        board.transform.Find("TextName").GetComponent<TextMeshProUGUI>().text = entry.player_name;
        //キャラ作成
        Image CharaImage = board.transform.Find("Avatar/Avatar/CharaImage").GetComponent<Image>();
        Main.Instance.makeCharaUI(entry.equip_info, CharaImage);

        board.transform.Find("TextMember").GetComponent<TextMeshProUGUI>().text = entry.member.ToString();
        board.transform.Find("TextLv").GetComponent<TextMeshProUGUI>().text = entry.level.ToString();
        board.transform.Find("TextGrade").GetComponent<TextMeshProUGUI>().text = entry.grade_name;

        // HPゲージを更新。
        int _hp = entry.hp;
        float _hp_max = entry.hp_max;
        float hp_val = Mathf.Min(_hp, _hp_max);

        Transform gauge = board.transform.Find("HPGauge/hp_gauge_bar/gauge");
        float gauge_width = gauge.GetComponent<RectTransform>().rect.width; ;

        int posx = (int)(((hp_val * 1.0f) / _hp_max) * gauge_width);
        gauge.transform.localPosition = new Vector3(posx - gauge_width, 0, 0);

        board.transform.Find("HPGauge/hp").GetComponent<TextMeshProUGUI>().text = _hp.ToString();
        board.transform.Find("HPGauge/max").GetComponent<TextMeshProUGUI>().text = _hp_max.ToString();

        board.transform.Find("StatusPanel/att1").GetComponent<TextMeshProUGUI>().text = entry.total_attack1.ToString();
        board.transform.Find("StatusPanel/att2").GetComponent<TextMeshProUGUI>().text = entry.total_attack2.ToString();
        board.transform.Find("StatusPanel/att3").GetComponent<TextMeshProUGUI>().text = entry.total_attack3.ToString();
        board.transform.Find("StatusPanel/spd").GetComponent<TextMeshProUGUI>().text = entry.total_speed.ToString();

        board.transform.Find("StatusPanel/def1").GetComponent<TextMeshProUGUI>().text = entry.total_defence1.ToString();
        board.transform.Find("StatusPanel/def2").GetComponent<TextMeshProUGUI>().text = entry.total_defence2.ToString();
        board.transform.Find("StatusPanel/def3").GetComponent<TextMeshProUGUI>().text = entry.total_defence3.ToString();
        board.transform.Find("StatusPanel/defX").GetComponent<TextMeshProUGUI>().text = entry.total_defenceX.ToString();

        Button ButtonBattle = board.transform.Find("ButtonBattle").GetComponent<Button>();
        ButtonBattle.onClick.RemoveAllListeners();

        if (Header.Instance.GetSummary().chara.user_id != entry.user_id)
        {

            //対戦ボタンクリック時イベントハンドラ
            ButtonBattle.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlaySE("se_btn");

                SceneController.Instance.Jump("HisPage", (() =>
                {
                    HisPageBehaviour _scene = FindObjectOfType<HisPageBehaviour>() as HisPageBehaviour;
                    _scene.Param = new HisPageBehaviour.Parameter
                    {
                        userId = entry.user_id,
                    };
                }));
            });
        }
        else
        {
            ButtonBattle.gameObject.SetActive(false);
        }

    }

    public void onGradeList()
    {
        AudioManager.Instance.PlaySE("se_btn");

        objGradeListPanel.SetActive(true);
        objGradeListPanel.transform.GetComponent<GradeListBehaviour>().Show();
    }

    public void onReloadClick()
    {
        AudioManager.Instance.PlaySE("se_btn");

        ListClear();

        //APIをたたく
        APIConnectManager.Instance.RivalList(reload);
    }

    public void onBannarClick()
    {
        AudioManager.Instance.PlaySE("se_btn");

        SceneController.Instance.PopUp("BattleEvent");
    }

    void ListClear()
    {
        //テンプレート非表示
        ListNone.gameObject.SetActive(false);
        ListUser.gameObject.SetActive(false);
        Content.SetActive(true);
        ListLoading.SetActive(false);

        foreach (Transform n in Content.transform)
        {
            //テンプレート以外は全部削除
            if (n.name != ListUser.name && n.name != ListNone.name && n.name != ListLoading.name)
                GameObject.Destroy(n.gameObject);
        }
    }


}
