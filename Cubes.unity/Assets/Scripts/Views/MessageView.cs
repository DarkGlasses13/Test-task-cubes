using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using Zenject;

namespace CubeGame
{
    /// <summary>
    /// Displays status messages above the bottom panel.
    /// Subscribes to MessageService and fades messages out after a delay.
    /// </summary>
    public class MessageView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Inject] private IMessageService _messageService;
        [Inject] private CubeAnimationService _animService;

        private void Start()
        {
            _canvasGroup.alpha = 0f;

            _messageService.OnMessage
                .Subscribe(ShowMessage)
                .AddTo(this);
        }

        private void ShowMessage(string text)
        {
            _canvasGroup.DOKill();
            _text.text = text;
            _animService.PlayFadeMessage(_canvasGroup);
        }
    }
}
