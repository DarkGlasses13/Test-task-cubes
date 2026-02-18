using System.Collections.Generic;
using UnityEngine;

namespace CubeGame
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "CubeGame/GameConfig")]
    public class GameConfigSO : ScriptableObject, IGameConfig
    {
        [Header("Cube Sprites (one per color, order matters)")]
        [SerializeField] private Sprite[] _cubeSprites;

        [Header("UI Settings")]
        [SerializeField] private float _cubeUISize = 120f;

        [Header("Tower Rules")]
        [Tooltip("Max horizontal offset as fraction of cube size (0.5 = 50%)")]
        [SerializeField] private float _maxHorizontalOffsetPercent = 0.5f;

        [Tooltip("Drop detection tolerance as multiplier of cube size")]
        [SerializeField] private float _dropTolerance = 1.5f;

        [Header("Save")]
        [SerializeField] private bool _enableSave = true;

        public int CubeCount => _cubeSprites != null ? _cubeSprites.Length : 0;
        public IReadOnlyList<Sprite> CubeSprites => _cubeSprites;
        public float CubeUISize => _cubeUISize;
        public float MaxHorizontalOffsetPercent => _maxHorizontalOffsetPercent;
        public float DropTolerance => _dropTolerance;
        public bool EnableSave => _enableSave;
    }
}
