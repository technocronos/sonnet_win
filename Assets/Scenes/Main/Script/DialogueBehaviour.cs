using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueBehaviour : MonoBehaviour
{

    private GameObject Dialogue;

    // Start is called before the first frame update
    void Start()
    {
        Dialogue = GameObject.Find("Dialogue");
    }

    public void onPressOK()
    {
        Debug.Log("DialogueBehaviour onPressOK start...");
        AudioManager.Instance.PlaySE("se_btn");

        Main.Instance.closeDialogue();
    }
}
