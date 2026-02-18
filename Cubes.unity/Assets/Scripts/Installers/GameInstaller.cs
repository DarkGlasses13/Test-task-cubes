using UnityEngine;
using Zenject;

namespace CubeGame
{
    public class GameInstaller : MonoInstaller
    {
        [SerializeField] private GameConfigSO _gameConfig;

        public override void InstallBindings()
        {
            Container.Bind<IGameConfig>().FromInstance(_gameConfig).AsSingle();
            Container.Bind<TowerModel>().AsSingle();
            Container.Bind<ITowerService>().To<TowerService>().AsSingle();
            Container.Bind<ISaveService>().To<SaveService>().AsSingle();
            Container.Bind<IMessageService>().To<MessageService>().AsSingle();
            Container.Bind<CubeAnimationService>().AsSingle();
        }
    }
}
