using System;

namespace CubeGame
{
    [Serializable]
    public struct CubeData
    {
        public int Id;
        public int ColorIndex;
        public float HorizontalOffset;

        public CubeData(int id, int colorIndex, float horizontalOffset)
        {
            Id = id;
            ColorIndex = colorIndex;
            HorizontalOffset = horizontalOffset;
        }
    }
}
