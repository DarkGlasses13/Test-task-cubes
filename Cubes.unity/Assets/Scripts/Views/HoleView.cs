using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace CubeGame
{
    [RequireComponent(typeof(RectTransform))]
    public class HoleView : MonoBehaviour
    {
        [SerializeField] RectTransform _leftArea;
        [SerializeField] float _widthPercent = 0.4f;
        [SerializeField] float _aspect = 2.5f;
        [SerializeField] float _verticalOffsetPercent = -0.2f;
        private RectTransform _rectTransform;
        private CanvasScaler _canvasScaler;

        public RectTransform RectTransform => _rectTransform;

        [Inject]
        public void Construct(CanvasScaler canvasScaler)
        {
            _canvasScaler = canvasScaler;
        }

        public void Construct()
        {
            _rectTransform = GetComponent<RectTransform>();
            UpdateLayout();
            gameObject.SetActive(true);
        }
        
        public void UpdateLayout()
        {
            float baseSize = Mathf.Min(_leftArea.rect.width, _leftArea.rect.height);
            float width = baseSize * _widthPercent;
            float height = width / _aspect;
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            Vector2 center = _leftArea.rect.center;
            float yOffset = _leftArea.rect.height * _verticalOffsetPercent;
            
            _rectTransform.anchoredPosition = new Vector2
            (
                center.x,
                center.y + yOffset
            );
        }

        public bool IsInsideHole(Vector2 screenPos, Camera cam)
        {
            if (_rectTransform == null) 
                return false;

            RectTransformUtility.ScreenPointToLocalPointInRectangle
            (
                _rectTransform,
                screenPos,
                cam,
                out Vector2 localPoint
            );

            Vector2 center = _rectTransform.rect.center;
            float dx = localPoint.x - center.x;
            float dy = localPoint.y - center.y;
            float a = _rectTransform.rect.width * 0.5f;
            float b = _rectTransform.rect.height * 0.5f;

            if (a <= 0f || b <= 0f) 
                return false;

            return (dx * dx) / (a * a) + (dy * dy) / (b * b) <= 1f;
        }
    }
}
