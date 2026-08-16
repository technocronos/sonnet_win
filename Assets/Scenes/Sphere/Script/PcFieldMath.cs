using UnityEngine;

public static class PcFieldMath
{
    public static Vector2Int Quantize(Vector2 position)
    {
        return new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
    }

    public static int SweepSteps(Vector2 delta, float maxStep)
    {
        return Mathf.Max(1, Mathf.CeilToInt(delta.magnitude / Mathf.Max(0.001f, maxStep)));
    }

    public static bool IsInside(Vector2Int cell, int width, int height)
    {
        return cell.x >= 0 && cell.y >= 0 && cell.x < width && cell.y < height;
    }

    public static bool IsDiagonalTransition(Vector2Int from, Vector2Int to)
    {
        return Mathf.Abs(to.x - from.x) == 1 && Mathf.Abs(to.y - from.y) == 1;
    }

    public static float ClampCameraAxis(float desired, float viewMin, float viewMax,
        float contentMin, float contentMax, float player, float safeMin, float safeMax)
    {
        float contentLower = viewMax - contentMax;
        float contentUpper = viewMin - contentMin;
        if (contentLower > contentUpper) return (contentLower + contentUpper) * 0.5f;
        float lower = Mathf.Max(contentLower, safeMin - player);
        float upper = Mathf.Min(contentUpper, safeMax - player);
        return lower > upper ? Mathf.Clamp(desired, contentLower, contentUpper)
            : Mathf.Clamp(desired, lower, upper);
    }
}
