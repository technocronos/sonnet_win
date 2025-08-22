using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmorkBehaviour : MonoBehaviour
{
    public Animator Anim;

    // Start is called before the first frame update
    public void PlayAnim(string Name)
    {
        Debug.Log("SmorkBehaviour PlayAnim start.. Name=" + Name);

        int hashAnim = Animator.StringToHash(Name);
        Anim.Play(hashAnim);

        //yield return null;
        //yield return new WaitForAnimation(Anim, 0);

    }
}
