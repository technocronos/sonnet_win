using MyScene;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuyItemPhaseBehaviour : MonoBehaviour
{
    public GameObject ContinueConfirmObj;

    public TextMeshProUGUI TextCountBuy;
    public TextMeshProUGUI TextItemNameBuy;
    public TextMeshProUGUI TextItemName;    
    public TextMeshProUGUI TextEffectBuy;
    public TextMeshProUGUI TextNaviBuy;
    public TextMeshProUGUI TextPrice;
    public TextMeshProUGUI TextTotalPrice;
    public TextMeshProUGUI TextCoinCount;
    public TextMeshProUGUI TextBuyCount;

    public Image ItemIconBuy;
    public Button btnUp;
    public Button btnDown;

    int price = 0;
    int buy_count = 1;

    jsonBattleBuyItem BattleBuyItem;
    BattleBehaviour Battle;

    const int CONTINUE_ITEM_ID = 1911;

    public string mode { get; set; }

    bool retry { get; set; } = false;

    // Start is called before the first frame update
    public static BuyItemPhaseBehaviour Instance
    {
        get
        {
            return instance;
        }
    }

    private static BuyItemPhaseBehaviour instance;

    private void Start()
    {
        instance = this;
    }

    public void BuyItemPhaseStart()
    {

        Battle = BattleBehaviour.Instance;

        TextItemName.text = Utility.getText("item_master_item_name_" + CONTINUE_ITEM_ID);

        string text = retry ? Utility.getText("BATTLE_NAV_FIN_TIMEOUT") : BattleBehaviour.Instance.getNaviSpeak(mode);
        NaviBehaviour.Instance.setText(text);

        NaviBehaviour.Instance.setSide("P");
        NaviBehaviour.Instance.Show(1);

        // APIを叩く
        APIConnectManager.Instance.BattleBuyItem(Battle.battle.battle_id, Battle.battle.validationCode, CONTINUE_ITEM_ID, onSend);

    }
    void onSend(string json)
    {
        BattleBehaviour Battle = BattleBehaviour.Instance;
        BattleBuyItem = JsonUtility.FromJson<jsonBattleBuyItem>(json);

        if (BattleBuyItem.result == "ok")
        {
            onLoaded();
        }
        else
        {
            string text = "";
            switch (BattleBuyItem.err_code)
            {
                // エラーがある場合はその内容を表示。
                case "not_found_battle":
                case "invalied_code":
                case "not_own_battle":
                case "cannot_purchase":
                    text = Utility.getText("API_ERROR_BattleBuyItem_" + BattleBuyItem.err_code);
                    break;
                default:
                    text = Utility.getText("API_ERROR_OTHER").Replace("{0}", BattleBuyItem.err_code);
                    break;
            }

            PreterBehaviour.Instance.setText(text);
            PreterBehaviour.Instance.PlayAnim("Norm");
            PreterBehaviour.Instance.Visible(true);

            NaviBehaviour.Instance.setText(Utility.getText("BATTLE_NAV_ALD_START"));
            NaviBehaviour.Instance.setSide("P");
            NaviBehaviour.Instance.Show(1);
        }

    }

    void onLoaded()
    {
        //初期化
        buy_count = 1;
        btnUp.interactable = true;
        btnDown.interactable = false;

        //アイテムアイコン
        ItemIconBuy.sprite = Utility.getAssetImage(Utility.getItemIconURL(BattleBuyItem.item_id));

        //値段
        TextPrice.text = BattleBuyItem.price.ToString();

        //合計値段
        TextTotalPrice.text = BattleBuyItem.price.ToString();


        //所持コイン
        TextCoinCount.text = BattleBuyItem.coin.ToString();

        TextBuyCount.text = this.buy_count.ToString();

        TextNaviBuy.text = Utility.getText("TEXT_NAVI_BUY_CONFIRM");

        TextCountBuy.text = "0";

        TextEffectBuy.text = Utility.ItemEffects(BattleBuyItem.item);

        //↑ボタンクリック時イベントハンドラ
        btnUp.onClick.RemoveAllListeners();
        btnUp.onClick.AddListener((() =>
        {
            if (this.buy_count < 10)
            {
                AudioManager.Instance.PlaySE("se_btn");
                this.buy_count++;

                TextBuyCount.text = this.buy_count.ToString();
                TextTotalPrice.text = (this.buy_count * BattleBuyItem.price).ToString();

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
                TextTotalPrice.text = (this.buy_count * BattleBuyItem.price).ToString();

                if (this.buy_count == 1)
                    btnDown.interactable = false;
                if (this.buy_count < 10)
                    btnUp.interactable = true;
            }
        }));
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
    void buyCoinEnd(int coin)
    {
        //所持コイン更新
        BattleBuyItem.coin = coin;
        TextCoinCount.text = BattleBuyItem.coin.ToString();

    }

    /// <summary>
    /// 買うボタンクリック時イベントハンドラ
    /// </summary>
    public void onBuyClick()
    {

        AudioManager.Instance.PlaySE("se_btn");
        //ShopAPIをたたく
        string category = "ITM";
        string currency = "coin";

        APIConnectManager.Instance.Shop(BattleBuyItem.item_id, category, currency, this.buy_count, ((string json) =>
        {
            //API結果受け取り
            jsonShop response = JsonUtility.FromJson<jsonShop>(json);

            if (response.result == "ok")
            {
                Battle.battle.continueItemCnt = this.buy_count;

                ContinueConfirmObj.SetActive(false);
                transform.gameObject.SetActive(false);

                Battle.continueConfirm();
            }
            else
            {
                Main.Instance.showDialogue(Utility.getText("API_ERROR_Shop_" + response.result), (() =>
                {
                    AudioManager.Instance.PlaySE("se_btn");
                }));
            }
        }));
    }

    /// <summary>
    /// 戻るボタンクリック時イベントハンドラ
    /// </summary>
    public void onBackClick()
    {
        AudioManager.Instance.PlaySE("se_btn");

        transform.gameObject.SetActive(false);
    }

}
