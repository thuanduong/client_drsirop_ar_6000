using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class SceneAssetLoader : AssetLoaderBase<SceneAssetLoader>
{
    public static async UniTask<Scene> LoadSceneAsync(string path, bool isActiveScene, LoadSceneMode loadSceneMode = LoadSceneMode.Additive, bool activateOnLoad = true, CancellationToken token = default)
    {
        var handle = Instance.GetOrCreatOperationHandle(path, loadSceneMode, activateOnLoad);
        var sceneInstance = await Instance.LoadAssetInternal<SceneInstance>(path, handle, token);
        if (isActiveScene)
        {
            SceneManager.SetActiveScene(sceneInstance.Scene);
        }
        return sceneInstance.Scene;
    }

    public static async UniTask<Scene> LoadLocalSceneAsync(string sceneName, bool isActiveScene, LoadSceneMode loadSceneMode = LoadSceneMode.Additive, CancellationToken token = default)
    {
        var sceneInstance = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
        await sceneInstance.WithCancellation(token);
        var scene = SceneManager.GetSceneByName(sceneName);
        if (isActiveScene)
        {
            SceneManager.SetActiveScene(scene);
        }
        return scene;
    }

    private AsyncOperationHandle GetOrCreatOperationHandle(string path, LoadSceneMode loadSceneMode, bool activateOnLoad)
    {
        if (!AsyncOperationHandleRefCount.TryGetHandler(path, out var handle))
        {
            handle = Addressables.LoadSceneAsync(path, loadSceneMode, activateOnLoad);
            AsyncOperationHandleRefCount.Add(path, handle);
        }
        return handle;
    }

    protected override void ReleaseHandle(AsyncOperationHandle handle)
    {
        Addressables.UnloadSceneAsync(handle);
    }
}
