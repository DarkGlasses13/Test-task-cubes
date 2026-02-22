using System.Threading;
using CubeGame;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AssetProvider
{
    public class AddressablesMessagesConfigProvider : IMessagesConfigProvider
    {
        private readonly string _address;
        private MessagesConfig _cache;
        private AsyncOperationHandle<MessagesConfigScriptableObject> _handle;

        public AddressablesMessagesConfigProvider(string address)
        {
            _address = address;
        }

        public async UniTask LoadAsync(CancellationToken cancellationToken = default)
        {
            if (_cache != null)
                return;

            _handle = Addressables.LoadAssetAsync<MessagesConfigScriptableObject>(_address);
            var scriptableObject = await _handle.ToUniTask(cancellationToken: cancellationToken);
            
            _cache = new MessagesConfig()
            {
                TableReference = scriptableObject.Data.TableReference,
                MsgCubePlaced = scriptableObject.Data.MsgCubePlaced,
                MsgCubeRemoved = scriptableObject.Data.MsgCubeRemoved,
                MsgCubeMissed = scriptableObject.Data.MsgCubeMissed,
                MsgTowerFull = scriptableObject.Data.MsgTowerFull
            };
        }

        public MessagesConfig Get() => _cache;

        public void Release()
        {
            if (_handle.IsValid())
                Addressables.Release(_handle);

            _cache = null;
        }
    }
}