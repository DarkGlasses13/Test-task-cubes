using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

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

        LocalizedString MsgCubePlaced { get; }
        LocalizedString MsgCubeRemoved { get; }
        LocalizedString MsgCubeMissed { get; }
        LocalizedString MsgTowerFull { get; }
    }
}
