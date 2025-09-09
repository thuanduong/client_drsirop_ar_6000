using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using Core;

public class ARChatState : InjectedBState
{

    private LoadingPresenter uiLoadingPresenter;
    private LoadingPresenter UiLoadingPresenter => uiLoadingPresenter ??= this.Container.Inject<LoadingPresenter>();


    private BackgroundPresenter backgroundPresenter;
    private BackgroundPresenter BackgroundPresenter => backgroundPresenter ??= Container.Inject<BackgroundPresenter>();


    private AudioPresenter audioPresenter;
    private AudioPresenter AudioPresenter => audioPresenter ??= Container.Inject<AudioPresenter>();


    


    private CancellationTokenSource cts;

    private ARChatPresenter arChat;

    public override void Enter()
    {
        base.Enter();
        cts.SafeCancelAndDispose();
        cts = new CancellationTokenSource();

        arChat ??= new ARChatPresenter(this.Container);

        SubcribeEvents();

        _ = OnStartState();

    }

    public override void Exit()
    {
        base.Exit();
        UnSubcribeEvents();

        arChat.Dispose();
        arChat = default;

        uiLoadingPresenter = default;

        cts.SafeCancelAndDispose();
        cts = default;
    }

    private async UniTask ShowBackGroundAsync()
    {
        //await BackgroundPresenter.LoadBackground("Sprite/UI/BackgroudGame/BG_MainMenu", FromAsset: true);
        await BackgroundPresenter.LoadBackgroundColor(ImageHelper.HexToColor("F1EDE0"), cts.Token);
        UiLoadingPresenter.HideLoading();
    }

    private async UniTask HideBackgroundAsync()
    {
        //await BackgroundPresenter.HideBackground();
        await BackgroundPresenter.HideBackgroundColor();
        UiLoadingPresenter.HideLoading();
    }

    private void SubcribeEvents()
    {
        if (arChat != null)
        {
            arChat.OnBack += ARChat_OnBack;
        }
    }

    private void UnSubcribeEvents()
    {
        if (arChat != null)
        {
            arChat.OnBack -= ARChat_OnBack;
        }
    }


    private async UniTask OnStartState()
    {
        try
        {
            await requestChatAsync(cts.Token);
            await BackgroundPresenter.HideBackgroundColor();
            await UiLoadingPresenter.HideLoadingAsync();
        }
        catch (Exception ex) { Debug.LogError($"Err: {ex.Message}"); }
    }


    private async UniTask requestChatAsync(CancellationToken cancellationToken)
    {
        try
        {
            var lm = UserData.Instance.GetOne<UserProfileModel>();
            var kq = await arChat.Connect(cancellationToken);

            //kq = true;// await CallService.RequestChat(lm.AgentId, lm.UserId, cancellationToken);
            if (kq)
            {
                //AudioPresenter.StopMusic();
                await arChat.Show(cancellationToken);
            }
            else
            {
                await UniTask.Delay(500, cancellationToken: cts.Token);
                await ARChat_OnBackASync();
            }

        }
        catch { }
    }

    private void ARChat_OnBack()
    {
        cts.SafeCancelAndDispose();
        cts = new CancellationTokenSource();
        _ = ARChat_OnBackASync();
    }

    private async UniTask ARChat_OnBackASync()
    {
        try
        {
            await arChat.Hide(cts.Token);
            await UniTask.Delay(500, cancellationToken: cts.Token);
            this.Machine.ChangeState<MainMenuState>();
        }
        catch { }
    }


}
