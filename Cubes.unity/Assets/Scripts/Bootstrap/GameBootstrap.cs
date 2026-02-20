using Cysharp.Threading.Tasks;
using Zenject;

namespace CubeGame
{
    public class GameBootstrap : IInitializable
    {
        private readonly GameStateMachine _stateMachine;
        private readonly LoadingState _loadingState;
        private readonly GameplayState _gameplayState;

        public GameBootstrap
        (
            GameStateMachine stateMachine,
            LoadingState loadingState,
            GameplayState gameplayState
        )
        {
            _stateMachine = stateMachine;
            _loadingState = loadingState;
            _gameplayState = gameplayState;
        }

        public void Initialize() => Run().Forget();

        private async UniTaskVoid Run()
        {
            _stateMachine
                .RegisterState(_loadingState)
                .RegisterState(_gameplayState);

            await _stateMachine.Enter<LoadingState>();
        }
    }
}
