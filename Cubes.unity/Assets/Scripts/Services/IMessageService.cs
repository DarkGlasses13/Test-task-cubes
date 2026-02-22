using System;
using UnityEngine.Localization;

namespace CubeGame
{
    public interface IMessageService
    {
        IObservable<string> OnMessage { get; }
        void ShowMessage(string tableReference, string tableEntryReference);
    }
}
