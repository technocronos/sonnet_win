using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArrowBehaviour : MonoBehaviour
{
    Tweener tweener = null;

    public void Show(string arrow, float x, float y)
    {
        //表示画像を初期化
        transform.Find("up").gameObject.SetActive(false);
        transform.Find("down").gameObject.SetActive(false);
        transform.Find("left").gameObject.SetActive(false);
        transform.Find("right").gameObject.SetActive(false);

        transform.Find(arrow).gameObject.SetActive(true);

        //開始位置にセット
        transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(x, y, 0);

        Image image = transform.Find(arrow).GetComponent<Image>();

        //点滅
        if (tweener == null)
            tweener = image.DOFade(0.0f, 1f).SetEase(Ease.InCubic).SetLoops(-1, LoopType.Yoyo);
    }

}
