#if UNITY_IOS
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEditor.Build.Reporting;

namespace HomaGames.HomaBelly.Utilities
{
    /// <summary>
    /// Since Core v1.6.0, developers can disable AppTrackingTransparency (aka IDFA) feature from Homa Lab
    /// or Data Privacy Settings. If AppTrackingTransparency is disabled, we do not want to include either
    /// AppTrackingTransparency.framework (see AppTrackingTransparencyPostProcessor) nor native iOS code
    /// referencing that framework.
    ///
    /// This PreprocessBuild class takes care of enabling/disabling iOS native code when AppTrackingTransparency
    /// is enabled/disabled
    /// </summary>
    public class AppTrackingTransparencyPreprocessBuild : IPreprocessBuildWithReport
    {
        // Source path to be included in the build if IDFA is enabled in the project
        private const string AttIOSSourcePath = "Assets/Homa Games/Homa Belly/Core/DataPrivacy/Plugins/iOS/AppTrackingTransparency.mm";
        // NOOP source path to be included in the build if IDFA is not enabled in the project
        private const string AttIOSSourceNoopPath = "Assets/Homa Games/Homa Belly/Core/DataPrivacy/Plugins/iOS/AppTrackingTransparencyNoop.mm";
        
        // We want to call it after default callbacks
        public int callbackOrder { get; } = 10;

        public void OnPreprocessBuild(BuildReport report)
        {
            bool enableAttFiles = true;

            DataPrivacy.Settings settings = DataPrivacy.Settings.EditorCreateOrLoadDataPrivacySettings();
            if (settings != null)
            {
                enableAttFiles = settings.ShowIdfa;
            }

            Debug.Log($"[App Tracking Transparency] IDFA gathering allowed: '{enableAttFiles}'. Updating AppTrackingTransparency Import Settings...");
            ImportSettingsUtils.UpdateImportSettings(AttIOSSourcePath, BuildTarget.iOS, enableAttFiles);
            ImportSettingsUtils.UpdateImportSettings(AttIOSSourceNoopPath, BuildTarget.iOS, !enableAttFiles);
        }
    }
}
#endif