using System;

namespace CubeGame
{
    [Serializable]
    public class MessagesConfig
    {
        public string TableReference;
        public string MsgCubePlaced;
        public string MsgCubeRemoved;
        public string MsgCubeMissed;
        public string MsgTowerFull;
    }
}