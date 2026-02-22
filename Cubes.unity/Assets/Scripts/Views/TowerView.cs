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
        private CubeAnimationService _animService;
        public RectTransform BuildZone => _buildZone;

        [Inject]
        public void Construct
        (
            CubeSizeProvider sizeProvider,
            CubeAnimationService animService
        )
        {
            _cubeSizeProvider = sizeProvider;
            _animService = animService;
        }

        // public void RebuildFromModel()
        // {
        //     ClearCubes();
        //     float cubeSize = _cubeSizeProvider.Size;
        //     Vector2 towerBase = _model.BasePosition.Value;
        //     float halfWidth = _buildZone.rect.width * 0.5f;
        //     float halfCube = cubeSize * 0.5f;
        //     towerBase.x = Mathf.Clamp(towerBase.x, -halfWidth + halfCube, halfWidth - halfCube);
        //     _model.SetBase(towerBase);
        //
        //     for (int place = 0; place < _model.Count; place++)
        //         CreateCube(_model.GetCube(place), false);
        // }

        // public void RemoveCube(int place)
        // {
        //     var removed = PickUpCube(place);
        //
        //     if (removed != null)
        //     {
        //         removed.gameObject.SetActive(false);
        //         Destroy(removed.gameObject);
        //     }
        // }

        // public TowerCubeView PickUpCube(int towerIndex)
        // {
        //     if (towerIndex < 0 || towerIndex >= _cubes.Count) 
        //         return null;
        //
        //     var picked = _cubes[towerIndex];
        //     picked.RectTransform.DOKill();
        //     picked.SetVisible(false);
        //     _cubes.RemoveAt(towerIndex);
        //     CollapseFrom(towerIndex);
        //     return picked;
        // }

        // private void CollapseFrom(int fromPlace)
        // {
        //     float cubeSize = _cubeSizeProvider.Size;
        //     Vector2 towerBase = _model.BasePosition.Value;
        //
        //     for (int i = fromPlace; i < _cubes.Count; i++)
        //     {
        //         _cubes[i].TowerIndex = i;
        //         float targetX = towerBase.x + _model.GetCube(i).HorizontalOffset;
        //         float targetY = cubeSize * 0.5f + i * cubeSize;
        //         _animService.PlayCollapse(_cubes[i].RectTransform, targetX, targetY);
        //     }
        // }

        public bool IsDropOnTower(Vector2 screenPos, Camera cam)
        {
            return RectTransformUtility.RectangleContainsScreenPoint(_zone, screenPos, cam);
        }

        // public bool IsDropOnTopCube(Vector2 screenPos, Camera cam)
        // {
        //     if (_cubes.Count == 0) return false;
        //
        //     Vector2 towerCoords = ScreenToTowerCoords(screenPos, cam);
        //     float cubeSize = _cubeSizeProvider.Size;
        //     float topCubeX = GetTopCubeX();
        //     float tolerance = cubeSize * _gameplayConfigProvider.Get().DropTolerance;
        //
        //     if (Mathf.Abs(towerCoords.x - topCubeX) > tolerance)
        //         return false;
        //
        //     float newCubeCenter = cubeSize * 0.5f + _model.Count * cubeSize;
        //     if (towerCoords.y < newCubeCenter)
        //         return false;
        //
        //     return true;
        // }

        public float GetZoneHeight() => _buildZone.rect.height;

        // public float GetBaseX() => _model.BasePosition.Value.x;

        // public float GetTopCubeX()
        // {
        //     if (_model.Count == 0)
        //         return _model.BasePosition.Value.x;
        //
        //     return _model.BasePosition.Value.x + _model.GetCube(_model.Count - 1).HorizontalOffset;
        // }

        public Vector2 ScreenToTowerCoords(Vector2 screenPos, Camera cam)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle
            (
                _buildZone,
                screenPos,
                cam,
                out Vector2 localPoint
            );

            Rect rect = _buildZone.rect;
            return new Vector2(localPoint.x - rect.center.x, localPoint.y - rect.yMin);
        }

        public TowerCubeView CreateCube(Vector2 baseCoords, float horizontalOffset, int place, Sprite sprite, bool animate)
        {
            float cubeSize = _cubeSizeProvider.Size;
            var instance = Instantiate(_cubePrefab, _buildZone);
            var rt = instance.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(cubeSize, cubeSize);
            float x = baseCoords.x + horizontalOffset;
            float y = cubeSize * 0.5f + place * cubeSize;
            rt.anchoredPosition = new Vector2(x, y);
            var cubeView = instance.GetComponent<TowerCubeView>();
            cubeView.Setup(sprite, cubeSize);

            if (animate)
                _animService.PlayBounce(rt);

            return cubeView;
        }

        // private void ClearCubes()
        // {
        //     foreach (var view in _cubes)
        //     {
        //         if (view == null) continue;
        //         view.RectTransform.DOKill();
        //         view.gameObject.SetActive(false);
        //         Destroy(view.gameObject);
        //     }
        //     _cubes.Clear();
        // }
    }
}
