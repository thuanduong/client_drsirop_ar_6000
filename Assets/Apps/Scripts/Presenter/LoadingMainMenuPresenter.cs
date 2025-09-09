using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class LoadingMainMenuPresenter : IDisposable
{
    private UILoadingMainMenu uiLoading = default;
    private CancellationTokenSource cts = default;

    public async UniTask ShowLoadingAsync()
    {
        cts.SafeCancelAndDispose();
        cts = new CancellationTokenSource();
        uiLoading ??= await UILoader.Instantiate<UILoadingMainMenu>(UICanvas.UICanvasType.Loading).AttachExternalCancellation(cancellationToken: cts.Token);
        uiLoading.SetEntity(new UILoadingMainMenu.Entity());
        await uiLoading.In();
    }

    public void HideLoading()
    {
        cts.SafeCancelAndDispose();
        uiLoading?.Out().Forget();
    }

    public void Dispose()
    {
        cts.SafeCancelAndDispose();
        UILoader.SafeRelease(ref uiLoading);
    }
}
