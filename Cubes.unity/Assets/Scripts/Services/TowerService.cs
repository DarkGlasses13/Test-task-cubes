using UnityEngine;

namespace CubeGame
{
    public class TowerService : ITowerService
    {
        private readonly TowerModel _model;
        private readonly IGameConfig _config;
        private readonly IMessageService _messageService;
        private readonly CubeSizeProvider _cubeSizeProvider;
        private int _nextCubeId;

        public bool IsEmpty => _model.Count == 0;
        public int CubeCount => _model.Count;

        private float CubeSize => _cubeSizeProvider.Size;

        public TowerService(TowerModel model, IGameConfig config,
            IMessageService messageService, CubeSizeProvider cubeSizeProvider)
        {
            _model = model;
            _config = config;
            _messageService = messageService;
            _cubeSizeProvider = cubeSizeProvider;
            _nextCubeId = _model.GetNextId();
        }

        public bool CanAddMore(float zoneHeight, float cubeSize)
        {
            return (_model.Count + 1) * cubeSize <= zoneHeight;
        }

        public CubeData PlaceCube(int colorIndex, float dropOffsetX)
        {
            float maxOffset = CubeSize * _config.MaxHorizontalOffsetPercent;

            float newAbsoluteOffset;
            if (_model.Count == 0)
            {
                newAbsoluteOffset = 0f;
            }
            else
            {
                float topOffset = _model.GetCube(_model.Count - 1).HorizontalOffset;
                float clampedRelative = Mathf.Clamp(dropOffsetX, -maxOffset, maxOffset);
                newAbsoluteOffset = topOffset + clampedRelative;
            }

            var cubeData = new CubeData(_nextCubeId++, colorIndex, newAbsoluteOffset);
            _model.AddCube(cubeData);
            _messageService.ShowMessage(_config.MsgCubePlaced);
            return cubeData;
        }

        public void RemoveCube(int towerIndex, bool silent = false)
        {
            if (towerIndex < 0 || towerIndex >= _model.Count) return;

            float belowOffset = towerIndex > 0
                ? _model.GetCube(towerIndex - 1).HorizontalOffset
                : 0f;

            _model.RemoveCubeAt(towerIndex);

            if (towerIndex > 0 && towerIndex < _model.Count)
            {
                float firstAboveOffset = _model.GetCube(towerIndex).HorizontalOffset;
                float gap = firstAboveOffset - belowOffset;
                float maxOff = CubeSize * _config.MaxHorizontalOffsetPercent;
                float clampedGap = Mathf.Clamp(gap, -maxOff, maxOff);
                float shift = gap - clampedGap;

                if (Mathf.Abs(shift) > 0.001f)
                {
                    for (int i = towerIndex; i < _model.Count; i++)
                    {
                        var cube = _model.GetCube(i);
                        cube.HorizontalOffset -= shift;
                        _model.SetCube(i, cube);
                    }
                }
            }

            if (!silent)
                _messageService.ShowMessage(_config.MsgCubeRemoved);
        }

        public void SetTowerBase(Vector2 localPosition)
        {
            _model.SetTowerBase(localPosition);
        }

        public void NotifyMiss()
        {
            _messageService.ShowMessage(_config.MsgCubeMissed);
        }

        public void NotifyTowerFull()
        {
            _messageService.ShowMessage(_config.MsgTowerFull);
        }
    }
}
