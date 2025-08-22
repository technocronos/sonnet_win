using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayBehaviour : MonoBehaviour
{

    public Animator Anim;

    public delegate void OnCompleteDelegate();
    public OnCompleteDelegate CompleteHandler;

    private static float _X = 78;
    private static float _Y = -512;

    public string side { get; set; }

    // Start is called before the first frame update
    public static LayBehaviour Instance
    {
        get
        {
            return instance;
        }
    }

    private static LayBehaviour instance;

    private void Start()
    {
        instance = this;
    }

    public void Show(OnCompleteDelegate _callback)
    {
        CompleteHandler += _callback;

        float x = 0;
        float y = _Y;

        if (side == "P")
            x = _X * -1;
        else
            x = _X;

        Debug.Log("LayBehaviour Show run.. side=" + side + " x=" + x);

        transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(x, y, 0);

        Anim.SetBool("lay", true);
    }

    public void EffectEnd()
    {
        Anim.SetBool("lay", false);

        CompleteHandler?.Invoke();
        CompleteHandler = null;
    }

    public void PlaySound(string name)
    {
        AudioManager.Instance.PlaySE(name);
    }
}
