public class DownloadAssetState : InjectedBState
{

    public override void Enter()
    {
        base.Enter();
        OnDownLoadDone();
    }

    private void OnDownLoadDone()
    {
        this.Machine.ChangeState<InitialState>();
    }

    public override void Exit()
    {
        base.Exit();
    }
}
