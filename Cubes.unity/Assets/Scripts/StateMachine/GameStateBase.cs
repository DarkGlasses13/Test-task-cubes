using Cysharp.Threading.Tasks;

namespace CubeGame
{
    public abstract class GameStateBase : IGameState
    {
        protected IGameStateSwitcher _switcher;

        protected GameStateBase(IGameStateSwitcher switcher)
        {
            _switcher = switcher;
        }

        public abstract UniTask Enter();
        public abstract UniTask Exit();
    }
}