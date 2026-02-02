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

    private void Start()
    {
        instance = this;
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
        star++;
        //Debug.Log("add star=" + star);

        Text.text = star.ToString();
    }

    public void clear()
    {
        star = 0;
        Text.text = star.ToString();
    }


}
