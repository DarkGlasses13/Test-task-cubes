using UnityEngine;

namespace CubeGame
{
    public class DropResolver
    {
        private readonly Camera _camera;
        private readonly TowerView _towerView;
        private readonly HoleView _holeView;
        private readonly ITowerService _towerService;
        private readonly CubeSizeProvider _cubeSizeProvider;

        public DropResolver
        (
            Camera camera,
            TowerView towerView,
            HoleView holeView,
            ITowerService towerService,
            CubeSizeProvider cubeSizeProvider
        )
        {
            _camera = camera;
            _towerView = towerView;
            _holeView = holeView;
            _towerService = towerService;
            _cubeSizeProvider = cubeSizeProvider;
        }

        public DropResult Resolve(Vector2 screenPos, bool checkHole)
        {
            if (checkHole && _holeView.IsInsideHole(screenPos, _camera))
                return DropResult.Hole;

            if (_towerView.IsDropOnTower(screenPos, _camera) == false)
                return DropResult.Miss;

            float cubeSize = _cubeSizeProvider.Size;

            if (_towerService.IsEmpty)
                return DropResult.PlaceFirst;

            if (_towerService.CanAddMore(_towerView.GetZoneHeight(), cubeSize) == false)
                return DropResult.TowerFull;

            if (_towerView.IsDropOnTopCube
            (
                screenPos,
                _camera,
                _towerService.CubeCount,
                _towerService.Base,
                _towerService.TopCube,
                _towerService.CubeCount
            ))
            {
                return DropResult.PlaceOnTop;
            }

            return DropResult.Miss;
        }
    }
}
