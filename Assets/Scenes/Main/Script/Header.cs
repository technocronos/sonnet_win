using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Scenes.Common.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.UI;

public class Header : MonoBehaviour
{
    public Text titleText;
    public Text TextAp;
    public Text TextBpNext;
    public TextMeshProUGUI Gold;
    public TextMeshProUGUI Member;
    public Image APGauge;
    public Image BPGauge;

    private Vector3 _startVector3;
    private static Header instance;

    private HomeApi summary = new HomeApi();

    private float APGaugeStartPos = -129f;
    private float APGaugeEndPos = 0f;

    private float bp_recover = 0f;
    private float ap_recover = 0f;

    private int ap_push_identifier = 0;
    private int bp_push_identifier = 0;

    private float actionPtBefore = 0;
    private float matchPtBefore = 0;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        _startVector3 = gameObject.transform.localPosition;
        //transform.localPosition = new Vector3(_startVector3.x, _startVector3.y + 200, _startVector3.z);

    }

    public static Header Instance
    {
        get
        {
            return instance;
        }
    }

    public HomeApi GetSummary()
    {
        return summary;
    }


    public void SetSummary(HomeApi _summary)
    {
        summary = _summary;

        //AP更新がある場合
        if (actionPtBefore != summary.actionPt)
        {
            int now = (int)(Utility.GetUnixTime(System.DateTime.Now) / 1000);

            ap_recover = (now - summary.lastAffected) * summary.ACTION_PT_RECOVERY;
            if ((summary.actionPt + ap_recover) >= summary.MaxActionPt)
            {
                if (ap_push_identifier != 0)
                {
                    Debug.Log("Notification cancel ap_push_identifier = " + ap_push_identifier);
                    LocalPushNotification.Cancel(ap_push_identifier);
                }
            }
            else
            {
                float short_ap = summary.MaxActionPt - (summary.actionPt + ap_recover);
                float ap_recover_time_sec = short_ap / summary.ACTION_PT_RECOVERY;
#if UNITY_IOS
                ap_push_identifier = Settings.IOSID_AP_RECV;
#endif
                ap_push_identifier = LocalPushNotification.AddSchedule(Utility.getText("LOCAL_PUSH_APRECOVER_TITLE"), Utility.getText("LOCAL_PUSH_APRECOVER_BODY"), 1, (int)(ap_recover_time_sec + 60f), Settings.CHANNELID_AP_RECV, ap_push_identifier);

                Debug.Log("Notification AddSchedule ap_push_identifier = " + ap_push_identifier + " ap_recover_time_sec = " + (int)(ap_recover_time_sec + 60f));

            }

            actionPtBefore = summary.actionPt;
        }

        //BP更新がある場合
        if (matchPtBefore != summary.matchPt)
        {
            int now = (int)(Utility.GetUnixTime(System.DateTime.Now) / 1000);

            bp_recover = (now - summary.lastAffected) * summary.MATCH_PT_RECOVERY;
            if ((summary.matchPt + bp_recover) >= summary.MaxMatchPt)
            {
                if (bp_push_identifier != 0)
                {
                    LocalPushNotification.Cancel(bp_push_identifier);
                }
            }
            else
            {
                float short_bp = summary.MaxMatchPt - (summary.matchPt + bp_recover);
                float bp_recover_time_sec = (int)(short_bp / summary.MATCH_PT_RECOVERY);
#if UNITY_IOS
                bp_push_identifier = Settings.IOSID_BP_RECV;
#endif
                bp_push_identifier = LocalPushNotification.AddSchedule(Utility.getText("LOCAL_PUSH_BPRECOVER_TITLE"), Utility.getText("LOCAL_PUSH_BPRECOVER_BODY"), 1, (int)(bp_recover_time_sec + 60f), Settings.CHANNELID_BP_RECV, bp_push_identifier);
            }

            matchPtBefore = summary.matchPt;
        }

        //初期化
        bp_recover = 0f;
        ap_recover = 0f;

        //レベル
        transform.Find("Overrap").Find("TextLv").GetComponent<Text>().text = summary.chara.level.ToString();
        //次のレベル
        transform.Find("Overrap").Find("TextLvNext").GetComponent<Text>().text =
            summary.exp.relative_exp.ToString() + "/" + summary.exp.relative_next;
        //経験値
        int posx = (int)(((summary.exp.relative_exp * 1.0f) / summary.exp.relative_next) * 167);
        transform.Find("MaskLeft").Find("Left").localPosition = new Vector3(-167 + posx, 1, 0);
        //BP
        transform.Find("Overrap").Find("TextBp").GetComponent<Text>().text = ((int)summary.matchPt).ToString();

        //マグナ
        Gold.text = summary.gold.ToString();
        //仲間
        Member.text = summary.member.current.ToString();

        if (transform.gameObject.activeInHierarchy)
            StartCoroutine("Recover");

    }

    public LocationInfo Location;

    /*
     * APとBPの回復をする。
     */
    IEnumerator Recover()
    {
        while (true)
        {
            //１秒に一回
            yield return new WaitForSeconds(0.5f);

            int now = (int)(Utility.GetUnixTime(System.DateTime.Now) / 1000);

            //--------------------------------------------------------------------------
            // AP回復計算
            //--------------------------------------------------------------------------
            // 時間によるAPポイント回復分を計算。
            ap_recover = (now - summary.lastAffected) * summary.ACTION_PT_RECOVERY;
            //Debug.Log("ap_recover = " + ap_recover);
            if ((summary.actionPt + ap_recover) >= summary.MaxActionPt)
            {
                TextAp.text = summary.MaxActionPt + "\n" + Utility.getText("TEXT_FULL_CONDITION");

                Vector3 cv = APGauge.transform.GetComponent<RectTransform>().anchoredPosition;
                APGauge.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(cv.x, APGaugeEndPos, cv.z);
            }
            else
            {
                float short_ap = summary.MaxActionPt - (summary.actionPt + ap_recover);
                float ap_recover_time_sec = short_ap / summary.ACTION_PT_RECOVERY;

                //Debug.Log("ap_recover_time_sec = " + ap_recover_time_sec);

                float ap_hour = Mathf.Floor(ap_recover_time_sec / 3600);
                float ap_minute = (int)(ap_recover_time_sec / 60) - (ap_hour * 60);
                float ap_second = ap_recover_time_sec - (60 * ((int)ap_recover_time_sec / 60));

                string apMinuteStr = String.Format("{0:D2}", (int)ap_minute);
                string apSecondStr = String.Format("{0:D2}", (int)ap_second);

                string ap_recover_time;

                if (ap_hour > 0)
                    ap_recover_time = ap_hour + ":" + apMinuteStr + ":" + apSecondStr;
                else
                    ap_recover_time = apMinuteStr + ":" + apSecondStr;

                // APゲージを更新。
                int ap_max = summary.MaxActionPt;
                int ap_val = (int)Mathf.Min(summary.actionPt + ap_recover, ap_max);

                TextAp.text = ap_val + "\n" + ap_recover_time;

                float gaugepos = APGaugeStartPos - ((APGaugeStartPos - APGaugeEndPos) / 100) * ap_val;

                Vector3 cv = APGauge.transform.GetComponent<RectTransform>().anchoredPosition;
                APGauge.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(cv.x, gaugepos, cv.z);
            }

            //--------------------------------------------------------------------------
            // BP回復計算
            //--------------------------------------------------------------------------
            // 時間によるBPポイント回復分を計算。
            bp_recover = (now - summary.lastAffected) * summary.MATCH_PT_RECOVERY;

            if ((summary.matchPt + bp_recover) >= summary.MaxMatchPt)
            {
                TextBpNext.text = Utility.getText("TEXT_FULL_CONDITION");
            }
            else
            {
                float short_bp = summary.MaxMatchPt - (summary.matchPt + bp_recover);
                float bp_recover_time_sec = (int)(short_bp / summary.MATCH_PT_RECOVERY);

                float bp_minute = (int)(bp_recover_time_sec / 60);
                float bp_second = bp_recover_time_sec - (60 * bp_minute);

                string bpMinuteStr = String.Format("{0:D2}", (int)bp_minute);
                string bpSecondStr = String.Format("{0:D2}", (int)bp_second);

                TextBpNext.text = bpMinuteStr + ":" + bpSecondStr;
            }

            // BPゲージを更新。
            int bp_max = summary.MaxMatchPt;
            float bp_val = Mathf.Min(summary.matchPt + bp_recover, bp_max);

            int posx = (int)(((bp_val * 1.0f) / summary.MaxMatchPt) * 167);
            BPGauge.transform.localPosition = new Vector3(160 - posx, 1, 0);
        }
    }


    public void SetTitle(string title)
    {
        titleText.text = title;
    }
    public void SetPosition()
    {
        return;

        float safeAreaH = Screen.safeArea.y;

#if UNITY_ANDROID
        safeAreaH = Screen.height - Screen.safeArea.yMax;
#endif
        transform.DOLocalMove(new Vector3(_startVector3.x, _startVector3.y - safeAreaH, _startVector3.z), 1).SetEase(Ease.OutCubic);
    }

    public void SetOutPosition()
    {
        return;

        transform.DOLocalMove(new Vector3(_startVector3.x, _startVector3.y + 200, _startVector3.z), 1).SetEase(Ease.OutCubic);
    }
}
