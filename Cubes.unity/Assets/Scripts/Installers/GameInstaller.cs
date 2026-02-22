using AssetProvider;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace CubeGame
{
    public class GameInstaller : MonoInstaller
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private TowerView _towerView;
        [SerializeField] private HoleView _holeView;
        [SerializeField] private AvailableCubesView _scrollView;
        [SerializeField] private DragProxyView _dragProxy;

        public override void InstallBindings()
        {
            var playerCamera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : _canvas.worldCamera;

            Container
                .Bind<ICubeSpritesProvider>()
                .To<AddressablesCubeSpritesProvider>()
                .AsSingle()
                .WithArguments("Cube sprite atlas");

            Container
                .Bind<IGameplayConfigProvider>()
                .To<AddressablesGameplayConfigProvider>()
                .AsSingle()
                .WithArguments("Gameplay config");
            
            Container
                .Bind<ICubeConfigsProvider>()
                .To<AddressablesCubeConfigsProvider>()
                .AsSingle()
                .WithArguments("Cube config");
            
            Container
                .Bind<IMessagesConfigProvider>()
                .To<AddressablesMessagesConfigProvider>()
                .AsSingle()
                .WithArguments("Messages config");
            
            Container.Bind<Canvas>().FromInstance(_canvas).AsSingle();
            Container.Bind<Camera>().FromInstance(playerCamera).AsSingle();
            Container.Bind<AvailableCubesModel>().AsSingle();
            Container.Bind<TowerModel>().AsSingle();
            Container.Bind<TowerView>().FromInstance(_towerView).AsSingle();
            Container.Bind<HoleView>().FromInstance(_holeView).AsSingle();
            Container.Bind<AvailableCubesView>().FromInstance(_scrollView).AsSingle();
            Container.Bind<DragProxyView>().FromInstance(_dragProxy).AsSingle();
            Container.Bind<ISaveService>().To<SaveService>().AsSingle();
            Container.Bind<IMessageService>().To<MessageService>().AsSingle();
            Container.Bind<CubeAnimationService>().AsSingle();
            Container.Bind<CubeSizeProvider>().AsSingle();
            Container.Bind<CubeEffectsService>().AsSingle();
            Container.Bind<GameController>().AsSingle();
            Container.BindInterfacesAndSelfTo<GameStateMachine>().AsSingle();
            Container.Bind<LoadingState>().AsSingle();
            Container.Bind<GameplayState>().AsSingle();
            Container.BindInterfacesTo<GameBootstrap>().AsSingle();
        }
    }
}
