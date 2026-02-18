using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace CubeGame
{
    /// <summary>
    /// Main game orchestrator. Coordinates drag & drop between views and services.
    /// References scene objects via SerializeField; services injected via Zenject.
    /// </summary>
    public class GameController : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private Canvas _canvas;
        [SerializeField] private TowerView _towerView;
        [SerializeField] private HoleView _holeView;
        [SerializeField] private CubeScrollView _scrollView;
        [SerializeField] private DragProxyView _dragProxy;

        [Inject] private ITowerService _towerService;
        [Inject] private ISaveService _saveService;
        [Inject] private IGameConfig _config;
        [Inject] private CubeAnimationService _animService;

        private Camera _uiCamera;
        private TowerCubeView _draggedTowerCube;

        private void Start()
        {
            _uiCamera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : _canvas.worldCamera;

            _dragProxy.Initialize(_canvas, _uiCamera);

            if (_config.EnableSave)
                _saveService.Load();
            else
                _saveService.ClearSave();

            _towerView.Initialize(this);
            _towerView.RebuildFromModel();
            _scrollView.Initialize(this);
        }

        // ===== Scroll cube drag handlers =====

        public void OnScrollCubeDragStarted(CubeItemView cube, PointerEventData e)
        {
            _dragProxy.BeginDrag(cube.Sprite, cube.ColorIndex, _config.CubeUISize, e.position);
        }

        public void OnScrollCubeDragging(CubeItemView cube, PointerEventData e)
        {
            _dragProxy.UpdatePosition(e.position);
        }

        public void OnScrollCubeDragEnded(CubeItemView cube, PointerEventData e)
        {
            Vector2 dropPos = e.position;
            Sprite sprite = cube.Sprite;
            int colorIndex = cube.ColorIndex;
            _dragProxy.EndDrag();

            if (!_towerView.IsDropOnTower(dropPos, _uiCamera))
            {
                _towerService.NotifyMiss();
                PlayMissAnimation(dropPos, sprite);
                return;
            }

            float cubeSize = _config.CubeUISize;

            if (!_towerService.CanAddMore(_towerView.GetZoneHeight(), cubeSize))
            {
                _towerService.NotifyTowerFull();
                PlayMissAnimation(dropPos, sprite);
                return;
            }

            if (_towerService.IsEmpty)
            {
                Vector2 towerCoords = _towerView.ScreenToTowerCoords(dropPos, _uiCamera);
                float halfWidth = _towerView.BuildZone.rect.width * 0.5f;
                float halfCube = cubeSize * 0.5f;
                float baseX = Mathf.Clamp(towerCoords.x, -halfWidth + halfCube, halfWidth - halfCube);

                _towerService.SetTowerBase(new Vector2(baseX, cubeSize * 0.5f));
                var data = _towerService.PlaceCube(colorIndex, 0f);
                _towerView.AddCubeVisual(data);
                SaveIfEnabled();
            }
            else if (_towerView.IsDropOnTopCube(dropPos, _uiCamera))
            {
                Vector2 towerCoords = _towerView.ScreenToTowerCoords(dropPos, _uiCamera);
                float dropOffsetX = towerCoords.x - _towerView.GetTowerBaseX();
                var data = _towerService.PlaceCube(colorIndex, dropOffsetX);
                _towerView.AddCubeVisual(data);
                SaveIfEnabled();
            }
            else
            {
                _towerService.NotifyMiss();
                PlayMissAnimation(dropPos, sprite);
            }
        }

        // ===== Tower cube drag handlers (to hole) =====

        public void OnTowerCubeDragStarted(TowerCubeView cube, PointerEventData e)
        {
            _draggedTowerCube = cube;
            cube.SetVisible(false);
            _dragProxy.BeginTowerDrag(
                cube.Sprite, cube.ColorIndex, cube.TowerIndex,
                _config.CubeUISize, e.position);
        }

        public void OnTowerCubeDragging(TowerCubeView cube, PointerEventData e)
        {
            _dragProxy.UpdatePosition(e.position);
        }

        public void OnTowerCubeDragEnded(TowerCubeView cube, PointerEventData e)
        {
            int towerIndex = _dragProxy.TowerIndex;
            Sprite sprite = _dragProxy.CurrentSprite;
            _dragProxy.EndDrag();

            if (_holeView.IsInsideHole(e.position, _uiCamera))
            {
                PlayHoleAnimation(e.position, sprite);
                _towerService.RemoveCube(towerIndex);
                _towerView.RemoveCubeVisual(towerIndex);
                SaveIfEnabled();
            }
            else
            {
                if (_draggedTowerCube != null)
                    _draggedTowerCube.SetVisible(true);
            }

            _draggedTowerCube = null;
        }

        private void SaveIfEnabled()
        {
            if (_config.EnableSave)
                _saveService.Save();
        }

        // ===== Animation helpers =====

        private void PlayMissAnimation(Vector2 screenPos, Sprite sprite)
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
            rt.sizeDelta = new Vector2(_config.CubeUISize, _config.CubeUISize);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.transform as RectTransform, screenPos, _uiCamera, out Vector2 localPoint);
            rt.anchoredPosition = localPoint;

            _animService.PlayExplode(rt).OnComplete(() => Destroy(go));
        }

        private void PlayHoleAnimation(Vector2 screenPos, Sprite sprite)
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
            rt.sizeDelta = new Vector2(_config.CubeUISize, _config.CubeUISize);

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

            _animService.PlaySwallowIntoHole(rt, holeLocal).OnComplete(() => Destroy(go));
        }
    }
}
