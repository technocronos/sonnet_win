using Scenes.Common.Scripts;
using System.Collections;
using System.Text;
using System.Web;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization.Settings;

public class PurchaseBehaviour : MonoBehaviour
{

    public Canvas canvas = null;
    public Purchaser Purchaser = null;
    public GameObject IAPPanel;
    public Button CloseButton;

    private static string developerPayload = "sonnet";
    private string url = "/index.php?module=Event&action=BuyCoin&opensocial_owner_id=";

    [SerializeField]
    private GameObject iapButton;

    static Canvas _canvas;

    // Use this for initialization
    void Start()
    {

        // Canvasコンポーネントを保持
        _canvas = GetComponent<Canvas>();

        int i = 0;
        //カタログを得る
        ProductCatalog cat = ProductCatalog.LoadDefaultCatalog();

        Debug.Log("count" + cat.allProducts.Count);

        //PurchaseProductを初期化
        Purchaser.ConsumableProducts = new Purchaser.PurchaseProduct[cat.allProducts.Count];

        foreach (var product in cat.allProducts)
        {
            Debug.Log("product.id=" + product.id);
            //PurchaseProductを作成
            Purchaser.PurchaseProduct p = new Purchaser.PurchaseProduct
            {
                UnityProductId = product.id,
                UnityEditorName = product.id,
                AppleName = product.id,
                GooglePlayName = product.id
            };

            Purchaser.ConsumableProducts[i] = p;

            IAPPanel.transform.Find("BuyButton_" + product.id + "/Text").GetComponent<TextMeshProUGUI>().text = "now loading..";

            i++;
        }

        //初期化したら非表示にしておく
        transform.gameObject.SetActive(false);
    }

    public delegate void EventCallback(int coin);

    private EventCallback eventCallback = null;

    // ダイアログを表示する。
    public void ShowPurchase(EventCallback _eventCallback = null)
    {
        eventCallback = _eventCallback;

        Debug.Log("ShopBehaviour showShop start...");
        // Canvas を有効にする
        if (canvas != null)
        {
            Debug.Log("ShopBehaviour showShop ok...");

            //全ボタン非活性
            SetInteractiveBuyButton(false);
            CloseButton.interactable = false;

            if (Purchaser != null)
            {
                //Purchaserを初期化する
                Purchaser.Initialize();
            }

            // コルーチンを実行  
            StartCoroutine("ProcessInit");
        }
    }

    // コルーチン  
    private IEnumerator ProcessInit()
    {
        Debug.Log("ProcessInit run..");
        // コルーチンの処理  
        do
        {
            // Purchaserが初期化されるのを待つ
            yield return new WaitForSeconds(1.0f);
        } while (!Purchaser.IsInitialized());

        Debug.Log("ProcessInit LoadDefaultCatalog..");
        //カタログを得る
        ProductCatalog cat = ProductCatalog.LoadDefaultCatalog();

        int select_lang = PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        int i = 0;

        foreach (var product in cat.allProducts)
        {

            Debug.Log("select_lang =" + select_lang);

            LocalizedProductDescription description;

            if (select_lang == 0)
            {
                description = product.defaultDescription;
            }
            else
            {
                description = product.GetDescription(TranslationLocale.en_US);
            }

            Debug.Log("description=" + description);

            ProductMetadata metadata = Purchaser.getMetaData(i);

            Debug.Log("BuyButton_" + product.id + "/Text");
            Debug.Log("metadata.localizedPriceString" + metadata.localizedPriceString);
            Debug.Log("description.Title " + description.Title);

            IAPPanel.transform.Find("BuyButton_" + product.id + "/Text").GetComponent<TextMeshProUGUI>().text = description.Title + " " + metadata.localizedPriceString;

            i++;
        }

        Debug.Log("ProcessInit SetInteractiveBuyButton..");
        //初期化が全部終わったのを確認してから全ボタン活性化
        PurchaseBehaviour.SetInteractiveBuyButton(true);
        CloseButton.interactable = true;

        //ペンディングされているものがないか調べる
        if (Purchaser.GetPendingProducts() != null)
        {
            //ある場合、ダイアログを出してペンディング処理を実行する
            Main.Instance.showDialogue(Utility.getText("TEXT_ERROR_PURCHASE_RESTORE"), (() =>
            {
                AudioManager.Instance.PlaySE("se_btn");
                this.onRestore();
            }), 4);

        }

        yield break;
    }

    //リストア処理を呼び出す
    public void onRestore()
    {
        Purchaser.BuyFailureReason result = Purchaser.RestorePurchases(this.onSuccess);
        Debug.Log("RestorePurchases result=" + result);

        //エラー以外の場合
        if (result == Purchaser.BuyFailureReason.None)
        {
            //全ボタン非活性
            PurchaseBehaviour.SetInteractiveBuyButton(false);
            CloseButton.interactable = false;
        }
        else
        {
            onBuyFailure(result);
        }
    }

    //購入ボタンクリック時イベントハンドラ
    public void OnButtonClick(string pId)
    {

        //音を鳴らす
        AudioManager.Instance.PlaySE("se_btn");

        string productId = "coin_" + pId;

        Debug.Log("ShopBehaviour button_click ok... productId=" + productId);

        //全ボタン非活性
        PurchaseBehaviour.SetInteractiveBuyButton(false);
        CloseButton.interactable = false;

        //購入ダイアログを出す
        Purchaser.BuyFailureReason r = Purchaser.BuyProductID(productId, developerPayload, this.onSuccess, this.onFailure);

        //エラーの場合
        if (r != Purchaser.BuyFailureReason.None)
        {
            Debug.Log("ShopBehaviour Failurereason=" + r);
            //全ボタン活性に戻す
            PurchaseBehaviour.SetInteractiveBuyButton(true);
            CloseButton.interactable = true;
            onBuyFailure(r);
        }
    }

    /// <summary>
    /// Purchaser.BuyFailureReasonのメッセージを表示する
    /// </summary>
    /// <param name="r"></param>
    void onBuyFailure(Purchaser.BuyFailureReason r)
    {
        string text = "";

        if (r.Equals(Purchaser.BuyFailureReason.Unknown))
        {
            text = Utility.getText("TEXT_ERROR_PURCHASE_UNKNOWN");
        }
        else if (r.Equals(Purchaser.BuyFailureReason.UnknownItem))
        {
            text = Utility.getText("TEXT_ERROR_PURCHASE_NO_ITEM");
        }
        else if (r.Equals(Purchaser.BuyFailureReason.NotReceiveMessage))
        {
            text = Utility.getText("TEXT_ERROR_PURCHASE_NO_RECEIVE");
        }
        else if (r.Equals(Purchaser.BuyFailureReason.NotInitialization))
        {
            text = Utility.getText("TEXT_ERROR_PURCHASE_NO_INIT");
        }
        else if (r.Equals(Purchaser.BuyFailureReason.NotSupported))
        {
            text = Utility.getText("TEXT_ERROR_PURCHASE_NOT_SUPPORT");
        }

        //ダイアログ表示
        Main.Instance.showDialogue(text, (() =>
        {
            AudioManager.Instance.PlaySE("se_btn");
        }), 4);
    }

    private void onSuccess(Product product)
    {
        Debug.Log("ShopBehaviour onSuccess ok...");

        Debug.Log(product.receipt);

        string _url = Settings.Domain + "://" + Settings.Host + url + SaveLoadManager.Instance.UserID;

        Debug.Log("url = " + _url);

        WWWKit.WWWClient form = new WWWKit.WWWClient(this, _url);

        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            form.AddData("receipt", product.receipt);
        }
        else
        {
            jsonPurchase receipt = JsonUtility.FromJson<jsonPurchase>(product.receipt);
            jsonPurchasePayload payload = JsonUtility.FromJson<jsonPurchasePayload>(receipt.Payload);

            Debug.Log(receipt);
            Debug.Log(payload);

            if (receipt.Store != "fake")
            {
                //jsonはreceiptとしてそのまま渡す
                form.AddData("receipt", payload.json);
                Debug.Log(payload.json);

                //シグネチャを渡す。URLエンコードが必要。
                //string signature = HttpUtility.UrlEncode(payload.signature, Encoding.UTF8);
                string signature = payload.signature;
                form.AddData("signature", signature);
                Debug.Log(signature);

                jsonPurchaseJson json = JsonUtility.FromJson<jsonPurchaseJson>(payload.json);

                form.AddData("order_id", json.orderId);
                form.AddData("product_id", json.productId);
                Debug.Log(json.orderId);
                Debug.Log(json.productId);
            }
            else
            {
                form.AddData("receipt", product.receipt);
            }
        }

#if UNITY_EDITOR
        form.AddHeader("user-agent", @"Mozilla/5.0 (iPhone; CPU iPhone OS 11_2_5 like Mac OS X) AppleWebKit/604.5.2 (KHTML, like Gecko) Version/11.0 Mobile/15D5046b Safari/604.1");
#elif UNITY_IPHONE
        form.AddHeader("user-agent", @"" + SystemInfo.operatingSystem + "/" + UnityEngine.iOS.Device.generation.ToString() + "/" + SystemInfo.deviceModel);
#elif UNITY_ANDROID
        form.AddHeader("user-agent", @"" + SystemInfo.operatingSystem + "/" + SystemInfo.deviceModel);
#endif

        form.Timeout = 60f;

        form.OnDone = (WWW www) =>
        {
            Debug.Log("www.text = " + www.text);

            bool result = false;

            //課金成功
            if (www.text.Equals("OK"))
            {
                Debug.Log("www.text OK");
                result = Purchaser.GetComponent<Purchaser>().ConfirmPendingPurchase(product);

                //homeAPIを実行して購入を反映させる
                APIConnectManager.Instance.Home(onSuccessEnd);
            }
            else
            {
                //課金失敗
                Debug.Log("www.text OnDone NG");
                Purchaser.GetComponent<Purchaser>().OnPurchaseFailed(product, PurchaseFailureReason.PaymentDeclined);

                Main.Instance.showDialogue(Utility.getText("TEXT_ERROR_PURCHASE_DB_ERROR"), (() =>
                {
                    AudioManager.Instance.PlaySE("se_btn");
                }), 4);
            }

            //全ボタン活性
            PurchaseBehaviour.SetInteractiveBuyButton(true);
            CloseButton.interactable = true;
        };

        form.OnFail = (WWW www) =>
        {
            //課金失敗
            Debug.Log("www.text OnFail NG");
            Purchaser.GetComponent<Purchaser>().OnPurchaseFailed(product, PurchaseFailureReason.Unknown);

            //全ボタン活性
            PurchaseBehaviour.SetInteractiveBuyButton(true);
            CloseButton.interactable = true;

            Main.Instance.showDialogue(Utility.getText("TEXT_ERROR_PURCHASE_DB_ERROR"), (() =>
            {
                AudioManager.Instance.PlaySE("se_btn");
            }), 4);
        };

        form.OnDisposed = () =>
        {
            //課金失敗
            Debug.Log("www.text OnDisposed NG");
            Purchaser.GetComponent<Purchaser>().OnPurchaseFailed(product, PurchaseFailureReason.PurchasingUnavailable);

            //全ボタン活性
            PurchaseBehaviour.SetInteractiveBuyButton(true);
            CloseButton.interactable = true;

            Main.Instance.showDialogue(Utility.getText("TEXT_ERROR_PURCHASE_TIMEOUT"), (() =>
            {
                AudioManager.Instance.PlaySE("se_btn");
            }), 4);
        };

        form.Request();

    }

    /*
     PurchasingUnavailable	システムの購入機能が利用できません。
     ExistingPurchasePending	新たに購入をリクエストしましたが、すでに購入処理中でした。
     ProductUnavailable	ストアで購入できる商品ではありません。
     SignatureInvalid	課金レシートのシグネチャ検証に失敗しました。
     UserCancelled	ユーザは購入の続行よりキャンセルを選びました。
     PaymentDeclined	支払いに問題がありました。
     Unknown	認識不能な問題のある購入のすべて。
     */
    private void onFailure(Product product, PurchaseFailureReason reason)
    {
        Debug.Log("ShopBehaviour onFailure PurchaseFailureReason " + reason);

        //全ボタン活性
        PurchaseBehaviour.SetInteractiveBuyButton(true);
        CloseButton.interactable = true;

        string text = "";

        if (reason.Equals(PurchaseFailureReason.Unknown))
        {
            text = Utility.getText("TEXT_ERROR_PURCHASE");
        }
        else if (reason.Equals(PurchaseFailureReason.PurchasingUnavailable))
        {
            text = Utility.getText("TEXT_ERROR_PURCHASE_NO_USE_SYSTEM");
        }
        else if (reason.Equals(PurchaseFailureReason.ExistingPurchasePending))
        {
            text = Utility.getText("TEXT_ERROR_PURCHASE_NOW_FUNK");
        }
        else if (reason.Equals(PurchaseFailureReason.ProductUnavailable))
        {
            text = Utility.getText("TEXT_ERROR_PURCHASE_NO_ITEM");
        }
        else if (reason.Equals(PurchaseFailureReason.SignatureInvalid))
        {
            text = Utility.getText("TEXT_ERROR_PURCHASE_RECEPT_INVALID");
        }
        else if (reason.Equals(PurchaseFailureReason.UserCancelled))
        {
            text = Utility.getText("TEXT_ERROR_PURCHASE_CANCEL");
        }
        else if (reason.Equals(PurchaseFailureReason.PaymentDeclined))
        {
            text = Utility.getText("TEXT_ERROR_PURCHASE_WRONG_PAYMENT");
        }

        //ダイアログ表示
        Main.Instance.showDialogue(text, (() =>
        {
            AudioManager.Instance.PlaySE("se_btn");
        }), 4);


    }

    // ショップを閉じる
    public void OnButtonClose()
    {
        //音を鳴らす
        AudioManager.Instance.PlaySE("se_btn");

        // ダイアログを閉じる
        transform.gameObject.SetActive(false);

    }

    //購入ボタンの活性・非活性を一括で切り換える
    public static void SetInteractiveBuyButton(bool b)
    {
        Debug.Log("SetInteractiveBuyButton " + b);
        //カタログを得る
        ProductCatalog cat = ProductCatalog.LoadDefaultCatalog();

        foreach (var product in cat.allProducts)
        {
            SetInteractive("BuyButton_" + product.id, b);
        }
    }

    /// ボタンの有効・無効を設定する
    public static void SetInteractive(string name, bool b)
    {
        //Debug.Log ("SetInteractive name=" + name);

        var IAPPannel = _canvas.transform.Find("IAPPanel");

        foreach (Transform child in IAPPannel)
        {
            //Debug.Log (child.name);
            // 子の要素をたどる
            if (child.name == name)
            {
                // 指定した名前と一致
                // Buttonコンポーネントを取得する
                Button btn = child.GetComponent<Button>();
                // 有効・無効フラグを設定
                btn.interactable = b;
                // おしまい
                return;
            }
        }
        // 指定したオブジェクト名が見つからなかった
        Debug.LogWarning("Not found objname:" + name);
    }

    void onSuccessEnd(string json)
    {
        //API結果受け取り
        HomeApi homeSummary = JsonUtility.FromJson<HomeApi>(json);

        Header.Instance.SetSummary(homeSummary);
        Footer.Instance.SetSummary(homeSummary);

        if (this.eventCallback != null)
        {
            //コールバック
            this.eventCallback?.Invoke(homeSummary.coin);
        }

        // ダイアログを閉じる
        transform.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        Debug.Log("OnDestory run..");
    }
}
