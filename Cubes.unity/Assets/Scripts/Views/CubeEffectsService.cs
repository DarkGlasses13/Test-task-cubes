using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace CubeGame
{
    public class CubeEffectsService
    {
        private readonly CubeAnimationService _animService;
        private readonly CubeSizeProvider _cubeSizeProvider;
        private readonly Canvas _canvas;
        private readonly HoleView _holeView;

        private Camera _uiCamera;

        public CubeEffectsService(CubeAnimationService animService, CubeSizeProvider cubeSizeProvider,
            Canvas canvas, HoleView holeView)
        {
            _animService = animService;
            _cubeSizeProvider = cubeSizeProvider;
            _canvas = canvas;
            _holeView = holeView;
        }

        public void SetCamera(Camera cam)
        {
            _uiCamera = cam;
        }

        public void PlayMiss(Vector2 screenPos, Sprite sprite)
        {
            var go = new GameObject("CubeMiss");
            go.transform.SetParent(_canvas.transform, false);
            go.transform.SetAsLastSibling();

            var rt = go.GetComponent<RectTransform>();
            if (rt == null) rt = go.AddComponent<RectTransform>();

            var img = go.AddComponent<Image>();
            go.AddComponent<CanvasGroup>();

            img.sprite = sprite;
            img.raycastTarget = false;
            rt.sizeDelta = new Vector2(_cubeSizeProvider.Size, _cubeSizeProvider.Size);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.transform as RectTransform, screenPos, _uiCamera, out Vector2 localPoint);
            rt.anchoredPosition = localPoint;

            _animService.PlayExplode(rt).OnComplete(() =>
            {
                go.SetActive(false);
                Object.Destroy(go);
            });
        }

        public void PlayHoleSwallow(Vector2 screenPos, Sprite sprite)
        {
            var go = new GameObject("CubeSwallow");
            go.transform.SetParent(_canvas.transform, false);
            go.transform.SetAsLastSibling();

            var rt = go.GetComponent<RectTransform>();
            if (rt == null) rt = go.AddComponent<RectTransform>();

            var img = go.AddComponent<Image>();
            go.AddComponent<CanvasGroup>();

            img.sprite = sprite;
            img.raycastTarget = false;
            rt.sizeDelta = new Vector2(_cubeSizeProvider.Size, _cubeSizeProvider.Size);

            var canvasRect = _canvas.transform as RectTransform;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, screenPos, _uiCamera, out Vector2 dropLocal);
            rt.anchoredPosition = dropLocal;

            Vector3[] holeCorners = new Vector3[4];
            _holeView.HoleRect.GetWorldCorners(holeCorners);
            Vector2 holeCenterWorld = (holeCorners[0] + holeCorners[2]) * 0.5f;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, RectTransformUtility.WorldToScreenPoint(_uiCamera, holeCenterWorld),
                _uiCamera, out Vector2 holeLocal);

            _animService.PlaySwallowIntoHole(rt, holeLocal).OnComplete(() =>
            {
                go.SetActive(false);
                Object.Destroy(go);
            });
        }
    }
}
