using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Threading;
using Cysharp.Threading.Tasks;

public class UIForgotPasswordPanelAnimation : UISequenceAnimationBase
{
    public RectTransform target;
    public float MaxSize = 1000;
    public CanvasGroup EmailPanel;
    public CanvasGroup OTPPanel;
    CancellationTokenSource cts;

    protected override Tween CreateInAnimation()
    {
        var targetS = target.sizeDelta;
        targetS.x = MaxSize;
        ShowDefaultPanel();
        return target.DOSizeDelta(targetS, 0.5f).SetUpdate(true);
    }

    protected override Tween CreateOutAnimation()
    {
        var targetS = target.sizeDelta;
        targetS.x = 0;
        return target.DOSizeDelta(targetS, 0.5f).SetUpdate(true);
    }

    private void ShowDefaultPanel()
    {
        EmailPanel.alpha = 1.0f;
        EmailPanel.interactable = true;
        EmailPanel.blocksRaycasts = true;
        OTPPanel.alpha = 0.0f;
        OTPPanel.interactable = false;
        OTPPanel.blocksRaycasts = false;
    }

    public async UniTask ShowEmailPanel()
    {
        cts.SafeCancelAndDispose();
        cts = new CancellationTokenSource();
        var sq = DOTween.Sequence();
        EmailPanel.interactable = false;
        EmailPanel.blocksRaycasts = false;
        OTPPanel.interactable = false;
        OTPPanel.blocksRaycasts = false;
        await sq.Append(OTPPanel.DOFade(0.0f, 0.15f))
            .Append(EmailPanel.DOFade(1.0f, 0.15f))
            .WithCancellation(cts.Token);
        EmailPanel.interactable = true;
        EmailPanel.blocksRaycasts = true;
    }

    public async UniTask ShowOTPPanel()
    {
        cts.SafeCancelAndDispose();
        cts = new CancellationTokenSource();

        var sq = DOTween.Sequence();
        EmailPanel.interactable = false;
        EmailPanel.blocksRaycasts = false;
        OTPPanel.interactable = false;
        OTPPanel.blocksRaycasts = false;
        await sq.Append(EmailPanel.DOFade(0.0f, 0.15f))
            .Append(OTPPanel.DOFade(1.0f, 0.15f))
            .WithCancellation(cts.Token);
        OTPPanel.interactable = true;
        OTPPanel.blocksRaycasts = true;
    }
}
