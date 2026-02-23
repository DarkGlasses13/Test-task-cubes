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

        public RectTransform RectTransform => _rectTransform;
        public Sprite Sprite => _image != null ? _image.sprite : null;
        public string Id { get; private set; }
        public int Place { get; private set; } = -1;
        public bool IsTowerCube => Place >= 0;

        [Inject]
        public void Construct(Canvas canvas, Camera cam)
        {
            _canvasRect = canvas.transform as RectTransform;
            _camera = cam;
            _rectTransform = GetComponent<RectTransform>();
            _image = GetComponent<Image>();
            gameObject.SetActive(false);
        }

        public void BeginDrag(AvailableCubeView view, float size, Vector2 screenPos)
        {
            Id = view.Id;
            Place = -1;
            Setup(view.Sprite, size, screenPos);
        }

        public void BeginDrag(TowerCubeView view, float size, Vector2 screenPos)
        {
            Id = view.Id;
            Place = view.Place;
            Setup(view.Sprite, size, screenPos);
        }

        private void Setup(Sprite sprite, float size, Vector2 screenPos)
        {
            _image.sprite = sprite;
            _image.raycastTarget = false;
            _rectTransform.sizeDelta = new Vector2(size, size);
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            UpdatePosition(screenPos);
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
