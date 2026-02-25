using UnityEngine;
using Zenject;

namespace CubeGame
{
    [RequireComponent(typeof(RectTransform))]
    public class HoleView : MonoBehaviour
    {
        private RectTransform _holeRect;
        private Camera _camera;

        public RectTransform HoleRect => _holeRect;

        [Inject]
        public void Construct(Camera cam)
        {
            _holeRect = GetComponent<RectTransform>();
            _camera = cam;
        }

        public bool IsInsideHole(Vector2 screenPos)
        {
            if (_holeRect == null) 
                return false;

            RectTransformUtility.ScreenPointToLocalPointInRectangle
            (
                _holeRect,
                screenPos,
                _camera,
                out Vector2 localPoint
            );

            Vector2 center = _holeRect.rect.center;
            float dx = localPoint.x - center.x;
            float dy = localPoint.y - center.y;
            float a = _holeRect.rect.width * 0.5f;
            float b = _holeRect.rect.height * 0.5f;

            if (a <= 0f || b <= 0f) 
                return false;

            return (dx * dx) / (a * a) + (dy * dy) / (b * b) <= 1f;
        }
    }
}
