using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using Zenject;

namespace CubeGame
{
    [RequireComponent(typeof(TextMeshProUGUI), typeof(CanvasGroup))]
    public class MessageView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private CanvasGroup _canvasGroup;
        private IMessageService _messageService;
        private CubeAnimationService _animService;

        [Inject]
        public void Construct(IMessageService  messageService, CubeAnimationService animService)
        {
            _text = GetComponent<TextMeshProUGUI>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _messageService = messageService;
            _animService = animService;
        }

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
