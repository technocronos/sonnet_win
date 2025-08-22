using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LineBehaviour : MonoBehaviour
{
    public GameObject TouchPanel;
    public TextMeshProUGUI SpeakerText;
    public TextMeshProUGUI SpeakPanelText;

    const int marginX = 0;
    const int marginY = 20;

    public void setSpeaker(string speaker)
    {
        SpeakerText.text = speaker;
    }

    public void show(string text, float x, float y)
    {
        SpeakPanelText.text = text;
        SphereBehaviour _sphere = SphereBehaviour.Instance;
        Rect _rect = transform.GetComponent<RectTransform>().rect;

        transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(x - (_rect.width / 2) + marginX, y + _rect.height + marginY, 0);
    }

    public void hide()
    {
        transform.gameObject.SetActive(false);
    }
}
