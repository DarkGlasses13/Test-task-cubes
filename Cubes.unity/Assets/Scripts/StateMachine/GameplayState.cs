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
        private readonly GameController _gameController;

        public GameplayState
        (
            IGameStateSwitcher switcher,
            CubeSizeProvider cubeSizeProvider,
            IGameplayConfigProvider gameplayConfigProvider,
            AvailableCubesModel availableCubesModel,
            HoleView holeView,
            GameController gameController
            
        ) : base(switcher)
        {
            _cubeSizeProvider = cubeSizeProvider;
            _gameplayConfigProvider = gameplayConfigProvider;
            _availableCubesModel = availableCubesModel;
            _holeView = holeView;
            _gameController = gameController;
        }

        public override async UniTask Enter()
        {
            _holeView.Construct();
            _cubeSizeProvider.Initialize(_holeView.RectTransform, _gameplayConfigProvider.Get().CubeSizeFillPercent);
            _availableCubesModel.Populate(_gameplayConfigProvider.Get().AvailableCubes);
            Canvas.ForceUpdateCanvases();
            _gameController.BindView();
            await UniTask.CompletedTask;
        }

        public override UniTask Exit() => UniTask.CompletedTask;
    }
}
