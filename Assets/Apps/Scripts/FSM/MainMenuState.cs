using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using Core;

public class MainMenuState : InjectedBState
{
    private LoadingPresenter uiLoadingPresenter;
    private LoadingPresenter UiLoadingPresenter => uiLoadingPresenter ??= this.Container.Inject<LoadingPresenter>();

    private UIMainMenuPresenter uiMainMenuPresenter;

    private BackgroundPresenter backgroundPresenter;
    private BackgroundPresenter BackgroundPresenter => backgroundPresenter ??= Container.Inject<BackgroundPresenter>();


    private AudioPresenter audioPresenter;
    private AudioPresenter AudioPresenter => audioPresenter ??= Container.Inject<AudioPresenter>();

    private CancellationTokenSource cts;

    public override void Enter()
    {
        base.Enter();
        cts.SafeCancelAndDispose();
        cts = new CancellationTokenSource();
        
        uiMainMenuPresenter ??= new UIMainMenuPresenter(this.Container);

        SubcribeEvents();

        _ = OnStartState();
        //_ = ShowBackGroundAsync();

    }

    public override void Exit()
    {
        base.Exit();
        UnSubcribeEvents();

        uiMainMenuPresenter.Dispose();
        uiMainMenuPresenter = default;

        uiLoadingPresenter = default;

        cts.SafeCancelAndDispose();
        cts = default;
    }

    private async UniTask ShowBackGroundAsync()
    {
        //await BackgroundPresenter.LoadBackground("Sprite/UI/BackgroudGame/BG_MainMenu", FromAsset: true);
        await BackgroundPresenter.LoadBackgroundColor(ImageHelper.HexToColor("F1EDE0") ,cts.Token);
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
        uiMainMenuPresenter.ToChat += ToChatState;
        uiMainMenuPresenter.ToCall += ToCallState;
    }

    private void UnSubcribeEvents()
    {
        uiMainMenuPresenter.ToChat -= ToChatState;
        uiMainMenuPresenter.ToCall -= ToCallState;
    }


    private async UniTask OnStartState()
    {
        try
        {
            //AudioPresenter.PlayMusic("MainTheme");
            await BackgroundPresenter.LoadBackgroundColor(ImageHelper.HexToColor("F1EDE0"), cts.Token);
            await ToMainMenu();
            await UniTask.Delay(500);
            await BackgroundPresenter.ChangeToColor(ImageHelper.HexToColor("A6DEAC"), 0.25f, cts.Token);
            await UniTask.Delay(1000);




        }
        catch (Exception ex) { Debug.LogError($"Err: {ex.Message}"); }
    }

    private async UniTask ToMainMenu()
    {
        try
        {
            await uiMainMenuPresenter.ShowMainMenuAsync().AttachExternalCancellation(cts.Token);
        }
        catch (Exception ex) { Debug.LogError($"Err: {ex.Message}"); }
    }

    private void ToChatState()
    {
        cts.SafeCancelAndDispose();
        cts = new CancellationTokenSource();
        _ = ToChatStateAsync();
    }

    private async UniTask ToChatStateAsync()
    {
        await UniTask.WhenAll(uiMainMenuPresenter.HideAsync(), UiLoadingPresenter.ShowLoadingAsync()).AttachExternalCancellation(cts.Token);
        this.Machine.ChangeState<ARChatState>();
    }

    private void ToCallState()
    {
        cts.SafeCancelAndDispose();
        cts = new CancellationTokenSource();
        _ = ToCallStateAsync();
    }

    private async UniTask ToCallStateAsync()
    {
        await UniTask.WhenAll(uiMainMenuPresenter.HideAsync(), UiLoadingPresenter.ShowLoadingAsync()).AttachExternalCancellation(cts.Token);
        this.Machine.ChangeState<ARCallState>();
    }

}
