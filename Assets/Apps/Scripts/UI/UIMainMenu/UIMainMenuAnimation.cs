using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

public class UIMainMenuAnimation : UISequenceAnimationBase
{
    public CanvasGroup canvasGroup;
    public GameObject RightBar;
    public GameObject LeftBar;

    protected override Tween CreateInAnimation()
    {
        BarRightIn();
        BarLeftIn();
        return canvasGroup.DOFade(0.0f, 1.0f, 0.5f).SetUpdate(true);
    }

    protected override Tween CreateOutAnimation()
    {
        BarRightOut();
        BarLeftOut();
        return canvasGroup.DOFade(1.0f, 0.0f, 0.5f).SetUpdate(true);
    }

    protected Tween BarRightIn()
    {
        var sequen = DOTween.Sequence();
        var children = RightBar.GetComponentsInChildren<Transform>().Where(x=>x.parent == RightBar.transform);
        children.ForEach(x => sequen.Append(x.AsRectTransform().DOAnchorPosXFrom(-300, 0.15f).SetEase(Ease.OutBack).SetDelay(0.05f)));
        sequen.SetDelay(0.1f);
        return sequen;
    }

    protected Tween BarRightOut()
    {
        var sequen = DOTween.Sequence();
        var children = RightBar.GetComponentsInChildren<Transform>().Where(x => x.parent == RightBar.transform);
        children.ForEach(x => sequen.Append(x.AsRectTransform().DOAnchorPosXToThenReverse(-300, 0.15f).SetEase(Ease.InBack).SetDelay(0.05f)));
        return sequen;
    }

    protected Tween BarLeftIn()
    {
        var sequen = DOTween.Sequence();
        var children = LeftBar.GetComponentsInChildren<Transform>().Where(x => x.parent == LeftBar.transform);
        children.ForEach(x => sequen.Append(x.AsRectTransform().DOAnchorPosXFrom(300, 0.15f).SetEase(Ease.OutBack).SetDelay(0.05f)));
        sequen.SetDelay(0.1f);
        return sequen;
    }

    protected Tween BarLeftOut()
    {
        var sequen = DOTween.Sequence();
        var children = LeftBar.GetComponentsInChildren<Transform>().Where(x => x.parent == LeftBar.transform);
        children.ForEach(x => sequen.Append(x.AsRectTransform().DOAnchorPosX(300, 0.15f).SetEase(Ease.InBack).SetDelay(0.05f)));
        return sequen;
    }
}
