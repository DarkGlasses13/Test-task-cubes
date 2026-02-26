using System.Linq;
using AssetProvider;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CubeGame
{
    public class TowerController : IGameController
    {
        private CompositeDisposable _disposables;
        private readonly IGameplayConfigProvider _gameplayConfigProvider;
        private readonly ICubeSpriteResolver _cubeSpriteResolver;
        private readonly CubeSizeProvider _cubeSizeProvider;
        private readonly TowerModel _model;
        private readonly DraggingCubeModel _draggingCubeModel;
        private readonly DragProxyView _dragProxyView;
        private readonly TowerView _view;
        private readonly DropCubeResolver _dropResolver;
        private readonly IMessagesConfigProvider _messagesConfigProvider;
        private readonly IMessageService _messageService;
        private readonly CubeEffectsService _cubeEffectsService;
        private readonly ISaveService _saveService;

        public TowerController
        (
            IGameplayConfigProvider gameplayConfigProvider,
            ICubeSpriteResolver cubeSpriteResolver,
            CubeSizeProvider cubeSizeProvider,
            TowerModel model,
            DraggingCubeModel draggingCubeModel,
            DragProxyView dragProxyView,
            TowerView view,
            DropCubeResolver dropResolver,
            IMessagesConfigProvider messagesConfigProvider,
            IMessageService messageService,
            CubeEffectsService cubeEffectsService,
            ISaveService saveService
        )
        {
            _gameplayConfigProvider = gameplayConfigProvider;
            _cubeSpriteResolver = cubeSpriteResolver;
            _cubeSizeProvider = cubeSizeProvider;
            _model = model;
            _draggingCubeModel = draggingCubeModel;
            _dragProxyView = dragProxyView;
            _view = view;
            _dropResolver = dropResolver;
            _messagesConfigProvider = messagesConfigProvider;
            _messageService = messageService;
            _cubeEffectsService = cubeEffectsService;
            _saveService = saveService;
        }

        public void Bind()
        {
            _disposables?.Dispose();
            _disposables = new CompositeDisposable();
            
            foreach (var data in _model.Cubes)
                BindCube(data, animate: false);
            
            _draggingCubeModel.Droped.Subscribe(OnDrop).AddTo(_disposables);
            
            _model.CubePicked
                .Subscribe(picked =>_view.Pick(picked.Place))
                .AddTo(_disposables);
            
            _model.Collapsed
                .Subscribe(collapsed =>
                {
                    var collapsedCubes = collapsed.Cubes.Select(data => data.HorizontalOffset).ToList();
                    _view.Collapse(collapsed.From, collapsedCubes);
                    _saveService.Save();
                    
                }).AddTo(_disposables);
            
            _model.CubePlaced.Subscribe(data => BindCube(data, animate: true)).AddTo(_disposables);
        }

        private void BindCube(TowerCubeData data, bool animate)
        {
            var placedView = _view.Place(data.HorizontalOffset, _cubeSpriteResolver.Resolve(data.Id), animate);
                
            placedView.DragStarted
                .Subscribe(pointerEventData =>
                {
                    OnDragStarted(data.Id, _view.GetPlace(placedView), pointerEventData);

                }).AddTo(placedView);
                
            placedView.Dragging.Subscribe(OnDragging).AddTo(_disposables);
                
            placedView.DragEnded
                .Subscribe(_ => OnDrop(data.Id))
                .AddTo(placedView);
        }

        private void OnDragStarted(string id, int place, PointerEventData pointerEventData)
        {
            Sprite sprite = _cubeSpriteResolver.Resolve(id);
            
            float maxHorizontalOffset =
                _cubeSizeProvider.Size *
                _gameplayConfigProvider.Get().MaxHorizontalOffsetPercent;

            _model.PickCube(place, maxHorizontalOffset);
            _draggingCubeModel.StartDragging(id);
            
            _dragProxyView.BeginDrag
            (
                _cubeSizeProvider.Size,
                sprite,
                pointerEventData.position
            );
        }
        
        private void OnDragging(PointerEventData pointerEventData) => _dragProxyView.UpdatePosition(pointerEventData.position);

        private void OnDrop(string id)
        {
            Sprite sprite = _cubeSpriteResolver.Resolve(id);
            
            var result = _dropResolver.Resolve(_dragProxyView.ScreenPosition, checkHole: true);
            _view.ResetPicked();
            HandleDropResult(result, id, _dragProxyView.ScreenPosition, sprite);
        }
        
        private void HandleDropResult(DropResult result, string id, Vector2 dropPos, Sprite sprite)
        {
            var messagesConfig = _messagesConfigProvider.Get();
            
            switch (result)
            {
                case DropResult.PlaceFirst:
                {
                    Vector2 towerCoords = _view.ScreenToTowerCoords(dropPos);
                    float halfWidth = _view.BuildZone.rect.width * 0.5f;
                    float halfCube = _cubeSizeProvider.Size * 0.5f;
                    float baseX = Mathf.Clamp(towerCoords.x, -halfWidth + halfCube, halfWidth - halfCube);
                    _model.PlaceCube(id, baseX);
                    _messageService.ShowMessage(messagesConfig.TableReference, messagesConfig.MsgCubePlaced);
                    _saveService.Save();
                    break;
                }

                case DropResult.PlaceOnTop:
                {
                    Vector2 towerCoords = _view.ScreenToTowerCoords(dropPos);
                    float dropOffsetX = towerCoords.x - _model.Top.HorizontalOffset;
                    float maxOffset = _cubeSizeProvider.Size * _gameplayConfigProvider.Get().MaxHorizontalOffsetPercent;
                    float topOffset = _model.Top.HorizontalOffset;
                    float clampedRelative = Mathf.Clamp(dropOffsetX, -maxOffset, maxOffset);
                    float newAbsoluteOffset = topOffset + clampedRelative;
                    _model.PlaceCube(id, newAbsoluteOffset);
                    _messageService.ShowMessage(messagesConfig.TableReference, messagesConfig.MsgCubePlaced);
                    _saveService.Save();
                    break;
                }

                case DropResult.Hole:
                {
                    _cubeEffectsService.PlayHoleSwallow(dropPos, sprite);
                    _messageService.ShowMessage(messagesConfig.TableReference, messagesConfig.MsgCubeRemoved);
                    _saveService.Save();
                    break;
                }

                case DropResult.TowerFull:
                case DropResult.Miss:
                {
                    _cubeEffectsService.PlayMiss(dropPos, sprite);
                    _messageService.ShowMessage(messagesConfig.TableReference, messagesConfig.MsgCubeMissed);
                    break;
                }
            }
            
            _dragProxyView.EndDrag();
        }

        public void Dispose() => _disposables?.Dispose();
    }
}
