using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class UIImageFillCountDownTimer : UIComponent<UIImageFillCountDownTimer.Entity>
{
    public class Entity {
        public long utcStartTick;
        public long utcEndTick;
        public long utcTick;
        [SerializeField] public Action outDatedEvent;

        public Entity() {}
        public Entity(long current, long start, long end)
        {
            utcTick = current;
            utcStartTick = start;
            utcEndTick = end;
        }
    }

    public Image image;
    private CancellationTokenSource cts;
    private float updateInterval = 0.02f;
    private float remainTime = 0;
    private bool outOfDate = false;
    private float maxTime = 0;

    private void Start()
    {
        defaultOut();
    }

    private void Reset()
    {
        if (image == null)
            image = GetComponent<Image>();
    }
    protected override void OnSetEntity()
    {
        cts.SafeCancelAndDispose();
        cts = new CancellationTokenSource();
        calculatorTime();
        updateClock();
    }

    private void OnDestroy()
    {
        cts.SafeCancelAndDispose();
        cts = default;
    }

    private void calculatorTime()
    {
        if (entity.utcEndTick > 0 && entity.utcStartTick > 0 && entity.utcTick > 0)
        {
            var time = TimeSpan.FromTicks(entity.utcEndTick - entity.utcStartTick);
            maxTime = (float)time.TotalSeconds;
            var cTime = TimeSpan.FromTicks(entity.utcTick - entity.utcStartTick);
            remainTime = maxTime - (float)cTime.TotalSeconds;
            outOfDate = false;
        }
        else
        {
            maxTime = 0;
            remainTime = 0;
            outOfDate = true;
        }
    }

    public async UniTask Run()
    {
        if (image != null)
        {
            while (!outOfDate && this.entity != default)
            {
                try
                {
                    remainTime -= updateInterval;
                    await UniTask.Delay(TimeSpan.FromSeconds(updateInterval), cancellationToken: cts.Token);
                    updateClock();
                    if (remainTime <= 0)
                    {
                        outOfDate = true;
                        this.entity.outDatedEvent?.Invoke();
                    }
                }
                catch
                {
                    break;
                }
            }
        }

        await UniTask.CompletedTask;
    }

    public void Stop()
    {
        outOfDate = true;
        maxTime = 0;
        remainTime = 0;
        defaultOut();
    }

    private void updateClock()
    {
        float percent = 0;
        if (maxTime > 0)
        {
            percent = remainTime / maxTime;
            image.fillAmount = percent;
        }
        else
        {
            defaultOut();
        }
    }

    private void defaultOut()
    {
        image.fillAmount = 0;
    }
}
