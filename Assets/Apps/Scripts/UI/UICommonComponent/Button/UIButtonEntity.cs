using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ButtonEntity
{
    public UnityEvent onClickEvent;
    public bool isDisable;
    public bool isInteractable;

    public ButtonEntity(Action action, bool isDisable = false, bool isInteractable = true)
    {
        onClickEvent = new UnityEvent();
        onClickEvent.AddListener(() => action?.Invoke());
        this.isDisable = isDisable;
        this.isInteractable = isInteractable;
    }

    public ButtonEntity(UnityEvent buttonEvent)
    {
        this.onClickEvent = buttonEvent;
    }

    public ButtonEntity()
    {
    }
}
