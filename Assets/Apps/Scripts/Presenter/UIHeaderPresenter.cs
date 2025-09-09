using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using UnityEngine;
using Cysharp.Threading.Tasks;
using Core;
using UniRx;

public class UIHeaderPresenter : IDisposable
{
    private UIHeader uiHeader;
    private CancellationTokenSource cts;
    private IDIContainer container;
    private UserProfileModel profile;
    private UserProfileModel Profile => profile ??= UserData.Instance.GetOne<UserProfileModel>();
    public event Action OnLogOut = ActionUtility.EmptyAction.Instance;

    public event Action OnShowFriend = ActionUtility.EmptyAction.Instance;

    private UIPopupSetting uiSetting;

    private AudioPresenter audioPresenter;
    private AudioPresenter AudioPresenter => audioPresenter ??= container.Inject<AudioPresenter>();

    public event System.Action OnBackEvent = ActionUtility.EmptyAction.Instance;

    public bool FLAG_NO_CHANGE { get; set; }
    private CompositeDisposable disposables = default;

    private bool __isShow = false;
    public bool IsShow => __isShow;

    public UIHeaderPresenter(IDIContainer container)
    {
        this.container = container;
        disposables?.Clear();
        disposables?.Dispose();
        disposables = new CompositeDisposable();

    }

    private void OnModelUpdate()
    {
        
    }

    public async UniTask ShowHeaderAsync()
    {
        cts.SafeCancelAndDispose();
        cts = new CancellationTokenSource();
        await InstantiateUIIfNeed();
        if(!__isShow)
        await uiHeader.In();
        __isShow = true;
    }

    public async UniTask ShowHeaderWithBackAsync()
    {
        cts.SafeCancelAndDispose();
        cts = new CancellationTokenSource();

        await InstantiateUIIfNeed();
        uiHeader.BackVisible.SetEntity(true);
        if (!__isShow)
            await uiHeader.In();
        __isShow = true;
    }

    public async UniTask HideHeaderAsync()
    {
        cts.SafeCancelAndDispose();
        cts = new CancellationTokenSource();
        if(__isShow)
            await uiHeader.Out();
        __isShow = false;
    }

    private async UniTask InstantiateUIIfNeed()
    {
        if (uiHeader == default)
        {
            uiHeader = await UILoader.Instantiate<UIHeader>(UICanvas.UICanvasType.Header, token: cts.Token);
            if (Profile == default)
            {
                profile = UserData.Instance.GetOne<UserProfileModel>();
                Debug.Log("Profile " + (profile != default));
            }

            uiHeader.SetEntity(new UIHeader.Entity()
            {
                Back = new ButtonEntity(OnBack),
                Settings = new ButtonEntity(()=>OnSetting().Forget()),
                BackVisible = false,
                SettingVisible = true,
            });

            Profile.OnChangedCommand.Subscribe( _ => OnModelUpdate()).AddTo(disposables);
        }
    }

    public void HideHeader()
    {
        cts.SafeCancelAndDispose();
        cts = default;
        uiHeader?.Out().Forget();
        __isShow = false;
    }

    public void Dispose()
    {
        ReleaseHeaderUI();
        disposables.Clear();
        audioPresenter = default;
    }

    public void ReleaseHeaderUI()
    {
        cts.SafeCancelAndDispose();
        cts = default;

        UILoader.SafeRelease(ref uiSetting);
        UILoader.SafeRelease(ref uiHeader);

        __isShow = false;
    }

    private async UniTask OnSetting()
    {
        await UniTask.CompletedTask;
        var ucs = new UniTaskCompletionSource();
        uiSetting ??= await UILoader.Instantiate<UIPopupSetting>(UICanvas.UICanvasType.PopUp, token: cts.Token);
        bool _LogOut = false;
        uiSetting.SetEntity(new UIPopupSetting.Entity()
        {
            closeBtn = new ButtonEntity(() => { ucs.TrySetResult(); }),
            logOutBtn = new ButtonEntity(() => {  _LogOut = true; ucs.TrySetResult(); }),
            outerBtn = new ButtonEntity(() => { ucs.TrySetResult(); }),
            bgmSlider = new UIProgressBarComponent.Entity()
            {
                progress = AudioPresenter.GetMusicVolumn(),
                OnChangeValue = UpdateBGM,
            },
            sfxSlider = new UIProgressBarComponent.Entity
            {
                progress = AudioPresenter.GetSFXVolumn(),
                OnChangeValue = UpdateSFX,
            },
        });
        await uiSetting.In();
        await ucs.Task;
        await uiSetting.Out();
        if (_LogOut) LogOut();
    }

    private void OnBack()
    {
        OnBackEvent.Invoke();
    }

    private void UpdateBGM(float f)
    {
        AudioPresenter.SetMusicVolumn(f);
    }

    private void UpdateSFX(float f)
    {
        AudioPresenter.SetSFXVolumn(f);
    }

    private void LogOut()
    {
        OnLogOut();
    }

}
