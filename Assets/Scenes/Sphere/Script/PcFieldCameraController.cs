using UnityEngine;

/// <summary>Player論理座標から独立してStage表示位置だけを制御するPC Camera。</summary>
public sealed class PcFieldCameraController
{
    private readonly SphereBehaviour sphere;
    private readonly RectTransform fieldRoot;
    private readonly RectTransform viewport;
    private Vector2 velocity;
    private readonly Vector2 initialPosition;

    public PcFieldCameraController(SphereBehaviour sphere, RectTransform fieldRoot)
    {
        this.sphere = sphere;
        this.fieldRoot = fieldRoot;
        viewport = fieldRoot == null ? null : fieldRoot.parent as RectTransform;
        initialPosition = fieldRoot == null ? Vector2.zero : fieldRoot.anchoredPosition;
    }

    public void Tick(Vector2 playerGrid, float smoothTime, float edgeFraction)
    {
        if (fieldRoot == null || viewport == null) return;
        Rect view = viewport.rect;
        float tip = sphere.TIP_SIZE;
        float margin = (tip - sphere.UNIT_SIZE) / 2f;
        Vector3 playerLocal = new Vector3(playerGrid.x * tip + margin,
            -(playerGrid.y * tip + margin), 0f);
        Vector2 player = viewport.InverseTransformPoint(fieldRoot.TransformPoint(playerLocal));
        Vector2 desired = view.center - player;

        Vector2 contentMin;
        Vector2 contentMax;
        GetContentBounds(out contentMin, out contentMax);
        float safeLeft = view.xMin + view.width * edgeFraction;
        float safeRight = view.xMax - view.width * edgeFraction;
        float safeBottom = view.yMin + view.height * edgeFraction;
        float safeTop = view.yMax - view.height * edgeFraction;

        Vector2 allowed = new Vector2(
            PcFieldMath.ClampCameraAxis(desired.x, view.xMin, view.xMax, contentMin.x, contentMax.x,
                player.x, safeLeft, safeRight),
            PcFieldMath.ClampCameraAxis(desired.y, view.yMin, view.yMax, contentMin.y, contentMax.y,
                player.y, safeBottom, safeTop));
        Vector2 current = fieldRoot.anchoredPosition;
        fieldRoot.anchoredPosition = Vector2.SmoothDamp(current, current + allowed,
            ref velocity, Mathf.Max(0.001f, smoothTime), Mathf.Infinity,
            Time.unscaledDeltaTime);
    }

    public void Reset()
    {
        velocity = Vector2.zero;
        if (fieldRoot != null) fieldRoot.anchoredPosition = initialPosition;
    }

    private void GetContentBounds(out Vector2 min, out Vector2 max)
    {
        float tip = sphere.TIP_SIZE;
        Vector2 a = viewport.InverseTransformPoint(fieldRoot.TransformPoint(new Vector3(
            -sphere.sphere.leftWid * tip,
            -(sphere.sphere.structHei + sphere.sphere.footHei) * tip, 0f)));
        Vector2 b = viewport.InverseTransformPoint(fieldRoot.TransformPoint(new Vector3(
            (sphere.sphere.structWid + sphere.sphere.rightWid) * tip,
            sphere.sphere.headHei * tip, 0f)));
        min = Vector2.Min(a, b);
        max = Vector2.Max(a, b);
    }

}
