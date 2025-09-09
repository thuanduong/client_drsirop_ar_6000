
// Using UNITY_IOS preprocessor because 'using UnityEditor.iOS.Xcode' is only available with iOS build tools
#if UNITY_IOS

using System.IO;
using UnityEditor;
using UnityEditor.iOS.Xcode;
using UnityEngine;


internal class ProjectExporterIos : ProjectExporter
{

    protected override void TransformExportedProject(string exportPath)
    {
        Debug.Log("Transforming Unity export for Flutter integration...");

        FileInfo pbxProjFileInfo = new FileInfo(Path.Combine(exportPath, "Unity-iPhone.xcodeproj", "project.pbxproj"));

        if(!pbxProjFileInfo.Exists) {
            ProjectExportHelpers.ShowErrorMessage($"Error transforming iOS export: couldn't find {pbxProjFileInfo.FullName}");
            return;
        }

        // We're using the GNU linker flag -U to force the FlutterEmbedUnityIosSendToFlutter symbol to be entered 
        // in the output file as an undefined symbol. This is incompatible with bitcode, with this error message
        // during build:
        // Error (Xcode): -U and -bitcode_bundle (Xcode setting ENABLE_BITCODE=YES) cannot be used together
        //
        // By default, the Unity project is build with bitcode enabled. Luckily, bitcode is now deprecated -
        // From Apple’s release note of Xcode 14 (https://developer.apple.com/documentation/xcode-release-notes/xcode-14-release-notes):
        // 
        // "Starting with Xcode 14, bitcode is no longer required for watchOS and tvOS applications, and the App Store no longer accepts 
        // bitcode submissions from Xcode 14.
        // Xcode no longer builds bitcode by default and generates a warning message if a project explicitly enables bitcode: “Building 
        // with bitcode is deprecated. Please update your project and/or target settings to disable bitcode.”
        // The capability to build with bitcode will be removed in a future Xcode release."
        //
        // So, disable bitcode for the Unity export project:
        PBXProject pbxProject = new PBXProject();
        pbxProject.ReadFromFile(pbxProjFileInfo.FullName);
        pbxProject.SetBuildProperty(pbxProject.ProjectGuid(), "ENABLE_BITCODE", "NO");

        // To be able to pass messages from Unity C# to the iOS part of the plugin (which then passes the message on to Flutter) 
        // the C# file you include in your Unity project declares (but does not define) a function called `FlutterEmbedUnityIos_sendToFlutter`.
        // This function is instead defined in the plugin. To join these two things together, we need to tell Xcode to ignore the fact that 
        // `FlutterEmbedUnityIos_sendToFlutter` is not defined in the Unity module, and instead link it to the definition in the plugin.
        // `-Wl` allows us to pass additional options to the linker when it is invoked
        // `-U` tells the linker to force the symbol `_FlutterEmbedUnityIos_sendToFlutter` to be entered in the output file as an undefined 
        //      symbol. It will be linked instead to a function defined in the plugin.
        pbxProject.AddBuildProperty(pbxProject.ProjectGuid(), "OTHER_LDFLAGS", "-Wl,-U,_FlutterEmbedUnityIos_sendToFlutter");

        // Change the Data folder Target Membership
        // This allows us to embed the Unity engine and the Unity game easily into the Flutter app as a single unit
        string dataGuid = pbxProject.FindFileGuidByProjectPath("Data");
        if(!string.IsNullOrEmpty(dataGuid)) {
            // Add the Data folder to the UnityFramework target
            string frameworkGuid = pbxProject.GetUnityFrameworkTargetGuid();
            pbxProject.AddFileToBuild(frameworkGuid, dataGuid);

            // Remove the Data folder from the Unity-iPhone target
            string mainGuid = pbxProject.GetUnityMainTargetGuid();
            pbxProject.RemoveFileFromBuild(mainGuid, dataGuid);
        }
        else {
            Debug.LogError("Could not find the 'Data' folder in the `Unity-iPhone project.");
        }

        // Save changes
        pbxProject.WriteToFile(pbxProjFileInfo.FullName);

        // Delete the BurstDebugInformation folder
        DirectoryInfo burstDebugInformation = new DirectoryInfo(Path.Join(exportPath, "..", "unityLibrary_BurstDebugInformation_DoNotShip"));
        if(burstDebugInformation.Exists) {
            Directory.Delete(burstDebugInformation.FullName, true);
            Debug.Log($"Deleted {burstDebugInformation.FullName}");
        }

        Debug.Log("Transforming Unity export for Flutter integration complete");
    }
}

#endif
