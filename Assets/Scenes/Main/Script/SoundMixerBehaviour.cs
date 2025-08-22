using Scenes.Common.Scripts;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.UI;

public class SoundMixerBehaviour : MonoBehaviour
{
    public TMP_Dropdown DropdownLang;

    public Slider SliderBgm;
    public Slider SliderSE;

    private static SoundMixerBehaviour instance;

    private StringTable strtbl;

    public static SoundMixerBehaviour Instance
    {
        get
        {
            return instance;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        SliderBgm.value = AudioManager.Instance.getBGMVol();
        SliderSE.value = AudioManager.Instance.getSEVol();

        strtbl = LocalizationSettings.StringDatabase.GetTable("StringTable");

        DropdownLang.value = PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);
    }

    public void onChangeBGM()
    {
        Debug.Log("onChangeBGM run..");

        float volume = SliderBgm.value;
        AudioManager.Instance.ChangeBgmVolume(volume);

    }

    public void onChangeSE()
    {
        Debug.Log("onChangeSE run..");

        float volume = SliderSE.value;
        AudioManager.Instance.ChangeSEVolume(volume);
    }

    public void changelang(int lang)
    {
        Debug.Log("changelang =" + lang);

        int _lang = PlayerPrefs.GetInt(Settings.LANGUAGE_SELECTED_KEY);

        if (_lang == lang)
            return;

        Main.Instance.ChangeLang(lang);

        //master再取得
        //APIConnectManager.Instance.MasterDataGet(MasterLoad);
    }

}
