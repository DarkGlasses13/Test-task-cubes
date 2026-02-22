using UnityEngine;

namespace CubeGame
{
    [CreateAssetMenu(fileName = "Gameplay config", menuName = "_Project/Gameplay config")]
    public class GameplayConfigScriptableObject : ScriptableObject
    {
        public GameplayConfig Data;
    }
}