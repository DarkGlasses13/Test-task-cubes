using Cysharp.Threading.Tasks;

namespace CubeGame
{
    public interface IConfigProvider
    {
        UniTask<IGameConfig> LoadConfigAsync();
    }
}
