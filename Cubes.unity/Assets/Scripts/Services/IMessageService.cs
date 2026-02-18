using System;

namespace CubeGame
{
    public interface IMessageService
    {
        IObservable<string> OnMessage { get; }
        void ShowMessage(string key);
    }
}
