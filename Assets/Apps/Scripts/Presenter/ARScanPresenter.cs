using UnityEngine;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public class ARScanPresenter : IDisposable
{
    private readonly IDIContainer Container;
    private CancellationTokenSource cts;

    BackgroundPresenter background;
    BackgroundPresenter Background => background ??= Container.Inject<BackgroundPresenter>();

    UIARScan uiScan;

    public System.Action OnSpawn = ActionUtility.EmptyAction.Instance;
    public System.Action OnBack = ActionUtility.EmptyAction.Instance;

    public ARScanPresenter(IDIContainer container)
    {
        this.Container = container;
    }
    public void Dispose()
    {
        cts.SafeCancelAndDispose();
        UILoader.SafeRelease(ref uiScan);
    }

    public async UniTask ShowScan(CancellationToken cancellationToken)
    {
        try
        {
            uiScan ??= await UILoader.Instantiate<UIARScan>(UICanvas.UICanvasType.Default, token: cancellationToken);
            await UniTask.Delay(100);
            var entity = new UIARScan.Entity()
            {
                scanText = "Téléphone en mouvement",
                btnBack = new ButtonEntity(Scan_OnBtnBack),
                btnSpawn = new ButtonEntity(OnSpawn),
            };
            uiScan.SetEntity(entity);
            await uiScan.In().AttachExternalCancellation(cancellationToken);
        }
        catch { 
        }
    }

    public async UniTask HideScan(CancellationToken cancellationToken)
    {
        try
        {
            if (uiScan != null)
            {
                await uiScan.Out().AttachExternalCancellation(cancellationToken);
            }
        }
        catch { }
    }

    private void Scan_OnBtnBack()
    {
        OnBack.Invoke();
    }


}
