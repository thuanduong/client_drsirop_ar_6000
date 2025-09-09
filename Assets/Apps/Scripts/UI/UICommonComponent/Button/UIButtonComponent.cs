using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using Cysharp.Threading.Tasks;

[RequireComponent(typeof(Button))]
public class UIButtonComponent : UIComponent<ButtonEntity>
{
    public Button button;

    private UnityEvent buttonEvent = new UnityEvent();
    private CancellationTokenSource cts;


    private void Awake()
    {
        this.button.onClick.AddListener(() => buttonEvent.Invoke());
    }

    private void OnDestroy()
    {
        cts.SafeCancelAndDispose();
        cts = default;
    }

    protected override void OnSetEntity()
    {
        cts.SafeCancelAndDispose();
        cts = new CancellationTokenSource();

        buttonEvent.RemoveAllListeners();
        buttonEvent.AddListener(() => this.entity.onClickEvent.Invoke());
        button.interactable = this.entity.isInteractable;
        button.gameObject.SetActive(!this.entity.isDisable);
    }

    public void SetEntity(UnityEvent buttonEvent)
    {
        this.entity = new ButtonEntity(buttonEvent);
        OnSetEntity();
    }

    public void SetEntity(Action action)
    {
        this.entity = new ButtonEntity(action);
        OnSetEntity();
    }

    public void SetAction(Action action)
    {
        if (this.entity != null)
        {
            this.entity.onClickEvent = new UnityEvent();
            this.entity.onClickEvent.AddListener(() => action());
            OnSetEntity();
        }
    }

    public void SetDisable(bool isDisable)
    {
        if (this.entity != null)
        {
            this.entity.isDisable = isDisable;
            OnSetEntity();
        }
    }

    public void SetInteracble(bool val)
    {
        if (this.entity != null)
        {
            this.entity.isInteractable = val;
            OnSetEntity();
        }
    }

    void Reset()
    {
        button ??= this.GetComponent<Button>();
    }

    public void DelayButtonPress(float Duration = 0.5f)
    {
        cts.SafeCancelAndDispose();
        cts = new CancellationTokenSource();
        delayButtonPress(Duration).Forget();
    }

    private async UniTask delayButtonPress(float Duration)
    {
        this.entity.isInteractable = false;
        await UniTask.Delay(TimeSpan.FromSeconds(Duration), cancellationToken: cts.Token);
        this.entity.isInteractable = true;
    }
}
