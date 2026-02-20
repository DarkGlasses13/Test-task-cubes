using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CubeGame
{
    public class GameplayState : GameStateBase
    {
        private readonly CubeSizeProvider _cubeSizeProvider;
        private readonly IGameConfig _config;
        private readonly CubeScrollView _scrollView;
        private readonly TowerView _towerView;
        private readonly CubeScrollView _cubeScrollView;
        private readonly GameController _gameController;

        public GameplayState
        (
            IGameStateSwitcher switcher,
            CubeSizeProvider cubeSizeProvider,
            IGameConfig config,
            CubeScrollView scrollView,
            TowerView towerView,
            CubeScrollView cubeScrollView,
            GameController gameController
            
        ) : base(switcher)
        {
            _cubeSizeProvider = cubeSizeProvider;
            _config = config;
            _scrollView = scrollView;
            _towerView = towerView;
            _cubeScrollView = cubeScrollView;
            _gameController = gameController;
        }

        public override async UniTask Enter()
        {
            Canvas.ForceUpdateCanvases();
            _cubeSizeProvider.Initialize(_scrollView.PanelHeight, _config.CubeSizeFillPercent);
            _cubeScrollView.PopulateCubes();
            _towerView.RebuildFromModel();
            _gameController.BindView();
            await UniTask.CompletedTask;
        }

        public override UniTask Exit() => UniTask.CompletedTask;
    }
}
