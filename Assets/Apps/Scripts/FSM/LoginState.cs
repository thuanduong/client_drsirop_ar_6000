using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class LoginState : InjectedBState
{
    
    LoginStatePresenter loginPresenter;
    BackgroundPresenter backgroundPresenter;
    
    

    public override void Enter()
    {
        base.Enter();
        OnStateEnterAsync().Forget();
      
    }

    private async UniTask OnStateEnterAsync()
    {
        loginPresenter = new LoginStatePresenter(Container);
        backgroundPresenter = Container.Inject<BackgroundPresenter>();
        //await backgroundPresenter.LoadBackground("Image/bg_login_img");
        //await loginPresenter.ConnectAndLoginAsync();
        this.Machine.ChangeState<LoadingMainMenuState>();
    }

   
    public override void Exit()
    {
        base.Exit();
        loginPresenter.Dispose();
        loginPresenter = default;
    }
}
