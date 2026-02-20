using System;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CubeGame
{
    [RequireComponent(typeof(RectTransform), typeof(Image))]
    public class TowerCubeView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private RectTransform _rectTransform;
        private Image _image;
        private readonly Subject<PointerEventData> _dragStarted = new();
        private readonly Subject<PointerEventData> _dragging = new();
        private readonly Subject<PointerEventData> _dragEnded = new();

        public IObservable<PointerEventData> DragStarted => _dragStarted;
        public IObservable<PointerEventData> Dragging => _dragging;
        public IObservable<PointerEventData> DragEnded => _dragEnded;

        public int TowerIndex { get; set; }
        public int ColorIndex { get; private set; }
        public Sprite Sprite => _image != null ? _image.sprite : null;
        public RectTransform RectTransform => _rectTransform;

        public void Setup(int towerIndex, int colorIndex, Sprite sprite, float size)
        {
            TowerIndex = towerIndex;
            ColorIndex = colorIndex;
            _rectTransform = GetComponent<RectTransform>();
            _rectTransform.sizeDelta = new Vector2(size, size);
            _image = GetComponent<Image>();
            _image.sprite = sprite;
        }

        public void OnBeginDrag(PointerEventData eventData) => _dragStarted.OnNext(eventData);

        public void OnDrag(PointerEventData eventData) => _dragging.OnNext(eventData);

        public void OnEndDrag(PointerEventData eventData) => _dragEnded.OnNext(eventData);

        public void SetVisible(bool visible)
        {
            if (_image != null)
                _image.enabled = visible;
        }
    }
}
