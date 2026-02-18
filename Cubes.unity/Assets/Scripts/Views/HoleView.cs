using UnityEngine;

namespace CubeGame
{
    /// <summary>
    /// The hole in the left zone. Detects if a drop point is inside the oval boundary.
    /// </summary>
    public class HoleView : MonoBehaviour
    {
        [SerializeField] private RectTransform _holeRect;

        /// <summary>
        /// Check if a screen position falls within the oval boundary of the hole.
        /// Uses the standard ellipse equation: (x/a)^2 + (y/b)^2 <= 1
        /// </summary>
        public bool IsInsideHole(Vector2 screenPos, Camera cam)
        {
            if (_holeRect == null) return false;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _holeRect, screenPos, cam, out Vector2 localPoint);

            Vector2 center = _holeRect.rect.center;
            float dx = localPoint.x - center.x;
            float dy = localPoint.y - center.y;

            float a = _holeRect.rect.width * 0.5f;
            float b = _holeRect.rect.height * 0.5f;

            if (a <= 0f || b <= 0f) return false;

            return (dx * dx) / (a * a) + (dy * dy) / (b * b) <= 1f;
        }
    }
}
