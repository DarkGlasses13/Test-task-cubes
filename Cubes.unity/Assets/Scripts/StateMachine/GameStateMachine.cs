using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace CubeGame
{
    public class GameStateMachine : IGameStateSwitcher
    {
        private readonly Dictionary<Type, IGameState> _states;
        private IGameState _currentState;

        public IGameState CurrentState => _currentState;

        public GameStateMachine()
        {
            _states = new Dictionary<Type, IGameState>();
        }

        public GameStateMachine RegisterState<TState>(TState state) where TState : IGameState
        {
            if (_states.TryAdd(typeof(TState), state) == false)
            {
                throw new InvalidOperationException("State already exists");
            }
            
            return this;
        }

        public async UniTask Enter<TState>() where TState : IGameState
        {
            if (_states.TryGetValue(typeof(TState), out var newState) == false)
            {
                throw new InvalidOperationException($"State {typeof(TState)} not registered");
            }

            if (_currentState != null)
            {
                await _currentState.Exit();
            }

            _currentState = newState;
            await _currentState.Enter();
        }
    }
}
