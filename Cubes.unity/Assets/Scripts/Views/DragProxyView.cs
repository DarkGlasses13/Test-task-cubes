using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace CubeGame
{
    [RequireComponent(typeof(RectTransform), typeof(Image))]
    public class DragProxyView : MonoBehaviour
    {
        private Image _image;
        private RectTransform _rectTransform;
        private RectTransform _canvasRect;
        private Camera _camera;

        public RectTransform RectTransform => _rectTransform;
        public Vector2 ScreenPosition => RectTransformUtility.WorldToScreenPoint(_camera, _rectTransform.position);

        [Inject]
        public void Construct(Canvas canvas, Camera cam)
        {
            _canvasRect = canvas.transform as RectTransform;
            _camera = cam;
            _rectTransform = GetComponent<RectTransform>();
            _image = GetComponent<Image>();
        }

        public void BeginDrag(float size, Sprite sprite, Vector2 screenPos)
        {
            _image.sprite = sprite;
            _image.raycastTarget = false;
            _rectTransform.sizeDelta = new Vector2(size, size);
            transform.SetAsLastSibling();
            UpdatePosition(screenPos);
            gameObject.SetActive(true);
        }

        public void UpdatePosition(Vector2 screenPos)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle
            (
                _canvasRect,
                screenPos,
                _camera,
                out Vector2 localPoint
            );
            
            _rectTransform.anchoredPosition = localPoint;
        }

        public void EndDrag() => gameObject.SetActive(false);
    }
}