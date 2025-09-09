using System;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

public class ARRoomPresenter : IDisposable
{
    private readonly IDIContainer Container;
    private CancellationTokenSource cts;

    private Scene? previousActiveScene;
    private Scene loadedScene = default;

    private const string sceneName = "ARScene";

    public ARRoomPresenter(IDIContainer container)
    {
        this.Container = container;
        cts = new CancellationTokenSource();
    }

    public void Dispose()
    {
        cts.SafeCancelAndDispose();
    }


    public async UniTask LoadScene(CancellationToken cancellationToken)
    {
        previousActiveScene = SceneManager.GetActiveScene();
        var loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        try
        {
            while (!loadOperation.isDone)
            {
                float progress = Mathf.Clamp01(loadOperation.progress / 0.9f);
                Debug.Log($"load scene {sceneName}: {progress * 100}%");
                await UniTask.Yield(cancellationToken);
            }

            loadedScene = SceneManager.GetSceneByName(sceneName);
            if (loadedScene.IsValid())
            {
                SceneManager.SetActiveScene(loadedScene);
                Debug.Log($"Scene {sceneName} loaded and active.");
            }
            else
            {
                Debug.LogError($"Cant find {sceneName} after load.");
            }

            await UniTask.WaitForSeconds(0.25f, cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
        {

        }
        finally
        {
        }
    }

    public async UniTask UnloadScene(CancellationToken cancellationToken)
    {
        Scene sceneToUnload = SceneManager.GetSceneByName(sceneName);

        if (!sceneToUnload.IsValid() || !sceneToUnload.isLoaded)
        {
            Debug.LogWarning($"Scene '{sceneName}' khong hop le hoac chua load.");
            return;
        }

        Debug.Log($"Start unload scene (Additive): {sceneName}");
        AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(sceneToUnload);
        try
        {
            while (!unloadOperation.isDone)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Debug.Log($"unload scene {sceneName}: {unloadOperation.progress * 100}%");
                await UniTask.Yield(cancellationToken);
            }

            Debug.Log($"Unload scene {sceneName} completed.");

            if (previousActiveScene.HasValue && previousActiveScene.Value.IsValid() && previousActiveScene.Value.isLoaded)
            {
                SceneManager.SetActiveScene(previousActiveScene.Value);
                Debug.Log($"Active scene: {previousActiveScene.Value.name}");
                previousActiveScene = null;
            }
            else
            {
                Debug.LogWarning("Cant load previous Scene or Previous scene is not valid.");
                previousActiveScene = null;
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log($"unload scene {sceneName} has been cancelled.");
        }
        finally
        {
        }
    }
}
