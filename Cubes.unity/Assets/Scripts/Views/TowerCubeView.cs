using System;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CubeGame
{
    [RequireComponent(typeof(RectTransform), typeof(Image), typeof(CanvasGroup))]
    public class TowerCubeView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private RectTransform _rectTransform;
        private Image _image;
        private CanvasGroup _canvasGroup;
        private readonly Subject<PointerEventData> _dragStarted = new();
        private readonly Subject<PointerEventData> _dragging = new();
        private readonly Subject<PointerEventData> _dragEnded = new();

        public IObservable<PointerEventData> DragStarted => _dragStarted;
        public IObservable<PointerEventData> Dragging => _dragging;
        public IObservable<PointerEventData> DragEnded => _dragEnded;
        public Sprite Sprite => _image != null ? _image.sprite : null;
        public RectTransform RectTransform => _rectTransform;

        public void Setup(Sprite sprite, float size)
        {
            _rectTransform = GetComponent<RectTransform>();
            _rectTransform.sizeDelta = new Vector2(size, size);
            _image = GetComponent<Image>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _image.sprite = sprite;
        }

        public void OnBeginDrag(PointerEventData eventData) => _dragStarted.OnNext(eventData);

        public void OnDrag(PointerEventData eventData) => _dragging.OnNext(eventData);

        public void OnEndDrag(PointerEventData eventData) => _dragEnded.OnNext(eventData);
        
        public void SetVisible(bool isVisible) => _canvasGroup.alpha = isVisible ? 1 : 0;
    }
}
