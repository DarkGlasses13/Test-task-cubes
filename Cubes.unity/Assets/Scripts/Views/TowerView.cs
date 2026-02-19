using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace CubeGame
{
    /// <summary>
    /// Manages the visual representation of the tower in the right zone.
    /// Does not subscribe to model changes — the controller drives updates explicitly
    /// so animations can be managed properly.
    /// </summary>
    public class TowerView : MonoBehaviour
    {
        [SerializeField] private RectTransform _towerZone;
        [SerializeField] private RectTransform _buildZone;
        [SerializeField] private GameObject _towerCubePrefab;

        [Inject] private TowerModel _model;
        [Inject] private IGameConfig _config;
        [Inject] private CubeAnimationService _animService;

        private readonly List<TowerCubeView> _cubeViews = new List<TowerCubeView>();
        private GameController _gameController;

        public RectTransform TowerZone => _towerZone;
        public RectTransform BuildZone => _buildZone;

        public void Initialize(GameController controller)
        {
            _gameController = controller;
        }

        /// <summary>
        /// Rebuild all tower visuals from model state (used on load).
        /// </summary>
        public void RebuildFromModel()
        {
            ClearViews();

            float cubeSize = _config.CubeUISize;
            Vector2 towerBase = _model.TowerBase.Value;

            // Clamp base X in case aspect ratio changed since last save
            float halfWidth = _buildZone.rect.width * 0.5f;
            float halfCube = cubeSize * 0.5f;
            towerBase.x = Mathf.Clamp(towerBase.x, -halfWidth + halfCube, halfWidth - halfCube);
            _model.SetTowerBase(towerBase);

            for (int i = 0; i < _model.Count; i++)
                CreateCubeView(i, _model.GetCube(i), false);
        }

        /// <summary>
        /// Add a visual cube on top of the tower with bounce animation.
        /// </summary>
        public TowerCubeView AddCubeVisual(CubeData data)
        {
            int index = _cubeViews.Count;
            return CreateCubeView(index, data, true);
        }

        /// <summary>
        /// Remove a cube visual, destroy it, and animate remaining cubes collapsing down.
        /// Call this AFTER the model has been updated.
        /// </summary>
        public void RemoveCubeVisual(int towerIndex)
        {
            if (towerIndex < 0 || towerIndex >= _cubeViews.Count) return;

            var removed = _cubeViews[towerIndex];
            _cubeViews.RemoveAt(towerIndex);
            removed.RectTransform.DOKill();
            Destroy(removed.gameObject);

            CollapseFrom(towerIndex);
        }

        /// <summary>
        /// Pick up a cube: hide it, remove from list, collapse remaining.
        /// Does NOT destroy the GameObject so drag events keep working.
        /// Call this AFTER the model has been updated.
        /// </summary>
        public void PickUpCubeVisual(int towerIndex)
        {
            if (towerIndex < 0 || towerIndex >= _cubeViews.Count) return;

            var picked = _cubeViews[towerIndex];
            picked.RectTransform.DOKill();
            picked.SetVisible(false);
            _cubeViews.RemoveAt(towerIndex);

            CollapseFrom(towerIndex);
        }

        private void CollapseFrom(int fromIndex)
        {
            float cubeSize = _config.CubeUISize;
            Vector2 towerBase = _model.TowerBase.Value;

            for (int i = fromIndex; i < _cubeViews.Count; i++)
            {
                _cubeViews[i].TowerIndex = i;
                float targetX = towerBase.x + _model.GetCube(i).HorizontalOffset;
                float targetY = cubeSize * 0.5f + i * cubeSize;
                _animService.PlayCollapse(_cubeViews[i].RectTransform, targetX, targetY);
            }
        }

        public bool IsDropOnTower(Vector2 screenPos, Camera cam)
        {
            return RectTransformUtility.RectangleContainsScreenPoint(_towerZone, screenPos, cam);
        }

        public bool IsDropOnTopCube(Vector2 screenPos, Camera cam)
        {
            if (_cubeViews.Count == 0) return false;

            Vector2 towerCoords = ScreenToTowerCoords(screenPos, cam);
            float cubeSize = _config.CubeUISize;
            Vector2 towerBase = _model.TowerBase.Value;
            float tolerance = cubeSize * _config.DropTolerance;

            if (Mathf.Abs(towerCoords.x - towerBase.x) > tolerance)
                return false;

            float newCubeCenter = cubeSize * 0.5f + _model.Count * cubeSize;
            if (towerCoords.y < newCubeCenter)
                return false;

            return true;
        }

        public float GetZoneHeight()
        {
            return _buildZone.rect.height;
        }

        public float GetTowerBaseX()
        {
            return _model.TowerBase.Value.x;
        }

        public float GetTopCubeX()
        {
            if (_model.Count == 0)
                return _model.TowerBase.Value.x;

            return _model.TowerBase.Value.x + _model.GetCube(_model.Count - 1).HorizontalOffset;
        }

        /// <summary>
        /// Convert screen position to tower coordinate system
        /// where Y=0 is at the bottom of the container.
        /// </summary>
        public Vector2 ScreenToTowerCoords(Vector2 screenPos, Camera cam)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _buildZone, screenPos, cam, out Vector2 localPoint);

            Rect r = _buildZone.rect;
            return new Vector2(localPoint.x - r.center.x, localPoint.y - r.yMin);
        }

        private TowerCubeView CreateCubeView(int index, CubeData data, bool animate)
        {
            float cubeSize = _config.CubeUISize;
            Vector2 towerBase = _model.TowerBase.Value;

            var go = Instantiate(_towerCubePrefab, _buildZone);
            var rt = go.GetComponent<RectTransform>();

            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(cubeSize, cubeSize);

            float x = towerBase.x + data.HorizontalOffset;
            float y = cubeSize * 0.5f + index * cubeSize;
            rt.anchoredPosition = new Vector2(x, y);

            var cubeView = go.GetComponent<TowerCubeView>();
            cubeView.Setup(
                index,
                data.ColorIndex,
                _config.CubeSprites[data.ColorIndex],
                cubeSize,
                _gameController.OnTowerCubeDragStarted,
                _gameController.OnTowerCubeDragging,
                _gameController.OnTowerCubeDragEnded
            );

            _cubeViews.Add(cubeView);

            if (animate)
                _animService.PlayBounce(rt);

            return cubeView;
        }

        private void ClearViews()
        {
            foreach (var view in _cubeViews)
            {
                if (view == null) continue;
                view.RectTransform.DOKill();
                Destroy(view.gameObject);
            }
            _cubeViews.Clear();
        }
    }
}
