using UnityEngine;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public class ARCallPresenter : IDisposable
{
    private readonly IDIContainer Container;
    private CancellationTokenSource cts;

    private AIConversationService callService;
    private AIConversationService CallService => callService ??= this.Container.Inject<AIConversationService>();


    UIARAsk uiAsk;

    public System.Action OnBack = ActionUtility.EmptyAction.Instance;

    public ARCallPresenter(IDIContainer container)
    {
        this.Container = container;
    }

    public void Dispose()
    {
        cts.SafeCancelAndDispose();
        UILoader.SafeRelease(ref uiAsk);
    }

    public async UniTask ShowCall(CancellationToken cancellationToken)
    {
        try
        {
            uiAsk ??= await UILoader.Instantiate<UIARAsk>(UICanvas.UICanvasType.Default, token: cancellationToken);
            await UniTask.Delay(100);
            var entity = new UIARAsk.Entity()
            {
                btnBack = new ButtonEntity(Ask_OnBtnBack, isDisable: true),
                btnEndCall = new ButtonEntity(Ask_OnEndCall),
            };
            uiAsk.SetEntity(entity);
            await UniTask.Delay(100);
            await uiAsk.In().AttachExternalCancellation(cancellationToken);
        }
        catch
        {
        }
    }

    public async UniTask HideCall(CancellationToken cancellationToken)
    {
        try
        {
            if (uiAsk != null)
            {
                await uiAsk.Out().AttachExternalCancellation(cancellationToken);
            }
        }
        catch { }
    }

    private void Ask_OnBtnBack()
    {
        CallService.RequestEndCall();
        OnBack.Invoke();
    }

    private void Ask_OnEndCall()
    {
        CallService.RequestEndCall();
        OnBack.Invoke();
    }

}
