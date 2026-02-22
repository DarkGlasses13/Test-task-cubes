using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CubeGame;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AssetProvider
{
    public class AddressablesCubeConfigsProvider : ICubeConfigsProvider
    {
        private readonly string _label;
        private AsyncOperationHandle<IList<CubeConfigScriptableObject>> _handle;
        Dictionary<string, CubeConfigScriptableObject> _cache;

        public AddressablesCubeConfigsProvider(string label)
        {
            _label = label;
        }

        public async UniTask LoadAsync(CancellationToken cancellationToken = default)
        {
            if (_cache != null)
                return;

            _handle = Addressables.LoadAssetsAsync<CubeConfigScriptableObject>(_label, null);
            var result = await _handle.ToUniTask(cancellationToken: cancellationToken);
            _cache = new Dictionary<string, CubeConfigScriptableObject>(result.Count);
            
            foreach (var config in result)
                _cache.Add(config.Data.Id, config);
        }

        public CubeConfig Get(string id) => _cache[id].Data;

        public CubeConfig Get(int index) => _cache.Values.ElementAt(index).Data;

        public IReadOnlyCollection<CubeConfig> Get() => _cache.Values.Select(so => so.Data).ToList();

        public void Release()
        {
            if (_handle.IsValid())
                Addressables.Release(_handle);

            _cache = null;
        }
    }
}