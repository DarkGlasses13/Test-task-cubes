using System;

namespace CubeGame
{
    [Serializable]
    public class GameplayConfig
    {
        public string[] AvailableCubes;
        public float CubeSizeFillPercent;
        public float MaxHorizontalOffsetPercent;
        public float DropTolerance;
        public bool EnableSave;
    }
}