using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace CubeGame
{
    [RequireComponent(typeof(ScrollRect))]
    public class AvailableCubesView : MonoBehaviour
    {
        private ScrollRect _scrollRect;
        [SerializeField] private RectTransform _content;
        [SerializeField] private AvailableCubeView _availableCubePrefab;
        [SerializeField] private float _spacing = 10f;
        private CubeSizeProvider _cubeSizeProvider;
        private ReactiveCollection<AvailableCubeView> _cubes = new();
        
        public ScrollRect ScrollRect => _scrollRect;
        public IReadOnlyReactiveCollection<AvailableCubeView> Cubes => _cubes;

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
        public void Construct(CubeSizeProvider cubeSizeProvider)
        {
            _cubeSizeProvider = cubeSizeProvider;
            _scrollRect = GetComponent<ScrollRect>();
        }
        
        public AvailableCubeView CreateCube(Sprite sprite)
        {
            float size = _cubeSizeProvider.Size;
            var instance = Instantiate(_availableCubePrefab, _content);
            instance.Construct(_scrollRect);
            instance.RectTransform.anchorMin = new Vector2(0f, 0.5f);
            instance.RectTransform.anchorMax = new Vector2(0f, 0.5f);
            instance.RectTransform.pivot = new Vector2(0.5f, 0.5f);
            instance.RectTransform.sizeDelta = new Vector2(size, size);
            instance.RectTransform.anchoredPosition = new Vector2(_spacing + _cubes.Count * (size + _spacing) + size * 0.5f, 0f);
            instance.SetSprite(sprite);
            _cubes.Add(instance);
            float totalWidth = _spacing + _cubes.Count * size + Mathf.Max(0, _cubes.Count - 1) * _spacing + _spacing;
            _content.sizeDelta = new Vector2(totalWidth, _content.sizeDelta.y);
            return instance;
        }

        public void RemoveCube(AvailableCubeView cube)
        {
            cube.gameObject.SetActive(false);
            _cubes.Remove(cube);
            Destroy(cube.gameObject);
        }
        
        public void Clear()
        {
            foreach (Transform child in _content)
            {
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }
        }
    }
}