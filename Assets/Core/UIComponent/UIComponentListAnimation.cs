using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class UIComponentListAnimation<Template, TemplateEntity> : UIComponentList<Template, TemplateEntity> where TemplateEntity : new()
                                                                                                                   where Template : UIComponent<TemplateEntity>
{
    private CancellationTokenSource cts;
    private Tween tweenAnimation;

    public UniTask PlayAnimationAsync(Func<Tween> animationFactory)
    {
        EndAnimation();
        if (animationFactory == null) return UniTask.CompletedTask;
        tweenAnimation = animationFactory.Invoke();
        return tweenAnimation?.AwaitForComplete(TweenCancelBehaviour.CancelAwait, cancellationToken: cts.Token) ?? UniTask.CompletedTask;
    }

    private void EndAnimation()
    {
        cts.SafeCancelAndDispose();
        cts = new CancellationTokenSource();
        tweenAnimation?.Kill(true);
    }

    private void OnDestroy()
    {
        tweenAnimation?.Kill();
    }

    public void In()
    {
        AnimationIn().Forget();
    }

    public virtual UniTask AnimationIn()
    {
        return PlayAnimationAsync(CreateInAnimation);
    }

    public virtual UniTask AnimationOut()
    {
        return PlayAnimationAsync(CreateOutAnimation);
    }

    protected virtual Tween CreateInAnimation() { return null; }

    protected virtual Tween CreateOutAnimation() { return null; }
}
