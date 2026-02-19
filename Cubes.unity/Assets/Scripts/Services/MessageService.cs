using System;
using UniRx;
using UnityEngine.Localization.Settings;

namespace CubeGame
{
    public class MessageService : IMessageService
    {
        private const string TableName = "Messages";

        private readonly Subject<string> _messageSubject = new Subject<string>();

        public IObservable<string> OnMessage => _messageSubject;

        public void ShowMessage(string key)
        {
            var handle = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(TableName, key);

            if (handle.IsDone)
            {
                _messageSubject.OnNext(handle.Result);
            }
            else
            {
                handle.Completed += op => _messageSubject.OnNext(op.Result);
            }
        }
    }
}
