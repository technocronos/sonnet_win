using UnityEngine;
using UnityEngine.UI;

/// <summary>Stage Cameraの影響を受けない、PoC用PC HUD。</summary>
public sealed class PcFieldHudController
{
    private readonly GameObject root;
    private readonly Text status;

    public PcFieldHudController(RectTransform canvas)
    {
        root = new GameObject("PcHud", typeof(RectTransform));
        RectTransform rect = root.GetComponent<RectTransform>();
        rect.SetParent(canvas, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.SetAsLastSibling();

        GameObject statusObject = new GameObject("QuestInfo", typeof(RectTransform), typeof(Text));
        RectTransform statusRect = statusObject.GetComponent<RectTransform>();
        statusRect.SetParent(rect, false);
        statusRect.anchorMin = new Vector2(0f, 1f);
        statusRect.anchorMax = new Vector2(0f, 1f);
        statusRect.pivot = new Vector2(0f, 1f);
        statusRect.anchoredPosition = new Vector2(24f, -24f);
        statusRect.sizeDelta = new Vector2(600f, 42f);
        status = statusObject.GetComponent<Text>();
        status.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        status.fontSize = 20;
        status.color = Color.white;
        status.alignment = TextAnchor.UpperLeft;
        status.raycastTarget = false;
        status.text = "WASD: Move";
    }

    public void SetStatus(string message)
    {
        if (status != null) status.text = message;
    }

    public void Dispose()
    {
        if (root != null) Object.Destroy(root);
    }
}
