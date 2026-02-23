using AssetProvider;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CubeGame
{
    public class GameplayState : GameStateBase
    {
        private readonly CubeSizeProvider _cubeSizeProvider;
        private readonly IGameplayConfigProvider _gameplayConfigProvider;
        private readonly AvailableCubesModel _availableCubesModel;
        private readonly HoleView _holeView;
        private readonly AvailableCubesView _availableCubesView;
        private readonly GameController _gameController;

        public GameplayState
        (
            IGameStateSwitcher switcher,
            CubeSizeProvider cubeSizeProvider,
            IGameplayConfigProvider gameplayConfigProvider,
            AvailableCubesModel availableCubesModel,
            HoleView holeView,
            AvailableCubesView availableCubesView,
            GameController gameController
            
        ) : base(switcher)
        {
            _cubeSizeProvider = cubeSizeProvider;
            _gameplayConfigProvider = gameplayConfigProvider;
            _availableCubesModel = availableCubesModel;
            _holeView = holeView;
            _availableCubesView = availableCubesView;
            _gameController = gameController;
        }

        public override async UniTask Enter()
        {
            _availableCubesModel.Populate(_gameplayConfigProvider.Get().AvailableCubes);
            Canvas.ForceUpdateCanvases();
            _holeView.Construct();
            _cubeSizeProvider.Initialize(_availableCubesView.PanelHeight, _gameplayConfigProvider.Get().CubeSizeFillPercent);
            _gameController.BindView();
            await UniTask.CompletedTask;
        }

        public override UniTask Exit() => UniTask.CompletedTask;
    }
}
