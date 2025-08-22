using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

//
// HPゲージと数字のコンビを制御する。
// 初期化時に以下の変数をセットしておくこと。
//     ・変数 side を "P" か "E" にセットして、どちらの側を表示するのかを示す。
//     ・変数 max をセットして、最大値を示す。
//     ・start にgotoする前に、value をセットして、開始値を示す。
public class HpGaugeBehaviour : MonoBehaviour
{
    public GameObject HpPGauge;
    public GameObject HpEGauge;
    public TextMeshProUGUI TextHpP;
    public TextMeshProUGUI TextHpE;

    public Dictionary<string, hpParam> HpInfo { get; set; } = new Dictionary<string, hpParam>();
    private string[] Sides = { "P", "E" };

    private bool start_flg { get; set; } = false;

    public class hpParam
    {
        public float displayVal;
        public float value;
        public float max;
    }

    public static HpGaugeBehaviour Instance
    {
        get
        {
            return instance;
        }
    }

    private static HpGaugeBehaviour instance;

    private void Start()
    {
        instance = this;

        //hpParam初期化
        foreach (string side in Sides)
            HpInfo[side] = new hpParam();
    }

    public void Init()
    {
        //表示を一旦クリア
        foreach (string side in Sides)
        {
            HpInfo[side].displayVal = 0;
            this.refresh(side);
        }


    }


    /// <summary>
    // 変数 displayVal にセットされた値で数値とゲージの表示を更新する。
    /// </summary>
    void refresh(string side)
    {
        switch (side)
        {
            case "P":
                // 数値の更新。
                TextHpP.text = HpInfo[side].displayVal.ToString();

                // ゲージの更新。
                int gaugeFrameP = (int)((HpInfo[side].displayVal / HpInfo[side].max) * 292);
                Vector3 posP = HpPGauge.transform.GetComponent<RectTransform>().anchoredPosition;
                HpPGauge.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3((17 - 292) + gaugeFrameP, posP.y, 0);

                break;
            case "E":
                // 数値の更新。
                TextHpE.text = HpInfo[side].displayVal.ToString();

                // ゲージの更新。
                int gaugeFrameE = (int)((float)(HpInfo[side].displayVal / HpInfo[side].max) * 292);
                Vector3 posE = HpEGauge.transform.GetComponent<RectTransform>().anchoredPosition;
                HpEGauge.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3((-310 + 292) - gaugeFrameE, posE.y, 0);
                break;
        }
    }

    //
    // HP表示の制御を開始する。
    // gotoによって、このフレームのスクリプトを実行し続ける。
    public float timeOut { set; get; } = 0.05f;
    private float timeElapsed { set; get; }

    void Update()
    {
        if (!start_flg) return;

        timeElapsed += Time.deltaTime;

        if (timeElapsed >= timeOut)
        {

            foreach (string side in Sides)
            {
                int sign = 0;

                // 表示値と実際値の大小関係を見て、表示値を増やすのか減らすのかを決定する。
                if (HpInfo[side].displayVal < HpInfo[side].value)
                {
                    sign = +1;
                }
                else if (HpInfo[side].displayVal > HpInfo[side].value)
                {
                    sign = -1;
                }
                else
                {
                    sign = 0;
                }

                // 増やすor減らすなら以下を実行。
                if (sign != 0)
                {
                    // いくつ増減するのかを絶対値で取得。
                    // 表示値と実際値の15%。
                    double crement = Mathf.Abs(HpInfo[side].value - HpInfo[side].displayVal) * 0.15;

                    // ただし、それが最大値の2%より小さくならないようにする。
                    if (crement < Mathf.Ceil((float)(HpInfo[side].max * 0.02)))
                    {

                        crement = Mathf.Ceil((float)(HpInfo[side].max * 0.02));

                        // …と言っても、そのせいで実際値を飛び越えたらダメなので、
                        // ちゃんとチェックする。
                        if (crement > Mathf.Abs(HpInfo[side].value - HpInfo[side].displayVal))
                            crement = Mathf.Abs(HpInfo[side].value - HpInfo[side].displayVal);
                    }

                    // 求めた増減値を表示値に反映。
                    HpInfo[side].displayVal += (int)(sign * crement);

                    // 表示値に反映。
                    this.refresh(side);
                }
            }

            timeElapsed = 0.0f;
        }

    }

    //
    // HP表示の制御を開始する。

    // gotoによって、このフレームのスクリプトを実行し続ける。
    public void HpStart()
    {
        start_flg = true;
    }

}
