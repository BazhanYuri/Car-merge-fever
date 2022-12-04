using GameAnalyticsSDK;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public class GameAnalyticsPreBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            CheckGameAnalyticsSettings();
        }

        private static void CheckGameAnalyticsSettings()
        {
            var gaSettings = Resources.Load("GameAnalytics/Settings", typeof(GameAnalyticsSDK.Setup.Settings));
            if (gaSettings == null)
            {
                TryRegenerateGaSettings();
            }
            
            SerializedObject o = new SerializedObject(gaSettings);
            var prop = o.FindProperty("m_EditorClassIdentifier");

            if (!string.IsNullOrEmpty(prop.stringValue))
            {
                TryRegenerateGaSettings();
            }
        }

        private static void ThrowSettingsException()
        {
            throw new BuildFailedException("Your GameAnalytics settings file is corrupted, please delete Assets/Resources/GameAnalytics/Settings.asset and regenerate it doing Window/GameAnalytics/Select Settings.");
        }

        private static void TryRegenerateGaSettings()
        {
            var gaSettings = Resources.Load("GameAnalytics/Settings", typeof(GameAnalyticsSDK.Setup.Settings));
            if (gaSettings)
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(gaSettings));
                AssetDatabase.Refresh();
            }

            if (GameAnalytics.SettingsGA)
                GameAnalyticsPostprocessor.Configure();
            else
                ThrowSettingsException();
        }
    }
}