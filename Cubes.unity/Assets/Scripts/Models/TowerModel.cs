using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace CubeGame
{
    public class TowerModel
    {
        private readonly ReactiveCollection<CubeInTowerData> _cubes = new();
        private readonly ReactiveProperty<Vector2> _basePosition = new(Vector2.zero);
        
        public IReadOnlyReactiveCollection<CubeInTowerData>  Cubes => _cubes;
        public IReadOnlyReactiveProperty<Vector2> BasePosition => _basePosition;

        public CubeInTowerData GetCube(int index) => _cubes[index];

        public void SetCube(int place, CubeInTowerData data) => _cubes[place] = data;

        public void AddCube(CubeInTowerData cube) => _cubes.Add(cube);

        public void RemoveCubeAt(int place)
        {
            if (place >= 0 && place < _cubes.Count)
            {
                _cubes.RemoveAt(place);
            }
        }

        public void SetBase(Vector2 pos) => _basePosition.Value = pos;
        
        public float GetTopCubeX()
        {
            if (Cubes.Count == 0)
                return BasePosition.Value.x;
        
            return BasePosition.Value.x + GetCube(Cubes.Count - 1).HorizontalOffset;
        }

        public void Clear()
        {
            _cubes.Clear();
            _basePosition.Value = Vector2.zero;
        }

        public TowerState ToState()
        {
            return new TowerState
            {
                BaseX = _basePosition.Value.x,
                BaseY = _basePosition.Value.y,
                Cubes = new List<CubeInTowerData>(_cubes)
            };
        }

        public void LoadState(TowerState state)
        {
            _cubes.Clear();
            
            if (state?.Cubes != null)
                foreach (var cubeInTowerData in state.Cubes)
                    _cubes.Add(cubeInTowerData);
            
            _basePosition.Value = new Vector2(state?.BaseX ?? 0f, state?.BaseY ?? 0f);
        }
    }
}
