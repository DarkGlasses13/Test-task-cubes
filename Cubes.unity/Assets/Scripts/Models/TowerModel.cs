using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace CubeGame
{
    public class TowerModel
    {
        private readonly List<CubeData> _cubes = new List<CubeData>();
        private readonly ReactiveProperty<Vector2> _towerBase = new ReactiveProperty<Vector2>(Vector2.zero);
        private readonly Subject<Unit> _onChanged = new Subject<Unit>();

        public IObservable<Unit> OnChanged => _onChanged;
        public IReadOnlyReactiveProperty<Vector2> TowerBase => _towerBase;
        public int Count => _cubes.Count;

        public CubeData GetCube(int index) => _cubes[index];

        public void SetCube(int index, CubeData data) => _cubes[index] = data;

        public void AddCube(CubeData cube)
        {
            _cubes.Add(cube);
            _onChanged.OnNext(Unit.Default);
        }

        public void RemoveCubeAt(int index)
        {
            if (index >= 0 && index < _cubes.Count)
            {
                _cubes.RemoveAt(index);
                _onChanged.OnNext(Unit.Default);
            }
        }

        public void SetTowerBase(Vector2 pos) => _towerBase.Value = pos;

        public void Clear()
        {
            _cubes.Clear();
            _towerBase.Value = Vector2.zero;
            _onChanged.OnNext(Unit.Default);
        }

        public TowerState ToState()
        {
            return new TowerState
            {
                TowerBaseX = _towerBase.Value.x,
                TowerBaseY = _towerBase.Value.y,
                Cubes = new List<CubeData>(_cubes)
            };
        }

        public void LoadState(TowerState state)
        {
            _cubes.Clear();
            if (state?.Cubes != null)
                _cubes.AddRange(state.Cubes);
            _towerBase.Value = new Vector2(state?.TowerBaseX ?? 0f, state?.TowerBaseY ?? 0f);
            _onChanged.OnNext(Unit.Default);
        }

        public int GetNextId()
        {
            int maxId = -1;
            for (int i = 0; i < _cubes.Count; i++)
            {
                if (_cubes[i].Id > maxId)
                    maxId = _cubes[i].Id;
            }
            return maxId + 1;
        }
    }
}
