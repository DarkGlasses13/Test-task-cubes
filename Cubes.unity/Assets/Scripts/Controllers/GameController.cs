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
        [Inject] private IMessageService _messageService;
        [Inject] private CubeAnimationService _animService;

        private Camera _uiCamera;
        private int _pickedColorIndex;
        private Sprite _pickedSprite;
        private GameObject _pickedCubeGO;

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
                float dropOffsetX = towerCoords.x - _towerView.GetTopCubeX();
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
            _pickedColorIndex = cube.ColorIndex;
            _pickedSprite = cube.Sprite;
            _pickedCubeGO = cube.gameObject;

            int towerIndex = cube.TowerIndex;
            _towerService.RemoveCube(towerIndex, silent: true);
            _towerView.PickUpCubeVisual(towerIndex);

            _dragProxy.BeginTowerDrag(
                _pickedSprite, _pickedColorIndex, towerIndex,
                _config.CubeUISize, e.position);
        }

        public void OnTowerCubeDragging(TowerCubeView cube, PointerEventData e)
        {
            _dragProxy.UpdatePosition(e.position);
        }

        public void OnTowerCubeDragEnded(TowerCubeView cube, PointerEventData e)
        {
            Vector2 dropPos = e.position;
            _dragProxy.EndDrag();

            if (_holeView.IsInsideHole(dropPos, _uiCamera))
            {
                _messageService.ShowMessage(LocalizationKeys.CubeRemoved);
                PlayHoleAnimation(dropPos, _pickedSprite);
                SaveIfEnabled();
            }
            else if (_towerView.IsDropOnTower(dropPos, _uiCamera)
                     && !_towerService.IsEmpty
                     && _towerView.IsDropOnTopCube(dropPos, _uiCamera)
                     && _towerService.CanAddMore(_towerView.GetZoneHeight(), _config.CubeUISize))
            {
                Vector2 towerCoords = _towerView.ScreenToTowerCoords(dropPos, _uiCamera);
                float dropOffsetX = towerCoords.x - _towerView.GetTopCubeX();
                var data = _towerService.PlaceCube(_pickedColorIndex, dropOffsetX);
                _towerView.AddCubeVisual(data);
                SaveIfEnabled();
            }
            else if (_towerView.IsDropOnTower(dropPos, _uiCamera) && _towerService.IsEmpty)
            {
                Vector2 towerCoords = _towerView.ScreenToTowerCoords(dropPos, _uiCamera);
                float halfWidth = _towerView.BuildZone.rect.width * 0.5f;
                float halfCube = _config.CubeUISize * 0.5f;
                float baseX = Mathf.Clamp(towerCoords.x, -halfWidth + halfCube, halfWidth - halfCube);

                _towerService.SetTowerBase(new Vector2(baseX, _config.CubeUISize * 0.5f));
                var data = _towerService.PlaceCube(_pickedColorIndex, 0f);
                _towerView.AddCubeVisual(data);
                SaveIfEnabled();
            }
            else
            {
                _towerService.NotifyMiss();
                PlayMissAnimation(dropPos, _pickedSprite);
                SaveIfEnabled();
            }

            if (_pickedCubeGO != null)
            {
                var rt = _pickedCubeGO.GetComponent<RectTransform>();
                if (rt != null) rt.DOKill();
                _pickedCubeGO.SetActive(false);
                Destroy(_pickedCubeGO);
            }

            _pickedCubeGO = null;
            _pickedSprite = null;
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

            _animService.PlayExplode(rt).OnComplete(() => 
            {
                go.SetActive(false);
                Destroy(go);
            });
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

            _animService.PlaySwallowIntoHole(rt, holeLocal).OnComplete(() => 
            {
                go.SetActive(false);
                Destroy(go);
            });
        }
    }
}
