using Cysharp.Threading.Tasks;

namespace CubeGame
{
    public class GameStateMachine
    {
        private IGameState _currentState;

        public IGameState CurrentState => _currentState;

        public async UniTask TransitionTo(IGameState newState)
        {
            _currentState?.Exit();
            _currentState = newState;
            await _currentState.Enter();
        }
    }
}
