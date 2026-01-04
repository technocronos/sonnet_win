using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using MyScene;
using System.Web;
using UnityEngine.Networking;

public class SuggestBehaviour : BaseBehaviour
{
    public GameObject ItemUse;
    public GameObject ItemBuy;

    public TextMeshProUGUI TextCountUse;
    public TextMeshProUGUI TextItemNameUse;
    public TextMeshProUGUI TextFlavorUse;
    public TextMeshProUGUI TextEffectUse;
    public TextMeshProUGUI TextNaviUse;
    public TextMeshProUGUI TextOkUse;

    public Image ItemIconUse;

    public TextMeshProUGUI TextCountBuy;
    public TextMeshProUGUI TextItemNameBuy;
    public TextMeshProUGUI TextEffectBuy;
    public TextMeshProUGUI TextNaviBuy;
    public TextMeshProUGUI TextOkBuy;
    public TextMeshProUGUI TextPrice;
    public TextMeshProUGUI TextTotalPrice;
    public TextMeshProUGUI TextCoinCount;
    public TextMeshProUGUI TextBuyCount;

    public Image ItemIconBuy;
    public Button btnUp;
    public Button btnDown;


    jsonSuggest suggest = null;
    int price = 0;
    int buy_count = 1;

    public class Parameter
    {
        public string type;
        public string targetId;
        public string backto;
        public string useto;
    }

    public Parameter Param;

    public static SuggestBehaviour Instance
    {
        get
        {
            return instance;
        }
    }

    private static SuggestBehaviour instance;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        instance = this;

        //suggestAPIをたたく
        APIConnectManager.Instance.Suggest(Param.type, Param.targetId, null, onLoaded);

    }

    void onLoaded(string json)
    {
        //API結果受け取り
        suggest = JsonUtility.FromJson<jsonSuggest>(json);

        //回復アイテム持っている場合
        if (suggest.uitem.num > 0)
        {
            ItemUse.SetActive(true);
            ItemBuy.SetActive(false);
            TextCountUse.text = suggest.uitem.num.ToString();
            TextCountBuy.text = suggest.uitem.num.ToString();

            TextItemNameUse.text = suggest.uitem.item_name;
            TextFlavorUse.text = suggest.uitem.flavor_text;
            TextEffectUse.text = Utility.ItemEffects(suggest.uitem);
        }
        else
        {
            ItemUse.SetActive(false);
            ItemBuy.SetActive(true);
            TextCountUse.text = "0";
            TextCountBuy.text = "0";

            TextEffectBuy.text = Utility.ItemEffects(suggest.item);

            TextItemNameBuy.text = suggest.item.item_name;
        }

        //ナビセリフ
        TextNaviUse.text = Utility.getText("SUGGEST_NAV_ITEMUSE_CONFIRM");
        TextNaviBuy.text = Utility.getText("SUGGEST_NAV_ITEMBUY_CONFIRM");

        //アイテムアイコン
        ItemIconUse.sprite = Utility.getAssetImage(Utility.getItemIconURL(suggest.item_id));
        ItemIconBuy.sprite = Utility.getAssetImage(Utility.getItemIconURL(suggest.item_id));

        //値段
        TextPrice.text = suggest.price.ToString();
        //合計値段
        TextTotalPrice.text = suggest.price.ToString();

        //所持コイン
        TextCoinCount.text = suggest.coin.ToString();

        TextBuyCount.text = this.buy_count.ToString();

        //↑ボタンクリック時イベントハンドラ
        //btnUp.onClick.RemoveAllListeners();
        btnUp.onClick.AddListener((() =>
        {
            if (this.buy_count < 10)
            {
                AudioManager.Instance.PlaySE("se_btn");
                this.buy_count++;

                TextBuyCount.text = this.buy_count.ToString();
                TextTotalPrice.text = (this.buy_count * suggest.price).ToString();

                if (this.buy_count == 10)
                    btnUp.interactable = false;

                if (this.buy_count > 1)
                    btnDown.interactable = true;
            }
        }));

        //↓ボタンクリック時イベントハンドラ
        //btnDown.onClick.RemoveAllListeners();
        btnDown.onClick.AddListener((() =>
        {
            if (this.buy_count > 1)
            {
                AudioManager.Instance.PlaySE("se_btn");
                this.buy_count--;

                TextBuyCount.text = this.buy_count.ToString();
                TextTotalPrice.text = (this.buy_count * suggest.price).ToString();

                if (this.buy_count == 1)
                    btnDown.interactable = false;
                if (this.buy_count < 10)
                    btnUp.interactable = true;
            }
        }));

        DispatchEvent(CwEvent.SCENE_READY);
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
        suggest.coin = coin;
        TextCoinCount.text = suggest.coin.ToString();
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

        APIConnectManager.Instance.Shop(suggest.item_id, category, currency, this.buy_count, ((string json) =>
        {
            //API結果受け取り
            jsonShop response = JsonUtility.FromJson<jsonShop>(json);

            if (response.result == "ok")
            {
                Trans(suggest.suggest_nexturl);
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
    /// 使うボタンクリック時イベントハンドラ
    /// </summary>
    public void onUseClick()
    {
        AudioManager.Instance.PlaySE("se_btn");

        APIConnectManager.Instance.Suggest(Param.type, Param.targetId, "use", onUseEnd);
    }

    void onUseEnd(string json)
    {
        //API結果受け取り
        jsonSuggest response = JsonUtility.FromJson<jsonSuggest>(json);

        switch (response.result)
        {
            case "ok":
                //usetoで示されているページに戻す
                Trans(Param.useto != null ? UnityWebRequest.UnEscapeURL(Param.useto) : UnityWebRequest.UnEscapeURL(Param.backto));
                break;
            case "error":
                Debug.Log(response.err_code);
                if (response.err_code == "no_item")
                    Main.Instance.showDialogue(Utility.getText("API_ERROR_Suggest_" + response.err_code), (() =>
                    {
                        AudioManager.Instance.PlaySE("se_btn");
                    }));
                else
                    Main.Instance.showDialogue(response.err_code, (() =>
                    {
                        AudioManager.Instance.PlaySE("se_btn");
                    }));
                break;
        }
    }

    /// <summary>
    /// 戻るボタンクリック時イベントハンドラ
    /// </summary>
    public void onBackClick()
    {
        AudioManager.Instance.PlaySE("se_btn");

        Trans(UnityWebRequest.UnEscapeURL(Param.backto));
    }
    public void onBackHomeClick()
    {
        AudioManager.Instance.PlaySE("se_btn");

        Trans("scene=Home");
    }

    public void Trans(string Url)
    {

        Dictionary<string, string> transUrl = new Dictionary<string, string>();
        transUrl = Utility.ParseUrl(Url);

        switch (transUrl["scene"])
        {
            case "Sphere":
                SceneController.Instance.Jump("Sphere", (() =>
                {
                    SphereBehaviour _sphere = FindObjectOfType<SphereBehaviour>() as SphereBehaviour;
                    _sphere.Param = new SphereBehaviour.Parameter
                    {
                        sphereId = int.Parse(transUrl["id"]),
                        reopen = transUrl.ContainsKey("reopen") ? transUrl["reopen"] : null,
                    };
                }));
                break;
            case "Battle":
                SceneController.Instance.Jump("Battle", (() =>
                {
                    BattleBehaviour _battle = FindObjectOfType<BattleBehaviour>() as BattleBehaviour;
                    _battle.Param = new BattleBehaviour.Parameter
                    {
                        battleId = int.Parse(transUrl["battleId"]),
                        firstscene = transUrl.ContainsKey("firstscene") ? transUrl["firstscene"] : null,
                        repaireId = transUrl.ContainsKey("repaireId") ? int.Parse(transUrl["repaireId"]) : 0,
                    };
                }));
                break;
            case "HisPage":
                SceneController.Instance.Jump("HisPage", (() =>
                {
                    HisPageBehaviour _scene = FindObjectOfType<HisPageBehaviour>() as HisPageBehaviour;
                    _scene.Param = new HisPageBehaviour.Parameter
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
                        backto = Param.backto,
                        useto = Param.useto,
                    };
                }));
                break;
            default:
                SceneController.Instance.Jump(transUrl["scene"]);
                break;
        }
    }

}
