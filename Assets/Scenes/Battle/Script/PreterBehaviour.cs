using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PreterBehaviour : MonoBehaviour
{

    public Animator PreterAnim;
    public TextMeshProUGUI PreterText;

    public static PreterBehaviour Instance
    {
        get
        {
            return instance;
        }
    }

    private static PreterBehaviour instance;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    public void Visible(bool visible)
    {
        transform.gameObject.SetActive(visible);
    }

    public void setText(string text)
    {
        PreterText.text = text;
    }

    string anim { get; set; } = "";

    public void PlayAnim(string _anim)
    {
        if (anim != "")
            PreterAnim.SetBool(anim, false);

        anim = _anim;

        PreterAnim.SetBool(anim, true);
    }

    public void setPos(string _pos)
    {
        Vector3 preter_pos = transform.GetComponent<RectTransform>().anchoredPosition;
        switch (_pos)
        {
            case "main":
                transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(preter_pos.x, BattleBehaviour.BOTTOM_POS, 0);
                break;
            case "center":
                transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(preter_pos.x, -413.125f, 0);
                break;
        }
    }
}
