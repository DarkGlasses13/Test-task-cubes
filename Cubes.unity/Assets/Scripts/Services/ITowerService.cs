using System.Collections.Generic;
using UnityEngine;

namespace CubeGame
{
    public interface ITowerService
    {
        bool IsEmpty { get; }
        int CubeCount { get; }
        IReadOnlyList<CubeInTowerData> Cubes { get; }
        CubeInTowerData TopCube { get; }
        Vector2 Base { get; }

        bool CanAddMore(float zoneHeight, float cubeSize);
        CubeInTowerData GetCube(int place);
        void SetBase(Vector2 localPosition);
        CubeInTowerData PlaceCube(string id, float dropOffsetX);
        void RemoveCube(int place, bool silent = false);
        void NotifyMiss();
        void NotifyTowerFull();
    }
}
