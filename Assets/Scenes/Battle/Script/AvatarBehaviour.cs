using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarBehaviour : MonoBehaviour
{

    public Animator Anim;

    public delegate void OnCompleteDelegate();
    public OnCompleteDelegate CompleteHandler;
    public OnCompleteDelegate CompleteHandlerFunc;

    private int hashAnim { get; set; }

    public IEnumerator PlayAnim(string anim, float spd = 1, OnCompleteDelegate _callback = null)
    {
        Debug.Log("AvatarBehaviour PlayAnim start.. anim = " + anim);

        if (_callback != null)
            CompleteHandler += _callback;

        Anim.SetFloat("speed", spd);
        hashAnim = Animator.StringToHash(anim);
        Anim.Play(hashAnim);

        yield return null;
        yield return new WaitForAnimation(Anim, 0);

        //Debug.Log("AvatarBehaviour PlayAnim end..");

        if (CompleteHandler != null)
        {
            CompleteHandler?.Invoke();
            CompleteHandler = null;
        }
    }

    /// <summary>
    /// 着替え用アニメーション
    /// </summary>
    /// <param name="anim"></param>
    /// <param name="_callbackfunc"></param>
    /// <param name="_callback"></param>
    public void PlayAnimFlg(string anim, OnCompleteDelegate _callbackfunc = null, OnCompleteDelegate _callback = null)
    {
        Debug.Log("AvatarBehaviour PlayAnim start.. anim = " + anim);

        if (_callbackfunc != null)
            CompleteHandlerFunc += _callbackfunc;

        if (_callback != null)
            CompleteHandler += _callback;

        Anim.SetBool(anim, true);

    }

    public void callFunc()
    {
        if (CompleteHandlerFunc != null)
        {
            CompleteHandlerFunc?.Invoke();
            CompleteHandlerFunc = null;
        }
    }



    public void EndAnim(string anim)
    {
        Debug.Log("AvatarBehaviour EndAnim run.. anim=" + anim);
        Anim.SetBool(anim, false);

        if (CompleteHandler != null)
        {
            CompleteHandler?.Invoke();
            CompleteHandler = null;
        }
    }


}
