using MyScene;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Scenes.Common.Scripts;
using DG.Tweening;
using UnityEngine.Localization.Settings;

public class GachaDetailBehaviour : BaseBehaviour
{

    public TextMeshProUGUI TextSummary;
    public TextMeshProUGUI TextNavi;
    public TextMeshProUGUI TextCoinCount;
    public TextMeshProUGUI TextTani;
    public TextMeshProUGUI TextTicketCount;

    public Image BannarPanel;

    public GameObject gacha_1;
    public GameObject gacha_11;
    public GameObject gacha_ticket;
    public GameObject btn_free;
    public GameObject btn_lineup;

    public GameObject HasTicketPanel;
    public GachaLineupBehaviour GachaLineupPanel;

    private string kind = "";
    private int count;

    public GameObject Arrow;

    public NaviController naviController;

    public Button ButtonCoin;

    public SmorkBehaviour SmorkEffects1;
    public SmorkBehaviour SmorkEffects2;

    public GameObject HasCoinPanel;

    public Image BG;

    private jsonConstants constants;
    private Sequence seq1 { get; set; }
    private Sequence seq11 { get; set; }
    private Sequence seqticket { get; set; }

    public class Parameter
    {
        public jsonGachaContents entry;
        public int ticketCount;
        public bool freeGacha;

    }

    public Parameter Param;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        //BG.sprite = Utility.getAssetImage("Image/BG/circle_bg");

        Debug.Log("GachaDetailBehaviour start..");
        setSafearea("GachaDetailCanvas");

        //定数取得
        constants = APIConnectManager.Instance.login.constants;

        GachaLineupPanel.gameObject.SetActive(false);

        //ナビのタッチは無効にしておく
        naviController.TouchPanel.gameObject.SetActive(false);
        naviController.gameObject.SetActive(false);
        Arrow.SetActive(false);

        SmorkEffects1.PlayAnim("smork");
        SmorkEffects2.PlayAnim("smork_fast");

        reload();

        DispatchEvent(CwEvent.SCENE_READY);
    }


    void reload()
    {
        TextNavi.text = Param.entry.flavor_text;
        btn_free.gameObject.SetActive(false);

        TextSummary.text = Param.entry.caption;

        TextTicketCount.text = Param.ticketCount.ToString();

        string url = Settings.Domain + "://" + Settings.Host + "/" + Utility.getGachaBannarURL(Param.entry.gacha_id, "_b") + ".png";
        if (Main.Locale != LocalizationSettings.AvailableLocales.Locales[0])
        {
            url = Settings.Domain + "://" + Settings.Host + "/" + Utility.getGachaBannarURL(Param.entry.gacha_id, "_b") + "_en.png";
        }
        StartCoroutine(GetTexture(BannarPanel, url));

        //硬貨の種別を設定する
        if (Param.entry.gacha_kind == 1)
        {
            this.kind = "gold";

            //マグナガチャは詳細無し
            btn_lineup.SetActive(false);


            //1連アイコン設定
            gacha_1.transform.Find("ImageCoin").gameObject.SetActive(false);
            gacha_1.transform.Find("ImageGold").gameObject.SetActive(true);

            gacha_1.transform.Find("TextCoinCount").GetComponent<TextMeshProUGUI>().text = Param.entry.price.ToString();
            gacha_1.transform.Find("TextTani").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_GOLD");

            //11連アイコン設定
            gacha_11.transform.Find("ImageCoin").gameObject.SetActive(false);
            gacha_11.transform.Find("ImageGold").gameObject.SetActive(true);

            gacha_11.transform.Find("TextCoinCount").GetComponent<TextMeshProUGUI>().text = Param.entry.price_bulk.ToString();
            gacha_11.transform.Find("TextTani").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_GOLD");

            //ガチャチケット非表示
            HasTicketPanel.SetActive(false);
            gacha_ticket.SetActive(false);

        }
        else if (Param.entry.gacha_kind == 2)
        {
            kind = "charge";

            //1連アイコン設定
            gacha_1.transform.Find("ImageCoin").gameObject.SetActive(true);
            gacha_1.transform.Find("ImageGold").gameObject.SetActive(false);

            gacha_1.transform.Find("TextCoinCount").GetComponent<TextMeshProUGUI>().text = Param.entry.price.ToString();
            gacha_1.transform.Find("TextTani").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_COIN");

            //11連アイコン設定
            gacha_11.transform.Find("ImageCoin").gameObject.SetActive(true);
            gacha_11.transform.Find("ImageGold").gameObject.SetActive(false);

            gacha_11.transform.Find("TextCoinCount").GetComponent<TextMeshProUGUI>().text = Param.entry.price_bulk.ToString();
            gacha_11.transform.Find("TextTani").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_COIN");

            //雑貨ガチャは11連無し
            if (Param.entry.gacha_id == 9998)
            {
                gacha_11.SetActive(false);
            }

            TextCoinCount.text = Header.Instance.GetSummary().coin.ToString();

            if (Param.ticketCount < Param.entry.freeticket_count)
            {
                gacha_ticket.GetComponent<Button>().interactable = false;
            }

            //持ちチケット個数設定
            TextTicketCount.text = Param.ticketCount.ToString();

            //かかるチケット個数設定
            gacha_ticket.transform.Find("TextCoinCount").GetComponent<TextMeshProUGUI>().text = Param.entry.freeticket_count.ToString();
            gacha_ticket.transform.Find("TextTani").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_TANI_MAI").Replace("{0}",string.Empty);

            //ガチャチケット表示
            HasTicketPanel.SetActive(true);
            gacha_ticket.SetActive(true);

            btn_lineup.GetComponent<Button>().onClick.AddListener((() =>
            {
                this.onLineUp(Param.entry.gacha_id);
            }));

            //チュートリアルガチャは詳細無し
            if (Header.Instance.GetSummary().tutorial_step == constants.User_Info_Tutorial.TUTORIAL_GACHA)
            {
                btn_lineup.gameObject.SetActive(false);
                ButtonCoin.interactable = false;

                btn_free.SetActive(true);

                gacha_1.SetActive(false);
                gacha_11.SetActive(false);
                gacha_ticket.SetActive(false);

                HomeApi summary = Header.Instance.GetSummary();
                /*
                summary.opening = new string[]{"ここがガチャの部屋じゃ\n今回は特別にタダじゃぞい。",
                                "りせまら、とかいうことをしてる暇があるなら修行するがよい！",
                };
                summary.openingNum = summary.opening.Length;

                naviController.gameObject.SetActive(true);
                naviController.onStart(summary, TutorialNaviSpeakEnd);
                */
                TutorialNaviSpeakEnd();
            }
        }
        else
        {
            this.kind = "free";
            this.count = 1;

            TextNavi.text = Utility.getText("TEXT_NAV_GACHA_FREE_NOTICE");
            btn_free.SetActive(true);

            gacha_1.SetActive(false);
            gacha_11.SetActive(false);
            gacha_ticket.SetActive(false);

            HasTicketPanel.SetActive(false);

            HasCoinPanel.SetActive(false);

            btn_lineup.GetComponent<Button>().onClick.AddListener((() =>
            {
                this.onLineUp(Param.entry.gacha_id);
            }));
        }

        if (gacha_1.active) {
            Image img = gacha_1.transform.Find("ImageFlash").GetComponent<Image>();

            if (gacha_1.GetComponent<Button>().interactable)
            {
                //Sequenceを宣言する
                seq1 = DOTween.Sequence();
                seq1.Append(DOVirtual.DelayedCall(1.5f, () => img.enabled = true));
                seq1.Append(DOVirtual.DelayedCall(1f, () => img.enabled = false));
                seq1.SetLoops(-1, LoopType.Restart);
            }
            else
            {
                img.enabled = false;
            }
        }
        if (gacha_11.active)
        {
            Image img = gacha_11.transform.Find("ImageFlash").GetComponent<Image>();
            if (gacha_11.GetComponent<Button>().interactable)
            {
                //Sequenceを宣言する
                seq11 = DOTween.Sequence();
                seq11.Append(DOVirtual.DelayedCall(1.5f, () => img.enabled = true));
                seq11.Append(DOVirtual.DelayedCall(1f, () => img.enabled = false));
                seq11.SetLoops(-1, LoopType.Restart);
            }
            else
            {
                img.enabled = false;
            }

        }
        if (gacha_ticket.active)
        {
            Image img = gacha_ticket.transform.Find("ImageFlash").GetComponent<Image>();

            if (gacha_ticket.GetComponent<Button>().interactable)
            {
                //Sequenceを宣言する
                seqticket = DOTween.Sequence();
                seqticket.Append(DOVirtual.DelayedCall(1.5f, () => img.enabled = true));
                seqticket.Append(DOVirtual.DelayedCall(1f, () => img.enabled = false));
                seqticket.SetLoops(-1, LoopType.Restart);
            }
            else
            {
                img.enabled = false;
            }
        }
        if (btn_free.active)
        {
            Image img = btn_free.transform.Find("ImageFlash").GetComponent<Image>();

            if (gacha_11.GetComponent<Button>().interactable)
            {
                //Sequenceを宣言する
                seqticket = DOTween.Sequence();
                seqticket.Append(DOVirtual.DelayedCall(1.5f, () => img.enabled = true));
                seqticket.Append(DOVirtual.DelayedCall(1f, () => img.enabled = false));
                seqticket.SetLoops(-1, LoopType.Restart);
            }
            else
            {
                img.enabled = false;
            }

        }
        
    }

    public void BlinkStop()
    {
        seq1.Kill();
        seq11.Kill();
        seqticket.Kill();
    }

    /// <summary>
    /// ナビがしゃべり終わった
    /// </summary>
    void TutorialNaviSpeakEnd()
    {
        naviController.gameObject.SetActive(false);

        //ナビカーソルを表示する
        Arrow.SetActive(true);
        Arrow.GetComponent<ArrowBehaviour>().Show("down", 0, 120);
    }

    public void onPurchase()
    {
        AudioManager.Instance.PlaySE("se_btn");
        Main.Instance.ShowPurchase(buyCoinEnd);
    }


    /// <summary>
    /// コイン購入成功時コールバック
    /// </summary>
    /// <param name="coin"></param>
    void buyCoinEnd(int coin)
    {
        //所持コイン更新
        HomeApi summary = Header.Instance.GetSummary();
        summary.coin = coin;
        Header.Instance.SetSummary(summary);

        reload();
    }

    jsonGachaPlay pley_response;

    void draw(string kind, int count)
    {

        int price;

        //マグナガチャ
        if (kind == "gold")
        {
            if (count == 11)
                price = Param.entry.price_bulk;
            else
                price = Param.entry.price;

            if (price > Header.Instance.GetSummary().gold)
            {
                Main.Instance.showDialogue(Utility.getText("API_ERROR_GachaPlay_not_sufficient_gold"));
                return;
            }
        }
        else if (kind == "ticket")
        {
            if (Param.entry.freeticket_count > Param.ticketCount)
            {
                Main.Instance.showDialogue(Utility.getText("API_ERROR_GachaPlay_not_sufficient_ticket"));
                return;
            }
        }
        else if (kind == "charge")
        {

            if (count == 11)
                price = Param.entry.price_bulk;
            else
                price = Param.entry.price;

            if (price > Header.Instance.GetSummary().coin)
            {
                Main.Instance.showDialogue(Utility.getText("API_ERROR_GachaPlay_not_sufficient_coin"));
                return;
            }

        }

        var text = Utility.getText("TEXT_NAV_GACHAPLAY_CONFIRM").Replace("{0}", Param.entry.gacha_name);

        Main.Instance.showConfirm(text, (() =>
        {

            APIConnectManager.Instance.GachaPlay(Param.entry.gacha_id, kind, count, (string json) =>
            {
                pley_response = JsonUtility.FromJson<jsonGachaPlay>(json);

                if (pley_response.result == "ok")
                {
                    if (pley_response.err_code != null)
                    {
                        string text = Utility.getText("API_ERROR_GachaPlay_" + pley_response.err_code);

                        Main.Instance.showDialogue(text);
                    }
                    else
                    {
                        AudioManager.Instance.StopBGM();

                        APIConnectManager.Instance.Home(onSuccessEnd);

                    }
                }

            });
        }));

    }

    public void onGacha1()
    {
        AudioManager.Instance.PlaySE("se_btn");

        this.draw(kind, 1);
    }
    public void onGacha11()
    {
        AudioManager.Instance.PlaySE("se_btn");

        this.draw(kind, 11);
    }

    public void onGachaTicket()
    {
        AudioManager.Instance.PlaySE("se_btn");

        this.draw("ticket", 1);
    }


    public void onGachaFree()
    {
        AudioManager.Instance.PlaySE("se_btn");

        //チュートリアルガチャ
        if (Header.Instance.GetSummary().tutorial_step == constants.User_Info_Tutorial.TUTORIAL_GACHA)
        {
            kind = "free";
            count = 1;
            APIConnectManager.Instance.GachaPlay(Param.entry.gacha_id, kind, count, (string json) =>
            {
                jsonGachaPlay response = JsonUtility.FromJson<jsonGachaPlay>(json);

                if (response.result == "ok")
                {
                    if (response.err_code != null)
                    {
                        string text = Utility.getText("API_ERROR_GachaPlay_" + pley_response.err_code);

                        Main.Instance.showDialogue(text);
                    }
                    else
                    {
                        AudioManager.Instance.StopBGM();

                        Dictionary<string, string> transUrl = new Dictionary<string, string>();
                        transUrl = Utility.ParseUrl(response.nextUrl);

                        SceneController.Instance.Jump("GachaResult", (() =>
                        {
                            GachaResultBehaviour _gacha_result = FindObjectOfType<GachaResultBehaviour>() as GachaResultBehaviour;
                            _gacha_result.Param = new GachaResultBehaviour.Parameter
                            {
                                dataId = transUrl["dataId"],
                            };
                        }));

                    }
                }
            });
        }
        else
        {
            this.draw(this.kind, 1);
        }
    }

    void onSuccessEnd(string json)
    {
        //API結果受け取り
        HomeApi homeSummary = JsonUtility.FromJson<HomeApi>(json);

        Header.Instance.SetSummary(homeSummary);
        Footer.Instance.SetSummary(homeSummary);

        gotonext();
    }

    void gotonext()
    {

        Dictionary<string, string> transUrl = new Dictionary<string, string>();
        transUrl = Utility.ParseUrl(pley_response.nextUrl);

        SceneController.Instance.Jump("GachaResult", (() =>
        {
            GachaResultBehaviour _gacha_result = FindObjectOfType<GachaResultBehaviour>() as GachaResultBehaviour;
            _gacha_result.Param = new GachaResultBehaviour.Parameter
            {
                dataId = transUrl["dataId"],
            };
        }));

    }

    public void onLineUp(int gacha_id)
    {
        AudioManager.Instance.PlaySE("se_btn");

        GachaLineupPanel.gameObject.SetActive(true);
        GachaLineupPanel.Show(gacha_id);
    }

    public void onButtonBack()
    {
        AudioManager.Instance.PlaySE("se_btn");

        SceneController.Instance.Jump("Gacha");
    }

}
