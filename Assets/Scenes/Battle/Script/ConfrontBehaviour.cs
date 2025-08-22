using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfrontBehaviour : MonoBehaviour
{
    public Animator Anim;

    public IEnumerator PlayAnim()
    {
        Debug.Log("ConfrontBehaviour PlayAnim start..");

        int hashAnim = Animator.StringToHash("Confront");
        Anim.Play(hashAnim);

        yield return null;
        yield return new WaitForAnimation(Anim, 0);

        Debug.Log("ConfrontBehaviour PlayAnim end..");

        // ムービを破棄。
        GameObject.Destroy(transform.gameObject);
    }
}
