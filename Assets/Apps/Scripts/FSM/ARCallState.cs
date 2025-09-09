using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using Core;

public class ARCallState : InjectedBState
{
    private LoadingPresenter uiLoadingPresenter;
    private LoadingPresenter UiLoadingPresenter => uiLoadingPresenter ??= this.Container.Inject<LoadingPresenter>();


    private BackgroundPresenter backgroundPresenter;
    private BackgroundPresenter BackgroundPresenter => backgroundPresenter ??= Container.Inject<BackgroundPresenter>();


    private AudioPresenter audioPresenter;
    private AudioPresenter AudioPresenter => audioPresenter ??= Container.Inject<AudioPresenter>();


    private AIConversationService callService;
    private AIConversationService CallService => callService ??= this.Container.Inject<AIConversationService>();

    private CancellationTokenSource cts;

    private ARScanPresenter arScan;
    private ARRoomPresenter arRoom;
    private ARCallPresenter arCall;

    private CharacterBasicComponent character;

    public override void Enter()
    {
        base.Enter();
        cts.SafeCancelAndDispose();
        cts = new CancellationTokenSource();

        arScan ??= new ARScanPresenter(this.Container);
        arRoom ??= new ARRoomPresenter(this.Container);
        arCall ??= new ARCallPresenter(this.Container);

        SubcribeEvents();

        _ = OnStartState();

    }

    public override void Exit()
    {
        base.Exit();
        UnSubcribeEvents();
        
        cts.SafeCancelAndDispose();
        cts = default;

        arScan.Dispose();
        arScan = default;

        arCall.Dispose();
        arCall = default;

        arRoom.Dispose();
        arRoom = default;

        
    }


    private void SubcribeEvents()
    {
        if (arScan != null)
        {
            arScan.OnSpawn += SpawnCharacter;
            arScan.OnBack += ARScan_OnBack;
        }
        if (arCall != null)
        {
            arCall.OnBack += ARCall_OnBack;
        }
        if (CallService != null)
        {
            CallService.OnCallStarted += ARCall_OnCallStart;
            CallService.OnUpdate += ARCall_OnUpdate;
            CallService.OnAgentStartTalking += ARCall_OnAgentStartTalking;
            CallService.OnAgentStopTalking += ARCall_OnAgentStopTalking;
        }
    }

    private void UnSubcribeEvents()
    {
        if (arScan != null)
        {
            arScan.OnSpawn -= SpawnCharacter;
            arScan.OnBack -= ARScan_OnBack;
        }

        if (arCall != null)
        {
            arCall.OnBack -= ARCall_OnBack;
        }

        if (callService != null)
        {
            callService.OnCallStarted -= ARCall_OnCallStart;
            callService.OnUpdate -= ARCall_OnUpdate;
            callService.OnAgentStartTalking -= ARCall_OnAgentStartTalking;
            callService.OnAgentStopTalking -= ARCall_OnAgentStopTalking;
        }
    }


    private async UniTask OnStartState()
    {
        try
        {
            await arRoom.LoadScene(cts.Token);
            await arScan.ShowScan(cts.Token);
            await BackgroundPresenter.HideBackgroundColor();
            await UiLoadingPresenter.HideLoadingAsync();

        }
        catch (Exception ex) { Debug.LogError($"Err: {ex.Message}"); }
    }

    private void SpawnCharacter()
    {
        cts.SafeCancelAndDispose();
        cts = new CancellationTokenSource();
        _ = spawnCharacter();
    }

    private async UniTask spawnCharacter()
    {
        try
        {
            var go = GameObject.FindAnyObjectByType<InstantPlacementSpawner>();
            if (go != null)
            {
                var mm = go.GetComponent<InstantPlacementSpawner>();
                await mm.Spawn();
            }
            await arScan.HideScan(cts.Token);

            await UniTask.Delay(100, cancellationToken: cts.Token);

            await CallService.Create(cts.Token);

            var characterGo = GameObject.FindAnyObjectByType<CharacterBasicComponent>();
            if (characterGo != null)
            {
                character = characterGo.GetComponent<CharacterBasicComponent>();
                if (character != null)
                {
                    character.UIRequestCall.SetEntity(new UIRequestCallBubble.Entity()
                    {
                        btnStartCall = new ButtonEntity(RequestCall),
                    });
                    character.UIMessageBubble.SetEntity(new UICharacterMessageBubble.Entity()
                    {
                        message = "",
                    });
                }
            }
            await UniTask.Delay(1000, cancellationToken: cts.Token);
            await character.UIRequestCall.In();
        }
        catch { }
    }

    private void RequestCall()
    {
        _ = requestCallAsync(cts.Token);
    }

    private async UniTask requestCallAsync(CancellationToken cancellationToken)
    {
        try
        {
            await character.UIRequestCall.Out();
            var lm = UserData.Instance.GetOne<UserProfileModel>();
            var kq = await CallService.RequestCall(lm.AgentId, lm.UserId, cancellationToken);
            if (kq)
            {
                //AudioPresenter.StopMusic();
                await arCall.ShowCall(cancellationToken);
                await character.UIMessageBubble.In();
            }
            else
            {
                await UniTask.Delay(500, cancellationToken: cts.Token);
                await character.UIRequestCall.In();
            }

        }
        catch { }
    }

    private void ARCall_OnBack()
    {
        cts.SafeCancelAndDispose();
        cts = new CancellationTokenSource();
        _ = ARCall_OnBackASync();
    }

    private async UniTask ARCall_OnBackASync()
    {
        try
        {
            await character.UIMessageBubble.Out();
            await arCall.HideCall(cts.Token);
            await UniTask.Delay(500, cancellationToken: cts.Token);
            await character.UIRequestCall.In().AttachExternalCancellation(cts.Token);

            
        }
        catch(Exception ex) { Debug.LogError($"Err: {ex.Message}"); }
    }

    private void ARScan_OnBack()
    {
        cts.SafeCancelAndDispose();
        cts = new CancellationTokenSource();
        _ = ARScan_OnBackASync();
    }
    private async UniTask ARScan_OnBackASync()
    {
        try
        {
            await UiLoadingPresenter.ShowLoadingAsync();
            
            await arScan.HideScan(cts.Token);
            await arRoom.UnloadScene(cts.Token);

            await UniTask.Delay(500, cancellationToken: cts.Token);
            await UiLoadingPresenter.HideLoadingAsync();

            this.Machine.ChangeState<MainMenuState>();
        }
        catch (Exception ex) { Debug.LogError($"Err: {ex.Message}"); }
    }

    private void ARCall_OnCallStart()
    {
        CallService.PublishMicrophone(autoRecord: true);
    }

    private void ARCall_OnUpdate(string role, int index, string message)
    {
        Debug.Log($"Mesaaage {message}");
        if (character != null)
        {
            bool isPlayer = role == "user" ? true : false;
            if (!isPlayer)
            {
                CallService.WaitAgentTalk();
                character.UIMessageBubble.SetText(message);
            }
        }
    }


    private void ARCall_OnAgentStartTalking()
    {
        //CallService.StopTalk();
    }

    private void ARCall_OnAgentStopTalking()
    {
        //CallService.StartTalk();
    }
}
