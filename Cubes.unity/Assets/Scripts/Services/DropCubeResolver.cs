using AssetProvider;
using UnityEngine;

namespace CubeGame
{
    public class DropCubeResolver
    {
        private readonly IGameplayConfigProvider _gameplayConfigProvider;
        private readonly HoleView _holeView;
        private readonly TowerView _towerView;
        private readonly TowerModel _towerModel;

        public DropCubeResolver
        (
            IGameplayConfigProvider  gameplayConfigProvider,
            HoleView holeView,
            TowerView towerView,
            TowerModel towerModel
        )
        {
            _gameplayConfigProvider = gameplayConfigProvider;
            _holeView = holeView;
            _towerView = towerView;
            _towerModel = towerModel;
        }
        
        public DropResult Resolve(Vector2 screenPos, bool checkHole)
        {
            if (checkHole && _holeView.IsInsideHole(screenPos))
                return DropResult.Hole;

            if (_towerView.IsInTowerZone(screenPos) == false)
                return DropResult.Miss;

            if (_towerModel.IsEmpty)
                return DropResult.PlaceFirst;
            
            if (_towerView.IsDropOnTopCube(screenPos, _gameplayConfigProvider.Get().DropTolerance))
                return DropResult.PlaceOnTop;

            return DropResult.Miss;
        }
    }
}