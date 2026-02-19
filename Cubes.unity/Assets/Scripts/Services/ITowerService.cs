using UnityEngine;

namespace CubeGame
{
    public interface ITowerService
    {
        bool IsEmpty { get; }
        int CubeCount { get; }

        bool CanAddMore(float zoneHeight, float cubeSize);

        /// <summary>
        /// Place a new cube on the tower. Returns the created CubeData.
        /// dropOffsetX — horizontal offset from tower base where the user dropped the cube.
        /// </summary>
        CubeData PlaceCube(int colorIndex, float dropOffsetX);

        /// <summary>
        /// Remove a cube at the given tower index.
        /// Cubes above will shift down.
        /// </summary>
        void RemoveCube(int towerIndex, bool silent = false);

        void SetTowerBase(Vector2 localPosition);
        void NotifyMiss();
        void NotifyTowerFull();
    }
}
