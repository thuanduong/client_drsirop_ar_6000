using System;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;


public interface IPingService
{
    UniTaskVoid StartPingService();
    void StopPingService();
}

public class PingService : IDisposable, IPingService
{
    private readonly IDIContainer container;
    private CancellationTokenSource cts;

    public static PingService Instantiate(IDIContainer container)
    {
        return new PingService(container);
    }

    public async UniTaskVoid StartPingService()
    {
        cts.SafeCancelAndDispose();
        cts = new CancellationTokenSource();

        while (!cts.IsCancellationRequested)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(5), ignoreTimeScale: true, cancellationToken: cts.Token);
        }
    }

    public void StopPingService()
    {
        DisposeUtility.SafeDispose(ref cts);
    }

    private PingService(IDIContainer container)
    {
        this.container = container;
    }

    public void Dispose()
    {
        DisposeUtility.SafeDispose(ref cts);
    }
}
