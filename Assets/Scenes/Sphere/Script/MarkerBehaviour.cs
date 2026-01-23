using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MarkerBehaviour : MonoBehaviour
{

    private Sprite[] _sprites { get; set; }
    private Image _image { get; set; }
    private int _count { get; set; } = 0;
    private int _frame { get; set; } = 0;

    //     color    マーカの色。"move", "target", "damag", "recov" のいずれか。
    public string color { get; set; } = "move";

    public void Init(string _color)
    {
        color = _color;

        string imagename = "";
        switch (color)
        {
            case "move":
                imagename = "rangemarker_b";
                break;
            case "target":
                imagename = "rangemarker_p";
                break;
            case "damag":
                imagename = "rangemarker_r";
                break;
            case "recov":
                imagename = "rangemarker_w";
                break;
        }

        _sprites = Resources.LoadAll<Sprite>("Image/" + imagename);
        _image = gameObject.GetComponent<Image>();

        _frame = _sprites.Length;

        _image.sprite = _sprites[_count];

        _image.DOFade(0.2f, 1.2f).From(0.7f).SetEase(Ease.InCubic).SetLoops(-1, LoopType.Yoyo);

        /*
                if (color != "move")
                    _image.DOFade(0.3f, 1f).SetEase(Ease.InCubic).SetLoops(-1, LoopType.Yoyo);
                else
                    StartCoroutine("anim");
        */
    }

    // マーカーのタイプを設定（color == "move" の場合のみ）
    public void SetMarkerType(string markerType)
    {
        if (color != "move") return;

        string imagename = "rangemarker_b_" + markerType;

        Sprite[] newSprites = Resources.LoadAll<Sprite>("Image/" + imagename);
        if (newSprites != null && newSprites.Length > 0)
        {
            _sprites = newSprites;
            _frame = _sprites.Length;
            _count = 0;
            _image.sprite = _sprites[_count];

            _image.DOFade(0.3f, 1f).SetEase(Ease.InCubic).SetLoops(-1, LoopType.Yoyo);
        }
    }

    IEnumerator anim()
    {
        while (true)
        {
            //0.1秒に一回
            yield return new WaitForSeconds(0.1f);

            if (_count >= _frame)
                _count = 0;

            _image.sprite = _sprites[_count];
            _count++;
        }
    }

    public void setPos(float posX, float posY)
    {
        // チップを該当の位置へ移動。
        transform.localPosition = new Vector3(posX, posY * -1, 0);
    }
}
