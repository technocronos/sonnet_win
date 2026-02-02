using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StarDispBehaviour : MonoBehaviour
{

    public TextMeshProUGUI Text;

    private int star = 0;

    public static StarDispBehaviour Instance
    {
        get
        {
            return instance;
        }
    }

    private static StarDispBehaviour instance;
    private Vector2 pos;
    private Tweener tween;

    private void Start()
    {
        instance = this;
        pos = transform.localPosition;
        tween = null;
    }

    /// <summary>
    /// ムービーにセットされた変数 type にしたがって、カードの表示を
    /// 切り替えるcall用ラベル。
    /// type には 1:火、2:水、3:雷、4:カラ をセットする。
    /// </summary>
    /// <param name="type"></param>
    public void Init()
    {
        clear();
    }

    public void add()
    {
        AudioManager.Instance.PlaySE("se_coin");

        if (tween == null)
        {
            tween = transform.DOPunchPosition(new Vector3(0, 5, 0), 1f, 10, 1f);
            tween.OnComplete(() => {
                transform.localPosition = pos;
                tween = null;
            });
        }

        star++;
        //Debug.Log("add star=" + star);

        Text.text = star.ToString();
    }

    public void clear()
    {
        star = 0;
        Text.text = star.ToString();
    }

    public void hide()
    {
        transform.gameObject.SetActive(false);
    }

}
