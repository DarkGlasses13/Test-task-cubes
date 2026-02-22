using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace AssetProvider
{
    public interface IMultipleAssetProvider<T>
    {
        UniTask LoadAsync(CancellationToken cancellationToken = default);
        T Get(string key);
        T Get(int index);
        IReadOnlyCollection<T> Get();
        void Release();
    }
}