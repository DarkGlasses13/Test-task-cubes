using Cysharp.Threading.Tasks;

namespace CubeGame
{
    public class SOConfigProvider : IConfigProvider
    {
        private readonly GameConfigSO _config;

        public SOConfigProvider(GameConfigSO config)
        {
            _config = config;
        }

        public UniTask<IGameConfig> LoadConfigAsync()
        {
            return UniTask.FromResult<IGameConfig>(_config);
        }
    }
}
