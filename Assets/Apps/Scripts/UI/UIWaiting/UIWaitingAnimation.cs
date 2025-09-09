using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using System.Linq;

public class UIWaitingAnimation : MonoBehaviour
{
    public RectTransform indicator;
    private Sequence sequence;

    private void Start()
    {
        Run();
    }

    public void OnEnable()
    {
        Run();
    }

    public void OnDisable()
    {
        sequence?.Kill(true);
    }

    void Run()
    {
        sequence?.Kill(true);
        sequence = DOTween.Sequence();
        sequence.Join(indicator.DOLocalRotate(new Vector3(0, 0, 360), 1.0f, RotateMode.FastBeyond360).SetRelative(true).SetEase(Ease.Linear));
        sequence.SetLoops(-1);
    }
}
