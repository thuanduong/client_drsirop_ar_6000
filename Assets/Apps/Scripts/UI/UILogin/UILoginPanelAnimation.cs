using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class UILoginPanelAnimation : UISequenceAnimationBase
{
    public RectTransform target;
    public float MaxSize = 1000;
    protected override Tween CreateInAnimation()
    {
        var targetS = target.sizeDelta;
        targetS.x = MaxSize;
        return target.DOSizeDelta(targetS, 0.5f).SetUpdate(true);
    }

    protected override Tween CreateOutAnimation()
    {
        var targetS = target.sizeDelta;
        targetS.x = 0;
        return target.DOSizeDelta(targetS, 0.5f).SetUpdate(true);
    }

    

}
