using System;
using System.Collections.Generic;

namespace CubeGame
{
    [Serializable]
    public class TowerState
    {
        public List<TowerCubeData> Cubes = new();
        public float BaseX;
        public float BaseY;
    }
}
