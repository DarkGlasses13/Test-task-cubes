using UnityEngine;

namespace CubeGame
{
    [CreateAssetMenu(fileName = "Cube config", menuName = "_Project/Cube config")]
    public class CubeConfigScriptableObject : ScriptableObject
    {
        [field: SerializeField] public CubeConfig Data { get; private set; }
    }
}