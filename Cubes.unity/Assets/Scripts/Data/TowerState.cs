using System;
using System.Collections.Generic;

namespace CubeGame
{
    [Serializable]
    public class TowerState
    {
        public List<CubeInTowerData> Cubes = new List<CubeInTowerData>();
        public float BaseX;
        public float BaseY;
    }
}
