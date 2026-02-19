using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CubeGame
{
    public class GameController
    {
        private readonly ITowerService _towerService;
        private readonly ISaveService _saveService;
        private readonly IGameConfig _config;
        private readonly IMessageService _messageService;
        private readonly CubeSizeProvider _cubeSizeProvider;
        private readonly DropHandler _dropHandler;
        private readonly CubeEffectsService _effectsService;
        private readonly Canvas _canvas;
        private readonly TowerView _towerView;
        private readonly HoleView _holeView;
        private readonly CubeScrollView _scrollView;
        private readonly DragProxyView _dragProxy;

        private Camera _uiCamera;
        private int _pickedColorIndex;
        private Sprite _pickedSprite;
        private GameObject _pickedCubeGO;

        private float CubeSize => _cubeSizeProvider.Size;

        public GameController(
            ITowerService towerService,
            ISaveService saveService,
            IGameConfig config,
            IMessageService messageService,
            CubeSizeProvider cubeSizeProvider,
            DropHandler dropHandler,
            CubeEffectsService effectsService,
            Canvas canvas,
            TowerView towerView,
            HoleView holeView,
            CubeScrollView scrollView,
            DragProxyView dragProxy)
        {
            _towerService = towerService;
            _saveService = saveService;
            _config = config;
            _messageService = messageService;
            _cubeSizeProvider = cubeSizeProvider;
            _dropHandler = dropHandler;
            _effectsService = effectsService;
            _canvas = canvas;
            _towerView = towerView;
            _holeView = holeView;
            _scrollView = scrollView;
            _dragProxy = dragProxy;
        }

        public void Initialize()
        {
            _uiCamera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : _canvas.worldCamera;

            _effectsService.SetCamera(_uiCamera);
            _dragProxy.Initialize(_canvas, _uiCamera);

            _towerView.Initialize(this);
            _towerView.RebuildFromModel();
            _scrollView.Initialize(this);
        }

        // ===== Scroll cube drag handlers =====

        public void OnScrollCubeDragStarted(CubeItemView cube, PointerEventData e)
        {
            _dragProxy.BeginDrag(cube.Sprite, cube.ColorIndex, CubeSize, e.position);
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

            var result = _dropHandler.Resolve(dropPos, _uiCamera, _towerView, _holeView, checkHole: false);
            HandleDropResult(result, dropPos, sprite, colorIndex);
        }

        // ===== Tower cube drag handlers =====

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
                CubeSize, e.position);
        }

        public void OnTowerCubeDragging(TowerCubeView cube, PointerEventData e)
        {
            _dragProxy.UpdatePosition(e.position);
        }

        public void OnTowerCubeDragEnded(TowerCubeView cube, PointerEventData e)
        {
            Vector2 dropPos = e.position;
            _dragProxy.EndDrag();

            var result = _dropHandler.Resolve(dropPos, _uiCamera, _towerView, _holeView, checkHole: true);
            HandleDropResult(result, dropPos, _pickedSprite, _pickedColorIndex);

            if (_pickedCubeGO != null)
            {
                var rt = _pickedCubeGO.GetComponent<RectTransform>();
                if (rt != null) rt.DOKill();
                _pickedCubeGO.SetActive(false);
                Object.Destroy(_pickedCubeGO);
            }

            _pickedCubeGO = null;
            _pickedSprite = null;
        }

        private void HandleDropResult(DropResult result, Vector2 dropPos, Sprite sprite, int colorIndex)
        {
            switch (result)
            {
                case DropResult.PlaceFirst:
                    PlaceFirstCube(dropPos, colorIndex);
                    break;

                case DropResult.PlaceOnTop:
                    PlaceOnTop(dropPos, colorIndex);
                    break;

                case DropResult.Hole:
                    _messageService.ShowMessage(_config.MsgCubeRemoved);
                    _effectsService.PlayHoleSwallow(dropPos, sprite);
                    SaveIfEnabled();
                    break;

                case DropResult.TowerFull:
                    _towerService.NotifyTowerFull();
                    _effectsService.PlayMiss(dropPos, sprite);
                    break;

                case DropResult.Miss:
                    _towerService.NotifyMiss();
                    _effectsService.PlayMiss(dropPos, sprite);
                    break;
            }
        }

        private void PlaceFirstCube(Vector2 dropPos, int colorIndex)
        {
            float cubeSize = CubeSize;
            Vector2 towerCoords = _towerView.ScreenToTowerCoords(dropPos, _uiCamera);
            float halfWidth = _towerView.BuildZone.rect.width * 0.5f;
            float halfCube = cubeSize * 0.5f;
            float baseX = Mathf.Clamp(towerCoords.x, -halfWidth + halfCube, halfWidth - halfCube);

            _towerService.SetTowerBase(new Vector2(baseX, cubeSize * 0.5f));
            var data = _towerService.PlaceCube(colorIndex, 0f);
            _towerView.AddCubeVisual(data);
            SaveIfEnabled();
        }

        private void PlaceOnTop(Vector2 dropPos, int colorIndex)
        {
            Vector2 towerCoords = _towerView.ScreenToTowerCoords(dropPos, _uiCamera);
            float dropOffsetX = towerCoords.x - _towerView.GetTopCubeX();
            var data = _towerService.PlaceCube(colorIndex, dropOffsetX);
            _towerView.AddCubeVisual(data);
            SaveIfEnabled();
        }

        private void SaveIfEnabled()
        {
            if (_config.EnableSave)
                _saveService.Save();
        }
    }
}
