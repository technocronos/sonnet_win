using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyScene;
using Scenes.Common.Scripts;
using UnityEngine.Localization.Settings;

public class GachaBehaviour : BaseBehaviour
{
    public GameObject Content;
    public TextMeshProUGUI TextSummary;
    public TextMeshProUGUI TextNavi;

    public GameObject Arrow;

    public NaviController naviController;

    public SmorkBehaviour SmorkEffects1;
    public SmorkBehaviour SmorkEffects2;

    public Image BG;

    private jsonConstants constants;

    jsonGacha gacha_list { get; set; }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        //BG.sprite = Utility.getAssetImage("Image/BG/circle_bg");

        Debug.Log("GachaBehaviour start..");
        setSafearea("GachaCanvas");

        Header.Instance.SetTitle(Utility.getText("TEXT_GACHA"));

        Content.transform.Find("gacha_event").gameObject.SetActive(false);
        Content.transform.Find("gacha_lotation").gameObject.SetActive(false);
        Content.transform.Find("gacha_coin").gameObject.SetActive(false);
        Content.transform.Find("gacha_gold").gameObject.SetActive(false);
        Content.transform.Find("gacha_zakka").gameObject.SetActive(false);

        //定数取得
        constants = APIConnectManager.Instance.login.constants;

        //ナビのタッチは無効にしておく
        naviController.TouchPanel.gameObject.SetActive(false);
        naviController.gameObject.SetActive(false);
        Arrow.SetActive(false);

        SmorkEffects1.PlayAnim("smork");
        SmorkEffects2.PlayAnim("smork_fast");

        //APIをたたく
        APIConnectManager.Instance.Gacha(onStart);

        DispatchEvent(CwEvent.SCENE_READY);
    }



    void onStart(string json)
    {
        gacha_list = JsonUtility.FromJson<jsonGacha>(json);

        var caption = "";

        foreach (jsonGachaContents gachacontent in gacha_list.gacha)
        {

            //マグナガチャ
            if (gachacontent.gacha_kind == 1)
            {
                Content.transform.Find("gacha_gold").gameObject.SetActive(true);

                string url = Settings.Domain + "://" + Settings.Host + "/" + Utility.getGachaBannarURL(gachacontent.gacha_id) + ".png";
                if (Main.Locale != LocalizationSettings.AvailableLocales.Locales[0])
                {
                    url = Settings.Domain + "://" + Settings.Host + "/" + Utility.getGachaBannarURL(gachacontent.gacha_id) + "_en.png";
                }
                StartCoroutine(GetTexture(Content.transform.Find("gacha_gold").GetComponent<Image>(), url));

                Content.transform.Find("gacha_gold").GetComponent<Button>().onClick.AddListener((() =>
                {
                    this.showGachaDetail(gachacontent, false);
                }));
            }
            else if (gachacontent.gacha_id == 9998 || gachacontent.gacha_id == 9997)
            {
                //雑貨ガチャ
                Content.transform.Find("gacha_zakka").gameObject.SetActive(true);

                string url = Settings.Domain + "://" + Settings.Host + "/" + Utility.getGachaBannarURL(gachacontent.gacha_id) + ".png";
                if (Main.Locale != LocalizationSettings.AvailableLocales.Locales[0])
                {
                    url = Settings.Domain + "://" + Settings.Host + "/" + Utility.getGachaBannarURL(gachacontent.gacha_id) + "_en.png";
                }
                StartCoroutine(GetTexture(Content.transform.Find("gacha_zakka").GetComponent<Image>(), url));

                Content.transform.Find("gacha_zakka").GetComponent<Button>().onClick.AddListener((() =>
                {

                    //フリーのガチャが引ける場合
                    if (Header.Instance.GetSummary().freeGacha)
                    {
                        jsonGachaContents contents = new jsonGachaContents();

                        contents.gacha_id = 9997;
                        contents.gacha_name = Utility.getText("gacha_master_gacha_name_9997");

                        this.showGachaDetail(contents, false);
                    }
                    else
                    {
                        this.showGachaDetail(gachacontent, false);
                    }

                }));

                if (Header.Instance.GetSummary().tutorial_step == constants.User_Info_Tutorial.TUTORIAL_GACHA)
                {
                    Content.transform.Find("gacha_zakka").GetComponent<Button>().interactable = false;
                }

            }
            else if (gachacontent.wk_flg)
            {
                //ローテーションガチャ
                Content.transform.Find("gacha_lotation").gameObject.SetActive(true);

                string url = Settings.Domain + "://" + Settings.Host + "/" + Utility.getGachaBannarURL(gachacontent.gacha_id) + ".png";
                if (Main.Locale != LocalizationSettings.AvailableLocales.Locales[0])
                {
                    url = Settings.Domain + "://" + Settings.Host + "/" + Utility.getGachaBannarURL(gachacontent.gacha_id) + "_en.png";
                }
                StartCoroutine(GetTexture(Content.transform.Find("gacha_lotation").GetComponent<Image>(), url));

                Content.transform.Find("gacha_lotation").GetComponent<Button>().onClick.AddListener((() =>
                {
                    this.showGachaDetail(gachacontent, true);
                }));

            }
            else if (gachacontent.sp_flg)
            {
                //SPガチャ
                Content.transform.Find("gacha_coin").gameObject.SetActive(true);

                string url = Settings.Domain + "://" + Settings.Host + "/" + Utility.getGachaBannarURL(gachacontent.gacha_id) + ".png";
                if (Main.Locale != LocalizationSettings.AvailableLocales.Locales[0])
                {
                    url = Settings.Domain + "://" + Settings.Host + "/" + Utility.getGachaBannarURL(gachacontent.gacha_id) + "_en.png";
                }
                StartCoroutine(GetTexture(Content.transform.Find("gacha_coin").GetComponent<Image>(), url));

                Content.transform.Find("gacha_coin").GetComponent<Button>().onClick.AddListener((() =>
                {
                    this.showGachaDetail(gachacontent, true);
                }));

            }
            else if (gachacontent.clear_event_id != 0)
            {
                //イベントガチャ
                Content.transform.Find("gacha_event").gameObject.SetActive(true);

                caption = gachacontent.caption;

                string url = Settings.Domain + "://" + Settings.Host + "/" + Utility.getGachaBannarURL(gachacontent.gacha_id) + ".png";
                if (Main.Locale != LocalizationSettings.AvailableLocales.Locales[0])
                {
                    url = Settings.Domain + "://" + Settings.Host + "/" + Utility.getGachaBannarURL(gachacontent.gacha_id) + "_en.png";
                }

                StartCoroutine(GetTexture(Content.transform.Find("gacha_event").GetComponent<Image>(), url));

                //告知タイムの時
                if (gachacontent.notice_time)
                {
                    //告知用に切り替える
                    Content.transform.Find("gacha_event").GetComponent<Button>().interactable = false;
                }
                else
                {
                    Content.transform.Find("gacha_event").GetComponent<Button>().onClick.AddListener((() =>
                    {
                        this.showGachaDetail(gachacontent, true);
                    }));
                }
            }
            else if (Header.Instance.GetSummary().tutorial_step == constants.User_Info_Tutorial.TUTORIAL_GACHA)
            {
                //チュートリアルガチャ
                Content.transform.Find("gacha_coin").gameObject.SetActive(true);

                string url = Settings.Domain + "://" + Settings.Host + "/" + Utility.getGachaBannarURL(gachacontent.gacha_id) + ".png";
                if (Main.Locale != LocalizationSettings.AvailableLocales.Locales[0])
                {
                    url = Settings.Domain + "://" + Settings.Host + "/" + Utility.getGachaBannarURL(gachacontent.gacha_id) + "_en.png";
                }
                StartCoroutine(GetTexture(Content.transform.Find("gacha_coin").GetComponent<Image>(), url));

                Content.transform.Find("gacha_coin").GetComponent<Button>().onClick.AddListener((() =>
                {
                    this.showGachaDetail(gachacontent, true);
                }));
            }
        };

        //キャプションを更新
        TextSummary.text = caption;

        //チュートリアル中の場合
        if (Header.Instance.GetSummary().tutorial_step == constants.User_Info_Tutorial.TUTORIAL_GACHA)
        {
            TextNavi.text = Utility.getText("TEXT_NAVI_TUTORIAL_GACHA_1");

            HomeApi summary = Header.Instance.GetSummary();

            string str = Utility.getText("TEXT_NAVI_TUTORIAL_GACHA_1") + "\n" + Utility.getText("TEXT_NAVI_TUTORIAL_GACHA_2");
            string[] arr = str.Split("\n");

            summary.opening = arr;
            summary.openingNum = summary.opening.Length;

            naviController.gameObject.SetActive(true);
            naviController.onStart(summary, "navistay", TutorialNaviSpeakEnd);
        }
        else
        {
            TextNavi.text = Utility.getText("TEXT_NAVI_GACHA_INFOMATION");
        }

        AudioManager.Instance.PlayBGM("bgm_menu", AudioManager.BGM_VOLUME_DEFULT);
    }

    /// <summary>
    /// ナビがしゃべり終わった
    /// </summary>
    void TutorialNaviSpeakEnd()
    {
        naviController.disappere();

        //ナビカーソルを表示する
        Arrow.SetActive(true);
        Arrow.GetComponent<ArrowBehaviour>().Show("down", 0, -408);
    }

    void showGachaDetail(jsonGachaContents entry, bool sp_flg)
    {
        //チュートリアル中でスペシャルガチャ以外の場合
        if (Header.Instance.GetSummary().tutorial_step == constants.User_Info_Tutorial.TUTORIAL_GACHA && !sp_flg)
            return;

        AudioManager.Instance.PlaySE("se_btn");

        SceneController.Instance.Jump("GachaDetail", (() =>
        {
            GachaDetailBehaviour _dacha_detail = FindObjectOfType<GachaDetailBehaviour>() as GachaDetailBehaviour;
            _dacha_detail.Param = new GachaDetailBehaviour.Parameter
            {
                entry = entry,
                ticketCount = gacha_list.ticketCount,
                freeGacha = gacha_list.freeGacha,
            };
        }));
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}
