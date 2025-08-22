using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class ProrogueTitle : MonoBehaviour
{
    public PlayableDirector director;
    public Image BG;

    // Start is called before the first frame update
    void Start()
    {
        BG.sprite = Utility.getAssetImage("Image/BG/bg1");
    }

    public void onTapClick()
    {
        Debug.Log("ProrogueTitle onTapClick");
        try
        {
            AudioManager.Instance.PlaySE("se_btn");
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
        director.time = 99f;
    }

    void OnEnable()
    {
        director.stopped += OnPlayableDirectorStopped;
    }

    void OnPlayableDirectorStopped(PlayableDirector aDirector)
    {
        if (director == aDirector)
            Debug.Log("ProrogueTitle PlayableDirector named " + aDirector.name + " is now stopped.");
    }

    void OnDisable()
    {
        director.stopped -= OnPlayableDirectorStopped;
    }
}
