using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Scenes.Common.Scripts;
using TMPro;

public class MessageBehaviour : MonoBehaviour
{
    public Canvas canvasMessageBehaviour;
    public TextMeshProUGUI messagetext;
    public Button PlayButton;
    public Button StoreButton;
    public GameObject Gurd;

    /// <summary>
    /// 	ボタンクリック時のデリゲート
    /// </summary>
    public delegate void DialogueClickEvent();

    /// <summary>
    /// 通知イベント
    /// </summary>
    private DialogueClickEvent OnButtonClickEvent;

    // Use this for initialization
    void Start()
    {
    }

    //OKボタンクリック時イベントハンドラ
    public void OnStoreButtonClick()
    {
        Debug.Log("MessageBehaviour OnButtonClick ok...");
        Application.OpenURL(string.Format("itms-apps://itunes.apple.com/app/id{0}?mt=8", Settings.IOS_APP_ID));
    }

    public void OnPlayButtonClick()
    {
        Debug.Log("MessageBehaviour OnButtonClick ok...");
        Application.OpenURL(string.Format("http://play.google.com/store/apps/details?id={0}", Settings.APP_BANDLE_ID));
    }

    // To open the dialogue from outside of the script. 
    public void Open(string Message, bool storebtnshow)
    {
        PlayButton.gameObject.SetActive(false);
        StoreButton.gameObject.SetActive(false);

        //メッセージをセット
        messagetext.text = Message;

        if (storebtnshow)
        {
            if (SystemInfo.operatingSystem.Contains("iOS"))
            {
                StoreButton.gameObject.SetActive(true);
            }
            else
            {
                PlayButton.gameObject.SetActive(true);
            }
        }

        Gurd.SetActive(false);
    }

    // To open the dialogue from outside of the script. 
    public void Close()
    {
        transform.gameObject.SetActive(false);
    }
}
