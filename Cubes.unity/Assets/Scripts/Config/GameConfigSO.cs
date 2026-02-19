using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

namespace CubeGame
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "CubeGame/GameConfig")]
    public class GameConfigSO : ScriptableObject, IGameConfig
    {
        [Header("Cube Sprites (one per color, order matters)")]
        [SerializeField] private Sprite[] _cubeSprites;

        [Header("UI Settings")]
        [Tooltip("Cube size as fraction of scroll panel height (0.85 = 85%)")]
        [Range(0.5f, 1f)]
        [SerializeField] private float _cubeSizeFillPercent = 0.85f;

        [Header("Tower Rules")]
        [Tooltip("Max horizontal offset as fraction of cube size (0.5 = 50%)")]
        [SerializeField] private float _maxHorizontalOffsetPercent = 0.5f;

        [Tooltip("Drop detection tolerance as multiplier of cube size")]
        [SerializeField] private float _dropTolerance = 1.5f;

        [Header("Save")]
        [SerializeField] private bool _enableSave = true;

        [Header("Messages")]
        [SerializeField] private LocalizedString _msgCubePlaced;
        [SerializeField] private LocalizedString _msgCubeRemoved;
        [SerializeField] private LocalizedString _msgCubeMissed;
        [SerializeField] private LocalizedString _msgTowerFull;

        public int CubeCount => _cubeSprites != null ? _cubeSprites.Length : 0;
        public IReadOnlyList<Sprite> CubeSprites => _cubeSprites;
        public float CubeSizeFillPercent => _cubeSizeFillPercent;
        public float MaxHorizontalOffsetPercent => _maxHorizontalOffsetPercent;
        public float DropTolerance => _dropTolerance;
        public bool EnableSave => _enableSave;

        public LocalizedString MsgCubePlaced => _msgCubePlaced;
        public LocalizedString MsgCubeRemoved => _msgCubeRemoved;
        public LocalizedString MsgCubeMissed => _msgCubeMissed;
        public LocalizedString MsgTowerFull => _msgTowerFull;
    }
}
