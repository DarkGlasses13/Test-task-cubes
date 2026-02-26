using System;
using System.Collections.Generic;
using System.Linq;
using AssetProvider;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CubeGame
{
    public class AvailableCubesController : IGameController
    {
        public struct Binding
        {
            public string Id;
            public AvailableCubeView View;
            public CompositeDisposable Disposables;
        }
        
        private readonly IGameplayConfigProvider  _gameplayConfigProvider;
        private readonly ICubeSpriteResolver _cubeSpriteResolver;
        private readonly AvailableCubesModel _model;
        private readonly AvailableCubesView _view;
        private readonly DraggingCubeModel _draggingCubeModel;
        private readonly CubeSizeProvider _cubeSizeProvider;
        private readonly DragProxyView _dragProxyView;
        private CompositeDisposable _disposables;
        private readonly List<Binding> _bindings = new();

        public AvailableCubesController
        (
            IGameplayConfigProvider gameplayConfigProvider,
            ICubeSpriteResolver cubeSpriteResolver,
            AvailableCubesModel model,
            AvailableCubesView view,
            DraggingCubeModel draggingCubeModel,
            CubeSizeProvider cubeSizeProvider,
            DragProxyView dragProxyView
        )
        {
            _gameplayConfigProvider = gameplayConfigProvider;
            _cubeSpriteResolver = cubeSpriteResolver;
            _model = model;
            _view = view;
            _draggingCubeModel = draggingCubeModel;
            _cubeSizeProvider = cubeSizeProvider;
            _dragProxyView = dragProxyView;
        }

        public void Bind()
        {
            _disposables?.Dispose();
            _disposables = new CompositeDisposable();
            
            foreach (var availableCubeId in _gameplayConfigProvider.Get().AvailableCubes)
            {
                Bind(availableCubeId);
            }

            _model.Cubes
                .ObserveAdd()
                .Subscribe(add => Bind(add.Value))
                .AddTo(_disposables);

            _model.Cubes
                .ObserveRemove()
                .Subscribe(remove => Unbind(remove.Value))
                .AddTo(_disposables);
        }

        private void Bind(string id)
        {
            if (_bindings.Any(binding => binding.Id == id) == false)
            {
                Sprite sprite = _cubeSpriteResolver.Resolve(id);
                var view = _view.CreateCube(sprite);
                    
                Binding binding = new()
                {
                    Id = id,
                    View = view,
                    Disposables = new CompositeDisposable(),
                };
                    
                binding.View.DragStarted
                    .Subscribe(pointerEventData => OnDragStarted(binding, pointerEventData))
                    .AddTo(binding.Disposables);
                    
                binding.View.Dragging
                    .Subscribe(OnDragging)
                    .AddTo(binding.Disposables);
                    
                binding.View.DragEnded
                    .Subscribe(pointerEventData => OnDragEnded(binding, pointerEventData))
                    .AddTo(binding.Disposables);
                    
                _bindings.Add(binding);
                _disposables.Add(binding.Disposables);
            }
        }

        private void OnDragStarted(Binding binding, PointerEventData pointerEventData)
        {
            _draggingCubeModel.StartDragging(binding.Id);
            Sprite sprite = _cubeSpriteResolver.Resolve(binding.Id);
            
            _dragProxyView.BeginDrag
            (
                _cubeSizeProvider.Size,
                sprite,
                pointerEventData.position
            );
        }

        public void OnDragging(PointerEventData pointerEventData) => _dragProxyView.UpdatePosition(pointerEventData.position);

        private void OnDragEnded(Binding binding, PointerEventData pointerEventData) => _draggingCubeModel.Drop();

        private void Unbind(string id)
        {
            if (_bindings.Any(binding => binding.Id == id))
            {
                foreach (var binding in _bindings.Where(binding => binding.Id == id))
                {
                    binding.Disposables?.Dispose();
                    _disposables.Remove(binding.Disposables);
                    _view.RemoveCube(binding.View);
                }
            }
        }

        public void Dispose() => _disposables?.Dispose();
    }
}