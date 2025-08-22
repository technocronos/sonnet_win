using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodeBehaviour : MonoBehaviour
{

    public Animator Anim;

    public delegate void OnCompleteDelegate();
    public OnCompleteDelegate CompleteHandler;
    private OnCompleteDelegate callback;

    public void ExprodeStart(OnCompleteDelegate _callback)
    {
        transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);

        callback = _callback;
        if (callback != null)
            CompleteHandler += callback;

        Anim.SetBool("Explode", true);
    }
    public void ExprodeEnd()
    {
        Anim.SetBool("Explode", false);

        // コールバック実行
        if (callback != null)
        {
            CompleteHandler?.Invoke();
            CompleteHandler = null;
        }
    }
}
