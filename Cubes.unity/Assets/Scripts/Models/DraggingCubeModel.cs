using System;
using UniRx;

namespace CubeGame
{
    public class DraggingCubeModel
    {
        private Subject<string> _draggingStarted = new();
        private StringReactiveProperty _cube = new(String.Empty);
        private Subject<string> _droped = new();
        
        public IObservable<string> DraggingStarted => _draggingStarted;
        public IReadOnlyReactiveProperty<string> Cube => _cube;
        public IObservable<string> Droped => _droped;

        public void StartDragging(string id)
        {
            _cube.Value = id;
            _draggingStarted.OnNext(id);
        }

        public void Drop()
        {
            var id = _cube.Value;
            _cube.Value = string.Empty;
            _droped.OnNext(id);
        }
    }
}