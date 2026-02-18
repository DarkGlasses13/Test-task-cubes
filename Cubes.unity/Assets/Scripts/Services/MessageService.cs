using System;
using System.Collections.Generic;
using UniRx;

namespace CubeGame
{
    public class MessageService : IMessageService
    {
        private readonly Subject<string> _messageSubject = new Subject<string>();

        // Simple localization dictionary — easy to replace with a real system later
        private readonly Dictionary<string, string> _strings = new Dictionary<string, string>
        {
            { LocalizationKeys.CubePlaced,  "Кубик установлен!" },
            { LocalizationKeys.CubeRemoved, "Кубик выброшен в дыру!" },
            { LocalizationKeys.CubeMissed,  "Промах! Кубик исчез." },
            { LocalizationKeys.TowerFull,   "Башня достигла максимальной высоты!" },
        };

        public IObservable<string> OnMessage => _messageSubject;

        public void ShowMessage(string key)
        {
            string text = _strings.TryGetValue(key, out var localized) ? localized : key;
            _messageSubject.OnNext(text);
        }
    }
}
