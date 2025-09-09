using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Core;

public class InitialState : InjectedBHState, IDisposable
{
    private UIHeaderPresenter uiHeaderPresenter;
    private StartUpStatePresenter StartUpStatePresenter => Container.Inject<StartUpStatePresenter>();
    
   
    public override void Enter()
    {
        OnEnterStateAsync().Forget();
    }

    private async UniTaskVoid OnEnterStateAsync()
    {
        //Presenter
        this.Container.Bind(new AudioPresenter(Container));
        this.Container.Bind(new LoadingPresenter());
        this.Container.Bind(new LoadingMainMenuPresenter());
        this.Container.Bind(new BackgroundPresenter(Container));
        

        uiHeaderPresenter = new UIHeaderPresenter(Container);
        this.Container.Bind(uiHeaderPresenter);
        this.Container.Bind(new UIBottomMenuPresenter(Container));


        //Service
        this.Container.Bind(PingService.Instantiate(Container));
        this.Container.Bind(LoginService.Instantiate(Container));
        this.Container.Bind(UserService.Instantiate(Container));
        this.Container.Bind(AIConversationService.Instantiate(Container));
        //API Server
        await ConfigServer();
        
        

        //Data
        UserData.Instance.RegistModelData<UserProfileModel>();
        UserData.Instance.Insert(new UserProfileModel());

        uiHeaderPresenter.OnLogOut += OnLogOut;

        initTestData();

        base.Enter();
        await UniTask.CompletedTask;
    }

    private void OnLogOut()
    {
        var audio = Container.Inject<AudioPresenter>();
        audio.StopMusic();
        StartUpStatePresenter.Reboot();
        Debug.Log("LOG OUT");
    }

    public override void AddStates()
    {
        base.AddStates();
        AddState<LoadingState>();
        AddState<LoginState>();
        AddState<LoadingMainMenuState>();
        AddState<MainMenuState>();

        AddState<ARCallState>();
        AddState<ARChatState>();

        SetInitialState<LoadingState>();
    }

    public override void Exit()
    {
        base.Exit();
        Dispose();
    }

    public void Dispose()
    {
        uiHeaderPresenter.OnLogOut -= OnLogOut;
        
        this.Container.RemoveAndDisposeIfNeed<PingService>();
        this.Container.RemoveAndDisposeIfNeed<LoginService>();
        this.Container.RemoveAndDisposeIfNeed<UserService>();
        this.Container.RemoveAndDisposeIfNeed<AIConversationService>();

        this.Container.RemoveAndDisposeIfNeed<AudioPresenter>();
        this.Container.RemoveAndDisposeIfNeed<UIHeaderPresenter>();
        this.Container.RemoveAndDisposeIfNeed<UIBottomMenuPresenter>();
        this.Container.RemoveAndDisposeIfNeed<BackgroundPresenter>();
        this.Container.RemoveAndDisposeIfNeed<LoadingPresenter>();
        this.Container.RemoveAndDisposeIfNeed<LoadingMainMenuPresenter>();

        UserData.Instance.Drop<UserProfileModel>();

        UserData.Instance.ClearData();

        MasterData.Instance.ClearData();

        uiHeaderPresenter = default;
    }


    private void initTestData()
    {

        Debug.Log("INIT TEST");
        UserService.Instance.Load();

    }

    private async UniTask ConfigServer()
    {
        var rq = Resources.LoadAsync<ServerDefineObject>("ServerDefine/ServerDefineObject");
        await rq;
        if (rq.asset != default)
        {
            ServerDefineObject ss = rq.asset as ServerDefineObject;
            ServerDefinePath.SetAPI(ss.API);
            ServerDefinePath.SetWS(ss.WS);
            Debug.Log(ss.API);
        }

    }
}