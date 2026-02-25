using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace CubeGame
{
    public class TowerView : MonoBehaviour
    {
        [SerializeField] private RectTransform _zone;
        [SerializeField] private RectTransform _buildZone;
        [SerializeField] private TowerCubeView _cubePrefab;
        private Camera _camera;
        private CubeSizeProvider _cubeSizeProvider;
        private CubeAnimationService _animationService;
        private readonly List<TowerCubeView> _cubes = new();
        private TowerCubeView _pickedCube;
        public RectTransform BuildZone => _buildZone;

        [Inject]
        public void Construct
        (
            Camera cam,
            CubeSizeProvider sizeProvider,
            CubeAnimationService animationService
        )
        {
            _camera = cam;
            _cubeSizeProvider = sizeProvider;
            _animationService = animationService;
        }
        
        public TowerCubeView GetCube(int place) => _cubes[place];
        
        public int GetPlace(TowerCubeView cube) => _cubes.IndexOf(cube);

        public bool IsInTowerZone(Vector2 screenPos)
        {
            return RectTransformUtility.RectangleContainsScreenPoint(_zone, screenPos, _camera);
        }

        public bool IsDropOnTopCube(Vector2 screenPos, float dropTolerance)
        {
            if (_cubes.Count == 0)
                return false;

            Vector2 towerCoords = ScreenToTowerCoords(screenPos);

            float topCubeX =
                _cubes.Last().RectTransform.anchoredPosition.x;

            float tolerance =
                _cubeSizeProvider.Size * dropTolerance;

            if (Mathf.Abs(towerCoords.x - topCubeX) > tolerance)
                return false;

            float newCubeCenter =
                _cubeSizeProvider.Size * 0.5f +
                _cubes.Count * _cubeSizeProvider.Size;

            if (towerCoords.y < newCubeCenter)
                return false;

            return true;
        }

        public Vector2 ScreenToTowerCoords(Vector2 screenPos)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle
            (
                _buildZone,
                screenPos,
                _camera,
                out Vector2 localPoint
            );
            
            return new Vector2(localPoint.x - _buildZone.rect.center.x, localPoint.y - _buildZone.rect.yMin);
        }

        public TowerCubeView Place(float horizontalOffset, Sprite sprite, bool animate)
        {
            var instance = Instantiate(_cubePrefab, _buildZone);
            instance.RectTransform.anchorMin = new Vector2(0.5f, 0f);
            instance.RectTransform.anchorMax = new Vector2(0.5f, 0f);
            instance.RectTransform.pivot = new Vector2(0.5f, 0.5f);
            instance.RectTransform.sizeDelta = new Vector2(_cubeSizeProvider.Size, _cubeSizeProvider.Size);
            float x = horizontalOffset;
            float y = _cubeSizeProvider.Size * 0.5f + _cubes.Count * _cubeSizeProvider.Size;
            instance.RectTransform.anchoredPosition = new Vector2(x, y);
            instance.SetSprite(sprite);
            _cubes.Add(instance);

            if (animate)
                _animationService.PlayBounce(instance.RectTransform);

            return instance;
        }

        public void Pick(int from)
        {
            if (from < 0 || from >= _cubes.Count) 
                return;
        
            var picked = _cubes[from];
            picked.RectTransform.DOKill();
            picked.SetVisible(false);
            _cubes.RemoveAt(from);
            ResetPicked();
            _pickedCube = picked;
        }
        
        public void Collapse(int from, IReadOnlyList<float> horizontalOffsets)
        {
            for (int i = 0; i < horizontalOffsets.Count; i++)
            {
                var cube = _cubes[from + i];
                float targetX = horizontalOffsets[i];
                float targetY = _cubeSizeProvider.Size * 0.5f + (from + i) * _cubeSizeProvider.Size;

                _animationService.PlayCollapse
                (
                    cube.RectTransform,
                    targetX,
                    targetY
                );
            }
        }

        public void ResetPicked()
        {
            if (_pickedCube != null)
            {
                _pickedCube.gameObject.SetActive(false);
                Destroy(_pickedCube.gameObject);
            }
        }
    }
}