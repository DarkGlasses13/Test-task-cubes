using UnityEngine;

namespace CubeGame
{
    public interface ICubeSpriteResolver
    {
        Sprite Resolve(string cubeId);
    }
}
