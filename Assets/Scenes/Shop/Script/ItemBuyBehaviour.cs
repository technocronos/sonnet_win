using CreateWave;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemBuyBehaviour : MonoBehaviour
{
    public TextMeshProUGUI TextItemName;
    public TextMeshProUGUI TextNavi;
    public TextMeshProUGUI TextPrice;
    public TextMeshProUGUI TextTotalPrice;
    public TextMeshProUGUI TextCoinCount;
    public TextMeshProUGUI TextBuyCount;
    public TextMeshProUGUI TextItemCategory;
    public TextMeshProUGUI TextEffect;
    public TextMeshProUGUI TextFlavor;

    public GameObject SaleIcon;
    public GameObject StatusPanel;
    public GameObject EffectPanel;
    public GameObject Navi;
    public GameObject HasCoinPanel;
    public GameObject PricePanel;

    public GameObject Arrow;
    public Button ButtonClose;

    private jsonConstants constants;

    public Image ItemIcon;
    public Button btnUp;
    public Button btnDown;

    public ItemResultBehaviour ItemResult;

    int price = 0;
    int buy_count = 1;
    int coin = 0;
    int gold = 0;

    jsonShopResultSet entry { get; set; }
    string currency { get; set; }

    const int CONTINUE_ITEM_ID = 1911;

    public string mode { get; set; }

    bool retry { get; set; } = false;

    // Start is called before the first frame update
    public static ItemBuyBehaviour Instance
    {
        get
        {
            return instance;
        }
    }

    private static ItemBuyBehaviour instance;

    private void Start()
    {
        instance = this;
    }

    public void Init(jsonShopResultSet _entry, string _currency, int _coin, int _gold)
    {
        //定数取得
        constants = APIConnectManager.Instance.login.constants;

        entry = _entry;
        currency = _currency;
        coin = _coin;
        gold = _gold;
        buy_count = 1;

        btnUp.interactable = true;
        btnDown.interactable = false;

        Arrow.SetActive(false);

        onLoaded();
    }


    void onLoaded()
    {

        string mount = "";
        switch (this.entry.category)
        {
            case "ITM":
                mount = Utility.getText("mount_master_mount_name_PLA_5");
                break;
            case "HED":
                mount = Utility.getText("mount_master_mount_name_PLA_3");
                break;
            case "BOD":
                mount = Utility.getText("mount_master_mount_name_PLA_2");
                break;
            case "WPN":
                mount = Utility.getText("mount_master_mount_name_PLA_1");
                break;
            case "ACS":
                mount = Utility.getText("mount_master_mount_name_PLA_4");
                break;
        }

        TextItemCategory.text = mount;

        //アイテム名
        TextItemName.text = entry.item_name;

        //アイテムアイコン
        ItemIcon.sprite = Utility.getAssetImage(Utility.getItemIconURL(entry.item_id));

        //セールアイコン
        if (entry.sale > 0)
        {
            SaleIcon.transform.Find("saleicon_10").gameObject.SetActive(false);
            SaleIcon.transform.Find("saleicon_20").gameObject.SetActive(false);
            SaleIcon.transform.Find("saleicon_50").gameObject.SetActive(false);

            SaleIcon.transform.Find("saleicon_" + entry.sale).gameObject.SetActive(true);
        }
        else
        {
            SaleIcon.SetActive(false);
        }

        //値段
        TextPrice.text = entry.price.ToString();

        //合計値段
        TextTotalPrice.text = entry.price.ToString();

        //フレーバーテキスト
        TextFlavor.text = entry.flavor_text;
        TextFlavor.gameObject.SetActive(false);

        if (this.entry.category != "ITM")
        {
            StatusPanel.SetActive(true);
            EffectPanel.SetActive(false);

            StatusPanel.transform.Find("att1").GetComponent<TextMeshProUGUI>().text = entry.attack1.ToString();
            StatusPanel.transform.Find("att2").GetComponent<TextMeshProUGUI>().text = entry.attack2.ToString();
            StatusPanel.transform.Find("att3").GetComponent<TextMeshProUGUI>().text = entry.attack3.ToString();
            StatusPanel.transform.Find("spd").GetComponent<TextMeshProUGUI>().text = entry.speed.ToString();

            StatusPanel.transform.Find("def1").GetComponent<TextMeshProUGUI>().text = entry.defence1.ToString();
            StatusPanel.transform.Find("def2").GetComponent<TextMeshProUGUI>().text = entry.defence2.ToString();
            StatusPanel.transform.Find("def3").GetComponent<TextMeshProUGUI>().text = entry.defence3.ToString();
            StatusPanel.transform.Find("defX").GetComponent<TextMeshProUGUI>().text = entry.defenceX.ToString();
        }
        else
        {
            TextEffect.text = entry.effect;

            StatusPanel.SetActive(false);
            EffectPanel.SetActive(true);
        }

        //通貨
        if (this.currency == "gold")
        {
            //ナビアイコン切り替え
            Navi.transform.Find("ImageNavi").gameObject.SetActive(true);
            Navi.transform.Find("ImageNavi1").gameObject.SetActive(false);

            HasCoinPanel.transform.Find("ImageCoin").gameObject.SetActive(false);
            HasCoinPanel.transform.Find("ImageGold").gameObject.SetActive(true);

            PricePanel.transform.Find("ImageCoin").gameObject.SetActive(false);
            PricePanel.transform.Find("ImageCoin").gameObject.SetActive(true);

            HasCoinPanel.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_HAS_GOLD");

            HasCoinPanel.transform.Find("TextTani").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_GOLD");
            PricePanel.transform.Find("TextTani").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_GOLD");

            PricePanel.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_UNIT_PRICE");

            TextNavi.text = Utility.getText("ITEMBUY_NAV_INFO");

            //所持マグナ
            HasCoinPanel.transform.Find("TextCoinCount").GetComponent<TextMeshProUGUI>().text = gold.ToString();
        }
        else
        {

            //ナビアイコン切り替え
            Navi.transform.Find("ImageNavi").gameObject.SetActive(false);
            Navi.transform.Find("ImageNavi1").gameObject.SetActive(true);

            HasCoinPanel.transform.Find("ImageCoin").gameObject.SetActive(true);
            HasCoinPanel.transform.Find("ImageGold").gameObject.SetActive(false);

            PricePanel.transform.Find("ImageCoin").gameObject.SetActive(true);
            PricePanel.transform.Find("ImageCoin").gameObject.SetActive(false);

            HasCoinPanel.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_HAS_COIN");

            HasCoinPanel.transform.Find("TextTani").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_COIN");
            PricePanel.transform.Find("TextTani").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_COIN");

            TextNavi.text = Utility.getText("ITEMBUY_NAV_INFO_2");

            //所持コイン
            HasCoinPanel.transform.Find("TextCoinCount").GetComponent<TextMeshProUGUI>().text = coin.ToString();
        }

        //値段
        TextPrice.text = entry.price.ToString();

        //合計値段
        TextTotalPrice.text = entry.price.ToString();

        TextBuyCount.text = this.buy_count.ToString();

        //↑ボタンクリック時イベントハンドラ
        btnUp.onClick.RemoveAllListeners();
        btnUp.onClick.AddListener((() =>
        {
            if (this.buy_count < 10)
            {
                AudioManager.Instance.PlaySE("se_btn");
                this.buy_count++;

                TextBuyCount.text = this.buy_count.ToString();
                TextTotalPrice.text = (this.buy_count * entry.price).ToString();

                if (this.buy_count == 10)
                    btnUp.interactable = false;

                if (this.buy_count > 1)
                    btnDown.interactable = true;
            }
        }));

        //↓ボタンクリック時イベントハンドラ
        btnDown.onClick.RemoveAllListeners();
        btnDown.onClick.AddListener((() =>
        {
            if (this.buy_count > 1)
            {
                AudioManager.Instance.PlaySE("se_btn");
                this.buy_count--;

                TextBuyCount.text = this.buy_count.ToString();
                TextTotalPrice.text = (this.buy_count * entry.price).ToString();

                if (this.buy_count == 1)
                    btnDown.interactable = false;
                if (this.buy_count < 10)
                    btnUp.interactable = true;
            }
        }));


        //チュートリアル中の場合
        if (Header.Instance.GetSummary().tutorial_step == constants.User_Info_Tutorial.TUTORIAL_SHOPPING)
        {
            btnUp.interactable = false;
            btnDown.interactable = false;

            ButtonClose.interactable = false;

            //ナビカーソルを表示する
            Arrow.SetActive(true);
            Arrow.GetComponent<ArrowBehaviour>().Show("down", 187, -208);
        }

    }


    /// <summary>
    /// コイン購入ボタンクリック時イベントハンドラ
    /// </summary>
    public void onCoinClick()
    {
        AudioManager.Instance.PlaySE("se_btn");
        Main.Instance.ShowPurchase(buyCoinEnd);
    }

    /// <summary>
    /// コイン購入成功時コールバック
    /// </summary>
    /// <param name="coin"></param>
    void buyCoinEnd(int result_coin)
    {
        //所持コイン更新
        coin = result_coin;
        TextCoinCount.text = coin.ToString();

    }

    /// <summary>
    /// 買うボタンクリック時イベントハンドラ
    /// </summary>
    public void onBuyClick()
    {

        AudioManager.Instance.PlaySE("se_btn");
        var text = Utility.getText("ITEMBUY_NAV_CONFIRM_1").Replace("{0}", this.entry.item_name).Replace("{1}", this.buy_count.ToString());

        Main.Instance.showConfirm(text, () =>
        {
            AudioManager.Instance.PlaySE("se_btn");

            //ShopAPIをたたく
            APIConnectManager.Instance.Shop(entry.item_id, this.entry.category, this.currency, this.buy_count, ((string json) =>
            {
                //API結果受け取り
                jsonShop response = JsonUtility.FromJson<jsonShop>(json);

                if (response.result == "ok")
                {
                    transform.gameObject.SetActive(false);
                    ItemResult.gameObject.SetActive(true);

                    int after_currency = response.gold;
                    if (this.currency != "gold")
                        after_currency = response.coin;

                    ItemResult.Init(this.entry.category, this.currency, response.buy_user_item_id, after_currency);
                }
                else 
                {
                    Main.Instance.showDialogue(Utility.getText("API_ERROR_Shop_" + response.result), (() =>
                    {
                        AudioManager.Instance.PlaySE("se_btn");
                    }));
                }

                //チュートリアル中の場合
                if (Header.Instance.GetSummary().tutorial_step == constants.User_Info_Tutorial.TUTORIAL_SHOPPING)
                {
                    Arrow.SetActive(false);
                }

            }));
        });

    }

    /// <summary>
    /// 戻るボタンクリック時イベントハンドラ
    /// </summary>
    public void onBackClick()
    {
        AudioManager.Instance.PlaySE("se_btn");

        btnUp.onClick.RemoveAllListeners();
        btnDown.onClick.RemoveAllListeners();

        transform.gameObject.SetActive(false);
    }
}
