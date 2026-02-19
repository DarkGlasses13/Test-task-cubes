using UnityEngine;
using Zenject;

namespace CubeGame
{
    public class GameInstaller : MonoInstaller
    {
        [SerializeField] private GameConfigSO _gameConfig;

        public override void InstallBindings()
        {
            var configHolder = new GameConfigHolder();
            Container.Bind<GameConfigHolder>().FromInstance(configHolder).AsSingle();
            Container.Bind<IGameConfig>().FromInstance(configHolder).AsSingle();

            Container.Bind<GameConfigSO>().FromInstance(_gameConfig).AsSingle();
            Container.Bind<IConfigProvider>().To<SOConfigProvider>().AsSingle();

            Container.Bind<TowerModel>().AsSingle();
            Container.Bind<ITowerService>().To<TowerService>().AsSingle();
            Container.Bind<ISaveService>().To<SaveService>().AsSingle();
            Container.Bind<IMessageService>().To<MessageService>().AsSingle();
            Container.Bind<CubeAnimationService>().AsSingle();
            Container.Bind<CubeSizeProvider>().AsSingle();

            Container.Bind<GameStateMachine>().AsSingle();
            Container.Bind<LoadingState>().AsSingle();
            Container.Bind<GameplayState>().AsSingle();
        }
    }
}
