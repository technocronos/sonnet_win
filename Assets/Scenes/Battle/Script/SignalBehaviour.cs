using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignalBehaviour : MonoBehaviour
{
    public Animator Anim;

    public delegate void OnCompleteDelegate();
    private OnCompleteDelegate callback;

    public bool motion { get; set; } = false;
    public int target { get; set; }

    public IEnumerator PlayAnim(OnCompleteDelegate _callback)
    {
        Debug.Log("SignalBehaviour PlayAnim start..");

        //前のアニメーションが終わるまで次のアニメーションはできない
        if (motion) yield break;

        int hashAnim = Animator.StringToHash("SignalAct");
        Anim.Play(hashAnim);

        callback = _callback;

        motion = true;
        yield return null;
        yield return new WaitForAnimation(Anim, 0);

        Debug.Log("SignalBehaviour PlayAnim end..");

        motion = false;
        int hashAnimwait = Animator.StringToHash("SignalWait");
        Anim.Play(hashAnimwait);

        // コールバック実行
        callback?.Invoke();
    }

    public void PlaySound()
    {
        AudioManager.Instance.PlaySE("se_kyoka");
    }



}
