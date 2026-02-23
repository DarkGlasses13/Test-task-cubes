using System.Collections.Generic;
using AssetProvider;
using UnityEngine;

namespace CubeGame
{
    public class TowerService : ITowerService
    {
        private readonly TowerModel _model;
        private readonly IGameplayConfigProvider _configProvider;
        private readonly IMessagesConfigProvider _messagesConfigProvider;
        private readonly IMessageService _messageService;
        private readonly CubeSizeProvider _cubeSizeProvider;
        private int _nextCubePlace;

        public bool IsEmpty => _model.Count == 0;
        public int CubeCount => _model.Count;
        public IReadOnlyList<CubeInTowerData> Cubes => _model.Cubes;
        public CubeInTowerData TopCube => _model.GetCube(_model.Count - 1);
        public Vector2 Base => _model.Base.Value;

        private float CubeSize => _cubeSizeProvider.Size;

        public TowerService
        (
            TowerModel model,
            IGameplayConfigProvider configProvider,
            IMessagesConfigProvider messagesConfigProvider,
            IMessageService messageService, 
            CubeSizeProvider cubeSizeProvider
        )
        {
            _model = model;
            _configProvider = configProvider;
            _messagesConfigProvider = messagesConfigProvider;
            _messageService = messageService;
            _cubeSizeProvider = cubeSizeProvider;
            _nextCubePlace = _model.GetNextPlace();
        }

        public bool CanAddMore(float zoneHeight, float cubeSize)
        {
            return (_model.Count + 1) * cubeSize <= zoneHeight;
        }

        public CubeInTowerData GetCube(int place) => _model.GetCube(place);

        public void SetBase(Vector2 localPosition) => _model.SetBase(localPosition);

        public CubeInTowerData PlaceCube(string id, float dropOffsetX)
        {
            float maxOffset = CubeSize * _configProvider.Get().MaxHorizontalOffsetPercent;

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

            var cubeData = new CubeInTowerData()
            {
                Id = id,
                HorizontalOffset = newAbsoluteOffset,
                Place = _nextCubePlace++
            };
            
            _model.AddCube(cubeData);
            var messagesConfig = _messagesConfigProvider.Get();
            _messageService.ShowMessage(messagesConfig.TableReference, messagesConfig.MsgCubePlaced);
            return cubeData;
        }

        public void RemoveCube(int place, bool silent = false)
        {
            if (place < 0 || place >= _model.Count) return;

            float belowOffset = place > 0
                ? _model.GetCube(place - 1).HorizontalOffset
                : 0f;

            _model.RemoveCubeAt(place);

            if (place > 0 && place < _model.Count)
            {
                float firstAboveOffset = _model.GetCube(place).HorizontalOffset;
                float gap = firstAboveOffset - belowOffset;
                float maxOff = CubeSize * _configProvider.Get().MaxHorizontalOffsetPercent;
                float clampedGap = Mathf.Clamp(gap, -maxOff, maxOff);
                float shift = gap - clampedGap;

                if (Mathf.Abs(shift) > 0.001f)
                {
                    for (int i = place; i < _model.Count; i++)
                    {
                        var cube = _model.GetCube(i);
                        cube.HorizontalOffset -= shift;
                        _model.SetCube(i, cube);
                    }
                }
            }

            if (!silent)
            {
                var messagesConfig = _messagesConfigProvider.Get();
                _messageService.ShowMessage(messagesConfig.TableReference, messagesConfig.MsgCubeRemoved);
            }
        }

        public void NotifyMiss()
        {
            var messagesConfig = _messagesConfigProvider.Get();
            _messageService.ShowMessage(messagesConfig.TableReference, messagesConfig.MsgCubeMissed);
        }

        public void NotifyTowerFull()
        {
            var messagesConfig = _messagesConfigProvider.Get();
            _messageService.ShowMessage(messagesConfig.TableReference, messagesConfig.MsgTowerFull);
        }
    }
}
