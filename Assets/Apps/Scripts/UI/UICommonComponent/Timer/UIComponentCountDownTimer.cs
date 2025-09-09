using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FormattedTextComponent))]
public class UIComponentCountDownTimer : UIComponent<UIComponentCountDownTimer.Entity>
{
    [Serializable]
    public class Entity
    {
        [SerializeField]
        public Action outDatedEvent;
        public int utcEndTimeStamp;
        public long utcTick;
    }

    public FormattedTextComponent timeLeftText;

    public enum Format
    {
        DD_HH_MM_SS,
        SS,
    }

    public Format format = Format.DD_HH_MM_SS;
    private DateTime endTimeStampDateTime;
    private float updateInterval = 1.0f;
    private float currentTime = 0.0f;
    private bool outOfDate = false;

    protected override void OnSetEntity()
    {
        if (this.entity.utcEndTimeStamp != 0)
            endTimeStampDateTime = UnixTimeStampToDateTime(this.entity.utcEndTimeStamp);
        else if (this.entity.utcTick != 0)
            endTimeStampDateTime = new DateTime(this.entity.utcTick);

        outOfDate = false;
        SetDateTimeLeft(true);
    }

    private string GetFormat(Format format)
    {
        return format switch
        {
            Format.DD_HH_MM_SS => "{0:00}:{1:00}:{2:00}",
            Format.SS => "{0:00}",
            _ => "{0:00}"
        };
    }

    private void Update()
    {
        if (!outOfDate && this.entity != default)
        {
            currentTime += Time.deltaTime;
            if (currentTime >= updateInterval)
            {
                currentTime -= updateInterval;
                SetDateTimeLeft(false);
            }
        }
    }

    private void SetDateTimeLeft(bool isInitial)
    {
        TimeSpan timeLeft = GetTimeLeft();
        if (GetTimeLeft().TotalSeconds <= 0)
        {
            if (!isInitial)
            {
                outOfDate = true;
                this.entity.outDatedEvent?.Invoke();
            }
            timeLeft = new TimeSpan(0, 0, 0, 0);
        }
        switch (format)
        {
            case Format.DD_HH_MM_SS:
                timeLeftText.SetEntity(string.Format(GetFormat(this.format), RemainHour(timeLeft.TotalHours), timeLeft.Minutes, timeLeft.Seconds));
                break;
            case Format.SS:
                timeLeftText.SetEntity(string.Format(GetFormat(this.format), timeLeft.TotalSeconds));
                break;
            default:
                break;
        }
    }

    private TimeSpan GetTimeLeft()
    {
        return (endTimeStampDateTime - DateTime.UtcNow);
    }

    public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
    {
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp);
        return dateTime;
    }

    public static int RemainHour(double TotalHours)
    {
        return (int)TotalHours;
    }

    void Reset()
    {
        timeLeftText ??= this.GetComponent<FormattedTextComponent>();
    }
}	