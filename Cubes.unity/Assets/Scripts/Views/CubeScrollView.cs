using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

namespace CubeGame
{
    public class CubeScrollView : MonoBehaviour
    {
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private RectTransform _content;
        [SerializeField] private CubeFromScrollView _prefab;
        [SerializeField] private float _spacing = 10f;
        private IGameConfig _config;
        private CubeSizeProvider _cubeSizeProvider;
        private ReactiveCollection<CubeFromScrollView> _cubes = new();
        public IReadOnlyReactiveCollection<CubeFromScrollView> Cubes => _cubes;

        public float PanelHeight
        {
            get
            {
                if (_scrollRect.viewport != null)
                {
                    return _scrollRect.viewport.rect.height;
                }

                return ((RectTransform)transform).rect.height;
            }
        }

        [Inject]
        public void Construct
        (
            IGameConfig config,
            CubeSizeProvider  cubeSizeProvider
        )
        {
            _config = config;
            _cubeSizeProvider = cubeSizeProvider;
        }

        public void PopulateCubes()
        {
            foreach (Transform child in _content)
            {
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }

            float size = _cubeSizeProvider.Size;
            int count = _config.CubeCount;
            float totalWidth = _spacing + count * size + Mathf.Max(0, count - 1) * _spacing + _spacing;
            _content.sizeDelta = new Vector2(totalWidth, _content.sizeDelta.y);

            for (int i = 0; i < count; i++)
            {
                var go = Instantiate(_prefab, _content);
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0f, 0.5f);
                rt.anchorMax = new Vector2(0f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(size, size);
                rt.anchoredPosition = new Vector2(_spacing + i * (size + _spacing) + size * 0.5f, 0f);
                var cubeItem = go.GetComponent<CubeFromScrollView>();
                cubeItem.Setup(i, _config.CubeSprites[i], _scrollRect);
                _cubes.Add(cubeItem);
            }
        }
    }
}
