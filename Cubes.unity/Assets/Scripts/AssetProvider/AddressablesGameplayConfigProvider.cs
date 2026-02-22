using System.Threading;
using CubeGame;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AssetProvider
{
    public class AddressablesGameplayConfigProvider : IGameplayConfigProvider
    {
        private readonly string _address;
        private GameplayConfig _cache;
        private AsyncOperationHandle<GameplayConfigScriptableObject> _handle;

        public AddressablesGameplayConfigProvider(string address)
        {
            _address = address;
        }

        public async UniTask LoadAsync(CancellationToken cancellationToken = default)
        {
            if (_cache != null)
                return;

            _handle = Addressables.LoadAssetAsync<GameplayConfigScriptableObject>(_address);
            var scriptableObject = await _handle.ToUniTask(cancellationToken: cancellationToken);
            _cache = scriptableObject.Data;
        }

        public GameplayConfig Get() => _cache;

        public void Release()
        {
            if (_handle.IsValid())
                Addressables.Release(_handle);

            _cache = null;
        }
    }
}