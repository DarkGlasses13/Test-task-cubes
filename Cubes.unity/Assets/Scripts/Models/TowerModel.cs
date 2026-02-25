using System;
using System.Collections.Generic;
using System.Linq;
using AssetProvider;
using UniRx;
using UnityEngine;

namespace CubeGame
{
    public class TowerModel
    {
        private readonly List<TowerCubeData> _cubes = new();
        private readonly Subject<TowerCubeData> _cubePlaced = new();
        private readonly Subject<(TowerCubeData Cube, int Place)> _cubePicked = new();
        private readonly Subject<(int From, IReadOnlyList<TowerCubeData> Cubes)> _collapsed = new();
        private readonly Subject<Unit> _cleared = new();

        #region Refactoring
        private readonly IGameplayConfigProvider _gameplayConfigProvider;
        private readonly CubeSizeProvider _cubeSizeProvider;
        
        public TowerModel(IGameplayConfigProvider gameplayConfigProvider, CubeSizeProvider cubeSizeProvider)
        {
            _gameplayConfigProvider = gameplayConfigProvider;
            _cubeSizeProvider = cubeSizeProvider;
        }
        #endregion
        
        public IObservable<TowerCubeData>  CubePlaced => _cubePlaced;
        public IObservable<(TowerCubeData Cube, int Place)> CubePicked => _cubePicked;
        public IObservable<(int From, IReadOnlyList<TowerCubeData> Cubes)> Collapsed => _collapsed;
        public IObservable<Unit> Cleared => _cleared;
        public bool IsEmpty => _cubes.Count == 0;
        public int Height => _cubes.Count;
        public TowerCubeData Top => _cubes.LastOrDefault();
        public TowerCubeData Base => _cubes.FirstOrDefault();
        public IReadOnlyList<TowerCubeData> Cubes => _cubes;
        
        public TowerCubeData GetCube(int place) => _cubes[place];

        public void PlaceCube(string id, float horizontalOffset)
        {
            var data = new TowerCubeData()
            {
                Id = id,
                HorizontalOffset = horizontalOffset
            };
            
            _cubes.Add(data);
            _cubePlaced.OnNext(data);
        }

        public void PickCube(int place) //TODO: нужно убрать из модели horizontal offset в таком виде, пусть будет просто от -1 до 1
                                        //(в случае первого тоже, просто view будет смотреть на ширину области и поймёт где должен находиться куб)
        {
            if (place < 0 || place >= Height)
                return;

            var pickedCube = _cubes[place];

            _cubes.RemoveAt(place);

            _cubePicked.OnNext((pickedCube, place));

            if (place >= _cubes.Count)
                return;

            if (place > 0)
            {
                float belowOffset =
                    _cubes[place - 1].HorizontalOffset;

                float firstAboveOffset =
                    _cubes[place].HorizontalOffset;

                float gap =
                    firstAboveOffset - belowOffset;

                float maxOffset =
                    _cubeSizeProvider.Size *
                    _gameplayConfigProvider.Get().MaxHorizontalOffsetPercent;

                float clampedGap =
                    Mathf.Clamp(gap, -maxOffset, maxOffset);

                float shift =
                    gap - clampedGap;

                if (Mathf.Abs(shift) > 0.001f)
                {
                    for (int i = place; i < _cubes.Count; i++)
                    {
                        var cube = _cubes[i];
                        cube.HorizontalOffset -= shift;
                        _cubes[i] = cube;
                    }
                }
            }

            var collapsed =
                new List<TowerCubeData>(_cubes.Count - place);

            for (int i = place; i < _cubes.Count; i++)
            {
                collapsed.Add(_cubes[i]);
            }

            if (collapsed.Count > 0)
                _collapsed.OnNext((place, collapsed));
        }

        public void Clear()
        {
            _cubes.Clear();
            _cleared.OnNext(Unit.Default);
        }
        
        public TowerState ToState()
        {
            return new TowerState
            {
                Cubes = new List<TowerCubeData>(_cubes)
            };
        }
        
        public void LoadState(TowerState state)
        {
            _cubes.Clear();

            if (state?.Cubes == null)
            {
                _cleared.OnNext(Unit.Default);
                return;
            }

            foreach (var cube in state.Cubes)
            {
                _cubes.Add(cube);
                _cubePlaced.OnNext(cube);
            }
        }
    }
}