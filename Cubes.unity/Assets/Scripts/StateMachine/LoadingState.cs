using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CubeGame
{
    public class LoadingState : GameStateBase
    {
        private readonly IConfigProvider _configProvider;
        private readonly GameConfigHolder _configHolder;
        private readonly ISaveService _saveService;
        private readonly Canvas _canvas;

        public LoadingState
        (
            IGameStateSwitcher switcher,
            IConfigProvider configProvider,
            GameConfigHolder configHolder,
            ISaveService saveService
            
        ) : base(switcher)
        {
            _configProvider = configProvider;
            _configHolder = configHolder;
            _saveService = saveService;
        }

        public override async UniTask Enter()
        {
            var config = await _configProvider.LoadConfigAsync();
            _configHolder.SetConfig(config);

            if (config.EnableSave)
            {
                _saveService.Load();
            }
            else
                _saveService.ClearSave();

            await _switcher.Enter<GameplayState>();
        }

        public override UniTask Exit() => UniTask.CompletedTask;
    }
}
