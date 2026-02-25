using System;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CubeGame
{
    [RequireComponent(typeof(RectTransform), typeof(Image))]
    public class AvailableCubeView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private Image _image;
        private ScrollRect _scrollRect;
        private bool _isDragging;
        private readonly Subject<PointerEventData> _dragStarted = new();
        private readonly Subject<PointerEventData> _dragging = new();
        private readonly Subject<PointerEventData> _dragEnded = new();
        
        public RectTransform RectTransform { get; private set; }
        public IObservable<PointerEventData> DragStarted => _dragStarted;
        public IObservable<PointerEventData> Dragging => _dragging;
        public IObservable<PointerEventData> DragEnded => _dragEnded;

        public void Construct(ScrollRect scrollRect)
        {
            RectTransform = GetComponent<RectTransform>();
            _image ??= GetComponent<Image>();
            _scrollRect = scrollRect;
        }

        public void SetSprite(Sprite sprite) => _image.sprite = sprite;

        public void OnBeginDrag(PointerEventData eventData)
        {
            Vector2 delta = eventData.position - eventData.pressPosition;
            float absX = Mathf.Abs(delta.x);
            float absY = Mathf.Abs(delta.y);

            if (absY > absX && delta.y > 0)
            {
                _isDragging = true;
                _scrollRect.velocity = Vector2.zero;
                _dragStarted.OnNext(eventData);
            }
            else
            {
                _isDragging = false;
                _scrollRect.OnBeginDrag(eventData);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_isDragging)
            {
                _dragging.OnNext(eventData);
            }
            else
                _scrollRect.OnDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_isDragging)
            {
                _dragEnded.OnNext(eventData);
            }
            else
                _scrollRect.OnEndDrag(eventData);
        }
    }
}