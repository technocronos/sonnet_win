using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerBehaviour : MonoBehaviour
{

    public Animator Anim;
    string anim = "";

    // Start is called before the first frame update
    public static PowerBehaviour Instance
    {
        get
        {
            return instance;
        }
    }

    private static PowerBehaviour instance;

    private void Start()
    {
        instance = this;
    }

    public void PlayAnim(string _anim)
    {
        if (anim != "")
            Anim.SetBool(anim, false);

        anim = _anim;
        Anim.SetBool(anim, true);
    }
}
