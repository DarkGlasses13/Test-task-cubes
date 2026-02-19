using Cysharp.Threading.Tasks;

namespace CubeGame
{
    public interface IGameState
    {
        UniTask Enter();
        void Exit();
    }
}
