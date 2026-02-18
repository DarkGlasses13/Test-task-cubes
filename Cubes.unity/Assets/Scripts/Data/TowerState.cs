using System;
using System.Collections.Generic;

namespace CubeGame
{
    [Serializable]
    public class TowerState
    {
        public List<CubeData> Cubes = new List<CubeData>();
        public float TowerBaseX;
        public float TowerBaseY;
    }
}
