using System;
using UniRx;
using UnityEngine.Localization;

namespace CubeGame
{
    public class MessageService : IMessageService
    {
        private readonly Subject<string> _messageSubject = new Subject<string>();

        public IObservable<string> OnMessage => _messageSubject;

        public void ShowMessage(string tableReference, string tableEntryReference)
        {
            if (string.IsNullOrEmpty(tableReference) || string.IsNullOrEmpty(tableEntryReference))
                return;

            var localizedString = new LocalizedString
            {
                TableReference = tableReference,
                TableEntryReference = tableEntryReference
            };

            var handle = localizedString.GetLocalizedStringAsync();

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
