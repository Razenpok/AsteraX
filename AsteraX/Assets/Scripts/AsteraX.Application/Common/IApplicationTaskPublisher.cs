using System.Threading;
using Cysharp.Threading.Tasks;

namespace AsteraX.Application.Common
{
    public interface IApplicationTaskPublisher
    {
        void Publish<T>(T task) where T : IApplicationTask;
        UniTask PublishAsync<T>(T task, CancellationToken ct = default) where T : IAsyncApplicationTask;
    }
}