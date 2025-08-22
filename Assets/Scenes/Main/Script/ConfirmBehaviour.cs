using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfirmBehaviour : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    public void onPressOK()
    {
        Debug.Log("ConfirmBehaviour onPressOK start...");
        AudioManager.Instance.PlaySE("se_btn");

        Main.Instance.closeConfirm(1);
    }

    public void onPressCancel()
    {
        Debug.Log("ConfirmBehaviour onPressCancel start...");
        AudioManager.Instance.PlaySE("se_btn");

        Main.Instance.closeConfirm(0);
    }
}
