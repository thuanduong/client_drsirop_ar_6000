using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using System.Threading;

public static class SceneLoader
{
    private static AsyncOperationHandle<SceneInstance> currentSceneHandle;

    public static async UniTask<SceneInstance> LoadSceneAsync (
       string sceneAddress,
       LoadSceneMode loadMode = LoadSceneMode.Single,
       bool activateOnLoad = true,
       CancellationToken cancellationToken = default)
    {
        if (currentSceneHandle.IsValid() && loadMode == LoadSceneMode.Single)
        {
            await UnloadCurrentSceneAsync();
        }

        Debug.Log($"Start Load scene: {sceneAddress}...");

        try
        {
            currentSceneHandle = Addressables.LoadSceneAsync(sceneAddress, loadMode, activateOnLoad);

            while (!currentSceneHandle.IsDone)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Debug.LogWarning($"Load '{sceneAddress}' cancelled.");
                    Addressables.Release(currentSceneHandle);
                    currentSceneHandle = default; 
                    throw new System.OperationCanceledException(cancellationToken);
                }
                Debug.Log($"Load scene: {sceneAddress} - {currentSceneHandle.PercentComplete * 100:F2}%");
                await UniTask.Yield();
            }

            if (currentSceneHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"Scene '{sceneAddress}' loaded !");
                return currentSceneHandle.Result;
            }
            else
            {
                Debug.LogError($"Error when load '{sceneAddress}': {currentSceneHandle.OperationException}");
                Addressables.Release(currentSceneHandle);
                currentSceneHandle = default;
            }
        }
        catch (System.OperationCanceledException)
        {
            
        }
        catch (System.Exception)
        {
            if (currentSceneHandle.IsValid())
            {
                Addressables.Release(currentSceneHandle);
                currentSceneHandle = default;
            }
        }
        return default;
    }

    public static async UniTask UnloadCurrentSceneAsync()
    {
        if (currentSceneHandle.IsValid())
        {
            Debug.Log($"Start unload scene: {currentSceneHandle.Result.Scene.name}...");
            try
            {
                await Addressables.UnloadSceneAsync(currentSceneHandle, true); 
                Debug.Log($"Scene '{currentSceneHandle.Result.Scene.name}' unloaded !");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error When unload scene '{currentSceneHandle.Result.Scene.name}': {ex}");
                throw;
            }
            finally
            {
                currentSceneHandle = default; 
            }
        }
        else
        {
            
        }
    }

    public static AsyncOperationHandle<SceneInstance> GetCurrentSceneHandle()
    {
        return currentSceneHandle;
    }
}
