using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

namespace CubeGame
{
    public class GameConfigHolder : IGameConfig
    {
        private IGameConfig _inner;

        public bool IsLoaded => _inner != null;

        public void SetConfig(IGameConfig config)
        {
            _inner = config ?? throw new ArgumentNullException(nameof(config));
        }

        public int CubeCount => _inner.CubeCount;
        public IReadOnlyList<Sprite> CubeSprites => _inner.CubeSprites;
        public float CubeSizeFillPercent => _inner.CubeSizeFillPercent;
        public float MaxHorizontalOffsetPercent => _inner.MaxHorizontalOffsetPercent;
        public float DropTolerance => _inner.DropTolerance;
        public bool EnableSave => _inner.EnableSave;

        public LocalizedString MsgCubePlaced => _inner.MsgCubePlaced;
        public LocalizedString MsgCubeRemoved => _inner.MsgCubeRemoved;
        public LocalizedString MsgCubeMissed => _inner.MsgCubeMissed;
        public LocalizedString MsgTowerFull => _inner.MsgTowerFull;
    }
}
