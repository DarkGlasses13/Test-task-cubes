using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace CubeGame
{
    /// <summary>
    /// Manages the bottom scroll panel with cube items.
    /// Populates cubes from config at initialization.
    /// </summary>
    public class CubeScrollView : MonoBehaviour
    {
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private RectTransform _content;
        [SerializeField] private GameObject _cubeItemPrefab;
        [SerializeField] private float _spacing = 10f;

        [Inject] private IGameConfig _config;

        private GameController _gameController;

        public void Initialize(GameController controller)
        {
            _gameController = controller;
            PopulateCubes();
        }

        private void PopulateCubes()
        {
            foreach (Transform child in _content)
            {
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }

            float size = _config.CubeUISize;
            int count = _config.CubeCount;
            float totalWidth = count * size + Mathf.Max(0, count - 1) * _spacing;
            _content.sizeDelta = new Vector2(totalWidth, _content.sizeDelta.y);

            for (int i = 0; i < count; i++)
            {
                var go = Instantiate(_cubeItemPrefab, _content);
                var rt = go.GetComponent<RectTransform>();

                rt.anchorMin = new Vector2(0f, 0.5f);
                rt.anchorMax = new Vector2(0f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(size, size);
                rt.anchoredPosition = new Vector2(i * (size + _spacing) + size * 0.5f, 0f);

                var cubeItem = go.GetComponent<CubeItemView>();
                cubeItem.Setup(
                    i,
                    _config.CubeSprites[i],
                    _scrollRect,
                    _gameController.OnScrollCubeDragStarted,
                    _gameController.OnScrollCubeDragging,
                    _gameController.OnScrollCubeDragEnded
                );
            }
        }
    }
}
