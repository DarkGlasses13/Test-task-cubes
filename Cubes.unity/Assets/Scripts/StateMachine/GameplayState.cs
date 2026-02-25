using AssetProvider;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CubeGame
{
    public class GameplayState : GameStateBase
    {
        private readonly CubeSizeProvider _cubeSizeProvider;
        private readonly IGameplayConfigProvider _gameplayConfigProvider;
        private readonly AvailableCubesView _availableCubesView;
        private readonly AvailableCubesController _availableCubesController;
        private readonly TowerController _towerController;

        public GameplayState
        (
            IGameStateSwitcher switcher,
            CubeSizeProvider cubeSizeProvider,
            IGameplayConfigProvider gameplayConfigProvider,
            AvailableCubesView availableCubesView,
            AvailableCubesController availableCubesController,
            TowerController towerController
            
        ) : base(switcher)
        {
            _cubeSizeProvider = cubeSizeProvider;
            _gameplayConfigProvider = gameplayConfigProvider;
            _availableCubesView = availableCubesView;
            _availableCubesController = availableCubesController;
            _towerController = towerController;
        }

        public override async UniTask Enter()
        {
            Canvas.ForceUpdateCanvases();
            _cubeSizeProvider.Initialize(_availableCubesView.PanelHeight, _gameplayConfigProvider.Get().CubeSizeFillPercent);
            _availableCubesController.Bind();
            _towerController.Bind();
            await UniTask.CompletedTask;
        }

        public override UniTask Exit() => UniTask.CompletedTask;
    }
}
