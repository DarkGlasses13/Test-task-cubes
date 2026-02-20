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
        public Sprite CurrentSprite => _image != null ? _image.sprite : null;
        public int ColorIndex { get; private set; }
        public int TowerIndex { get; private set; } = -1;
        public bool IsTowerCube => TowerIndex >= 0;

        [Inject]
        public void Construct(Canvas canvas, Camera playerCamera)
        {
            _canvasRect = canvas.transform as RectTransform;
            _camera = playerCamera;
            _rectTransform = GetComponent<RectTransform>();
            _image = GetComponent<Image>();
            gameObject.SetActive(false);
        }

        public void BeginDrag(Sprite sprite, int colorIndex, float size, Vector2 screenPos)
        {
            ColorIndex = colorIndex;
            TowerIndex = -1;
            Setup(sprite, size, screenPos);
        }

        public void BeginTowerDrag(Sprite sprite, int colorIndex, int towerIndex, float size, Vector2 screenPos)
        {
            ColorIndex = colorIndex;
            TowerIndex = towerIndex;
            Setup(sprite, size, screenPos);
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

        public void EndDrag()
        {
            gameObject.SetActive(false);
        }
    }
}
