using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Blink : MonoBehaviour
{

    //public
    public float speed = 1.0f;

    //private
    private TextMeshProUGUI textMesh;
    private Text text;
    private Image image;
    private float time;

    private enum ObjType
    {
        TEXT,
        IMAGE,
        TextMeshProUGUI,
    };
    private ObjType thisObjType = ObjType.TEXT;

    void Start()
    {
        //アタッチしてるオブジェクトを判別
        if (this.gameObject.GetComponent<Image>())
        {
            thisObjType = ObjType.IMAGE;
            image = this.gameObject.GetComponent<Image>();
        }
        else if (this.gameObject.GetComponent<Text>())
        {
            thisObjType = ObjType.TEXT;
            text = this.gameObject.GetComponent<Text>();
        }
        else if (this.gameObject.GetComponent<TextMeshProUGUI>())
        {
            thisObjType = ObjType.TextMeshProUGUI;
            textMesh = this.gameObject.GetComponent<TextMeshProUGUI>();
        }
    }

    void Update()
    {
        //オブジェクトのAlpha値を更新
        if (thisObjType == ObjType.IMAGE)
        {
            image.color = GetAlphaColor(image.color);
        }
        else if (thisObjType == ObjType.TEXT)
        {
            text.color = GetAlphaColor(text.color);
        }
        else if (thisObjType == ObjType.TextMeshProUGUI)
        {
            textMesh.color = GetAlphaColor(textMesh.color);
        }
    }

    //Alpha値を更新してColorを返す
    Color GetAlphaColor(Color color)
    {
        time += Time.deltaTime * 5.0f * speed;
        color.a = Mathf.Sin(time) * 0.5f + 0.5f;

        return color;
    }
}
