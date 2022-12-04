using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public class InitialConfiguration : IPreprocessBuildWithReport
    {
        private const string SESSION_SDK_CHECK_KEY = "homa_belly.androidtargetpopup";
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
#if HOMA_DEVELOPMENT
            ForceDevelopmentBuild();
            UnityEngine.Debug.LogWarning(
                "Build started with HOMA_DEVELOPMENT Setting enabled. This build should not be pushed to the stores.");
#endif
        }

        [InitializeOnLoadMethod]
        static void Configure()
        {
            #region Android settings

#if HOMA_DEVELOPMENT
            ForceDevelopmentBuild();
#else
            // Gradle build system
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            EnsureMinimumAndroidSdkVersion();
            EnsureTargetAndroidSdkVersion();
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.iOS, ApiCompatibilityLevel.NET_4_6);
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android, ApiCompatibilityLevel.NET_4_6);
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.Low);
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.iOS, ManagedStrippingLevel.Low);
#endif

            ConfigureGradleTemplate();

            #endregion

            HomaBellyEditorLog.Debug("Project configured");
        }

        private static void EnsureTargetAndroidSdkVersion()
        {
            if (Application.isBatchMode)
            {
                return;
            }
            
#if UNITY_ANDROID
            if (SessionState.GetBool(SESSION_SDK_CHECK_KEY, false))
                return;
            if (!HomaBellyManifestConfiguration.TryGetInt(out int requiredVersion, "homabelly_core",
                    "i_android_target_sdk_version")) return;

            if (PlayerSettings.Android.targetSdkVersion >= (AndroidSdkVersions) requiredVersion
                || PlayerSettings.Android.targetSdkVersion == AndroidSdkVersions.AndroidApiLevelAuto)
                return;

            int userResponse = EditorUtility.DisplayDialogComplex("Target android version too low",
                "Your app doesn't meet Google's target API level requirement.",
                $"Change to {requiredVersion}", "I will do it myself", "Change to latest available");


            if (userResponse == 0)
            {
                PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions) requiredVersion;
            }
            else if (userResponse == 2)
            {
                PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            }

            SessionState.SetBool(SESSION_SDK_CHECK_KEY, true);
#endif
        }

        private static void EnsureMinimumAndroidSdkVersion()
        {
            HomaBellyManifestConfiguration.TryGetInt(out int manifestRequiredMinimumVersion, "homabelly_core",
                "i_android_minimum_sdk_version");

            PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions) Mathf.Max(
                (int) PlayerSettings.Android.minSdkVersion, 22, manifestRequiredMinimumVersion);
        }

        private static void ConfigureGradleTemplate()
        {
            string mainTemplateGradlePath = Application.dataPath + "/Plugins/Android/mainTemplate.gradle";
            if (File.Exists(mainTemplateGradlePath))
            {
                HomaBellyEditorLog.Debug("Gradle file detected");
            }
        }

        /// <summary>
        /// Forcing debug key, those builds cannot be pushed to the stores
        /// </summary>
        private static void ForceDevelopmentBuild()
        {
            PlayerSettings.Android.useCustomKeystore = false;
        }
    }
}