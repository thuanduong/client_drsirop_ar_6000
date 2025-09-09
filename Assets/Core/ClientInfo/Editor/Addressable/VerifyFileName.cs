using System.Collections;
using System.Collections.Generic;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEditor;
using Cysharp.Threading.Tasks;


public class VerifyFileName
{
    private const string DefaultPath = "Assets/AssetBundles/";
    [MenuItem("Tools/Adressables/Quick Fix Name")]
    public static void QuickFixName()
    {
        DoFixNameAsync().Forget();
    }

    private static async UniTask DoFixNameAsync()
    {
        var settings = AddressableAssetSettingsDefaultObject.GetSettings(false);
        List<AddressableAssetEntry> entries = new List<AddressableAssetEntry>();
        settings.GetAllAssets(entries, true);
        var len = entries.Count;
        for (int i = 0; i < len; i++)
        {
            var asset = entries[i];
            var oldName = asset.address;
            var targetName = asset.MainAsset.name;
            asset.address = FixName(oldName, targetName);
            await UniTask.DelayFrame(1);
        }
        Debug.Log("Finish!");
    }

    private static string FixName(string oldName, string objName)
    {
        string newName = oldName;
        if (oldName.StartsWith(DefaultPath))
        {
            newName = oldName.Remove(0, DefaultPath.Length);
        }
        var ll = newName.LastIndexOf("/");
        var mm = newName.Remove(ll + 1);
        mm += objName;
        return mm;
    }
}
