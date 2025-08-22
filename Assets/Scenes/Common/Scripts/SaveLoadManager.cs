using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;  //Encodingに必要

//SaveLoadManagerはシングルトンにします。
public class SaveLoadManager
{
    private static SaveLoadManager instance;
    private const string VOLUME_VOICE = "volume_voice";
    private const string VOLUME_SE = "volume_se";
    private const string USER_ID = "user_id";
    private float _volumeVoice;
    private float _volumeBgm;
    private float _volumeSe;
    private string _userId;
    private SaveLoadManager()
    {
        _userId = PlayerPrefs.GetString(USER_ID, "none");
    }

    public static SaveLoadManager Instance
    {
        get
        {
            if (instance == null) instance = new SaveLoadManager();
            return instance;
        }
    }

    public string UserID
    {
        get
        {
            return _userId;
        }

        set
        {
            _userId = value;
            PlayerPrefs.SetString(USER_ID, value);
        }
    }

    public float VolumeSe
    {
        get
        {
            return _volumeSe;
        }

        set
        {
            _volumeSe = value;
            PlayerPrefs.SetFloat(VOLUME_SE, value);
        }
    }

    public float VolumeVoice
    {
        get
        {
            return _volumeVoice;
        }

        set
        {
            _volumeVoice = value;
            PlayerPrefs.SetFloat(VOLUME_VOICE, value);
        }
    }
}
