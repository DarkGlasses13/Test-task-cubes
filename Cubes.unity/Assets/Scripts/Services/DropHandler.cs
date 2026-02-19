using UnityEngine;

namespace CubeGame
{
    public enum DropResult { PlaceFirst, PlaceOnTop, Hole, TowerFull, Miss }

    public class DropHandler
    {
        private readonly ITowerService _towerService;
        private readonly CubeSizeProvider _cubeSizeProvider;

        public DropHandler(ITowerService towerService, CubeSizeProvider cubeSizeProvider)
        {
            _towerService = towerService;
            _cubeSizeProvider = cubeSizeProvider;
        }

        public DropResult Resolve(Vector2 screenPos, Camera cam,
            TowerView tower, HoleView hole, bool checkHole)
        {
            if (checkHole && hole.IsInsideHole(screenPos, cam))
                return DropResult.Hole;

            if (!tower.IsDropOnTower(screenPos, cam))
                return DropResult.Miss;

            float cubeSize = _cubeSizeProvider.Size;

            if (_towerService.IsEmpty)
                return DropResult.PlaceFirst;

            if (!_towerService.CanAddMore(tower.GetZoneHeight(), cubeSize))
                return DropResult.TowerFull;

            if (tower.IsDropOnTopCube(screenPos, cam))
                return DropResult.PlaceOnTop;

            return DropResult.Miss;
        }
    }
}
