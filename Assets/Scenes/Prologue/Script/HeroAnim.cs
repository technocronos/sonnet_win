using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class HeroAnim : MonoBehaviour
{
    public PlayableDirector director;

    public Image BG;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("HeroAnim Start");

        BG.sprite = Utility.getAssetImage("Image/BG/blue_bg");

    }

    public void onSkipClick()
    {
        Debug.Log("HeroAnim onSkipClick");
        try
        {
            AudioManager.Instance.PlaySE("se_btn");
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
        director.time = 56f;
    }

    void OnEnable()
    {
        director.stopped += OnPlayableDirectorStopped;
    }

    void OnPlayableDirectorStopped(PlayableDirector aDirector)
    {
        if (director == aDirector)
            Debug.Log("PlayableDirector named " + aDirector.name + " is now stopped.");
    }

    void OnDisable()
    {
        director.stopped -= OnPlayableDirectorStopped;
    }
}
