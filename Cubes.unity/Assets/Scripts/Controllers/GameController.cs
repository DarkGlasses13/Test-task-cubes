using System;
using System.Collections.Generic;
using AssetProvider;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CubeGame
{
    public class GameController : IDisposable
    {
        private readonly AvailableCubesModel _availableCubesModel;
        private readonly TowerModel _towerModel;
        private readonly ICubeConfigsProvider _cubeConfigsProvider;
        private readonly ISaveService _saveService;
        private readonly IGameplayConfigProvider _gameplayConfigProvider;
        private readonly IMessagesConfigProvider _messagesConfigProvider;
        private readonly IMessageService _messageService;
        private readonly Canvas _canvas;
        private readonly CubeSizeProvider _cubeSizeProvider;
        private readonly CubeEffectsService _effectsService;
        private readonly AvailableCubesView _availableCubesView;
        private readonly TowerView _towerView;
        private readonly HoleView _holeView;
        private readonly DragProxyView _dragProxyView;
        private readonly ICubeSpritesProvider _cubeSpritesProvider;
        private readonly CubeAnimationService _cubeAnimationService;
        private readonly List<(string, AvailableCubeView, CompositeDisposable)> _availableCubesMap = new();
        private readonly List<(CubeInTowerData Data, TowerCubeView View, CompositeDisposable Disposable)> _towerCubesMap = new();
        private (CubeInTowerData Data, TowerCubeView View) _pickedTowerCube;
        private CompositeDisposable _disposables;

        public GameController
        (
            AvailableCubesModel availableCubesModel,
            TowerModel towerModel,
            ICubeConfigsProvider cubeConfigsProvider,
            ISaveService saveService,
            IGameplayConfigProvider gameplayConfigProvider,
            IMessagesConfigProvider messagesConfigProvider,
            IMessageService messageService,
            Canvas canvas,
            CubeSizeProvider cubeSizeProvider,
            CubeEffectsService effectsService,
            AvailableCubesView availableCubesView,
            TowerView towerView,
            HoleView holeView,
            DragProxyView dragProxyView,
            ICubeSpritesProvider cubeSpritesProvider,
            CubeAnimationService cubeAnimationService
        )
        {
            _availableCubesModel = availableCubesModel;
            _towerModel = towerModel;
            _cubeConfigsProvider = cubeConfigsProvider;
            _saveService = saveService;
            _gameplayConfigProvider = gameplayConfigProvider;
            _messagesConfigProvider = messagesConfigProvider;
            _messageService = messageService;
            _canvas = canvas;
            _cubeSizeProvider = cubeSizeProvider;
            _effectsService = effectsService;
            _availableCubesView = availableCubesView;
            _towerView = towerView;
            _holeView = holeView;
            _dragProxyView = dragProxyView;
            _cubeSpritesProvider = cubeSpritesProvider;
            _cubeAnimationService = cubeAnimationService;
        }

        public void BindView()
        {
            _disposables?.Dispose();
            _disposables = new CompositeDisposable();
            
            BindAvailableCubes();
            BindTower();
        }

        private void BindTower()
        {
            foreach (var cubeData in _towerModel.Cubes)
            {
                var cubeConfig = _cubeConfigsProvider.Get(cubeData.Id);
                
                if (int.TryParse(cubeConfig.SpriteKey, out var spriteIndex))
                {
                    var cubeView = _towerView.CreateCube
                    (
                        _towerModel.BasePosition.Value,
                        cubeData.HorizontalOffset,
                        cubeData.Place,
                        _cubeSpritesProvider.Get(spriteIndex),
                        animate: false
                    );
                    
                    BindTowerCubeView(cubeData, cubeView);
                }
            }
        }

        private void BindAvailableCubes()
        {
            foreach (var availableCubeId in _availableCubesModel.AvailableCubes)
            {
                var cubeConfig = _cubeConfigsProvider.Get(availableCubeId);

                if (int.TryParse(cubeConfig.SpriteKey, out var spriteIndex))
                {
                    var cubeView = _availableCubesView.CreateCube(_cubeSpritesProvider.Get(spriteIndex));
                    BindAvailableCubeView(availableCubeId, cubeView);
                }
            }

            _availableCubesModel.AvailableCubes
                .ObserveAdd()
                .Subscribe(add =>
                {
                    var view = _availableCubesView.CreateCube(_cubeSpritesProvider.Get(add.Value));
                    BindAvailableCubeView(add.Value, view);
                    
                }).AddTo(_disposables);

            _availableCubesModel.AvailableCubes
                .ObserveRemove()
                .Subscribe(remove => UnbindAvailableCube(remove.Value))
                .AddTo(_disposables);
        }

        private void BindAvailableCubeView(string id, AvailableCubeView view)
        {
            var cubeDisposables = new CompositeDisposable();
                    
            view.DragStarted
                .Subscribe(pointerEventData => OnAvailableCubeDragStarted(view, pointerEventData))
                .AddTo(cubeDisposables);
                    
            view.Dragging
                .Subscribe(OnCubeDragging)
                .AddTo(cubeDisposables);
                    
            view.DragEnded
                .Subscribe(pointerEventData => OnAvailableCubeDragEnded(id, view, pointerEventData))
                .AddTo(cubeDisposables);
            
            _availableCubesMap.Add((id, view, cubeDisposables));
        }

        private void UnbindAvailableCube(string id)
        {
            var index = _availableCubesMap.FindIndex(bind => bind.Item1 == id);

            if (index >= 0)
            {
                var availableCube = _availableCubesMap[index];
                availableCube.Item3?.Dispose();
                _availableCubesMap.RemoveAt(index);
            }
        }

        private void BindTowerCubeView(CubeInTowerData data, TowerCubeView view)
        {
            var cubeDisposables = new CompositeDisposable();
                    
            view.DragStarted
                .Subscribe(pointerEventData => OnTowerCubeDragStarted(data.Place, pointerEventData))
                .AddTo(cubeDisposables);
                    
            view.Dragging
                .Subscribe(OnCubeDragging)
                .AddTo(cubeDisposables);
                    
            view.DragEnded
                .Subscribe(OnTowerCubeDragEnded)
                .AddTo(cubeDisposables);
            
            _towerCubesMap.Add((data, view, cubeDisposables));
        }

        private void UnbindTowerCube(CubeInTowerData data)
        {
            var index = _towerCubesMap.FindIndex(bind => bind.Item1.Id == data.Id);

            if (index >= 0)
            {
                var towerCube = _towerCubesMap[index];
                towerCube.Item3?.Dispose();
                _towerCubesMap.RemoveAt(index);
            }
        }

        public void OnAvailableCubeDragStarted(AvailableCubeView view, PointerEventData pointerEventData)
        {
            _dragProxyView.BeginDrag(view.Sprite, _cubeSizeProvider.Size, pointerEventData.position);
        }

        public void OnCubeDragging(PointerEventData pointerEventData) => _dragProxyView.UpdatePosition(pointerEventData.position);

        public void OnAvailableCubeDragEnded(string id, AvailableCubeView view, PointerEventData pointerEventData)
        {
            _dragProxyView.EndDrag();
            var result = ResolveDrop(pointerEventData.position, checkHole: false);
            HandleDropResult(result, id, pointerEventData.position, view.Sprite);
        }

        public void OnTowerCubeDragStarted(int place, PointerEventData pointerEventData)
        {
            if (place < 0 || place >= _towerCubesMap.Count) 
                return;
        
            var pickedTowerCube = _towerCubesMap[place];
            _pickedTowerCube = (pickedTowerCube.Data, pickedTowerCube.View);
            RemoveCubeFromModel(_pickedTowerCube.Data.Place, withMessage: true);
            _pickedTowerCube.View.RectTransform.DOKill();
            _pickedTowerCube.View.SetVisible(false);
            _towerCubesMap.RemoveAt(_pickedTowerCube.Data.Place);
            CollapseFrom(_pickedTowerCube.Data.Place);
        
            _dragProxyView.BeginDrag
            (
                _pickedTowerCube.View.Sprite,
                _cubeSizeProvider.Size,
                pointerEventData.position
            );
        }

        public void OnTowerCubeDragEnded(PointerEventData pointerEventData)
        {
            _dragProxyView.EndDrag();
            var result = ResolveDrop(pointerEventData.position, checkHole: true);
            
            HandleDropResult
            (
                result,
                _pickedTowerCube.Data.Id,
                pointerEventData.position,
                _pickedTowerCube.View.Sprite
            );
            
            _pickedTowerCube = default;
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
                    NotifyTowerFull();
                    _effectsService.PlayMiss(dropPos, sprite);
                    break;
                
                case DropResult.Miss:
                    NotifyMiss();
                    _effectsService.PlayMiss(dropPos, sprite);
                    break;
            }
        }

        private void PlaceFirstCube(string id, Vector2 dropPos, Sprite sprite)
        {
            Vector2 towerCoords = _towerView.ScreenToTowerCoords(dropPos, _canvas.worldCamera);
            float halfWidth = _towerView.BuildZone.rect.width * 0.5f;
            float halfCube = _cubeSizeProvider.Size * 0.5f;
            float baseX = Mathf.Clamp(towerCoords.x, -halfWidth + halfCube, halfWidth - halfCube);
            SetTowerBase(new Vector2(baseX, _cubeSizeProvider.Size * 0.5f));
            var data = PlaceCubeInModel(id, 0f);
            
            var view = _towerView.CreateCube
            (
                _towerModel.BasePosition.Value,
                data.HorizontalOffset,
                data.Place,
                sprite,
                animate: false
            );
            
            BindTowerCubeView(data, view);
            SaveIfEnabled();
        }

        private void PlaceOnTop(string id, Vector2 dropPos, Sprite sprite)
        {
            Vector2 towerCoords = _towerView.ScreenToTowerCoords(dropPos, _canvas.worldCamera);
            float dropOffsetX = towerCoords.x - _towerModel.GetTopCubeX();
            var data = PlaceCubeInModel(id, dropOffsetX);
            
            var view = _towerView.CreateCube
            (
                _towerModel.BasePosition.Value,
                data.HorizontalOffset,
                data.Place,
                sprite,
                animate: true
            );
            
            BindTowerCubeView(data, view);
            SaveIfEnabled();
        }

        private void CollapseFrom(int fromPlace)
        {
            float cubeSize = _cubeSizeProvider.Size;
            Vector2 towerBase = _towerModel.BasePosition.Value;
        
            for (int i = fromPlace; i < _towerCubesMap.Count; i++)
            {
                float targetX = towerBase.x + _towerModel.GetCube(i).HorizontalOffset;
                float targetY = cubeSize * 0.5f + i * cubeSize;
                _cubeAnimationService.PlayCollapse(_towerCubesMap[i].View.RectTransform, targetX, targetY);
            }
        }
        
        public bool IsDropOnTopCube(Vector2 screenPos, Camera cam)
        {
            if (_towerCubesMap.Count == 0) return false;
        
            Vector2 towerCoords = _towerView.ScreenToTowerCoords(screenPos, cam);
            float cubeSize = _cubeSizeProvider.Size;
            float topCubeX = _towerModel.GetTopCubeX();
            float tolerance = cubeSize * _gameplayConfigProvider.Get().DropTolerance;
        
            if (Mathf.Abs(towerCoords.x - topCubeX) > tolerance)
                return false;
        
            float newCubeCenter = cubeSize * 0.5f + _towerModel.Cubes.Count * cubeSize;
            if (towerCoords.y < newCubeCenter)
                return false;
        
            return true;
        }
        
        public DropResult ResolveDrop(Vector2 screenPos, bool checkHole)
        {
            if (checkHole && _holeView.IsInsideHole(screenPos, _canvas.worldCamera))
                return DropResult.Hole;

            if (_towerView.IsDropOnTower(screenPos, _canvas.worldCamera) == false)
                return DropResult.Miss;

            if (_towerModel.Cubes.Count == 0)
                return DropResult.PlaceFirst;

            if (CanAddMore(_towerView.GetZoneHeight(), _cubeSizeProvider.Size) == false)
                return DropResult.TowerFull;

            if (IsDropOnTopCube(screenPos, _canvas.worldCamera))
                return DropResult.PlaceOnTop;

            return DropResult.Miss;
        }
        
        private bool CanAddMore(float zoneHeight, float cubeSize) => (_towerModel.Cubes.Count + 1) * cubeSize <= zoneHeight;

        private CubeInTowerData PlaceCubeInModel(string id, float dropOffsetX)
        {
            float maxOffset = _cubeSizeProvider.Size * _gameplayConfigProvider.Get().MaxHorizontalOffsetPercent;

            float newAbsoluteOffset;
            
            if (_towerModel.Cubes.Count == 0)
            {
                newAbsoluteOffset = 0f;
            }
            else
            {
                float topOffset = _towerModel.GetCube(_towerModel.Cubes.Count - 1).HorizontalOffset;
                float clampedRelative = Mathf.Clamp(dropOffsetX, -maxOffset, maxOffset);
                newAbsoluteOffset = topOffset + clampedRelative;
            }

            var cubeData = new CubeInTowerData()
            {
                Id = id,
                Place = _towerModel.Cubes.Count,
                HorizontalOffset = newAbsoluteOffset
            };
            
            _towerModel.AddCube(cubeData);
            var messageConfig = _messagesConfigProvider.Get();
            _messageService.ShowMessage(messageConfig.TableReference, messageConfig.MsgCubePlaced);
            return cubeData;
        }

        private void RemoveCubeFromModel(int place, bool withMessage = false)
        {
            if (place < 0 || place >= _towerModel.Cubes.Count) return;

            float belowOffset = place > 0
                ? _towerModel.GetCube(place - 1).HorizontalOffset
                : 0f;

            _towerModel.RemoveCubeAt(place);

            for (int i = place; i < _towerModel.Cubes.Count; i++)
            {
                var cube = _towerModel.GetCube(i);
                cube.Place = i;
                _towerModel.SetCube(i, cube);
            }

            if (place > 0 && place < _towerModel.Cubes.Count)
            {
                float firstAboveOffset = _towerModel.GetCube(place).HorizontalOffset;
                float gap = firstAboveOffset - belowOffset;
                float maxOff = _cubeSizeProvider.Size * _gameplayConfigProvider.Get().MaxHorizontalOffsetPercent;
                float clampedGap = Mathf.Clamp(gap, -maxOff, maxOff);
                float shift = gap - clampedGap;

                if (Mathf.Abs(shift) > 0.001f)
                {
                    for (int i = place; i < _towerModel.Cubes.Count; i++)
                    {
                        var cube = _towerModel.GetCube(i);
                        cube.HorizontalOffset -= shift;
                        _towerModel.SetCube(i, cube);
                    }
                }
            }

            if (!withMessage)
            {
                var messageConfig = _messagesConfigProvider.Get();
                _messageService.ShowMessage(messageConfig.TableReference, messageConfig.MsgCubeRemoved);
            }
        }

        private void SetTowerBase(Vector2 localPosition) => _towerModel.SetBase(localPosition);

        private void NotifyMiss()
        {
            var messageConfig = _messagesConfigProvider.Get();
            _messageService.ShowMessage(messageConfig.TableReference, messageConfig.MsgCubeMissed);
        }

        private void NotifyTowerFull()
        {
            var messageConfig = _messagesConfigProvider.Get();
            _messageService.ShowMessage(messageConfig.TableReference, messageConfig.MsgTowerFull);
        }

        private void SaveIfEnabled()
        {
            if (_gameplayConfigProvider.Get().EnableSave)
                _saveService.Save();
        }

        public void Dispose() => _disposables?.Dispose();
    }
}
