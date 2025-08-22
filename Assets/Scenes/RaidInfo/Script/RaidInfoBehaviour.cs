using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using TMPro;
using System;
using UnityEngine.UI;
using CreateWave;
using Scenes.Common.Scripts;
using UnityEngine.Localization.Settings;

public class RaidInfoBehaviour : BaseBehaviour
{

    public TextMeshProUGUI TextNavi;
    public TextMeshProUGUI TextTarm;
    public TextMeshProUGUI TextExplain;
    public Text TextTitle;

    public Button ButtonZukan;
    public Button ButtonRanking;

    public Image Bannar;

    public Image BG;

    int raid_dungeon_id;

    jsonConstants constants;

    jsonRaidDungeon response { get; set; }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        //ヘッダー・フッターに情報を渡す
        Header.Instance.SetTitle(Utility.getText("TEXT_RAIDDUNGEON"));

        BG.sprite = Utility.getAssetImage("Image/BG/bg2");

        //safearea対応
        setSafearea("RaidInfoCanvas");

        //定数取得
        constants = APIConnectManager.Instance.login.constants;


        TextExplain.text = Header.Instance.GetSummary().raid_dungeon.description;

        //APIをたたく
        APIConnectManager.Instance.RaidDungeon(onStart);

        DispatchEvent(CwEvent.SCENE_READY);
    }

    /// <summary>
    /// スタート時呼び出し
    /// </summary>
    /// <param name="json">json</param>
    void onStart(string json)
    {
        response = JsonUtility.FromJson<jsonRaidDungeon>(json);

        raid_dungeon_id = response.id;

        //TextExplain.text = response.description;
        TextTitle.text = response.navi_title;
        TextNavi.text = response.navi_serifu;

        if (response.status == constants.Raid_Dungeon.NONE)
        {
            TextTarm.text = "";
            TextExplain.text = Utility.getText("TEXT_RAIDDUNGEON_STATUS_NONE_2");

            ButtonZukan.interactable = false;
            ButtonRanking.interactable = false;

            return;
        }

        string f = "yyyy-MM-dd HH:mm:ss";
        DateTime start_date = DateTime.ParseExact(response.start_at, f, null);
        DateTime end_date = DateTime.ParseExact(response.end_at, f, null);

        string format = Utility.getText("TEXT_DATE_TIME_FORMAT");

        TextTarm.text = Utility.getText("RAIDDUNGEIN_TEXT_TARM").Replace("{0}", start_date.ToString(format)).Replace("{1}", end_date.ToString(format));

        string url = Settings.Domain + "://" + Settings.Host + "/" + "img/parts/sp/bannar/b_r_" + response.id.ToString("D5") + ".png";

        if (Main.Locale != LocalizationSettings.AvailableLocales.Locales[0])
        {
            url = Settings.Domain + "://" + Settings.Host + "/" + "img/parts/sp/bannar/b_r_" + response.id.ToString("D5") + "_en.png";
        }

        StartCoroutine(GetTexture(Bannar, url, "Bannar/b_q_99999_2"));

        if (response.require_kind == constants.Raid_Dungeon.REQUIRE_ETHADDR)
        {
            Bannar.GetComponent<Button>().onClick.AddListener(() =>
            {
                Main.Instance.EtheriumCanvasShow();
            });
        }
    }

    public void gotoRanking()
    {
        AudioManager.Instance.PlaySE("se_btn");

        SceneController.Instance.Jump("RaidRanking", (() =>
        {
            RaidRankingBehaviour _info = FindObjectOfType<RaidRankingBehaviour>() as RaidRankingBehaviour;
            _info.Param = new RaidRankingBehaviour.Parameter
            {
                raid_dungeon_id = this.raid_dungeon_id
            };
        }));
    }

    public void gotoZukan()
    {
        AudioManager.Instance.PlaySE("se_btn");

        SceneController.Instance.Jump("RaidZukan", (() =>
        {
            RaidZukanBehaviour _info = FindObjectOfType<RaidZukanBehaviour>() as RaidZukanBehaviour;
            _info.Param = new RaidZukanBehaviour.Parameter
            {
                raid_dungeon_id = this.raid_dungeon_id
            };
        }));

    }

}
