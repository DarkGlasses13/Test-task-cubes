using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace CubeGame
{
    [RequireComponent(typeof(Image))]
    public class DragProxyView : MonoBehaviour
    {
        private Image _image;
        private RectTransform _rectTransform;
        private RectTransform _canvasRect;
        private Camera _camera;

        [Inject]
        public void Construct(Canvas canvas, Camera playerCamera)
        {
            _canvasRect = canvas.transform as RectTransform;
            _camera = playerCamera;
            _rectTransform = GetComponent<RectTransform>();
            _image = GetComponent<Image>();
            gameObject.SetActive(false);
        }

        public void BeginDrag(Sprite sprite, float size, Vector2 screenPos) => Setup(sprite, size, screenPos);

        private void Setup(Sprite sprite, float size, Vector2 screenPos)
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

        public void EndDrag()
        {
            gameObject.SetActive(false);
        }
    }
}
