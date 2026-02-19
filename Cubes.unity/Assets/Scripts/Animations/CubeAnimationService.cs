using DG.Tweening;
using UnityEngine;

namespace CubeGame
{
    public class CubeAnimationService
    {
        public Sequence PlayBounce(RectTransform target, float bounceHeight = 30f, float duration = 0.4f)
        {
            target.DOKill();
            float startY = target.anchoredPosition.y;
            var seq = DOTween.Sequence().SetTarget(target);
            seq.Append(target.DOAnchorPosY(startY + bounceHeight, duration * 0.35f)
                .SetEase(Ease.OutQuad));
            seq.Append(target.DOAnchorPosY(startY, duration * 0.65f)
                .SetEase(Ease.OutBounce));
            return seq;
        }

        public Sequence PlayExplode(RectTransform target, float duration = 0.5f)
        {
            target.DOKill();
            var cg = target.GetComponent<CanvasGroup>();
            if (cg == null) cg = target.gameObject.AddComponent<CanvasGroup>();

            var seq = DOTween.Sequence().SetTarget(target);
            seq.Join(target.DOScale(Vector3.zero, duration).SetEase(Ease.InBack));
            seq.Join(target.DORotate(new Vector3(0, 0, 360), duration, RotateMode.FastBeyond360));
            seq.Join(DOTween.To(() => cg.alpha, a => cg.alpha = a, 0f, duration));
            return seq;
        }

        public Sequence PlaySwallowIntoHole(RectTransform target, Vector2 holeLocalPos, float duration = 0.4f)
        {
            target.DOKill();
            var cg = target.GetComponent<CanvasGroup>();
            if (cg == null) cg = target.gameObject.AddComponent<CanvasGroup>();

            var seq = DOTween.Sequence().SetTarget(target);
            seq.Join(target.DOAnchorPos(holeLocalPos, duration).SetEase(Ease.InQuad));
            seq.Join(target.DOScale(Vector3.zero, duration).SetEase(Ease.InBack));
            seq.Join(DOTween.To(() => cg.alpha, a => cg.alpha = a, 0f, duration * 0.8f).SetDelay(duration * 0.2f));
            return seq;
        }

        public Sequence PlayCollapse(RectTransform target, float targetY, float bounceHeight = 20f, float duration = 0.45f)
        {
            target.DOKill();
            var seq = DOTween.Sequence().SetTarget(target);
            seq.Append(target.DOAnchorPosY(targetY, duration * 0.45f).SetEase(Ease.InQuad));
            seq.Append(target.DOAnchorPosY(targetY + bounceHeight, duration * 0.2f).SetEase(Ease.OutQuad));
            seq.Append(target.DOAnchorPosY(targetY, duration * 0.35f).SetEase(Ease.OutBounce));
            return seq;
        }

        public Sequence PlayDragPickup(RectTransform target)
        {
            target.DOKill();
            var seq = DOTween.Sequence().SetTarget(target);
            seq.Append(target.DOScale(1.15f, 0.1f).SetEase(Ease.OutQuad));
            seq.Append(target.DOScale(1f, 0.1f).SetEase(Ease.InQuad));
            return seq;
        }

        public Sequence PlayFadeMessage(CanvasGroup cg, float showDuration = 1.5f, float fadeDuration = 0.5f)
        {
            cg.DOKill();
            cg.alpha = 1f;
            var seq = DOTween.Sequence().SetTarget(cg);
            seq.AppendInterval(showDuration);
            seq.Append(DOTween.To(() => cg.alpha, a => cg.alpha = a, 0f, fadeDuration));
            return seq;
        }
    }
}
