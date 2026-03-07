// CrossHealth - Unity Plugin for HealthKit & Health Connect
// iOS PostProcessBuild Script
// Copyright (c) 2025. All rights reserved.

#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

namespace CrossHealth.Editor
{
    /// <summary>
    /// Post-process build script that automatically configures the Xcode project
    /// for HealthKit integration when building for iOS.
    ///
    /// Performed actions:
    /// 1. Adds HealthKit capability to the Xcode project
    /// 2. Adds HealthKit framework to linked frameworks
    /// 3. Adds required privacy descriptions to Info.plist
    /// </summary>
    public static class CrossHealthPostProcessor
    {
        /// <summary>
        /// The usage description shown to users when requesting HealthKit read access.
        /// Override this before building if you want a custom message.
        /// </summary>
        public static string HealthShareUsageDescription =
            "This app reads your health data to provide personalized fitness insights and track your wellness progress.";

        /// <summary>
        /// The usage description shown to users when writing to HealthKit.
        /// Required even if not currently writing data, for future compatibility.
        /// </summary>
        public static string HealthUpdateUsageDescription =
            "This app may save health and fitness data to Apple Health.";

        [PostProcessBuild(100)]
        public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target != BuildTarget.iOS)
                return;

            // ================================================================
            // 1. Modify Xcode project settings
            // ================================================================
            string projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
            PBXProject project = new PBXProject();
            project.ReadFromFile(projectPath);

            // Get the main target GUID
            string mainTargetGuid = project.GetUnityMainTargetGuid();
            string frameworkTargetGuid = project.GetUnityFrameworkTargetGuid();

            // Add HealthKit framework
            project.AddFrameworkToProject(mainTargetGuid, "HealthKit.framework", false);
            project.AddFrameworkToProject(frameworkTargetGuid, "HealthKit.framework", false);

            // Save project modifications
            project.WriteToFile(projectPath);

            // ================================================================
            // 2. Add HealthKit capability via entitlements
            // ================================================================
            string entitlementsPath = pathToBuiltProject + "/Unity-iPhone/Unity-iPhone.entitlements";
            ProjectCapabilityManager capabilityManager = new ProjectCapabilityManager(
                projectPath,
                entitlementsPath,
                null,
                mainTargetGuid
            );

            capabilityManager.AddHealthKit();
            capabilityManager.WriteToFile();

            // ================================================================
            // 3. Modify Info.plist
            // ================================================================
            string plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(plistPath);

            PlistElementDict rootDict = plist.root;

            // Add HealthKit usage descriptions
            if (!rootDict.values.ContainsKey("NSHealthShareUsageDescription"))
            {
                rootDict.SetString("NSHealthShareUsageDescription", HealthShareUsageDescription);
            }

            if (!rootDict.values.ContainsKey("NSHealthUpdateUsageDescription"))
            {
                rootDict.SetString("NSHealthUpdateUsageDescription", HealthUpdateUsageDescription);
            }

            // Add HealthKit to UIRequiredDeviceCapabilities (optional, use if you want
            // to restrict the app to devices with HealthKit)
            // Uncomment the following if you want to make HealthKit a requirement:
            // PlistElementArray capabilities = rootDict.CreateArray("UIRequiredDeviceCapabilities");
            // capabilities.AddString("healthkit");

            plist.WriteToFile(plistPath);

            UnityEngine.Debug.Log("[CrossHealth] iOS PostProcessBuild completed: HealthKit capability and Info.plist configured.");
        }
    }
}
#endif
