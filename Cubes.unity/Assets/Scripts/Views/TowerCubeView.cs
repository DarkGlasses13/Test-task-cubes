using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CubeGame
{
    /// <summary>
    /// A cube placed in the tower. Can be dragged to the hole.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class TowerCubeView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private Image _image;
        private RectTransform _rectTransform;

        private Action<TowerCubeView, PointerEventData> _onDragStarted;
        private Action<TowerCubeView, PointerEventData> _onDragging;
        private Action<TowerCubeView, PointerEventData> _onDragEnded;

        public int TowerIndex { get; set; }
        public int ColorIndex { get; private set; }
        public Sprite Sprite => _image != null ? _image.sprite : null;
        public RectTransform RectTransform => _rectTransform;

        public void Setup(
            int towerIndex,
            int colorIndex,
            Sprite sprite,
            float size,
            Action<TowerCubeView, PointerEventData> onDragStarted,
            Action<TowerCubeView, PointerEventData> onDragging,
            Action<TowerCubeView, PointerEventData> onDragEnded)
        {
            TowerIndex = towerIndex;
            ColorIndex = colorIndex;
            _image = GetComponent<Image>();
            _image.sprite = sprite;
            _rectTransform = GetComponent<RectTransform>();
            _rectTransform.sizeDelta = new Vector2(size, size);
            _onDragStarted = onDragStarted;
            _onDragging = onDragging;
            _onDragEnded = onDragEnded;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _onDragStarted?.Invoke(this, eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            _onDragging?.Invoke(this, eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _onDragEnded?.Invoke(this, eventData);
        }

        public void SetVisible(bool visible)
        {
            if (_image != null)
                _image.enabled = visible;
        }
    }
}
