using MyScene;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.ScrollRect;

public class Shop : MonoBehaviour
{
    public Quest Quest;

    public ScrollRect objScrollRect;

    public GameObject ListItem;
    public GameObject ListEquip;
    public GameObject ListNone;
    public GameObject ListOnLoad;
    public GameObject Content;
    public GameObject viewport;

    public GameObject Navi;

    public GameObject ItemBuy;
    public GameObject ItemResult;

    public TextMeshProUGUI next_lv;

    public GameObject Arrow;
    public NaviController naviController;

    public Toggle TabEqp;
    public Toggle TabItem;
    public Toggle TabMagna;
    public Toggle TabCoin;

    public Button ButtonCoin;

    public class Parameter
    {
        public string currency;
        public string category;
    }

    public Parameter Param;

    string currency;
    string category;

    jsonShopList list { set; get; }
    private jsonConstants constants;

    // Start is called before the first frame update
    public void show()
    {
        Header.Instance.SetTitle(Utility.getText("TEXT_SHOP"));

        ItemBuy.SetActive(false);
        ItemResult.SetActive(false);

        //定数取得
        constants = APIConnectManager.Instance.login.constants;

        //gold マグナ coin コイン
        if (Param == null)
        {
            Param = new Shop.Parameter
            {
                currency = "gold",
                category = "ITM",
            };
        }

        this.currency = Param.currency;
        this.category = Param.category;

        ListItem.gameObject.SetActive(false);
        ListEquip.gameObject.SetActive(false);


        //ナビのタッチは無効にしておく
        naviController.TouchPanel.gameObject.SetActive(false);
        naviController.gameObject.SetActive(false);

        Arrow.SetActive(false);

        listClear();

        //APIをたたく
        APIConnectManager.Instance.ShopList(this.category, this.currency, onStart);
    }

    public void onStart(string json)
    {
        list = JsonUtility.FromJson<jsonShopList>(json);

        reload();
    }

    void reload()
    {

        objScrollRect.verticalNormalizedPosition = 1.0f;

        if (list.resultset == null)
        {
            ListNone.SetActive(true);
            ListOnLoad.SetActive(false);

            return;
        }

        //チュートリアル中でない場合
        if (Header.Instance.GetSummary().tutorial_step >= constants.User_Info_Tutorial.TUTORIAL_END)
        {
            if (list.next != null)
            {
                if (list.next.unlock_level > 0)
                    next_lv.text = Utility.getText("SHOP_TEXT_ITEM_RELEASE_LV").Replace("{0}", list.next.unlock_level.ToString());
                else
                    next_lv.text = "";
            }
        }
        else if (Header.Instance.GetSummary().tutorial_step == constants.User_Info_Tutorial.TUTORIAL_SHOPPING)
        {
            //チュートリアル中の場合
            next_lv.text = "";

            TabEqp.interactable = false;
            TabItem.interactable = false;
            TabMagna.interactable = false;
            TabCoin.interactable = false;

            ButtonCoin.interactable = false;

            HomeApi summary = Header.Instance.GetSummary();

            summary.opening = Utility.getText("TEXT_NAVI_TUTORIAL_SHOP_1").Split("\n");

            summary.openingNum = summary.opening.Length;

            naviController.gameObject.SetActive(true);
            naviController.onStart(summary, null, tutorial_navi_speak_end);
        }

        //ナビアイコン初期化
        Navi.transform.Find("ImageNavi").gameObject.SetActive(false);
        Navi.transform.Find("ImageNavi2").gameObject.SetActive(false);

        if (this.currency == "gold")
        {
            Navi.transform.Find("Flame/TextNavi").GetComponent<TextMeshProUGUI>().text = Utility.getText("SHOP_NAV_BUY_ITEM_1");

            //ナビアイコン切り替え
            Navi.transform.Find("ImageNavi").gameObject.SetActive(true);
        }
        else
        {
            Navi.transform.Find("Flame/TextNavi").GetComponent<TextMeshProUGUI>().text = Utility.getText("SHOP_NAV_BUY_ITEM_2");

            //ナビアイコン切り替え
            Navi.transform.Find("ImageNavi2").gameObject.SetActive(true);
        }

        int i = 0;
        foreach (jsonShopResultSet item in list.resultset)
        {
            GameObject _list = null;
            if (this.category == "ITM")
            {

                // リストを複製
                _list = UnityEngine.Object.Instantiate(ListItem, new Vector3(0, 0, 0), Quaternion.identity, Content.transform);
                _list.name = "ListItem" + i;
            }
            else
            {
                // リストを複製
                _list = UnityEngine.Object.Instantiate(ListEquip, new Vector3(0, 0, 0), Quaternion.identity, Content.transform);
                _list.name = "ListEquip" + i;
            }
            setupEntryBoard(item, _list.transform.Find("Panel").gameObject);

            _list.SetActive(true);

            i++;
        }
        /*
        i = 0;
        foreach (Transform n in Content.transform)
        {
            //テンプレート以外
            if (n.name != ListItem.name && n.name != ListEquip.name && n.name != ListNone.name)
            {
                EnhancedScroll(n.gameObject, i);
                i++;
            }
        }
        */
        ListOnLoad.SetActive(false);
    }

    void tutorial_navi_speak_end()
    {
        naviController.disappere();

        //ナビカーソルを表示する
        Arrow.SetActive(true);
        Arrow.GetComponent<ArrowBehaviour>().Show("up", 108, -365);
    }

    //---------------------------------------------------------------------------------------------------------
    /**
     * 引数に指定されたエントリを、指定されたボードに表示するときに呼ばれる。
     */
    void setupEntryBoard(jsonShopResultSet entry, GameObject board)
    {

        if (this.category == "ITM")
        {
            board.transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = entry.item_name;

            board.transform.Find("TextEffects").GetComponent<TextMeshProUGUI>().text = entry.effect;
        }
        else
        {
            string mount = "";

            switch (entry.category)
            {
                case "HED":
                    mount = Utility.getText("mount_master_mount_name_PLA_3");
                    board.transform.Find("mountIcon").GetComponent<Image>().sprite = Utility.getAssetImage("icon_head");
                    break;
                case "BOD":
                    mount = Utility.getText("mount_master_mount_name_PLA_2");
                    board.transform.Find("mountIcon").GetComponent<Image>().sprite = Utility.getAssetImage("icon_body");
                    break;
                case "WPN":
                    mount = Utility.getText("mount_master_mount_name_PLA_1");
                    board.transform.Find("mountIcon").GetComponent<Image>().sprite = Utility.getAssetImage("icon_weapon");
                    break;
                case "ACS":
                    mount = Utility.getText("mount_master_mount_name_PLA_4");
                    board.transform.Find("mountIcon").GetComponent<Image>().sprite = Utility.getAssetImage("icon_acs");
                    break;
            }

            board.transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = entry.item_name + "(" + entry.set_name + ")";

            board.transform.Find("StatusPanel/att1").GetComponent<TextMeshProUGUI>().text = entry.attack1.ToString();
            board.transform.Find("StatusPanel/att2").GetComponent<TextMeshProUGUI>().text = entry.attack2.ToString();
            board.transform.Find("StatusPanel/att3").GetComponent<TextMeshProUGUI>().text = entry.attack3.ToString();
            board.transform.Find("StatusPanel/spd").GetComponent<TextMeshProUGUI>().text = entry.speed.ToString();

            board.transform.Find("StatusPanel/def1").GetComponent<TextMeshProUGUI>().text = entry.defence1.ToString();
            board.transform.Find("StatusPanel/def2").GetComponent<TextMeshProUGUI>().text = entry.defence2.ToString();
            board.transform.Find("StatusPanel/def3").GetComponent<TextMeshProUGUI>().text = entry.defence3.ToString();
            board.transform.Find("StatusPanel/defX").GetComponent<TextMeshProUGUI>().text = entry.defenceX.ToString();
        }

        board.transform.Find("HasCount/TextHasCount").GetComponent<TextMeshProUGUI>().text = entry.hold.ToString();
        board.transform.Find("TextPrice").GetComponent<TextMeshProUGUI>().text = entry.price.ToString();
        board.transform.Find("FlavorText").GetComponent<TextMeshProUGUI>().text = entry.flavor_text;

        board.transform.Find("ItemBack/ItemIcon").GetComponent<Image>().sprite = Utility.getAssetImage(Utility.getItemIconURL(entry.item_id));

        if (entry.sale > 0)
        {
            board.transform.Find("SaleIcon").gameObject.SetActive(true);

            board.transform.Find("SaleIcon/saleicon_10").gameObject.SetActive(false);
            board.transform.Find("SaleIcon/saleicon_20").gameObject.SetActive(false);
            board.transform.Find("SaleIcon/saleicon_50").gameObject.SetActive(false);

            board.transform.Find("SaleIcon/saleicon_" + entry.sale).gameObject.SetActive(true);
        }
        else
        {
            board.transform.Find("SaleIcon").gameObject.SetActive(false);
        }

        if (this.currency == "gold")
        {
            board.transform.Find("CaptionCurrency").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_GOLD");
        }
        else
        {
            board.transform.Find("CaptionCurrency").GetComponent<TextMeshProUGUI>().text = Utility.getText("TEXT_COIN");
        }


        switch (this.currency)
        {
            case "gold":
                board.transform.Find("CurrencyIcon/GoldIcon").gameObject.SetActive(true);
                board.transform.Find("CurrencyIcon/CoinIcon").gameObject.SetActive(false);
                break;
            case "coin":
                board.transform.Find("CurrencyIcon/GoldIcon").gameObject.SetActive(false);
                board.transform.Find("CurrencyIcon/CoinIcon").gameObject.SetActive(true);
                break;
        }

        //イベントハンドラ登録
        board.transform.Find("ButtonBuy").GetComponent<Button>().onClick.AddListener((() =>
        {
            AudioManager.Instance.PlaySE("se_btn");

            //チュートリアル中の場合
            if (Header.Instance.GetSummary().tutorial_step == constants.User_Info_Tutorial.TUTORIAL_SHOPPING)
            {
                Arrow.SetActive(false);
            }

            ItemBuy.SetActive(true);

            ItemBuy.GetComponent<ItemBuyBehaviour>().Init(entry, this.currency, Header.Instance.GetSummary().coin, Header.Instance.GetSummary().gold);
        }));

    }

    float pos = 0;

    /// <summary>
    /// 
    /// </summary>
    public void onScroll()
    {
        int margin = 100;
        float curpos = Content.transform.GetComponent<RectTransform>().anchoredPosition.y;

        bool reloadflg = false;

        if (curpos >= pos && margin < (curpos - pos))
            reloadflg = true;

        if (curpos < pos && margin < (pos - curpos))
            reloadflg = true;

        if (reloadflg)
        {
            int i = 0;
            foreach (Transform n in Content.transform)
            {
                //テンプレート以外
                if (n.name != ListItem.name && n.name != ListEquip.name && n.name != ListNone.name)
                {
                    EnhancedScroll(n.gameObject, i);
                    i++;
                }
            }

            pos = curpos;
        }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    /// <param name="align"></param>
    void EnhancedScroll(GameObject target, float align)
    {
        target.SetActive(true);

        var contentRect = Content.transform.GetComponent<RectTransform>();
        VerticalLayoutGroup content = Content.GetComponent<VerticalLayoutGroup>();

        var targetRect = target.transform.GetComponent<RectTransform>();
        var targetPos = (content.padding.top + (targetRect.rect.height * align) + (content.padding.bottom * align)) - contentRect.anchoredPosition.y;
        var viewportHeight = viewport.GetComponent<RectTransform>().rect.height + 300;

        if (viewportHeight < targetPos)
        {
            //下に範囲外のものは非表示
            target.SetActive(false);
        }
        else if (targetPos < (targetRect.rect.height + content.padding.bottom) * -1)
        {
            //上に範囲外のものは非表示
            //target.SetActive(false);
        }

        Debug.Log("EnhancedScroll:" + "targetPos=" + targetPos);

    }

    /// <summary>
    /// リストを全部消す
    /// </summary>
    public void listClear()
    {
        ListOnLoad.SetActive(true);
        ListNone.SetActive(false);
        pos = 0;
        foreach (Transform n in Content.transform)
        {
            //テンプレート以外は全部削除
            if (n.name != ListItem.name && n.name != ListEquip.name && n.name != ListNone.name)
                GameObject.Destroy(n.gameObject);
        }
    }

    /// <summary>
    /// 装備・アイテム切り替えイベントハンドラ
    /// </summary>
    public void onChangeCategory(string category)
    {

        if (this.category == category)
            return;

        //チュートリアル中の場合は無視
        //if (Header.Instance.GetSummary().tutorial_step < constants.User_Info_Tutorial.TUTORIAL_END)
            //return;

        AudioManager.Instance.PlaySE("se_btn");

        this.category = category;

        listClear();

        APIConnectManager.Instance.ShopList(this.category, this.currency, onStart);
    }

    /// <summary>
    /// マグナ・コイン切り替えイベントハンドラ
    /// </summary>
    public void onChangeCurrency(string currency)
    {
        if (this.currency == currency)
            return;

        //チュートリアル中の場合は無視
        if (Header.Instance.GetSummary().tutorial_step < constants.User_Info_Tutorial.TUTORIAL_END)
            return;

        AudioManager.Instance.PlaySE("se_btn");

        this.currency = currency;

        listClear();
        APIConnectManager.Instance.ShopList(this.category, this.currency, onStart);
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
        reload();
    }


    public void onButtonClose()
    {
        AudioManager.Instance.PlaySE("se_btn");

        Header.Instance.SetTitle(Quest.quest_title);
        this.gameObject.SetActive(false);
    }
}
