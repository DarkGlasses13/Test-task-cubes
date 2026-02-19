using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CubeGame
{
    public class GameplayState : IGameState
    {
        private readonly CubeSizeProvider _cubeSizeProvider;
        private readonly IGameConfig _config;
        private readonly GameController _gameController;
        private readonly CubeScrollView _scrollView;

        public GameplayState(CubeSizeProvider cubeSizeProvider, IGameConfig config,
            GameController gameController, CubeScrollView scrollView)
        {
            _cubeSizeProvider = cubeSizeProvider;
            _config = config;
            _gameController = gameController;
            _scrollView = scrollView;
        }

        public UniTask Enter()
        {
            Canvas.ForceUpdateCanvases();
            _cubeSizeProvider.Initialize(_scrollView.PanelHeight, _config.CubeSizeFillPercent);
            _gameController.Initialize();
            return UniTask.CompletedTask;
        }

        public void Exit() { }
    }
}
