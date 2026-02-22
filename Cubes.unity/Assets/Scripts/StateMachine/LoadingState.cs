using AssetProvider;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CubeGame
{
    public class LoadingState : GameStateBase
    {
        private readonly ICubeSpritesProvider _cubeSpritesProvider;
        private readonly IGameplayConfigProvider _configProvider;
        private readonly ICubeConfigsProvider _cubeConfigsProvider;
        private readonly IMessagesConfigProvider _messagesConfigProvider;
        private readonly ISaveService _saveService;
        private readonly Canvas _canvas;

        public LoadingState
        (
            IGameStateSwitcher switcher,
            ICubeSpritesProvider cubeSpritesProvider,
            IGameplayConfigProvider configProvider,
            ICubeConfigsProvider cubeConfigsProvider,
            IMessagesConfigProvider messagesConfigProvider,
            ISaveService saveService
            
        ) : base(switcher)
        {
            _cubeSpritesProvider = cubeSpritesProvider;
            _configProvider = configProvider;
            _cubeConfigsProvider = cubeConfigsProvider;
            _messagesConfigProvider = messagesConfigProvider;
            _saveService = saveService;
        }

        public override async UniTask Enter()
        {
            await _cubeSpritesProvider.LoadAsync();
            await _configProvider.LoadAsync();
            await _cubeConfigsProvider.LoadAsync();
            await _messagesConfigProvider.LoadAsync();
            var config = _configProvider.Get();

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
