using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace AsteraX.Application.Common
{
    public interface IApplicationTaskSubscriber
    {
        IDisposable Subscribe<T>(Action<T> handler) where T : IApplicationTask;
        IDisposable Subscribe<T>(Func<T, CancellationToken, UniTask> handler, CancellationToken ct = default)
            where T : IAsyncApplicationTask;
    }
}