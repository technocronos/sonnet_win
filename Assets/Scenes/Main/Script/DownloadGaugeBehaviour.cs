using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DownloadGaugeBehaviour : MonoBehaviour
{
    public TextMeshProUGUI Text;
    public GameObject Gauge;

    public GaugeParam GaugeInfo { get; set; }

    private const int GAUGE_LENGTH = 590;

    private bool start_flg { get; set; } = false;

    public class GaugeParam
    {
        public float displayVal;
        public float value;
        public float max;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    public void Init(int max)
    {
        GaugeInfo = new GaugeParam();

        //表示を一旦クリア
        GaugeInfo.displayVal = 0;
        GaugeInfo.max = max;

        // 数値の更新。
        Text.text = "";

        refresh();
    }


    void refresh()
    {
        // ゲージの更新。
        int gaugeFrameP = (int)((GaugeInfo.displayVal / GaugeInfo.max) * GAUGE_LENGTH);
        Vector3 posP = transform.GetComponent<RectTransform>().anchoredPosition;
        Gauge.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3((28 - GAUGE_LENGTH) + gaugeFrameP, posP.y, 0);
    }


    //
    // HP表示の制御を開始する。
    // gotoによって、このフレームのスクリプトを実行し続ける。
    public float timeOut { set; get; } = 0.05f;
    private float timeElapsed { set; get; }

    void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed >= timeOut)
        {
            int sign = 0;

            // 表示値と実際値の大小関係を見て、表示値を増やすのか減らすのかを決定する。
            if (GaugeInfo.displayVal < GaugeInfo.value)
            {
                sign = +1;
            }
            else if (GaugeInfo.displayVal > GaugeInfo.value)
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
                double crement = Mathf.Abs(GaugeInfo.value - GaugeInfo.displayVal) * 0.15;

                // ただし、それが最大値の2%より小さくならないようにする。
                if (crement < Mathf.Ceil((float)(GaugeInfo.max * 0.02)))
                {

                    crement = Mathf.Ceil((float)(GaugeInfo.max * 0.02));

                    // …と言っても、そのせいで実際値を飛び越えたらダメなので、
                    // ちゃんとチェックする。
                    if (crement > Mathf.Abs(GaugeInfo.value - GaugeInfo.displayVal))
                        crement = Mathf.Abs(GaugeInfo.value - GaugeInfo.displayVal);
                }

                // 求めた増減値を表示値に反映。
                GaugeInfo.displayVal += (int)(sign * crement);

                // 表示値に反映。
                this.refresh();
            }

            timeElapsed = 0.0f;
        }

    }

    public void setText(string text)
    {
        Text.text = text;
    }

}
