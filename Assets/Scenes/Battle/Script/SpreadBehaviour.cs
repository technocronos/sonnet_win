using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpreadBehaviour : MonoBehaviour
{
    public Animator Anim;

    public delegate void OnCompleteDelegate(string result);
    public OnCompleteDelegate CompleteHandler;
    private OnCompleteDelegate callback;

    public void PlayAnim(string anim, OnCompleteDelegate _callback)
    {
        Debug.Log("SpreadBehaviour PlayAnim start.. anim = " + anim);

        callback = _callback;

        Anim.SetBool(anim, true);
    }

    public void PlaySound(string name)
    {
        Debug.Log("SpreadBehaviour PlaySound start.. name=" + name);

        AudioManager.Instance.PlaySE(name);
    }

    public void EndAnim(string anim)
    {
        Debug.Log("SpreadBehaviour EndAnim start..");

        // コールバック実行
        if (callback != null)
        {
            callback?.Invoke(anim);
            //callback = null;
        }
    }
}
