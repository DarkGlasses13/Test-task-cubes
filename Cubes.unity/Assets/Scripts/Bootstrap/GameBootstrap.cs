using UnityEngine;
using Zenject;

namespace CubeGame
{
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private GameController _gameController;
        [SerializeField] private CubeScrollView _scrollView;

        [Inject] private GameStateMachine _stateMachine;
        [Inject] private LoadingState _loadingState;
        [Inject] private GameplayState _gameplayState;

        private async void Start()
        {
            _gameplayState.SetSceneReferences(_gameController, _scrollView);
            await _stateMachine.TransitionTo(_loadingState);
            await _stateMachine.TransitionTo(_gameplayState);
        }
    }
}
