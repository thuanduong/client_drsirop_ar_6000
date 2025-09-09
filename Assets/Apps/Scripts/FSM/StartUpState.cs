using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class StartUpState : InjectedBHState
{
    private bool isNeedResetState = false;
    private StartUpStatePresenter startUpStateHandler;
    private CancellationTokenSource cts;

    
    public override void Enter()
    {
        OnEnterStateAsync().Forget();
    }

    private async UniTaskVoid OnEnterStateAsync()
    {
        cts.SafeCancelAndDispose();
        cts = new CancellationTokenSource();

        startUpStateHandler = new StartUpStatePresenter();
        startUpStateHandler.OnReboot += OnReboot;
        await startUpStateHandler.ShowClientInfoAsync();
        Container.Bind(startUpStateHandler);
        

        base.Enter();
    }

    public override void AddStates()
    {
        base.AddStates();
        this.AddState<InitialState>();
        this.AddState<DownloadAssetState>();
        this.SetInitialState<DownloadAssetState>();
    }

    private void OnReboot()
    {
        isNeedResetState = true;
        this.Machine.CurrentState.Exit();
    }

    public override void Exit()
    {
        try
        {
            base.Exit();
            cts.SafeCancelAndDispose();
            cts = default;

            startUpStateHandler.OnReboot -= OnReboot;
            Container.RemoveAndDisposeIfNeed<StartUpStatePresenter>();
        }
        finally
        {
            this.Machine.RemoveAllStates();
            if (isNeedResetState)
            {
                ((MonoFSMContainer)this.Machine).Reset();
                this.Machine.Initialize();
            }
            isNeedResetState = false;
        }
    }
}