using System.Collections;
using Cysharp.Threading.Tasks;
using System.Threading;

public class LoadingMainMenuState : InjectedBState
{
    private CancellationTokenSource cts;
    BackgroundPresenter backgroundPresenter;
    private UILoadingMainMenu uiLoadingMainMenuState;

    public override void Enter()
    {
        base.Enter();
        ShowLoadingThenChangeState().Forget();
    }

    private async UniTaskVoid ShowLoadingThenChangeState()
    {
        cts.SafeCancelAndDispose();
        cts = new CancellationTokenSource();
        var uiLoadingPresenter = this.Container.Inject<LoadingMainMenuPresenter>();
        backgroundPresenter = Container.Inject<BackgroundPresenter>();
        //await backgroundPresenter.LoadBackground("Sprite/UI/HomeUI/BG_Home");
        uiLoadingPresenter.ShowLoadingAsync().Forget();
        await UniTask.WhenAll(LoadData(), UniTask.Delay(4000)).AttachExternalCancellation(cts.Token);
        uiLoadingPresenter.HideLoading();
        this.Machine.ChangeState<MainMenuState>();
       
    }

    private async UniTask LoadData()
    {
        await UniTask.CompletedTask;
    }
   
    public override void Exit()
    {
        base.Exit();
        cts.SafeCancelAndDispose();
        cts = default;

    }
}