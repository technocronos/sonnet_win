using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class WebRequestProgressNotifier
{
    UnityWebRequestAsyncOperation _asyncOp;
    IProgress<float> _progress;

    public WebRequestProgressNotifier(UnityWebRequestAsyncOperation asyncOp, IProgress<float> progress)
    {
        _asyncOp = asyncOp;
        _progress = progress;
    }

    public bool NotifyProgress()
    {
        _progress.Report(_asyncOp.progress);

        return _asyncOp.isDone;
    }
}

public class LoadingAnime : MonoBehaviour
{
    static LoadingAnime instance;
    List<WebRequestProgressNotifier> items = new List<WebRequestProgressNotifier>();

    public static LoadingAnime Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameObject("ProgressUpdater").AddComponent<LoadingAnime>();
            }

            return instance;
        }
    }

    private Sprite[] _sprites;
    private Image _image;
    private int _count = 0;
    private int _frame = 36;

    // Start is called before the first frame update
    void Start()
    {
        // Resoucesから対象のテクスチャから生成したスプライト一覧を取得
        //_sprites = Resources.LoadAll<Sprite>("Image/loading");
        //_image = gameObject.GetComponent<Image>();
    }


    public void AddItem(WebRequestProgressNotifier item)
    {
        if (!item.NotifyProgress())
        {
            items.Add(item);
        }
    }

    void Update()
    {
        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];

            if (item.NotifyProgress())
            {
                items[i] = null;
            }
        }

        // パフォーマンス的にあまりよろしくない実装なのでどうにかしたい感
        items.RemoveAll(item => item == null);
        /*
        _count++;
        if (_count % 4 == 0)
        {
            _frame++;
            if (_frame >= _sprites.Length)
            {
                _frame = 36;
                _count = 0;
            }
            _image.sprite = _sprites[_frame];
        }
        */
    }

    /*
    void Update ()
    {
        _count++;
        if (_count % 4 == 0)
        {
            _frame ++;
            if (_frame >= _sprites.Length)
            {
                _frame = 36;
                _count = 0;
            }
            _image.sprite = _sprites[_frame];
        }
    }
    */
}
