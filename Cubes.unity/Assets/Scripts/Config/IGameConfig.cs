using System.Collections.Generic;
using UnityEngine;

namespace CubeGame
{
    public interface IGameConfig
    {
        int CubeCount { get; }
        IReadOnlyList<Sprite> CubeSprites { get; }
        float CubeUISize { get; }
        float MaxHorizontalOffsetPercent { get; }
        float DropTolerance { get; }
        bool EnableSave { get; }
    }
}
