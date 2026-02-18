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
        /// colorIndex is the index into the config's sprite array.
        /// </summary>
        CubeData PlaceCube(int colorIndex);

        /// <summary>
        /// Remove a cube at the given tower index.
        /// Cubes above will shift down.
        /// </summary>
        void RemoveCube(int towerIndex);

        void SetTowerBase(Vector2 localPosition);
        void NotifyMiss();
        void NotifyTowerFull();
    }
}
