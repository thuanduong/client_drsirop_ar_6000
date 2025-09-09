using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using TMPro;

public class UIComponentCountTimer : UIComponent<UIComponentCountTimer.Entity>
{
    private CompositeDisposable disposables = default;

    public class Entity
    {
        public float Time;
        public Entity() { }
    }

    public FormattedTextComponent TextTime;

    protected override void OnSetEntity()
    {
        disposables?.Clear();
        disposables?.Dispose();
        disposables = new CompositeDisposable();

        UpdateCountDown();
        Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(_ =>
        {
            UpdateCountDown();
        }).AddTo(disposables);
    }

    void UpdateCountDown()
    {
        entity.Time += 1.0f;
        TextTime.SetEntity(Mathf.RoundToInt(entity.Time));
    }

    private void OnDestroy()
    {
        disposables?.Clear();
    }
}
