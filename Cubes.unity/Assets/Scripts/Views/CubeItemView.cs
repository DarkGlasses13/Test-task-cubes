using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CubeGame
{
    /// <summary>
    /// A cube in the bottom scroll panel.
    /// Distinguishes horizontal scroll from vertical (upward) cube drag.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class CubeItemView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private Image _image;
        private ScrollRect _scrollRect;
        private bool _isDraggingCube;

        private Action<CubeItemView, PointerEventData> _onDragStarted;
        private Action<CubeItemView, PointerEventData> _onDragging;
        private Action<CubeItemView, PointerEventData> _onDragEnded;

        public int ColorIndex { get; private set; }
        public Sprite Sprite => _image != null ? _image.sprite : null;

        public void Setup(
            int colorIndex,
            Sprite sprite,
            ScrollRect scrollRect,
            Action<CubeItemView, PointerEventData> onDragStarted,
            Action<CubeItemView, PointerEventData> onDragging,
            Action<CubeItemView, PointerEventData> onDragEnded)
        {
            ColorIndex = colorIndex;
            _image = GetComponent<Image>();
            _image.sprite = sprite;
            _scrollRect = scrollRect;
            _onDragStarted = onDragStarted;
            _onDragging = onDragging;
            _onDragEnded = onDragEnded;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            Vector2 delta = eventData.position - eventData.pressPosition;
            float absX = Mathf.Abs(delta.x);
            float absY = Mathf.Abs(delta.y);

            if (absY > absX && delta.y > 0)
            {
                _isDraggingCube = true;
                _scrollRect.velocity = Vector2.zero;
                _onDragStarted?.Invoke(this, eventData);
            }
            else
            {
                _isDraggingCube = false;
                _scrollRect.OnBeginDrag(eventData);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_isDraggingCube)
                _onDragging?.Invoke(this, eventData);
            else
                _scrollRect.OnDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_isDraggingCube)
                _onDragEnded?.Invoke(this, eventData);
            else
                _scrollRect.OnEndDrag(eventData);
        }
    }
}
