using System.Collections.Generic;
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
        private CubeSizeProvider _cubeSizeProvider;
        private CubeAnimationService _animationService;
        private readonly List<TowerCubeView> _cubes = new();
        public RectTransform BuildZone => _buildZone;
        public IReadOnlyList<TowerCubeView> Cubes => _cubes;

        [Inject]
        public void Construct
        (
            CubeSizeProvider sizeProvider,
            CubeAnimationService animationService
        )
        {
            _cubeSizeProvider = sizeProvider;
            _animationService = animationService;
        }

        public void RemoveCube(int from, Vector2 towerBase, IReadOnlyList<CubeInTowerData> cubesData)
        {
            var removed = PickUpCube(from, towerBase, cubesData);
            
            if (removed != null)
                Destroy(removed.gameObject);
        }

        public TowerCubeView PickUpCube(int from, Vector2 towerBase, IReadOnlyList<CubeInTowerData> cubesData)
        {
            if (from < 0 || from >= _cubes.Count) 
                return null;

            var picked = _cubes[from];
            picked.RectTransform.DOKill();
            picked.SetVisible(false);
            _cubes.RemoveAt(from);
            CollapseFrom(from, towerBase, cubesData);
            return picked;
        }

        private void CollapseFrom(int fromIndex, Vector2 towerBase, IReadOnlyList<CubeInTowerData> cubesData)
        {
            float cubeSize = _cubeSizeProvider.Size;

            for (int i = fromIndex; i < _cubes.Count; i++)
            {
                _cubes[i].Place = i;
                float targetX = towerBase.x + cubesData[i].HorizontalOffset;
                float targetY = cubeSize * 0.5f + i * cubeSize;
                _animationService.PlayCollapse(_cubes[i].RectTransform, targetX, targetY);
            }
        }

        public bool IsDropOnTower(Vector2 screenPos, Camera cam)
        {
            return RectTransformUtility.RectangleContainsScreenPoint(_zone, screenPos, cam);
        }

        public bool IsDropOnTopCube
        (
            Vector2 screenPos,
            Camera cam,
            int cubeCount,
            Vector2 towerBase,
            CubeInTowerData topCubeData,
            float dropTolerance
        )
        {
            if (_cubes.Count == 0) return false;

            Vector2 towerCoords = ScreenToTowerCoords(screenPos, cam);
            float cubeSize = _cubeSizeProvider.Size;
            float topCubeX = GetTopCubeX(cubeCount, towerBase, topCubeData);
            float tolerance = cubeSize * dropTolerance;

            if (Mathf.Abs(towerCoords.x - topCubeX) > tolerance)
                return false;

            float newCubeCenter = cubeSize * 0.5f + cubeCount * cubeSize;
            
            if (towerCoords.y < newCubeCenter)
                return false;

            return true;
        }

        public float GetZoneHeight() => _buildZone.rect.height;

        public float GetTopCubeX(int cubeCount, Vector2 towerBase, CubeInTowerData topCubeData)
        {
            if (cubeCount == 0)
                return towerBase.x;

            return towerBase.x + topCubeData.HorizontalOffset;
        }

        public Vector2 ScreenToTowerCoords(Vector2 screenPos, Camera cam)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle
            (
                _buildZone,
                screenPos,
                cam,
                out Vector2 localPoint
            );

            Rect r = _buildZone.rect;
            return new Vector2(localPoint.x - r.center.x, localPoint.y - r.yMin);
        }

        public TowerCubeView CreateCube(CubeInTowerData data, Vector2 towerBase, Sprite sprite, bool animate)
        {
            float cubeSize = _cubeSizeProvider.Size;
            var instance = Instantiate(_cubePrefab, _buildZone);
            var rt = instance.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(cubeSize, cubeSize);
            float x = towerBase.x + data.HorizontalOffset;
            float y = cubeSize * 0.5f + _cubes.Count * cubeSize;
            rt.anchoredPosition = new Vector2(x, y);
            var cubeView = instance.GetComponent<TowerCubeView>();
            cubeView.Setup(data.Id, _cubes.Count, cubeSize, sprite);
            _cubes.Add(cubeView);

            if (animate)
                _animationService.PlayBounce(rt);

            return cubeView;
        }

        public void ClearCubes()
        {
            foreach (var view in _cubes)
            {
                if (view == null) continue;
                view.RectTransform.DOKill();
                view.gameObject.SetActive(false);
                Destroy(view.gameObject);
            }
            _cubes.Clear();
        }
    }
}
