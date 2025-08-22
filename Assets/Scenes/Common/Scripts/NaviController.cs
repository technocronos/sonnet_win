using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NaviController : MonoBehaviour
{
    public Animator Anim;
    public TextMeshProUGUI NaviText;
    public Button TouchPanel;
    public GameObject TouchObj;
    public Image MsgWin;
    public Image shadow;


    private string[] navi_speak;
    private int arraynum = 0;

    public delegate void EventCallback();
    private EventCallback _eventCallback = null;

    private EventCallback _naviCallback = null;

    private HomeApi homeSummary = null;
    private string waitmode;

    /// <summary>
    /// ナビがそのままいつづけるフラグ。ONにするとタップできない通常のメッセージ表示。
    /// </summary>
    private bool navi_remain { set; get; } = false;

    private void Start()
    {
    }

    public IEnumerator PlayAnim(string anim, string afteranim = null, EventCallback callback = null)
    {
        Debug.Log("NaviController PlayAnim start..");
        Anim.Rebind();
        if (callback != null)
            _naviCallback = callback;

        int hashAnim = Animator.StringToHash(anim);
        Anim.Play(hashAnim);

        yield return null;
        yield return new WaitForAnimation(Anim, 0);

        if (afteranim != null)
        {
            int hashAnimAfter = Animator.StringToHash(afteranim);
            Anim.Play(hashAnimAfter);
        }

        Debug.Log("NaviController PlayAnim end..");

        if (_naviCallback != null)
        {
            _naviCallback?.Invoke();
            _naviCallback = null;
        }

    }

    public void onStart(HomeApi _homeSummary, string _waitmode = null, EventCallback eventCallback = null)
    {
        _eventCallback = eventCallback;

        string[] _speak;
        homeSummary = _homeSummary;

        waitmode = _waitmode;

        if (waitmode == null)
            waitmode = "naviwait";

        //開始メッセージがある場合
        if (homeSummary.openingNum > 0)
        {
            _speak = homeSummary.opening;
        }
        else if (homeSummary.specialNum > 0)
        {
            TouchPanel.enabled = true;
            _speak = homeSummary.special;
        }
        else if (homeSummary.start_speak1 != null)
        {
            // オープニングもスペシャルメッセージもないなら、通常のメッセージ表示。
            _speak = new string[1];
            _speak[0] = homeSummary.start_speak1;
            navi_remain = true;
        }
        else
        {
            _speak = new string[0];
            navi_remain = true;
        }


        StartCoroutine(PlayAnim("naviappere", null, () =>
        {
            Anim.SetTrigger(waitmode);
            this.setNaviSpeak(_speak);
        }));

    }

    private void setNaviSpeak(string[] speak)
    {
        Debug.Log("NaviController setNaviSpeak");

        //内容を初期化
        NaviText.text = "";
        touchEnable(false);
        arraynum = 0;
        navi_speak = speak;

        //しゃべる内容がある場合、とりあえず最初のセリフを表示
        if (navi_speak.Length > 0)
        {
            NaviText.text = navi_speak[arraynum];
            arraynum++;
            if (!navi_remain)
                touchEnable(true);
        }
        else
        {
            NaviText.text = Utility.getText("TEXT_NAVI_NOTHING");
        }
    }

    /*
     * タップ時イベントハンドラ
     */
    public void onTapClick()
    {
        Debug.Log("NaviController onTapClick");

        try
        {
            AudioManager.Instance.PlaySE("se_btn");
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }

        //喋り終わったなら・・
        if (navi_speak.Length <= arraynum)
        {
            SpeakEnd();
        }
        //次のテキストを表示する
        else
        {
            NaviText.text = navi_speak[arraynum];
            arraynum++;
        }
    }

    /*
     * しゃべりが終わった後の処理
     */
    private void SpeakEnd()
    {
        touchEnable(false);

        if (_eventCallback != null)
        {
            //コールバックがあるならコールする
            _eventCallback?.Invoke();
        }
        else
        {
            //ナビアニメーションで退場
            disappere();
        }
        _eventCallback = null;
    }

    //ナビアニメーションで退場
    public void disappere()
    {
        StartCoroutine(PlayAnim("navidisappere", null, () =>
        {
            NaviText.text = "";
            transform.gameObject.SetActive(false);
        }));
    }

    /*
     * タッチパネルを無効にする
     */
    private void touchEnable(bool flg)
    {
        TouchObj.SetActive(flg);
    }
}
