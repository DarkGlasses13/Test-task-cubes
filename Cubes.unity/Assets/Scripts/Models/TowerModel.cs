using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace CubeGame
{
    public class TowerModel
    {
        private readonly List<CubeInTowerData> _cubes = new List<CubeInTowerData>();
        private readonly ReactiveProperty<Vector2> _base = new ReactiveProperty<Vector2>(Vector2.zero);
        private readonly Subject<Unit> _onChanged = new Subject<Unit>();

        public IObservable<Unit> OnChanged => _onChanged;
        public int Count => _cubes.Count;
        public IReadOnlyList<CubeInTowerData> Cubes => _cubes;
        public IReadOnlyReactiveProperty<Vector2> Base => _base;

        public CubeInTowerData GetCube(int place) => _cubes[place];

        public void SetCube(int place, CubeInTowerData data) => _cubes[place] = data;

        public void AddCube(CubeInTowerData cube)
        {
            _cubes.Add(cube);
            _onChanged.OnNext(Unit.Default);
        }

        public void RemoveCubeAt(int place)
        {
            if (place >= 0 && place < _cubes.Count)
            {
                _cubes.RemoveAt(place);
                _onChanged.OnNext(Unit.Default);
            }
        }

        public void SetBase(Vector2 pos) => _base.Value = pos;

        public int GetNextPlace()
        {
            int maxId = -1;
            
            for (int i = 0; i < _cubes.Count; i++)
            {
                if (_cubes[i].Place > maxId)
                    maxId = _cubes[i].Place;
            }
            
            return maxId + 1;
        }

        public void Clear()
        {
            _cubes.Clear();
            _base.Value = Vector2.zero;
            _onChanged.OnNext(Unit.Default);
        }

        public void LoadState(TowerState state)
        {
            _cubes.Clear();
            
            if (state?.Cubes != null)
                _cubes.AddRange(state.Cubes);
            
            _base.Value = new Vector2(state?.BaseX ?? 0f, state?.BaseY ?? 0f);
            _onChanged.OnNext(Unit.Default);
        }

        public TowerState ToState()
        {
            return new TowerState
            {
                BaseX = _base.Value.x,
                BaseY = _base.Value.y,
                Cubes = new List<CubeInTowerData>(_cubes)
            };
        }
    }
}
