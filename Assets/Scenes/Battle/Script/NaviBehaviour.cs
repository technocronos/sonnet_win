using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NaviBehaviour : MonoBehaviour
{

    public Image Icon1;
    public Image Icon2;
    public Image Icon3;
    public TextMeshProUGUI NaviText;

    public static NaviBehaviour Instance
    {
        get
        {
            return instance;
        }
    }

    private static NaviBehaviour instance;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    public void Visible(bool visible)
    {
        transform.gameObject.SetActive(visible);
    }

    public void Show(int graphNo)
    {
        Icon1.gameObject.SetActive(false);
        Icon2.gameObject.SetActive(false);
        Icon3.gameObject.SetActive(false);
        switch (graphNo)
        {
            case 1:
                Icon1.gameObject.SetActive(true);
                break;
            case 2:
                Icon2.gameObject.SetActive(true);
                break;
            case 3:
                Icon3.gameObject.SetActive(true);
                break;
        }
        Visible(true);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="icon"></param>
    public void setIcon(int iconNo, string imageName)
    {
        if (iconNo == 2)
            Icon2.sprite = Utility.getAssetImage("Image/Dtech/" + imageName);
        else if (iconNo == 3)
            Icon3.sprite = Utility.getAssetImage("Image/Dtech/" + imageName);
    }

    public void setText(string text)
    {
        transform.Find("TextNavi").GetComponent<TextMeshProUGUI>().text = text;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="side"></param>
    public void setSide(string side)
    {

    }

    public void setPos(Vector3 pos)
    {
        transform.GetComponent<RectTransform>().anchoredPosition = pos;
    }

}
