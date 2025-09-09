using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

public static class ObjectLoader
{
    public static void SafeRelease(ref GameObject obj, string assetPath)
    {
        if (obj != default)
        {
            Object.Destroy(obj);
            obj = default;
            PrimitiveAssetLoader.UnloadAssetAtPath(assetPath);
        }
    }

    public static async UniTask<T> Instantiate<T>(string prefix, Transform parent = default, CancellationToken token = default) where T : MonoBehaviour
    {
        var type = typeof(T).ToString();
        var prefab = await LoadResource<T>(prefix, type, token);
        var instance = GameObject.Instantiate<T>(prefab, parent, false);
        return instance;
    }

    public static async UniTask<GameObject> InstantiateGO(string path, Transform parent = default, CancellationToken token = default)
    {
        var prefab = await PrimitiveAssetLoader.LoadAssetAsync<GameObject>(path, token);
        var instance = GameObject.Instantiate(prefab, parent, false);
        return instance;
    }

    private static async UniTask<T> LoadResource<T>(string prefix, string type, CancellationToken token) where T : MonoBehaviour
    {
        var go = await PrimitiveAssetLoader.LoadAssetAsync<GameObject>(GetPathFromType(prefix, type), token);
        return go.GetComponent<T>();
    }


    public static async UniTask<T> LoadObject<T>(string path, CancellationToken token = default) where T : Object
    {
        var prefab = await PrimitiveAssetLoader.LoadAssetAsync<T>(path, token);
        return prefab;
    }

    public static void SafeReleaseLoaded<T>(string path) where T : MonoBehaviour
    {
        PrimitiveAssetLoader.UnloadAssetAtPath(path);
    }

    public static void SafeReleaseScriptableObject<T>(string path) where T : ScriptableObject
    {
        PrimitiveAssetLoader.UnloadAssetAtPath(path);
    }

    private static string GetPathFromType(string prefix, string type)
    {
        return $"{prefix}/{type}";
    }

    public static void SafeRelease<T>(string prefix, ref T obj) where T : MonoBehaviour
    {
        if (obj != null)
        {
            Object.Destroy(obj.gameObject);
            obj = null;
            PrimitiveAssetLoader.UnloadAssetAtPath(GetPathFromType(prefix, typeof(T).ToString()));
        }
    }

    public static void SafeRelease<T>(string prefix, ref T[] uis) where T : MonoBehaviour
    {
        for (int i = 0; i < uis.Length; i++)
        {
            var ui = uis[i];
            SafeRelease(prefix, ref ui);
        }
        uis = default;
    }

}
