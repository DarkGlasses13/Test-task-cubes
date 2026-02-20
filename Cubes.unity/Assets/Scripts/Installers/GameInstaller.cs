using UnityEngine;
using Zenject;

namespace CubeGame
{
    public class GameInstaller : MonoInstaller
    {
        [SerializeField] private GameConfigSO _gameConfig;

        [Header("Scene References")]
        [SerializeField] private Canvas _canvas;
        [SerializeField] private TowerView _towerView;
        [SerializeField] private HoleView _holeView;
        [SerializeField] private CubeScrollView _scrollView;
        [SerializeField] private DragProxyView _dragProxy;

        public override void InstallBindings()
        {
            var configHolder = new GameConfigHolder();
            
            var playerCamera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : _canvas.worldCamera;
            
            Container.Bind<GameConfigHolder>().FromInstance(configHolder).AsSingle();
            Container.Bind<IGameConfig>().FromInstance(configHolder).AsSingle();
            Container.Bind<GameConfigSO>().FromInstance(_gameConfig).AsSingle();
            Container.Bind<IConfigProvider>().To<SOConfigProvider>().AsSingle();
            Container.Bind<Canvas>().FromInstance(_canvas).AsSingle();
            Container.Bind<Camera>().FromInstance(playerCamera).AsSingle();
            Container.Bind<TowerView>().FromInstance(_towerView).AsSingle();
            Container.Bind<HoleView>().FromInstance(_holeView).AsSingle();
            Container.Bind<CubeScrollView>().FromInstance(_scrollView).AsSingle();
            Container.Bind<DragProxyView>().FromInstance(_dragProxy).AsSingle();
            Container.Bind<TowerModel>().AsSingle();
            Container.Bind<ITowerService>().To<TowerService>().AsSingle();
            Container.Bind<ISaveService>().To<SaveService>().AsSingle();
            Container.Bind<IMessageService>().To<MessageService>().AsSingle();
            Container.Bind<CubeAnimationService>().AsSingle();
            Container.Bind<CubeSizeProvider>().AsSingle();
            Container.Bind<CubeEffectsService>().AsSingle();
            Container.Bind<DropHandler>().AsSingle();
            Container.Bind<GameController>().AsSingle();
            Container.BindInterfacesAndSelfTo<GameStateMachine>().AsSingle();
            Container.Bind<LoadingState>().AsSingle();
            Container.Bind<GameplayState>().AsSingle();
            Container.BindInterfacesTo<GameBootstrap>().AsSingle();
        }
    }
}
