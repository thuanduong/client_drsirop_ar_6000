using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


public class LoadingPresenter : IDisposable
{
    private UILoading uiLoading = default;
    private CancellationTokenSource cts = default; 

    public async UniTask ShowLoadingAsync()
    {
      
        cts.SafeCancelAndDispose();
        cts = new CancellationTokenSource();
        uiLoading ??= await UILoader.Instantiate<UILoading>(UICanvas.UICanvasType.Loading).AttachExternalCancellation(cancellationToken: cts.Token);
        uiLoading.SetEntity(new UILoading.Entity());
        await uiLoading.In();
    }

    public async UniTask ShowWaiting(float percent)
    {
        cts.SafeCancelAndDispose();
        cts = new CancellationTokenSource();
        uiLoading ??= await UILoader.Instantiate<UILoading>(UICanvas.UICanvasType.Loading).AttachExternalCancellation(cancellationToken: cts.Token);
        uiLoading.SetEntity(new UILoading.Entity(percent));
        await uiLoading.In();
    }

    public void HideLoading()
    {
        cts.SafeCancelAndDispose();
        uiLoading?.Out().Forget();
    }

    public async UniTask HideLoadingAsync()
    {
        cts.SafeCancelAndDispose();
        await uiLoading.Out();
    }

    public void Dispose()
    {
        cts.SafeCancelAndDispose();
        UILoader.SafeRelease(ref uiLoading);
    }

    public async UniTask ActiveWaiting(bool f, float Duration = 0.5f)
    {
        if (f)
            await ShowWaiting(Duration);
        else
            await HideLoadingAsync();
    }
}