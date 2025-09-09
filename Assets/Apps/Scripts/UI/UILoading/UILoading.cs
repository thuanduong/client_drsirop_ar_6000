using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILoading : PopupEntity<UILoading.Entity>
{
    [Serializable]
    public class Entity
    {
        public float percent = 1.0f;
        public Entity() { percent = 1.0f; }
        public Entity(float p)
        {
            percent = p;
        }
    }

    public CanvasGroup canvasInside;

    protected override void OnSetEntity()
    {
        canvasInside.alpha = entity.percent;
    }

    public void SetAlpha(float per)
    {
        canvasInside.alpha = entity.percent;
    }
}
