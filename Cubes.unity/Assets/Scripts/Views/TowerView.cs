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
        private IGameConfig _config;
        private CubeSizeProvider _cubeSizeProvider;
        private CubeAnimationService _animService;
        private TowerModel _model;
        private readonly List<TowerCubeView> _cubes = new();
        public RectTransform BuildZone => _buildZone;
        public IReadOnlyList<TowerCubeView> Cubes => _cubes;

        [Inject]
        public void Construct
        (
            IGameConfig config,
            CubeSizeProvider sizeProvider,
            CubeAnimationService animService,
            TowerModel  model
        )
        {
            _config = config;
            _cubeSizeProvider = sizeProvider;
            _animService = animService;
            _model = model;
        }

        public void RebuildFromModel()
        {
            ClearCubes();
            float cubeSize = _cubeSizeProvider.Size;
            Vector2 towerBase = _model.TowerBase.Value;
            float halfWidth = _buildZone.rect.width * 0.5f;
            float halfCube = cubeSize * 0.5f;
            towerBase.x = Mathf.Clamp(towerBase.x, -halfWidth + halfCube, halfWidth - halfCube);
            _model.SetTowerBase(towerBase);

            for (int i = 0; i < _model.Count; i++)
                CreateCube(i, _model.GetCube(i), false);
        }

        public TowerCubeView AddCube(CubeData data)
        {
            int index = _cubes.Count;
            return CreateCube(index, data, true);
        }

        public void RemoveCube(int towerIndex)
        {
            var removed = PickUpCube(towerIndex);
            
            if (removed != null)
                Destroy(removed.gameObject);
        }

        public TowerCubeView PickUpCube(int towerIndex)
        {
            if (towerIndex < 0 || towerIndex >= _cubes.Count) 
                return null;

            var picked = _cubes[towerIndex];
            picked.RectTransform.DOKill();
            picked.SetVisible(false);
            _cubes.RemoveAt(towerIndex);
            CollapseFrom(towerIndex);
            return picked;
        }

        private void CollapseFrom(int fromIndex)
        {
            float cubeSize = _cubeSizeProvider.Size;
            Vector2 towerBase = _model.TowerBase.Value;

            for (int i = fromIndex; i < _cubes.Count; i++)
            {
                _cubes[i].TowerIndex = i;
                float targetX = towerBase.x + _model.GetCube(i).HorizontalOffset;
                float targetY = cubeSize * 0.5f + i * cubeSize;
                _animService.PlayCollapse(_cubes[i].RectTransform, targetX, targetY);
            }
        }

        public bool IsDropOnTower(Vector2 screenPos, Camera cam)
        {
            return RectTransformUtility.RectangleContainsScreenPoint(_zone, screenPos, cam);
        }

        public bool IsDropOnTopCube(Vector2 screenPos, Camera cam)
        {
            if (_cubes.Count == 0) return false;

            Vector2 towerCoords = ScreenToTowerCoords(screenPos, cam);
            float cubeSize = _cubeSizeProvider.Size;
            float topCubeX = GetTopCubeX();
            float tolerance = cubeSize * _config.DropTolerance;

            if (Mathf.Abs(towerCoords.x - topCubeX) > tolerance)
                return false;

            float newCubeCenter = cubeSize * 0.5f + _model.Count * cubeSize;
            if (towerCoords.y < newCubeCenter)
                return false;

            return true;
        }

        public float GetZoneHeight() => _buildZone.rect.height;

        public float GetBaseX() => _model.TowerBase.Value.x;

        public float GetTopCubeX()
        {
            if (_model.Count == 0)
                return _model.TowerBase.Value.x;

            return _model.TowerBase.Value.x + _model.GetCube(_model.Count - 1).HorizontalOffset;
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

        private TowerCubeView CreateCube(int index, CubeData data, bool animate)
        {
            float cubeSize = _cubeSizeProvider.Size;
            Vector2 towerBase = _model.TowerBase.Value;
            var instance = Instantiate(_cubePrefab, _buildZone);
            var rt = instance.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(cubeSize, cubeSize);
            float x = towerBase.x + data.HorizontalOffset;
            float y = cubeSize * 0.5f + index * cubeSize;
            rt.anchoredPosition = new Vector2(x, y);
            var cubeView = instance.GetComponent<TowerCubeView>();
            cubeView.Setup(index, data.ColorIndex, _config.CubeSprites[data.ColorIndex], cubeSize);
            _cubes.Add(cubeView);

            if (animate)
                _animService.PlayBounce(rt);

            return cubeView;
        }

        private void ClearCubes()
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
