using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;

namespace AssetProvider
{
    public class AddressablesCubeSpritesProvider : ICubeSpritesProvider
    {
        private readonly string _address;
        private Dictionary<string, Sprite> _cache;
        private List<Sprite> _ordered;
        private AsyncOperationHandle<SpriteAtlas> _handle;

        public AddressablesCubeSpritesProvider(string address)
        {
            _address = address;
        }

        public async UniTask LoadAsync(CancellationToken cancellationToken = default)
        {
            if (_cache != null)
                return;

            _handle = Addressables.LoadAssetAsync<SpriteAtlas>(_address);
            var atlas = await _handle.ToUniTask(cancellationToken: cancellationToken);
            var sprites = new Sprite[atlas.spriteCount];
            atlas.GetSprites(sprites);

            _ordered = sprites
                .OrderBy(s => ExtractIndex(s.name))
                .ToList();

            _cache = _ordered.ToDictionary(s => s.name, s => s);
        }

        public Sprite Get(string key) => _cache[key];

        public Sprite Get(int index) => _ordered[index];

        public IReadOnlyCollection<Sprite> Get() => _ordered;

        public void Release()
        {
            if (_handle.IsValid())
                Addressables.Release(_handle);

            _cache = null;
            _ordered = null;
        }
        
        private int ExtractIndex(string name)
        {
            var digits = new string(name.Where(char.IsDigit).ToArray());
            return int.Parse(digits);
        }
    }
}