using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build;
using UnityEngine;

public class ClientBuilder : EditorWindow
{
    [System.Flags]
    public enum BuildPlatform
    {
        Windows,
        Mac,
        Ios,
        Android,
        WebGL,
        Linux
    }



    [MenuItem("Window/ClientBuilder")]
    public static void ShowWindows()
    {
        EditorWindow.GetWindow(typeof(ClientBuilder));
    }

    private const string AssetVersionLabel = "Asset Version";
    private const string ClientVersionScripableObjectPath = "ClientInfo/ClientInfo";
    private string assetVersion;
    private string assetProfileId;
    private ClientInfo clientInfo;
    private ClientInfo.Enviroment currentEnviroment;
    private string currentClientVersion;
    private bool isBuildClient = false;
    private bool isBuildAsset = false;


    private bool isAndroidBuildAAB = false;

    private int platformFlag = 0;
    private string[] platforms;
    private List<BuildPlatform> choosingPlatForms => GetSelectedIndexes<BuildPlatform>(platformFlag);

    private int buildModeFlag = 0;
    private string[] buildModes;
    private List<ClientInfo.AssetBuildMode> choosingBuildModes => GetSelectedIndexes<ClientInfo.AssetBuildMode>(buildModeFlag);

    private void Awake()
    {
        //clientInfo = AssetDatabase.LoadAssetAtPath<ClientInfo>(ClientVersionScripableObjectPath);
        clientInfo = Resources.Load<ClientInfo>(ClientVersionScripableObjectPath);
        if (clientInfo == null) Debug.LogError("Client Info is null");
        else Debug.Log("Current Enviroment " + clientInfo.CurrentEnviroment);

        if (!AddressableAssetSettingsDefaultObject.SettingsExists)
        {
            Debug.LogWarning("Addressable Settings don't exist, creating new ones.");

            AddressableAssetSettingsDefaultObject.Settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
        }

        currentEnviroment = clientInfo.CurrentEnviroment;
        GetVersionBaseOnEnviroment(currentEnviroment);
        platforms = GetEnumAsStringArray<BuildPlatform>();
        buildModes = GetEnumAsStringArray<ClientInfo.AssetBuildMode>();
    }

    private static string[] GetEnumAsStringArray<T>() where T : System.Enum
    {
        return Enum.GetValues(typeof(T))
                              .Cast<T>()
                              .Select(x => x.ToString())
                              .ToArray();
    }

    private void GetVersionBaseOnEnviroment(ClientInfo.Enviroment currentEnviroment)
    {
        if (clientInfo == null || AddressableAssetSettingsDefaultObject.Settings == null) return;
        var enviromentInfo = clientInfo[currentEnviroment];
        currentClientVersion = enviromentInfo.Version;
        assetVersion = enviromentInfo.AssetVersion;
        assetProfileId = AddressableAssetSettingsDefaultObject.Settings.profileSettings.GetProfileId(currentEnviroment.ToString());
        Debug.Log($"Profile {assetProfileId} Asset Version {assetVersion}");
    }

    private void OnGUI()
    {
        ShowVersionSettings();
        ShowClientBuildSetting();
        ShowAssetBuildSetting();
        ShowPlatformsDropdown();
        ShowAndroidBuilAAB();
        ShowBuildModeDropdown();

        if (GUILayout.Button("Build"))
        {
            ApplySettings();
            if (ValidateOptions())
            {
                foreach (var platform in choosingPlatForms)
                {
                    var (targetGroup, target) = GetTargetsFromPlatform(platform);
                    EditorUserBuildSettings.SwitchActiveBuildTarget(targetGroup, target);
                    foreach (var assetBuildMode in choosingBuildModes)
                    {
                        if (platform == BuildPlatform.Android)
                        {
                            if (isAndroidBuildAAB)
                            {
                                BuildAAB(assetBuildMode);
                            }
                            Build(assetBuildMode);
                        }
                        else if (platform == BuildPlatform.Linux)
                        {
                            EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Server;
                            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Server, ScriptingImplementation.IL2CPP);
                            Build(assetBuildMode);
                        }
                        else
                        {
                            Build(assetBuildMode);
                        }
                    }
                }
            }
        }
    }

    private (BuildTargetGroup targetGroup, BuildTarget target) GetTargetsFromPlatform(BuildPlatform platform)
    {
        return platform switch
        {
            BuildPlatform.Windows => (BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64),
            BuildPlatform.Android => (BuildTargetGroup.Android, BuildTarget.Android),
            BuildPlatform.Mac => (BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX),
            BuildPlatform.Ios => (BuildTargetGroup.iOS, BuildTarget.iOS),
            BuildPlatform.WebGL => (BuildTargetGroup.WebGL, BuildTarget.WebGL),
            BuildPlatform.Linux => (BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64),
            _ => throw new NotImplementedException()
        };
    }

    private bool ValidateOptions()
    {
        if (choosingPlatForms.Count() <= 0)
        {
            Debug.LogError("Choose at least 1 platform to perform builds");
            return false;
        }
        else if (choosingBuildModes.Count() <= 0)
        {
            Debug.LogError("Choose at least 1 Asset build modes to perform builds");
            return false;
        }
        else if (!isBuildAsset && !isBuildClient)
        {
            Debug.LogError("Choose asset build or client build");
            return false;
        }
        return true;
    }

    public static List<T> GetSelectedIndexes<T>(int val) where T : System.Enum, IConvertible
    {
        List<T> selectedItem = new List<T>();
        var enumAsArray = GetEnumAsArray<T>();
        int length = enumAsArray.Length;
        for (int i = 0; i < length; i++)
        {
            int layer = 1 << i;
            if ((Convert.ToInt32(val) & layer) != 0)
            {
                selectedItem.Add(enumAsArray[i]);
            }
        }
        return selectedItem;
    }

    private static T[] GetEnumAsArray<T>() where T : System.Enum
    {
        return Enum.GetValues(typeof(T))
                              .Cast<T>()
                              .ToArray();
    }

    private void ShowBuildModeDropdown()
    {
        buildModeFlag = EditorGUILayout.MaskField("Build Modes", buildModeFlag, buildModes);
    }

    private void ShowPlatformsDropdown()
    {
        platformFlag = EditorGUILayout.MaskField("Platform", platformFlag, platforms);
    }

    private void BuildAAB(ClientInfo.AssetBuildMode assetBuildMode)
    {
        if (isBuildAsset)
        {
            var result = BuildAsset(assetBuildMode);
            if (!string.IsNullOrEmpty(result?.Error))
            {
                throw new Exception(result.Error);
            }
        }

        if (isBuildClient)
        {
            var scenes = EditorBuildSettings.scenes.Select(x => x.path).ToArray();
            BuildPipeline.BuildPlayer(scenes,
                                           $"{Application.dataPath.Replace("Assets", "")}Builds/{currentClientVersion}/{currentEnviroment}/{UnityEditor.EditorUserBuildSettings.activeBuildTarget}/{GetAssetTypeAsString(assetBuildMode)}/{Application.productName}_{currentEnviroment}_{currentClientVersion}{GetFileExtensionForAndroid(true)}",
                                           EditorUserBuildSettings.activeBuildTarget,
                                           GetBuildOptions(true));
        }
    }
    private void Build(ClientInfo.AssetBuildMode assetBuildMode)
    {
        if (isBuildAsset)
        {
            var result = BuildAsset(assetBuildMode);
            if (!string.IsNullOrEmpty(result?.Error))
            {
                throw new Exception(result.Error);
            }
        }

        if (isBuildClient)
        {
            var scenes = EditorBuildSettings.scenes.Select(x => x.path).ToArray();
            var path = $"{Application.dataPath.Replace("Assets", "")}Builds/{currentClientVersion}/{currentEnviroment}/{UnityEditor.EditorUserBuildSettings.activeBuildTarget}/{GetAssetTypeAsString(assetBuildMode)}/{currentEnviroment}/{Application.productName}{GetFileExtension()}";
            var options = GetBuildOptions();
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebGL)
            {
                BuildWebGLGame(scenes, path, options);
            }
            else
            {
                BuildPipeline.BuildPlayer(scenes,
                                          path,
                                          EditorUserBuildSettings.activeBuildTarget,
                                          options);
            }
        }
    }

    static BuildOptions GetBuildOptions(bool isAbb = false)
    {
        var playerOptionsInternal = new BuildPlayerOptions();
        var development = EditorUserBuildSettings.development;
        if (development)
            playerOptionsInternal.options |= BuildOptions.Development;
        if (EditorUserBuildSettings.allowDebugging & development)
            playerOptionsInternal.options |= BuildOptions.AllowDebugging;
        if (EditorUserBuildSettings.symlinkSources)
            playerOptionsInternal.options |= BuildOptions.SymlinkSources;
        if (EditorUserBuildSettings.enableHeadlessMode)
            playerOptionsInternal.options |= BuildOptions.EnableHeadlessMode;
        if (EditorUserBuildSettings.connectProfiler && (development || EditorUserBuildSettings.activeBuildTarget == BuildTarget.WSAPlayer))
            playerOptionsInternal.options |= BuildOptions.ConnectWithProfiler;
        if (EditorUserBuildSettings.buildWithDeepProfilingSupport & development)
            playerOptionsInternal.options |= BuildOptions.EnableDeepProfilingSupport;
        if (EditorUserBuildSettings.buildScriptsOnly)
            playerOptionsInternal.options |= BuildOptions.BuildScriptsOnly;
        if (isAbb)
        {
            EditorUserBuildSettings.buildAppBundle = true;
        }
        else
        {
            EditorUserBuildSettings.buildAppBundle = false;
        }
  
        return playerOptionsInternal.options;
    }

    private string GetFileExtensionForAndroid(bool isAAB)
    {
        if (isAAB)
            return ".aab";
        return ".apk";
       
    }

    private string GetFileExtension()
    {
        
        return EditorUserBuildSettings.activeBuildTarget switch
        {
            UnityEditor.BuildTarget.StandaloneOSX => ".app",
            UnityEditor.BuildTarget.Android => ".apk",
            UnityEditor.BuildTarget.StandaloneWindows64 => ".exe",
            UnityEditor.BuildTarget.WebGL => "/",
            UnityEditor.BuildTarget.iOS => "",
            UnityEditor.BuildTarget.StandaloneLinux64 => ".x86_64",
            _ => throw new NotImplementedException()
        };
    }

    private string GetAssetTypeAsString(ClientInfo.AssetBuildMode assetBuildMode)
    {
        return assetBuildMode switch
        {
            ClientInfo.AssetBuildMode.Streaming => "StreamingAssets",
            ClientInfo.AssetBuildMode.Remote => "RemoteAssets",
            _ => throw new NotImplementedException(),
        };
    }

    private AddressablesPlayerBuildResult BuildAsset(ClientInfo.AssetBuildMode assetBuildMode)
    {
        switch (assetBuildMode)
        {
            case ClientInfo.AssetBuildMode.Streaming:
                return AssetBuilder.BuildAssetToStreamingGroup();
            case ClientInfo.AssetBuildMode.Remote:
                return AssetBuilder.BuildAssetToRemoteGroup();
            default:
                return default;
        }
    }

    private void ShowVersionSettings()
    {
        EditorGUI.BeginChangeCheck();
        currentEnviroment = (ClientInfo.Enviroment)EditorGUILayout.EnumPopup("Current Enviroment", currentEnviroment);
        if (EditorGUI.EndChangeCheck())
        {
            GetVersionBaseOnEnviroment(currentEnviroment);
        }
        assetVersion = EditorGUILayout.TextField(AssetVersionLabel, assetVersion);
        currentClientVersion = EditorGUILayout.TextField("Client Version", currentClientVersion);
        if (GUILayout.Button("Apply"))
        {
            ApplySettings();
        }
    }

    private void ApplySettings()
    {
        if (AddressableAssetSettingsDefaultObject.Settings == null) Debug.LogError("Setting is null");
        AddressableAssetSettingsDefaultObject.Settings.activeProfileId = assetProfileId;
        AddressableAssetSettingsDefaultObject.Settings.profileSettings.SetValue(assetProfileId, AssetVersionLabel, assetVersion);
        clientInfo.CurrentEnviroment = currentEnviroment;
        clientInfo.Version = currentClientVersion;
        clientInfo.AssetVersion = assetVersion;
        EditorUtility.SetDirty(clientInfo);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void ShowClientBuildSetting()
    {
        isBuildClient = GUILayout.Toggle(isBuildClient, "Build Client");
    }

    private void ShowAssetBuildSetting()
    {
        isBuildAsset = GUILayout.Toggle(isBuildAsset, "Build Asset");
    }
    private void ShowAndroidBuilAAB()
    {
        isAndroidBuildAAB = GUILayout.Toggle(isAndroidBuildAAB, "Build Android AAB (Google Play AppBundle)");
    }

    private void BuildWebGLGame(string[] scenes, string path, BuildOptions options)
    {
        string desktopBuildName = "WebGL_Build";
        string mobileBuildName = "WebGL_Mobile";

        string desktopPath = $"{path}/{desktopBuildName}";
        string mobilePath = $"{path}/{mobileBuildName}";

        EditorUserBuildSettings.webGLBuildSubtarget = WebGLTextureSubtarget.DXT;
        BuildPipeline.BuildPlayer(scenes, desktopPath, BuildTarget.WebGL, options);

        EditorUserBuildSettings.webGLBuildSubtarget = WebGLTextureSubtarget.ASTC;
        BuildPipeline.BuildPlayer(scenes, mobilePath, BuildTarget.WebGL, options);

        // Copy the mobile.data file to the desktop build directory to consolidate them both
        CopyFileOrDirectory(Path.Combine(mobilePath, "Build", mobileBuildName + ".data"), Path.Combine(desktopPath, "Build", mobileBuildName + ".data"));
    }

    void CopyFileOrDirectory(string source, string destination)
    {
        if (File.Exists(source))
        {
            // Copy the file and overwrite if it already exists
            File.Copy(source, destination, true);
            Debug.Log("File copied and overwritten successfully.");
        }
        else if (Directory.Exists(source))
        {
            // Copy the directory and its contents
            CopyDirectory(source, destination);
            Debug.Log("Directory copied successfully.");
        }
        else
        {
            Debug.LogError("Source path does not exist.");
        }
    }

    void CopyDirectory(string sourceDir, string destinationDir)
    {
        Directory.CreateDirectory(destinationDir);

        foreach (string file in Directory.GetFiles(sourceDir))
        {
            string destFile = Path.Combine(destinationDir, Path.GetFileName(file));
            File.Copy(file, destFile, true);
        }

        foreach (string dir in Directory.GetDirectories(sourceDir))
        {
            string destDir = Path.Combine(destinationDir, Path.GetFileName(dir));
            CopyDirectory(dir, destDir);
        }
    }
}
