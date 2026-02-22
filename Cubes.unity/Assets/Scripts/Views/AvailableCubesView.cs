using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace CubeGame
{
    public class AvailableCubesView : MonoBehaviour
    {
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private RectTransform _content;
        [SerializeField] private AvailableCubeView _availableCubePrefab;
        [SerializeField] private float _spacing = 10f;
        private CubeSizeProvider _cubeSizeProvider;
        private ReactiveCollection<AvailableCubeView> _cubes = new();
        
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
        }
        
        public AvailableCubeView CreateCube(Sprite sprite)
        {
            float size = _cubeSizeProvider.Size;
            var instance = Instantiate(_availableCubePrefab, _content);
            var rt = instance.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(size, size);
            rt.anchoredPosition = new Vector2(_spacing + _cubes.Count * (size + _spacing) + size * 0.5f, 0f);
            var cubeView = instance.GetComponent<AvailableCubeView>();
            cubeView.Setup(sprite, _scrollRect);
            _cubes.Add(cubeView);
            int count = _cubes.Count;
            float totalWidth = _spacing + count * size + Mathf.Max(0, count - 1) * _spacing + _spacing;
            _content.sizeDelta = new Vector2(totalWidth, _content.sizeDelta.y);

            return cubeView;
        }
        
        public void ClearCubes()
        {
            foreach (Transform child in _content)
            {
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }
        }
    }
}
