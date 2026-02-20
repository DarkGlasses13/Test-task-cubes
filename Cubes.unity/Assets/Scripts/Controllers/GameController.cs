using System;
using System.Collections.Generic;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace CubeGame
{
    public class GameController : IDisposable
    {
        private readonly ITowerService _towerService;
        private readonly ISaveService _saveService;
        private readonly IGameConfig _config;
        private readonly IMessageService _messageService;
        private readonly Canvas _canvas;
        private readonly CubeSizeProvider _cubeSizeProvider;
        private readonly DropHandler _dropHandler;
        private readonly CubeEffectsService _effectsService;
        private readonly CubeScrollView _cubeScrollView;
        private readonly TowerView _towerView;
        private readonly HoleView _holeView;
        private readonly DragProxyView _dragProxyView;
        private TowerCubeView _pickedTowerCubeView;
        private readonly Dictionary<CubeFromScrollView, CompositeDisposable> _scrollCubeDisposables = new();
        private readonly Dictionary<TowerCubeView, CompositeDisposable> _towerCubeDisposables = new();
        private CompositeDisposable _disposables;

        private float CubeSize => _cubeSizeProvider.Size;

        public GameController
        (
            ITowerService towerService,
            ISaveService saveService,
            IGameConfig config,
            IMessageService messageService,
            Canvas canvas,
            CubeSizeProvider cubeSizeProvider,
            DropHandler dropHandler,
            CubeEffectsService effectsService,
            CubeScrollView cubeScrollView,
            TowerView towerView,
            HoleView holeView,
            DragProxyView dragProxyView
        )
        {
            _towerService = towerService;
            _saveService = saveService;
            _config = config;
            _messageService = messageService;
            _canvas = canvas;
            _cubeSizeProvider = cubeSizeProvider;
            _dropHandler = dropHandler;
            _effectsService = effectsService;
            _cubeScrollView = cubeScrollView;
            _towerView = towerView;
            _holeView = holeView;
            _dragProxyView = dragProxyView;
        }

        public void BindView()
        {
            _disposables?.Dispose();
            _disposables = new CompositeDisposable();
            
            foreach (var cube in _cubeScrollView.Cubes)
            {
                BindScrollCube(cube);
            }
            
            foreach (var cube in _towerView.Cubes)
            {
                BindTowerCube(cube);
            }

            _cubeScrollView.Cubes
                .ObserveAdd()
                .Subscribe(add => BindScrollCube(add.Value))
                .AddTo(_disposables);

            _cubeScrollView.Cubes
                .ObserveRemove()
                .Subscribe(remove => UnbindScrollCube(remove.Value))
                .AddTo(_disposables);
        }

        private void BindScrollCube(CubeFromScrollView view)
        {
            var cubeDisposables = new CompositeDisposable();
                    
            view.DragStarted
                .Subscribe(pointerEventData => OnScrollCubeDragStarted(view, pointerEventData))
                .AddTo(cubeDisposables);
                    
            view.Dragging
                .Subscribe(OnCubeDragging)
                .AddTo(cubeDisposables);
                    
            view.DragEnded
                .Subscribe(pointerEventData => OnScrollCubeDragEnded(view, pointerEventData))
                .AddTo(cubeDisposables);
                    
            _scrollCubeDisposables[view] = cubeDisposables;
        }

        private void UnbindScrollCube(CubeFromScrollView view)
        {
            if (_scrollCubeDisposables.TryGetValue(view, out var disposables))
            {
                disposables?.Dispose();
                _scrollCubeDisposables.Remove(view);
            }
        }

        private void BindTowerCube(TowerCubeView view)
        {
            var cubeDisposables = new CompositeDisposable();
                    
            view.DragStarted
                .Subscribe(pointerEventData => OnTowerCubeDragStarted(view, pointerEventData))
                .AddTo(cubeDisposables);
                    
            view.Dragging
                .Subscribe(OnCubeDragging)
                .AddTo(cubeDisposables);
                    
            view.DragEnded
                .Subscribe(OnTowerCubeDragEnded)
                .AddTo(cubeDisposables);
                    
            _towerCubeDisposables[view] = cubeDisposables;
        }

        private void UnbindTowerCube(TowerCubeView view)
        {
            if (_towerCubeDisposables.TryGetValue(view, out var disposables))
            {
                disposables?.Dispose();
                _towerCubeDisposables.Remove(view);
            }
        }

        public void OnScrollCubeDragStarted(CubeFromScrollView view, PointerEventData pointerEventData)
        {
            _dragProxyView.BeginDrag(view.Sprite, view.ColorIndex, CubeSize, pointerEventData.position);
        }

        public void OnCubeDragging(PointerEventData pointerEventData) => _dragProxyView.UpdatePosition(pointerEventData.position);

        public void OnScrollCubeDragEnded(CubeFromScrollView view, PointerEventData pointerEventData)
        {
            Vector2 dropPos = pointerEventData.position;
            Sprite sprite = view.Sprite;
            int colorIndex = view.ColorIndex;
            _dragProxyView.EndDrag();

            var result = _dropHandler.Resolve
            (
                dropPos,
                _canvas.worldCamera,
                _towerView,
                _holeView,
                checkHole: false
            );
            
            HandleDropResult(result, dropPos, sprite, colorIndex);
        }

        public void OnTowerCubeDragStarted(TowerCubeView view, PointerEventData pointerEventData)
        {
            _pickedTowerCubeView  = view;
            _towerService.RemoveCube(_pickedTowerCubeView.TowerIndex, silent: true);
            _towerView.PickUpCube(_pickedTowerCubeView.TowerIndex);

            _dragProxyView.BeginTowerDrag
            (
                _pickedTowerCubeView.Sprite,
                _pickedTowerCubeView.ColorIndex,
                _pickedTowerCubeView.TowerIndex,
                CubeSize,
                pointerEventData.position
            );
        }

        public void OnTowerCubeDragEnded(PointerEventData pointerEventData)
        {
            Vector2 dropPos = pointerEventData.position;
            _dragProxyView.EndDrag();

            var result = _dropHandler.Resolve
            (
                dropPos,
                _canvas.worldCamera,
                _towerView, 
                _holeView,
                checkHole: true
            );
            
            HandleDropResult(result, dropPos, _pickedTowerCubeView.Sprite, _pickedTowerCubeView.ColorIndex);

            var rt = _pickedTowerCubeView.RectTransform;
                
            if (rt != null) 
                rt.DOKill();
                
            UnbindTowerCube(_pickedTowerCubeView);
            _pickedTowerCubeView.gameObject.SetActive(false);
            Object.Destroy(_pickedTowerCubeView.gameObject);
            _pickedTowerCubeView = null;
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
            Vector2 towerCoords = _towerView.ScreenToTowerCoords(dropPos, _canvas.worldCamera);
            float halfWidth = _towerView.BuildZone.rect.width * 0.5f;
            float halfCube = cubeSize * 0.5f;
            float baseX = Mathf.Clamp(towerCoords.x, -halfWidth + halfCube, halfWidth - halfCube);
            _towerService.SetTowerBase(new Vector2(baseX, cubeSize * 0.5f));
            var data = _towerService.PlaceCube(colorIndex, 0f);
            BindTowerCube(_towerView.AddCube(data));
            SaveIfEnabled();
        }

        private void PlaceOnTop(Vector2 dropPos, int colorIndex)
        {
            Vector2 towerCoords = _towerView.ScreenToTowerCoords(dropPos, _canvas.worldCamera);
            float dropOffsetX = towerCoords.x - _towerView.GetTopCubeX();
            var data = _towerService.PlaceCube(colorIndex, dropOffsetX);
            BindTowerCube(_towerView.AddCube(data));
            SaveIfEnabled();
        }

        private void SaveIfEnabled()
        {
            if (_config.EnableSave)
                _saveService.Save();
        }

        public void Dispose() => _disposables?.Dispose();
    }
}
