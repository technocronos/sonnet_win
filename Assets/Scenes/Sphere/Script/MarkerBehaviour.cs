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

        if (color != "move")
            _image.DOFade(0.3f, 1f).SetEase(Ease.InCubic).SetLoops(-1, LoopType.Yoyo);
        else
            StartCoroutine("anim");
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

    public void setPos(int posX, int posY)
    {
        // チップを該当の位置へ移動。
        transform.localPosition = new Vector3(posX, posY * -1, 0);
    }
}
