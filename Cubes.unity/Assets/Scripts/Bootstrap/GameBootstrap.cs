using Zenject;

namespace CubeGame
{
    public class GameBootstrap : IInitializable
    {
        private readonly GameStateMachine _stateMachine;
        private readonly LoadingState _loadingState;
        private readonly GameplayState _gameplayState;

        public GameBootstrap(GameStateMachine stateMachine, LoadingState loadingState,
            GameplayState gameplayState)
        {
            _stateMachine = stateMachine;
            _loadingState = loadingState;
            _gameplayState = gameplayState;
        }

        public async void Initialize()
        {
            await _stateMachine.TransitionTo(_loadingState);
            await _stateMachine.TransitionTo(_gameplayState);
        }
    }
}
