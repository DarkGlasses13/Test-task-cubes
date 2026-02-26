using AssetProvider;
using UnityEngine;

namespace CubeGame
{
    public class CubeSpriteResolver : ICubeSpriteResolver
    {
        private readonly ICubeConfigsProvider _cubeConfigsProvider;
        private readonly ICubeSpritesProvider _cubeSpritesProvider;

        public CubeSpriteResolver(ICubeConfigsProvider cubeConfigsProvider, ICubeSpritesProvider cubeSpritesProvider)
        {
            _cubeConfigsProvider = cubeConfigsProvider;
            _cubeSpritesProvider = cubeSpritesProvider;
        }

        public Sprite Resolve(string cubeId)
        {
            var cubeConfig = _cubeConfigsProvider.Get(cubeId);
            if (int.TryParse(cubeConfig.SpriteKey, out var spriteIndex))
                return _cubeSpritesProvider.Get(spriteIndex);

            return null;
        }
    }
}
