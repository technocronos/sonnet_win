using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization.Settings;

public class PageFrame : EventDispatcher
{
    public InfoResultSet infoResultSet;
    // Start is called before the first frame update
    void Start()
    {

    }

    public void OnSet(InfoResultSet infoResultSet)
    {
        this.infoResultSet = infoResultSet;

        if(Main.Locale != LocalizationSettings.AvailableLocales.Locales[0])
        {
            transform.Find("Title").GetComponent<TextMeshProUGUI>().text = infoResultSet.title_en;
            transform.Find("Body").GetComponent<TextMeshProUGUI>().text = infoResultSet.body_en;
        }
        else
        {
            transform.Find("Title").GetComponent<TextMeshProUGUI>().text = infoResultSet.title;
            transform.Find("Body").GetComponent<TextMeshProUGUI>().text = infoResultSet.body;
        }

        transform.Find("Date").GetComponent<TextMeshProUGUI>().text = infoResultSet.notify_at;
    }

    public void TapInfo()
    {
        DispatchEvent(CwEvent.INFO_CLICK);
    }

    private void OnDestroy()
    {
        DestroyListener();
    }
}
