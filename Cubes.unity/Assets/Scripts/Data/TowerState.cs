using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace CubeGame
{
    [Serializable]
    public class TowerState
    {
        public List<CubeInTowerData> Cubes = new List<CubeInTowerData>();
        [FormerlySerializedAs("TowerBaseX")] public float BaseX;
        [FormerlySerializedAs("TowerBaseY")] public float BaseY;
    }
}
