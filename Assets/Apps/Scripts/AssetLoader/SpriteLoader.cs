using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
public static class SpriteLoader
{
    public static async UniTask<Sprite> LoadSprite(string path, CancellationToken token = default)
    {
        var tex = await PrimitiveAssetLoader.LoadAssetAsync<Texture2D>(path, token);
        if (tex != default)
        {
            return ImageHelper.CreateSprite(tex);
        }
        return null;
    }

}
