using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class apDispBehaviour : MonoBehaviour
{

    //Sphereインスタンス
    SphereBehaviour Sphere { get; set; }
    Transform gauge { get; set; }
    TextMeshProUGUI value { get; set; }

    //
    // 行動ptを表示するムービー。
    public void init()
    {
        Sphere = SphereBehaviour.Instance;
        gauge = transform.Find("APgauge/gauge");
        value = transform.Find("value").GetComponent<TextMeshProUGUI>();

        this.refInfo();
        transform.gameObject.SetActive(true);
    }

    // 
    // 現在の行動ptを参照して、表示に反映する。
    public void refInfo()
    {
        // APゲージを更新。
        int _ap = Sphere.actPt;
        int _ap_max = Sphere.ACTPT_MAX;
        float ap_val = Mathf.Min(_ap, _ap_max);
        value.text = ap_val.ToString();

        int posx = (int)(((ap_val * 1.0f) / _ap_max) * 308);
        gauge.transform.localPosition = new Vector3(posx - 308, 0, 0);

        //HeaderのAP更新通知
        APIConnectManager.Instance.Home((string json) =>
        {
            //API結果受け取り
            HomeApi summary = JsonUtility.FromJson<HomeApi>(json);
            Header.Instance.SetSummary(summary);
        });


    }
}
