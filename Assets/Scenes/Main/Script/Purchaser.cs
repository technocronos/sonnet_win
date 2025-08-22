using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

/// <summary>
/// 課金処理を扱うクラスです。
/// 非消費、サブスクリプション型は動作未検証です。
/// Android/iOSのみ動作検証しています。
/// INFO:[iOS]未購入のアイテムがあるとInitialize後すぐに認証ダイアログが表示されるので、呼び出すタイミングを調整してください
/// INFO:[All]初期化前にアイテム情報を設定してください
/// INFO:[All]BuyProductIDやRestorePurchasesは連打できないようにUI側で制御する必要があります
/// </summary>
public class Purchaser : MonoBehaviour, IStoreListener
{
    /// <summary>
    /// 初期化状態の定数
    /// </summary>
    private enum InitializeState
    {
        /// <summary>
        /// 初期化をまだ行っていない
        /// </summary>
        NotInitialization,
        /// <summary>
        /// 初期化中
        /// </summary>
        Initializing,
        /// <summary>
        /// 初期化成功
        /// </summary>
        Successful,
        /// <summary>
        /// 初期化失敗
        /// </summary>
        Failure
    }

    /// <summary>
    /// 購入失敗時の定数
    /// </summary>
    public enum BuyFailureReason
    {
        /// <summary>
        /// エラー無し
        /// </summary>
        None,
        /// <summary>
        /// 課金システムが初期化されていない
        /// </summary>
        NotInitialization,
        /// <summary>
        /// 課金システムが初期化中
        /// </summary>
        Initializing,
        /// <summary>
        /// 販売されていないアイテムを指定した
        /// </summary>
        UnknownItem,
        /// <summary>
        /// 課金メッセージを受け取れない場合
        /// 主ににコールバックを設定していないミス
        /// </summary>
        NotReceiveMessage,
        /// <summary>
        /// 通信不可状態（課金システムの初期化は完了）
        /// </summary>
        NetworkUnavailable,
        /// <summary>
        /// 非サポート（リストアの場合）
        /// </summary>
        NotSupported,
        /// <summary>
        /// 不明なエラー
        /// </summary>
        Unknown
    }

    /// <summary>
    /// UnityEditorのダミーストア名
    /// </summary>
    private const string UnityEditorStore = "dummy";

    /// <summary>
    /// IStoreController（基本的な課金処理を行うオブジェクト）
    /// </summary>
    private static IStoreController StoreController;

    /// <summary>
    /// IExtensionProvider（各ストアに依存する課金処理を行うオブジェクト）
    /// </summary>
    private static IExtensionProvider StoreExtensionProvider;

    /// <summary>
    /// 消費型アイテム
    /// </summary>
    public PurchaseProduct[] ConsumableProducts;
    /// <summary>
    /// 非消費型アイテム
    /// </summary>
    public PurchaseProduct[] NonConsumableProducts;
    /// <summary>
    /// サブスクリプション型アイテム
    /// </summary>
    public PurchaseProduct[] SubscriptionProducts;

    /// <summary>
    /// 課金成功時のデリゲート
    /// </summary>
    public delegate void SuccessEvent(Product product);

    /// <summary>
    /// 課金完了通知イベント
    /// </summary>
    private SuccessEvent OnPurchaseSuccessResult;

    /// <summary>
    /// 課金失敗時のデリゲート
    /// </summary>
    public delegate void FailureEvent(Product product, PurchaseFailureReason reason);

    /// <summary>
    /// 課金失敗通知イベント
    /// </summary>
    private FailureEvent OnPurchaseFailureResult;

    /// <summary>
    /// Pendingとなったアイテム
    /// </summary>
    private Dictionary<string, Product> PendingProduct;

    /// <summary>
    /// InitializeState
    /// </summary>
    private InitializeState InitPurchaseState;

    /// <summary>
    /// インスタンス
    /// </summary>
    private static Purchaser instance;

    /// <summary>
    /// シングルトン
    /// </summary>
    /// <value>Purchaser</value>
    public static Purchaser Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType<Purchaser>();
                instance.InitPurchaseState = InitializeState.NotInitialization;
            }
            return instance;
        }
    }

    /// <summary>
    /// 課金システムの初期化を行います。
    /// INFO:アイテム情報をサーバから取得するなどを考慮して外部から初期化を行います。
    /// </summary>
    public void Initialize()
    {
        Debug.Log("Initialize run..");
        // Unity IAPが初期化されていない場合はシステムを初期化
        if (StoreController == null)
        {
            InitializePurchasing();
        }
    }

    /// <summary>
    /// 課金システムの初期化を行います。
    /// </summary>
    private void InitializePurchasing()
    {
        Debug.Log("InitializePurchasing run..");
        // 既にIAPが初期化されている場合は何もしない
        if (IsInitialized())
        {
            return;
        }

        InitPurchaseState = InitializeState.Initializing;
        PendingProduct = new Dictionary<string, Product>();

        // Unityの課金システム構築
        StandardPurchasingModule module = StandardPurchasingModule.Instance();
#if UNITY_EDITOR
        module.useFakeStoreUIMode = FakeStoreUIMode.StandardUser;
#endif
        ConfigurationBuilder builder = ConfigurationBuilder.Instance(module);

        // 消費型アイテム
        AddProduct(builder, ProductType.Consumable, ConsumableProducts);
        // 非消費型アイテム
        AddProduct(builder, ProductType.NonConsumable, NonConsumableProducts);
        // サブスクリプション
        AddProduct(builder, ProductType.Subscription, SubscriptionProducts);

        // 非同期の課金処理の初期化を開始
        UnityPurchasing.Initialize(this, builder);
    }

    /// <summary>
    /// ProductTypeごとにアイテムを追加します。
    /// </summary>
    /// <param name="builder">ConfigurationBuilder.</param>
    /// <param name="productType">ProductType.</param>
    /// <param name="products">PurchaseProductの配列.</param>
    private static void AddProduct(ConfigurationBuilder builder, ProductType productType, PurchaseProduct[] products)
    {
        Debug.Log("AddProduct run..");
        if (products == null)
        {
            return;
        }

        int length = products.Length;
        if (length == 0)
        {
            return;
        }
        // アイテムの追加
        for (int i = 0; i < length; i++)
        {
            IDs ids = new IDs();
            PurchaseProduct product = products[i];
            // UnityEditor
            if (!String.IsNullOrEmpty(product.UnityProductId))
            {
                ids.Add(product.UnityProductId, UnityEditorStore);
            }
            // AppleAppStore
            if (!String.IsNullOrEmpty(product.AppleName))
            {
                ids.Add(product.AppleName, AppleAppStore.Name);
            }
            // GooglePlay
            if (!String.IsNullOrEmpty(product.GooglePlayName))
            {
                ids.Add(product.GooglePlayName, GooglePlay.Name);
            }

            Debug.Log("product.UnityProductId=" + product.UnityProductId);
            builder.AddProduct(product.UnityProductId, productType, ids);
        }
    }

    public ProductMetadata getMetaData(int i)
    {
        if(StoreController != null)
        {
            return StoreController.products.all[i].metadata;
        }

        return null;
    }

    /// <summary>
    /// UnityIAPの初期化を行っているか返します
    /// </summary>
    /// <returns><c>true</c> なら初期化済み、<c>false</c>なら未初期化</returns>
	public bool IsInitialized()
    {
        return StoreController != null && StoreExtensionProvider != null;
    }

    /// <summary>
    /// アイテムの購入を行います。
    /// </summary>
    /// <returns>BuyFailureReason</returns>
    /// <param name="productId">アイテムID</param>
    /// <param name="successResult">Success result.</param>
    /// <param name="failureEvent">Failure event.</param>
    public BuyFailureReason BuyProductID(string productId, SuccessEvent successResult, FailureEvent failureEvent)
    {
        return BuyProductID(productId, null, successResult, failureEvent);
    }

    /// <summary>
    /// アイテムの購入を行います。
    /// </summary>
    /// <returns>BuyFailureReason(NotSupported以外全て)</returns>
    /// <param name="productId">アイテムID</param>
    /// <param name="developerPayload">購入の一致を検証するためのID</param>
    /// <param name="successResult">Success result.</param>
    /// <param name="failureEvent">Failure event.</param>
    public BuyFailureReason BuyProductID(string productId, string developerPayload, SuccessEvent successResult, FailureEvent failureEvent)
    {
        Debug.Log("BuyProductID runn..");
        // コールバックが通知できない場合は何もしない
        if (successResult == null || failureEvent == null)
        {
            Debug.Log("BuyProductID .NotReceiveMessage..");
            return BuyFailureReason.NotReceiveMessage;
        }

        // アプリが強制終了しても処理を続行する
        try
        {
            // 課金システムが未初期化の場合はなにもしない
            if (!IsInitialized())
            {
                BuyFailureReason reason = BuyFailureReason.Unknown;
                switch (InitPurchaseState)
                {
                    case InitializeState.NotInitialization:
                        reason = BuyFailureReason.NotInitialization;
                        Debug.Log("BuyProductID NotInitialization..");
                        break;
                    case InitializeState.Failure:
                        reason = BuyFailureReason.NotInitialization;
                        Debug.Log("BuyProductID Failure..");
                        break;
                    case InitializeState.Initializing:
                        reason = BuyFailureReason.Initializing;
                        Debug.Log("BuyProductID Initializing..");
                        break;
                }
                return reason;
            }

            Product product = StoreController.products.WithID(productId);
            // 購入できないアイテムの場合
            if (product == null || !product.availableToPurchase)
            {
                Debug.Log("BuyProductID UnknownItem..");
                return BuyFailureReason.UnknownItem;
            }

            // 通信不可の場合は何もしない（初期化は必ず終了している）
            if (!hasNetworkConnection())
            {
                Debug.Log("BuyProductID NetworkUnavailable..");
                return BuyFailureReason.NetworkUnavailable;
            }

            // 以降はエラーが無いのでコールバック設定
            OnPurchaseSuccessResult = successResult;
            OnPurchaseFailureResult = failureEvent;

            // Androidの場合は必ずDeveloperPayloadを送る
            if (Application.platform == RuntimePlatform.Android)
            {
                if (developerPayload == null)
                {
                    return BuyFailureReason.NotReceiveMessage;
                }
                Debug.Log("BuyProductID InitiatePurchase ok..");
                StoreController.InitiatePurchase(product, developerPayload);
            }
            else
            {
                Debug.Log("BuyProductID InitiatePurchase ok..");
                StoreController.InitiatePurchase(product);
            }
            // 成功
            return BuyFailureReason.None;
        }
        catch (Exception e)
        {
            // 何らかのエラーが発生（課金処理は未発生）したためコールバックを取り外す
            Debug.Log("BuyProductID fail Unknown.." + e.Message);


            OnPurchaseSuccessResult = null;
            OnPurchaseFailureResult = null;
            return BuyFailureReason.Unknown;
        }
    }

    /// <summary>
    /// IStoreControllerのConfirmPendingPurchaseを実行します
    /// </summary>
    /// <returns>trueの場合は消費実行</returns>
    /// <param name="product">Product.</param>
    public bool ConfirmPendingPurchase(Product product)
    {
        Debug.Log("ConfirmPendingPurchase run..");
        if (!IsInitialized())
        {
            return false;
        }

        // 通信不可の場合は消費しない
        if (!hasNetworkConnection())
        {
            return false;
        }

        // 完了の通知とPendingアイテム情報の更新
        //たとえUpdatePendingProductで消費できなくとも、初期化後にまたアイテム情報がやってくる
        StoreController.ConfirmPendingPurchase(product);
        UpdatePendingProduct(product.transactionID, product, PurchaseProcessingResult.Complete);
        return true;
    }

    /// <summary>
    /// 全てのペンディングとなったアイテムを取得します。
    /// </summary>
    /// <returns>The pending products.</returns>
    public Product[] GetPendingProducts()
    {
        if (!IsInitialized())
        {
            return null;
        }

        // アイテムがない場合はnullを返す
        if (PendingProduct.Values.Count == 0)
        {
            return null;
        }

        Product[] pendingProducts = new Product[PendingProduct.Values.Count];
        PendingProduct.Values.CopyTo(pendingProducts, 0);
        return pendingProducts;
    }

    /// <summary>
    /// アイテム情報を取得します。
    /// </summary>
    /// <returns>Product</returns>
    /// <param name="unityProductId">Unityで扱うアイテムID.</param>
    public Product GetProduct(string unityProductId)
    {
        if (!IsInitialized())
        {
            return null;
        }

        return StoreController.products.WithID(unityProductId);
    }

    /// <summary>
    /// 全てのアイテム情報を取得します。
    /// </summary>
    /// <returns>Product.</returns>
    public Product[] GetProducts()
    {
        if (!IsInitialized())
        {
            return null;
        }

        return StoreController.products.all;
    }

    /// <summary>
    /// アイテムのリストア処理を行います。
    /// <return>NotInitialization/NetworkUnavailable/None/NotSupportedのいずれか</return>
    /// </summary>
    public BuyFailureReason RestorePurchases(SuccessEvent successEvent)
    {
        Debug.Log("RestorePurchases run..");
        // iPhone/OSXでない場合
        if (Application.platform != RuntimePlatform.IPhonePlayer &&
            Application.platform != RuntimePlatform.OSXPlayer)
        {
            return BuyFailureReason.NotSupported;
        }

        // 初期化されていない場合は何もしない
        if (!IsInitialized())
        {
            return BuyFailureReason.NotInitialization;
        }

        // 通信不可の場合は何もしない
        if (!hasNetworkConnection())
        {
            return BuyFailureReason.NetworkUnavailable;
        }


        // iPhone/OSXでない場合
        if (Application.platform != RuntimePlatform.IPhonePlayer &&
            Application.platform != RuntimePlatform.OSXPlayer)
        {
            IGooglePlayStoreExtensions google = StoreExtensionProvider.GetExtension<IGooglePlayStoreExtensions>();

            google.RestoreTransactions((result, message) =>
            {
                Debug.Log("RestoreTransactions run.. result=" + result + "message=" + message);

                Product[] pendingProducts = GetPendingProducts();
                // Pendingアイテムがない場合は通知のみ
                if (pendingProducts == null)
                {
                    successEvent.Invoke(null);
                    return;
                }

                // 消費型アイテムの場合は自動で購入処理が呼ばれないので、呼び出し側から購入処理を行ってもらう
                int length = pendingProducts.Length;
                for (int i = 0; i < length; i++)
                {
                    Product targetProduct = pendingProducts[i];
                    if (targetProduct.hasReceipt && targetProduct.definition.type == ProductType.Consumable)
                    {
                        successEvent.Invoke(targetProduct);
                    }
                }
            });

        }
        else
        {

            // リストア処理
            IAppleExtensions apple = StoreExtensionProvider.GetExtension<IAppleExtensions>();

            apple.RestoreTransactions((result, message) =>
            {
                Product[] pendingProducts = GetPendingProducts();
                // Pendingアイテムがない場合は通知のみ
                if (pendingProducts == null)
                {
                    successEvent.Invoke(null);
                    return;
                }

                // TODO:リストで返すほうがいいか検討
                // 消費型アイテムの場合は自動で購入処理が呼ばれないので、呼び出し側から購入処理を行ってもらう
                int length = pendingProducts.Length;
                for (int i = 0; i < length; i++)
                {
                    Product targetProduct = pendingProducts[i];
                    if (targetProduct.hasReceipt && targetProduct.definition.type == ProductType.Consumable)
                    {
                        successEvent.Invoke(targetProduct);
                    }
                }
            });
        }

        return BuyFailureReason.None;
    }

    /// <summary>
    /// 通信接続があるかチェックします。
    /// </summary>
    /// <returns><c>true</c>の場合は通信接続がある</returns>
    private static bool hasNetworkConnection()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }

    /// <summary>
    /// IStoreListenerの初期化完了通知です。
    /// </summary>
    /// <param name="controller">IStoreController.</param>
    /// <param name="extensions">IExtensionProvider.</param>
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        StoreController = controller;
        StoreExtensionProvider = extensions;
        InitPurchaseState = InitializeState.Successful;
    }

    /// <summary>
    /// IStoreListenerの初期化失敗通知です。
    /// </summary>
    /// <param name="error">InitializationFailureReason.</param>
    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log("OnInitializeFailed Failure.." + error.ToString());
        InitPurchaseState = InitializeState.Failure;
    }

    /// <summary>
    /// IStoreListenerのアプリ内課金成功の通知です。
    /// コールバック完了後delegateは参照を失います
    /// </summary>
    /// <returns>PurchaseProcessingResult.</returns>
    /// <param name="args">PurchaseEventArgs.</param>
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        Debug.Log("ProcessPurchase run..");

        string unityProductId = args.purchasedProduct.definition.id;
        Product product = args.purchasedProduct;

        // 通信前提のため一度Pendingに追加する
        UpdatePendingProduct(product.transactionID, product, PurchaseProcessingResult.Pending);

        // コールバックが通知できない場合はここで処理を終える
        if (OnPurchaseSuccessResult == null || OnPurchaseFailureResult == null)
        {
            Debug.Log("ProcessPurchase PurchaseProcessingResult.Pending..");
            return PurchaseProcessingResult.Pending;
        }

        // 初期化時に登録されていないアイテムの場合（アプリの不具合・サーバの設定ミス等）
        if (unityProductId == null)
        {
            Debug.Log("ProcessPurchase unityProductId null");

            OnPurchaseFailureResult.Invoke(product, PurchaseFailureReason.ProductUnavailable);
            Purchaser.Instance.OnPurchaseSuccessResult = null;
            Purchaser.Instance.OnPurchaseFailureResult = null;
            return PurchaseProcessingResult.Pending;
        }

        // アプリの強制終了にも耐えうるようにここで処理
        try
        {
            Debug.Log("ProcessPurchase ok..");
            // アイテムの購入完了処理
            // 未登録のアイテムの除外はしていない->過去に購入した現在は販売していないアイテムが未消費の可能性があるため
            OnPurchaseSuccessResult.Invoke(product);
        }
        catch (Exception)
        {
            Debug.Log("ProcessPurchase PurchaseFailureReason.Unknown..");
            // 不明なエラーが発生（成功のコールバックで強制終了している場合もここで通知されるので、レシートの有無で判断する）
            OnPurchaseFailureResult.Invoke(product, PurchaseFailureReason.Unknown);
        }

        // delegateの参照を削除
        Purchaser.Instance.OnPurchaseSuccessResult = null;
        Purchaser.Instance.OnPurchaseFailureResult = null;
        return PurchaseProcessingResult.Pending;
    }

    /// <summary>
    /// Pending状態のアイテムを更新します。
    /// </summary>
    /// <param name="transactionId">IAPのトランザクションID</param>
    /// <param name="product">アイテム.</param>
    /// <param name="result">PurchaseProcessingResult.</param>
    private void UpdatePendingProduct(string transactionId, Product product, PurchaseProcessingResult result)
    {
        Debug.Log("UpdatePendingProduct transactionId=" + transactionId);

        // レシートを持っていない場合は何もしない
        if (!product.hasReceipt)
        {
            return;
        }

        // Pendingの場合は最新のものに更新
        if (result == PurchaseProcessingResult.Pending)
        {
            Debug.Log("UpdatePendingProduct result=Pending");
            if (PendingProduct.ContainsKey(transactionId))
            {
                Debug.Log("UpdatePendingProduct remove");
                PendingProduct.Remove(transactionId);
            }
            PendingProduct.Add(transactionId, product);
        }
        else if (result == PurchaseProcessingResult.Complete)
        {
            Debug.Log("UpdatePendingProduct result=Complete");
            // 完了した場合は削除
            if (PendingProduct.ContainsKey(transactionId))
            {
                PendingProduct.Remove(transactionId);
            }
        }
    }

    /// <summary>
    /// IStoreListenerアプリ内課金の失敗の通知です。
    /// この処理は非同期で行われます。コールバック完了後delegateは参照を失います
    /// </summary>
    /// <param name="product">Product.</param>
    /// <param name="failureReason">PurchaseFailureReason.</param>
    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log("OnPurchaseFailed result=" + failureReason);
        if (OnPurchaseFailureResult != null)
        {
            OnPurchaseFailureResult.Invoke(product, failureReason);
        }

        // delegateの参照を削除
        Purchaser.Instance.OnPurchaseSuccessResult = null;
        Purchaser.Instance.OnPurchaseFailureResult = null;
    }

    /// <summary>
    /// 課金アイテムの設定
    /// </summary>
    [System.Serializable]
    public class PurchaseProduct
    {
        /// <summary>
        ///  Unity上で扱うアイテムID
        /// </summary>
        public string UnityProductId;

        /// <summary>
        /// UnityEditor上で扱うアイテムID
        /// </summary>
        public string UnityEditorName;

        /// <summary>
        /// Appleストアで扱うアイテムID
        /// </summary>
        public string AppleName;

        /// <summary>
        /// GooglePlayで扱うアイテムID
        /// </summary>
        public string GooglePlayName;
    }

    public void OnInitializeFailed(InitializationFailureReason error, string? message)
    {

    }
}
