using System.Threading;
using Cysharp.Threading.Tasks;

namespace AssetProvider
{
    public interface ISingleAssetProvider<T>
    {
        UniTask LoadAsync(CancellationToken cancellationToken = default);
        T Get();
        void Release();
    }
}