using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class ExpDispBehaviour : MonoBehaviour
{
    public Transform gauge;
    public TextMeshProUGUI TextGain;
    public TextMeshProUGUI TextExp;
    public TextMeshProUGUI TextLv;

    //
    // 行動ptを表示するムービー。
    public void show(int add_exp, float _exp, float _exp_max, int level = 0)
    {
        TextGain.text = "+" + add_exp;
        TextExp.text = _exp + "/" + _exp_max;

        if(level > 0)
        {
            TextGain.gameObject.SetActive(false);
            TextLv.gameObject.SetActive(true);
 
            TextLv.text = "Lv " + level.ToString();
        }

        // expゲージを更新。
        int posx = (int)(((_exp * 1.0f) / _exp_max) * 308);
        gauge.transform.localPosition = new Vector3(posx - 308, 0, 0);

        transform.gameObject.SetActive(true);
    }

}
