using System;
using System.Collections.Generic;
using AssetProvider;
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
        private readonly IGameplayConfigProvider _gameplayConfigProvider;
        private readonly ICubeConfigsProvider _cubeConfigsProvider;
        private readonly ICubeSpritesProvider _cubeSpritesProvider;
        private readonly IMessagesConfigProvider _messagesConfigProvider;
        private readonly IMessageService _messageService;
        private readonly Canvas _canvas;
        private readonly CubeSizeProvider _cubeSizeProvider;
        private readonly DropResolver _dropResolver;
        private readonly CubeEffectsService _effectsService;
        private readonly AvailableCubesView _availableCubesView;
        private readonly TowerView _towerView;
        private readonly DragProxyView _dragProxyView;
        private TowerCubeView _pickedTowerCubeView;
        private readonly Dictionary<AvailableCubeView, CompositeDisposable> _scrollCubeDisposables = new();
        private readonly Dictionary<TowerCubeView, CompositeDisposable> _towerCubeDisposables = new();
        private CompositeDisposable _disposables;

        public GameController
        (
            ITowerService towerService,
            ISaveService saveService,
            IGameplayConfigProvider gameplayConfigProvider,
            ICubeConfigsProvider cubeConfigsProvider,
            ICubeSpritesProvider cubeSpritesProvider,
            IMessagesConfigProvider messagesConfigProvider,
            IMessageService messageService,
            Canvas canvas,
            CubeSizeProvider cubeSizeProvider,
            DropResolver dropResolver,
            CubeEffectsService effectsService,
            AvailableCubesView availableCubesView,
            TowerView towerView,
            DragProxyView dragProxyView
        )
        {
            _towerService = towerService;
            _saveService = saveService;
            _gameplayConfigProvider = gameplayConfigProvider;
            _cubeConfigsProvider = cubeConfigsProvider;
            _cubeSpritesProvider = cubeSpritesProvider;
            _messagesConfigProvider = messagesConfigProvider;
            _messageService = messageService;
            _canvas = canvas;
            _cubeSizeProvider = cubeSizeProvider;
            _dropResolver = dropResolver;
            _effectsService = effectsService;
            _availableCubesView = availableCubesView;
            _towerView = towerView;
            _dragProxyView = dragProxyView;
        }

        public void BindView()
        {
            _disposables?.Dispose();
            _disposables = new CompositeDisposable();
            BindAvailableCubes();
            BindTower();
        }

        private void BindAvailableCubes()
        {
            foreach (var availableCubeId in _gameplayConfigProvider.Get().AvailableCubes)
            {
                var cubeConfig = _cubeConfigsProvider.Get(availableCubeId);

                if (int.TryParse(cubeConfig.SpriteKey, out var spriteIndex))
                {
                    var view = _availableCubesView.CreateCube(availableCubeId, _cubeSpritesProvider.Get(spriteIndex));
                    BindCube(view);
                }
            }

            _availableCubesView.Cubes
                .ObserveAdd()
                .Subscribe(add => BindCube(add.Value))
                .AddTo(_disposables);

            _availableCubesView.Cubes
                .ObserveRemove()
                .Subscribe(remove => UnbindCube(remove.Value))
                .AddTo(_disposables);
        }

        private void BindTower()
        {
            foreach (var data in _towerService.Cubes)
            {
                var cubeConfig = _cubeConfigsProvider.Get(data.Id);

                if (int.TryParse(cubeConfig.SpriteKey, out var spriteIndex))
                {
                    var view = _towerView.CreateCube(data, _towerService.Base, _cubeSpritesProvider.Get(spriteIndex), animate: false);
                    BindCube(view);
                }
            }
        }

        private void BindCube(AvailableCubeView view)
        {
            var cubeDisposables = new CompositeDisposable();
                    
            view.DragStarted
                .Subscribe(pointerEventData => OnCubeDragStarted(view, pointerEventData))
                .AddTo(cubeDisposables);
                    
            view.Dragging
                .Subscribe(OnCubeDragging)
                .AddTo(cubeDisposables);
                    
            view.DragEnded
                .Subscribe(pointerEventData => OnCubeDragEnded(view, pointerEventData))
                .AddTo(cubeDisposables);
                    
            _scrollCubeDisposables[view] = cubeDisposables;
        }

        private void UnbindCube(AvailableCubeView view)
        {
            if (_scrollCubeDisposables.TryGetValue(view, out var disposables))
            {
                disposables?.Dispose();
                _scrollCubeDisposables.Remove(view);
            }
        }

        private void BindCube(TowerCubeView view)
        {
            var cubeDisposables = new CompositeDisposable();
                    
            view.DragStarted
                .Subscribe(pointerEventData => OnCubeDragStarted(view, pointerEventData))
                .AddTo(cubeDisposables);
                    
            view.Dragging
                .Subscribe(OnCubeDragging)
                .AddTo(cubeDisposables);
                    
            view.DragEnded
                .Subscribe(OnTowerCubeDragEnded)
                .AddTo(cubeDisposables);
                    
            _towerCubeDisposables[view] = cubeDisposables;
        }

        private void UnbindCube(TowerCubeView view)
        {
            if (_towerCubeDisposables.TryGetValue(view, out var disposables))
            {
                disposables?.Dispose();
                _towerCubeDisposables.Remove(view);
            }
        }

        public void OnCubeDragStarted(AvailableCubeView view, PointerEventData pointerEventData)
        {
            _dragProxyView.BeginDrag(view, _cubeSizeProvider.Size, pointerEventData.position);
        }

        public void OnCubeDragging(PointerEventData pointerEventData) => _dragProxyView.UpdatePosition(pointerEventData.position);

        public void OnCubeDragEnded(AvailableCubeView view, PointerEventData pointerEventData)
        {
            Vector2 dropPos = pointerEventData.position;
            Sprite sprite = view.Sprite;
            _dragProxyView.EndDrag();
            var result = _dropResolver.Resolve(dropPos, checkHole: false);
            HandleDropResult(result, view.Id, dropPos, sprite);
        }

        public void OnCubeDragStarted(TowerCubeView view, PointerEventData pointerEventData)
        {
            _pickedTowerCubeView  = view;
            _towerService.RemoveCube(_pickedTowerCubeView.Place, silent: true);
            _towerView.PickUpCube(_pickedTowerCubeView.Place, _towerService.Base, _towerService.Cubes);
            _dragProxyView.BeginDrag(view, _cubeSizeProvider.Size, pointerEventData.position);
        }

        public void OnTowerCubeDragEnded(PointerEventData pointerEventData)
        {
            Vector2 dropPos = pointerEventData.position;
            _dragProxyView.EndDrag();

            var result = _dropResolver.Resolve(dropPos, checkHole: true);
            
            HandleDropResult(result, _pickedTowerCubeView.Id, dropPos, _pickedTowerCubeView.Sprite);

            var rt = _pickedTowerCubeView.RectTransform;
                
            if (rt != null) 
                rt.DOKill();
                
            UnbindCube(_pickedTowerCubeView);
            _pickedTowerCubeView.gameObject.SetActive(false);
            Object.Destroy(_pickedTowerCubeView.gameObject);
            _pickedTowerCubeView = null;
        }

        private void HandleDropResult(DropResult result, string id, Vector2 dropPos, Sprite sprite)
        {
            switch (result)
            {
                case DropResult.PlaceFirst:
                    PlaceFirstCube(id, dropPos, sprite);
                    break;

                case DropResult.PlaceOnTop:
                    PlaceOnTop(id, dropPos, sprite);
                    break;

                case DropResult.Hole:
                    var messagesConfig = _messagesConfigProvider.Get();
                    _messageService.ShowMessage(messagesConfig.TableReference, messagesConfig.MsgCubeRemoved);
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

        private void PlaceFirstCube(string id, Vector2 dropPos,Sprite sprite)
        {
            float cubeSize = _cubeSizeProvider.Size;
            Vector2 towerCoords = _towerView.ScreenToTowerCoords(dropPos, _canvas.worldCamera);
            float halfWidth = _towerView.BuildZone.rect.width * 0.5f;
            float halfCube = cubeSize * 0.5f;
            float baseX = Mathf.Clamp(towerCoords.x, -halfWidth + halfCube, halfWidth - halfCube);
            _towerService.SetBase(new Vector2(baseX, cubeSize * 0.5f));
            var data = _towerService.PlaceCube(id, 0f);
            BindCube(_towerView.CreateCube(data, _towerService.Base, sprite, animate: true));
            SaveIfEnabled();
        }

        private void PlaceOnTop(string id, Vector2 dropPos, Sprite sprite)
        {
            Vector2 towerCoords = _towerView.ScreenToTowerCoords(dropPos, _canvas.worldCamera);
            
            float dropOffsetX = towerCoords.x - _towerView.GetTopCubeX
            (
                _towerService.CubeCount,
                _towerService.Base,
                _towerService.TopCube
            );
            
            var data = _towerService.PlaceCube(id, dropOffsetX);
            BindCube(_towerView.CreateCube(data, _towerService.Base, sprite, animate: true));
            SaveIfEnabled();
        }

        private void SaveIfEnabled()
        {
            if (_gameplayConfigProvider.Get().EnableSave)
                _saveService.Save();
        }

        public void Dispose() => _disposables?.Dispose();
    }
}
