using Cysharp.Threading.Tasks;

namespace CubeGame
{
    public interface IGameStateSwitcher
    {
        UniTask Enter<TState>() where TState : IGameState;
    }
}