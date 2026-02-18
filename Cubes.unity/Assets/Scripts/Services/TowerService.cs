using UnityEngine;

namespace CubeGame
{
    public class TowerService : ITowerService
    {
        private readonly TowerModel _model;
        private readonly IGameConfig _config;
        private readonly IMessageService _messageService;
        private int _nextCubeId;

        public bool IsEmpty => _model.Count == 0;
        public int CubeCount => _model.Count;

        public TowerService(TowerModel model, IGameConfig config, IMessageService messageService)
        {
            _model = model;
            _config = config;
            _messageService = messageService;
            _nextCubeId = _model.GetNextId();
        }

        public bool CanAddMore(float zoneHeight, float cubeSize)
        {
            return (_model.Count + 1) * cubeSize <= zoneHeight;
        }

        public CubeData PlaceCube(int colorIndex, float dropOffsetX)
        {
            float maxOffset = _config.CubeUISize * _config.MaxHorizontalOffsetPercent;
            float clampedOffset = _model.Count == 0 ? 0f : Mathf.Clamp(dropOffsetX, -maxOffset, maxOffset);

            var cubeData = new CubeData(_nextCubeId++, colorIndex, clampedOffset);
            _model.AddCube(cubeData);
            _messageService.ShowMessage(LocalizationKeys.CubePlaced);
            return cubeData;
        }

        public void RemoveCube(int towerIndex)
        {
            if (towerIndex < 0 || towerIndex >= _model.Count) return;
            _model.RemoveCubeAt(towerIndex);
            _messageService.ShowMessage(LocalizationKeys.CubeRemoved);
        }

        public void SetTowerBase(Vector2 localPosition)
        {
            _model.SetTowerBase(localPosition);
        }

        public void NotifyMiss()
        {
            _messageService.ShowMessage(LocalizationKeys.CubeMissed);
        }

        public void NotifyTowerFull()
        {
            _messageService.ShowMessage(LocalizationKeys.TowerFull);
        }
    }
}
