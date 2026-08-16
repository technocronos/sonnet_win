using DG.Tweening;
using UnityEngine;

public class CloudDrift : MonoBehaviour
{
    [SerializeField] private float horizontalDistance = 130f;
    [SerializeField] private float horizontalDuration = 40f;
    [SerializeField] private float verticalDistance = 20f;
    [SerializeField] private float verticalDuration = 16f;
    [SerializeField] private float scaleAmount = 0.03f;
    [SerializeField] private float scaleDuration = 12f;

    private RectTransform rectTransform;
    private Tween horizontalTween;
    private Tween verticalTween;
    private Tween scaleTween;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        Vector2 startPosition = rectTransform.anchoredPosition;
        Vector3 startScale = rectTransform.localScale;

        horizontalTween = rectTransform
            .DOAnchorPosX(startPosition.x + horizontalDistance, horizontalDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        verticalTween = rectTransform
            .DOAnchorPosY(startPosition.y + verticalDistance, verticalDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        scaleTween = rectTransform
            .DOScale(startScale * (1f + scaleAmount), scaleDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void OnDestroy()
    {
        horizontalTween?.Kill();
        verticalTween?.Kill();
        scaleTween?.Kill();
    }
}
