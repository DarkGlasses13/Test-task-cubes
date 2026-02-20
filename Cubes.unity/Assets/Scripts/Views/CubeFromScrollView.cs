using System;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CubeGame
{
    [RequireComponent(typeof(Image))]
    public class CubeFromScrollView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private Image _image;
        private ScrollRect _scrollRect;
        private bool _isDraggingCube;
        private readonly Subject<PointerEventData> _dragStarted = new();
        private readonly Subject<PointerEventData> _dragging = new();
        private readonly Subject<PointerEventData> _dragEnded = new();

        public IObservable<PointerEventData> DragStarted => _dragStarted;
        public IObservable<PointerEventData> Dragging => _dragging;
        public IObservable<PointerEventData> DragEnded => _dragEnded;

        public int ColorIndex { get; private set; }
        public Sprite Sprite => _image != null ? _image.sprite : null;

        public void Setup
        (
            int colorIndex,
            Sprite sprite,
            ScrollRect scrollRect
        )
        {
            ColorIndex = colorIndex;
            _image ??= GetComponent<Image>();
            _image.sprite = sprite;
            _scrollRect = scrollRect;
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
                _dragStarted.OnNext(eventData);
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
            {
                _dragging.OnNext(eventData);
            }
            else
                _scrollRect.OnDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_isDraggingCube)
            {
                _dragEnded.OnNext(eventData);
            }
            else
                _scrollRect.OnEndDrag(eventData);
        }
    }
}
