using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CubeGame
{
    public class GameplayState : IGameState
    {
        private readonly CubeSizeProvider _cubeSizeProvider;
        private readonly IGameConfig _config;

        private GameController _gameController;
        private CubeScrollView _scrollView;

        public GameplayState(CubeSizeProvider cubeSizeProvider, IGameConfig config)
        {
            _cubeSizeProvider = cubeSizeProvider;
            _config = config;
        }

        public void SetSceneReferences(GameController controller, CubeScrollView scrollView)
        {
            _gameController = controller;
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
