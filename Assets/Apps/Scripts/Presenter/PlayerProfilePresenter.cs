using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using UnityEngine;
using Cysharp.Threading.Tasks;



public class PlayerProfilePresenter : IDisposable
{
    private IDIContainer Container { get; }
    private CancellationTokenSource cts;

    private LoadingPresenter loadingPresenter;
    private LoadingPresenter LoadingPresenter => loadingPresenter ??= Container.Inject<LoadingPresenter>();

    public PlayerProfilePresenter(IDIContainer container)
    {
        this.Container = container;
    }

    public void Dispose()
    {
        cts.SafeCancelAndDispose();
        cts = default;
    }

    public async UniTask FetchData()
    {
        cts.SafeCancelAndDispose();
        cts = new CancellationTokenSource();

        await UniTask.CompletedTask;
    }


}
