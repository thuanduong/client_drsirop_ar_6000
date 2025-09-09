using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using UnityEngine;
using Cysharp.Threading.Tasks;

public class UIBottomMenuPresenter : IDisposable
{
    private IDIContainer Container { get; }

    private UIBottomMenu ui;
    private CancellationTokenSource cts;

    public event Action OnButtonShopClicked = ActionUtility.EmptyAction.Instance;
    public event Action OnButtonQuestClicked = ActionUtility.EmptyAction.Instance;
    public event Action OnButtonPlayClicked = ActionUtility.EmptyAction.Instance;
    public event Action OnButtonLeaderboardClicked = ActionUtility.EmptyAction.Instance;
    public event Action OnButtonCardClicked = ActionUtility.EmptyAction.Instance;

    private bool __isShowing = false;
    public bool IsShow => __isShowing;

    public UIBottomMenuPresenter(IDIContainer container)
    {
        this.Container = container;
    }

    public async UniTask ShowAsync()
    {
        if (__isShowing) return;
        cts.SafeCancelAndDispose();
        cts = new CancellationTokenSource();
        if (ui == default)
        {
            ui = await UILoader.Instantiate<UIBottomMenu>(UICanvas.UICanvasType.Header, token: cts.Token);

            ui.SetEntity(new UIBottomMenu.Entity()
            {
                toggleShop = new UIToggleComponent.Entity() { onActiveToggle = OnToggleShop },
                toggleQuest = new UIToggleComponent.Entity() { onActiveToggle = OnToggleQuest },
                togglePlay = new UIToggleComponent.Entity() { onActiveToggle = OnTogglePlay, isOn = true },
                toggleLeaderboard = new UIToggleComponent.Entity() { onActiveToggle = OnToggleLeaderBoard },
                toggleCard = new UIToggleComponent.Entity() { onActiveToggle = OnToggleCard },
            });
        }
        __isShowing = true;
        await ui.In();
    }

    public async UniTask HideAsync()
    {
        if (!__isShowing) return;
        cts.SafeCancelAndDispose();
        cts = new CancellationTokenSource();
        __isShowing = false;
        await ui.Out();
    }

    public void Dispose()
    {
        cts.SafeCancelAndDispose();
        cts = default;
        UILoader.SafeRelease(ref ui);
    }

    private void OnToggleShop(bool active)
    {
        if (active)
        {
            OnButtonShopClicked.Invoke();
        }
    }

    private void OnToggleQuest(bool active)
    {
        if (active)
        {
            OnButtonQuestClicked.Invoke();
        }
    }

    private void OnTogglePlay(bool active)
    {
        if (active)
        {
            OnButtonPlayClicked.Invoke();
        }
    }

    private void OnToggleLeaderBoard(bool active)
    {
        if (active)
        {
            OnButtonLeaderboardClicked.Invoke();
        }
    }

    private void OnToggleCard(bool active)
    {
        if (active)
        {
            OnButtonCardClicked.Invoke();
        }
    }

}
