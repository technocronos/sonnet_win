using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrnamentBehaviour : MonoBehaviour
{
    public Animator Anim;

    public void setPos(int x, int y)
    {
        transform.localPosition = new Vector3(x, y, 0);
    }

    public void Play(int _no)
    {
        string name = "orn" + _no;
        transform.Find(name).gameObject.SetActive(true);

        Anim.SetBool(name, true);
    }


}
