using Cysharp.Threading.Tasks;

namespace CubeGame
{
    public class LoadingState : IGameState
    {
        private readonly IConfigProvider _configProvider;
        private readonly GameConfigHolder _configHolder;
        private readonly ISaveService _saveService;

        public LoadingState(IConfigProvider configProvider, GameConfigHolder configHolder,
            ISaveService saveService)
        {
            _configProvider = configProvider;
            _configHolder = configHolder;
            _saveService = saveService;
        }

        public async UniTask Enter()
        {
            var config = await _configProvider.LoadConfigAsync();
            _configHolder.SetConfig(config);

            if (config.EnableSave)
                _saveService.Load();
            else
                _saveService.ClearSave();
        }

        public void Exit() { }
    }
}
