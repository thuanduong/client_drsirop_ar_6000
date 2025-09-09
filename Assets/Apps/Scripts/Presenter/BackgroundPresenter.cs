using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

public class BackgroundPresenter : IDisposable
{

    private readonly IDIContainer container;
    private CancellationTokenSource cts;

    UIBackground backgroundSprite;
    UIBackgroundColor backgroundColor;
    public bool FLAG_NO_CHANGE { get; set; }

    public BackgroundPresenter(IDIContainer container)
    {
        this.container = container;
    }

    public void Dispose()
    {
        UILoader.SafeRelease(ref backgroundSprite);
        UILoader.SafeRelease(ref backgroundColor);
    }

    public async UniTask LoadBackgroundWithSprite(string path, bool FromAsset = false)
    {
        cts.SafeCancelAndDispose();
        cts = new CancellationTokenSource();

        backgroundSprite ??= await UILoader.Instantiate<UIBackground>(canvasType : UICanvas.UICanvasType.BackGround, token: cts.Token);
        Sprite s = null;
        if (FromAsset)
        {
            s = await SpriteLoader.LoadSprite(path, token: cts.Token);
        }
        else
        {
            await loadBackgroundSprite(path, (i) => s = i);
        }
        var entity = new UIBackground.Entity()
        {
            sprite = s
        };
        backgroundSprite.SetEntity(entity);
        await backgroundSprite.In();
    }

    public async UniTask ChangeBackgroundImage(string newPath)
    {
        Sprite newSprite = null;
        await loadBackgroundSprite(newPath, (sprite) => newSprite = sprite);
        var entity = new UIBackground.Entity()
        {
            sprite = newSprite
        };
        backgroundSprite.SetEntity(entity);
    }

    private async UniTask loadBackgroundSprite(string path, Action<Sprite> callback)
    {
        ResourceRequest resourceRequest = Resources.LoadAsync<Sprite>(path);
        await resourceRequest.ToUniTask();
        Sprite loadedSprite = resourceRequest.asset as Sprite;
        callback?.Invoke(loadedSprite);
    }

    public async UniTask HideBackgroundSprite()
    {
        if (backgroundSprite != null)
            await backgroundSprite.Out();
    }

    public async UniTask ShowBackgroundSprite()
    {
        if (backgroundSprite != null)
            await backgroundSprite.In();
    }

    public async UniTask LoadBackgroundColor(Color color, CancellationToken cancellationToken = default)
    {
        try
        {
            backgroundColor ??= await UILoader.Instantiate<UIBackgroundColor>(canvasType: UICanvas.UICanvasType.BackGround, token: cancellationToken);
            var entity = new UIBackgroundColor.Entity()
            {
                defaultColor = color,
            };
            backgroundColor.SetEntity(entity);
            await backgroundColor.In();
        }
        catch { }
    }

    public async UniTask ShowBackgroundColor()
    {
        if (backgroundColor != null)
            await backgroundColor.In();
    }

    public async UniTask ChangeToColor(Color color, float duration, CancellationToken cancellationToken = default)
    {
        try
        {
            if (backgroundColor != null)
            {
                await backgroundColor.ChangeColor(color, duration);
            }
        }
        catch { }
    }

    public async UniTask HideBackgroundColor()
    {
        if (backgroundColor != null)
            await backgroundColor.Out();
    }

}
