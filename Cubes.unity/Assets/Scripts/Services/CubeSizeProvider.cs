using UnityEngine;

namespace CubeGame
{
    public class CubeSizeProvider
    {
        public float Size { get; private set; }

        public void Initialize(RectTransform holeRectTransform, float sizePercent)
        {
            Size = holeRectTransform.rect.height * sizePercent;
        }
    }
}
