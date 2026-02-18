using UnityEngine;
using UnityEngine.UI;

namespace CubeGame
{
    /// <summary>
    /// Floating cube that follows the pointer during drag.
    /// Lives under Canvas root so it renders on top of everything.
    /// </summary>
    public class DragProxyView : MonoBehaviour
    {
        [SerializeField] private Image _image;

        private RectTransform _rectTransform;
        private RectTransform _canvasRect;
        private Camera _camera;

        public RectTransform RectTransform => _rectTransform;
        public int ColorIndex { get; private set; }
        public int TowerIndex { get; private set; } = -1;
        public bool IsTowerCube => TowerIndex >= 0;

        public void Initialize(Canvas canvas, Camera camera)
        {
            _canvasRect = canvas.transform as RectTransform;
            _camera = camera;
            _rectTransform = GetComponent<RectTransform>();
            if (_image == null) _image = GetComponent<Image>();
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
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRect, screenPos, _camera, out Vector2 localPoint);
            _rectTransform.anchoredPosition = localPoint;
        }

        public void EndDrag()
        {
            gameObject.SetActive(false);
        }
    }
}
