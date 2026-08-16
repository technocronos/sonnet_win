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

        RectTransform lineRect = transform.GetComponent<RectTransform>();
        lineRect.anchoredPosition = new Vector3(x - (_rect.width / 2) + marginX,
            y + _rect.height + marginY, 0);
        ClampToSphereViewport(lineRect, y);
    }

    private void ClampToSphereViewport(RectTransform lineRect, float unitY)
    {
        RectTransform stageRect = lineRect.parent as RectTransform;
        RectTransform viewport = stageRect == null ? null : stageRect.parent as RectTransform;
        if (stageRect == null || viewport == null) return;

        Canvas.ForceUpdateCanvases();
        Vector3[] corners = new Vector3[4];
        Vector2 min;
        Vector2 max;
        GetViewportBounds(lineRect, viewport, corners, out min, out max);

        Rect bounds = viewport.rect;
        if (max.y > bounds.yMax)
        {
            // 上端ではキャラの下へ反転する。
            lineRect.anchoredPosition = new Vector2(lineRect.anchoredPosition.x,
                unitY - lineRect.rect.height - marginY);
            Canvas.ForceUpdateCanvases();
            GetViewportBounds(lineRect, viewport, corners, out min, out max);
        }

        Vector2 correction = Vector2.zero;
        if (min.x < bounds.xMin) correction.x += bounds.xMin - min.x;
        if (max.x > bounds.xMax) correction.x -= max.x - bounds.xMax;
        if (min.y < bounds.yMin) correction.y += bounds.yMin - min.y;
        if (max.y > bounds.yMax) correction.y -= max.y - bounds.yMax;
        if (correction.sqrMagnitude <= 0f) return;

        Vector3 worldOrigin = viewport.TransformPoint(Vector3.zero);
        Vector3 worldCorrected = viewport.TransformPoint(correction);
        Vector3 stageDelta = stageRect.InverseTransformVector(worldCorrected - worldOrigin);
        lineRect.anchoredPosition += new Vector2(stageDelta.x, stageDelta.y);
    }

    private void GetViewportBounds(RectTransform lineRect, RectTransform viewport,
        Vector3[] corners, out Vector2 min, out Vector2 max)
    {
        lineRect.GetWorldCorners(corners);
        min = viewport.InverseTransformPoint(corners[0]);
        max = min;
        for (int i = 1; i < corners.Length; i++)
        {
            Vector2 point = viewport.InverseTransformPoint(corners[i]);
            min = Vector2.Min(min, point);
            max = Vector2.Max(max, point);
        }
    }

    public void hide()
    {
        transform.gameObject.SetActive(false);
    }
}
