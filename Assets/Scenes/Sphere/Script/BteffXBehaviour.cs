using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BteffXBehaviour : MonoBehaviour
{
    [SerializeField]
    Fade fade = null;

    public void Play()
    {
        transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
        transform.GetComponent<RectTransform>().SetAsLastSibling();

        fade.FadeIn(0.7f);
    }

}
