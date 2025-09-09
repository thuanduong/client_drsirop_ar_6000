using Cysharp.Threading.Tasks;
using System.Threading;

public class LoadingState : InjectedBState
{
    private CancellationTokenSource cts;

    BackgroundPresenter backgroundPresenter;

    public override void Enter()
    {
        base.Enter();
        ShowLoadingThenChangeState().Forget();
    }

    

    private async UniTaskVoid ShowLoadingThenChangeState()
    {
        cts.SafeCancelAndDispose();
        cts = new CancellationTokenSource();
        var uiLoadingPresenter = this.Container.Inject<LoadingPresenter>();
        uiLoadingPresenter.ShowLoadingAsync().Forget();

        //var audioPresenter = this.Container.Inject<AudioPresenter>();
        //audioPresenter.ShowAudioAsync().Forget();

        await UniTask.Delay(1000).AttachExternalCancellation(cts.Token);
        this.Machine.ChangeState<LoginState>();
        uiLoadingPresenter.HideLoading();
    }

    public override void Exit()
    {
        base.Exit();
        cts.SafeCancelAndDispose();
        cts = default;

    }
}