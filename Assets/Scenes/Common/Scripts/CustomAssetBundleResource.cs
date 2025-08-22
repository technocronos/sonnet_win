using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

public class CustomAssetBundleResource : IAssetBundleResource
{
    private AssetBundle _assetBundle;
    private DownloadHandlerAssetBundle _downloadHandler;
    private AsyncOperation _requestOperation;
    private ProvideHandle _provideHandle;
    private AssetBundleRequestOptions _options;
    private int _retryCount;

    /// <summary>
    /// 初期化する
    /// </summary>
    public void Setup(ProvideHandle provideHandle)
    {
        _retryCount = 0;
        _assetBundle = null;
        _downloadHandler = null;
        _provideHandle = provideHandle;
        _options = _provideHandle.Location.Data as AssetBundleRequestOptions;
        _requestOperation = null;
        provideHandle.SetProgressCallback(GetProgress);
    }

    /// <summary>
    /// ロード・ダウンロードする
    /// </summary>
    public void Fetch()
    {
        var location = _provideHandle.Location;
        var url = location.InternalId;

        // UnityWebRequestを生成
        var webRequest = !string.IsNullOrEmpty(_options.Hash) ?
            UnityWebRequestAssetBundle.GetAssetBundle(url, Hash128.Parse(_options.Hash), _options.Crc) :
            UnityWebRequestAssetBundle.GetAssetBundle(url, _options.Crc);
        if (_options.Timeout > 0)
        {
            webRequest.timeout = _options.Timeout;
        }
        if (_options.RedirectLimit > 0)
        {
            webRequest.redirectLimit = _options.RedirectLimit;
        }
#if !UNITY_2019_3_OR_NEWER
        webRequest.chunkedTransfer = _options.ChunkedTransfer;
#endif
        if (_provideHandle.ResourceManager.CertificateHandlerInstance != null)
        {
            webRequest.certificateHandler = _provideHandle.ResourceManager.CertificateHandlerInstance;
            webRequest.disposeCertificateHandlerOnDispose = false;
        }
        webRequest.disposeDownloadHandlerOnDispose = false;

        // UnityWebRequestを送信
        _requestOperation = webRequest.SendWebRequest();
        _requestOperation.completed += op =>
        {
            var webReq = (op as UnityWebRequestAsyncOperation).webRequest;
            if (string.IsNullOrEmpty(webReq.error))
            {
                // 成功時の処理
                _downloadHandler = webReq.downloadHandler as DownloadHandlerAssetBundle;
                _provideHandle.Complete(this, true, null);
            }
            else
            {
                // エラーがあった場合の処理
                _downloadHandler = webReq.downloadHandler as DownloadHandlerAssetBundle;
                _downloadHandler.Dispose();
                _downloadHandler = null;
                if (_retryCount >= _options.RetryCount)
                {
                    // エラー終了
                    var exception = new Exception($"Download failed : {webReq.url}");
                    _provideHandle.Complete<CustomAssetBundleResource>(null, false, exception);
                }
                else
                {
                    // リトライ
                    Fetch();
                    _retryCount++;
                }
            }
            webReq.Dispose();
        };
    }

    /// <summary>
    /// アンロードする
    /// </summary>
    public void Unload()
    {
        if (_assetBundle != null)
        {
            _assetBundle.Unload(true);
            _assetBundle = null;
        }
        if (_downloadHandler != null)
        {
            _downloadHandler.Dispose();
            _downloadHandler = null;
        }
        _requestOperation = null;
    }

    /// <summary>
    /// ロード・ダウンロードされたAssetBundleを取得する
    /// </summary>
    public AssetBundle GetAssetBundle()
    {
        if (_assetBundle == null && _downloadHandler != null)
        {
            _assetBundle = _downloadHandler.assetBundle;
            _downloadHandler.Dispose();
            _downloadHandler = null;
        }
        return _assetBundle;
    }

    /// <summary>
    /// ロード・ダウンロード進捗を取得する
    /// </summary>
    private float GetProgress() => _requestOperation != null ? _requestOperation.progress : 0.0f;
}

[System.ComponentModel.DisplayName("Custom AssetBundle Provider")]
public class CustomAssetBundleProvider : ResourceProviderBase
{
    /// <summary>
    /// ProvideHandleに入っている情報が示すAssetBundleを読み込む処理
    /// </summary>
    public override void Provide(ProvideHandle providerInterface)
    {
        var res = new CustomAssetBundleResource();
        res.Setup(providerInterface);
        res.Fetch();
    }

    /// <summary>
    /// 読み込み結果としてAssetロード用のProviderに渡すための型を返却
    /// Assets from Bundles Providerを使う場合にはIAssetBundleResourceを指定
    /// </summary>
    public override Type GetDefaultType(IResourceLocation location) => typeof(IAssetBundleResource);

    /// <summary>
    /// 解放処理
    /// </summary>
    public override void Release(IResourceLocation location, object asset)
    {
        if (location == null)
            throw new ArgumentNullException("location");
        if (asset == null)
        {
            Debug.LogWarningFormat("Releasing null asset bundle from location {0}.  This is an indication that the bundle failed to load.", location);
            return;
        }
        var bundle = asset as CustomAssetBundleResource;
        if (bundle != null)
        {
            bundle.Unload();
            return;
        }
        return;
    }
}
