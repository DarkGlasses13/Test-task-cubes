using UnityEngine;

namespace CubeGame
{
    [CreateAssetMenu(fileName = "Messages config", menuName = "_Project/Messages config")]
    public class MessagesConfigScriptableObject : ScriptableObject
    {
        public MessagesConfig Data;
    }
}
