using System.Collections.Generic;
using System.Linq;
using GameAnalyticsSDK;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    [FilePath("Assets/Homa Games/Homa Belly/Preserved/GameAnalytics/CustomDimensionData.asset", FilePathAttribute.Location.ProjectFolder)]
    public class GameAnalyticsCustomDimensionData : ScriptableSingleton<GameAnalyticsCustomDimensionData>
    {
        public bool ForceDisableCustomDimensionIntegration;

        [SerializeField] 
        private bool LastManuallySet;

        [SerializeField]
        private List<string> PreviousCustomDimension1;
        [SerializeField]
        private List<string> PreviousCustomDimension2;
        [SerializeField]
        private List<string> PreviousCustomDimension3;

        private List<string>[] PreviousCustomDimensions => new[]
            { PreviousCustomDimension1, PreviousCustomDimension2, PreviousCustomDimension3 };

        public bool AreCustomDimensionsManuallySet(List<string> customDimension1, List<string> customDimension2, List<string> customDimension3)
        {
            var GameAnalyticsCustomDimensions = new[] { 
                GameAnalytics.SettingsGA.CustomDimensions01,
                GameAnalytics.SettingsGA.CustomDimensions02,
                GameAnalytics.SettingsGA.CustomDimensions03 };

            var customDimensions = new[]
            {
                customDimension1,
                customDimension2,
                customDimension3,
            };
        
            for (var i = 0; i < GameAnalyticsCustomDimensions.Length; i++)
            {
                var gameAnalyticsCustomDimension = GameAnalyticsCustomDimensions[i];
                var customDimension = customDimensions[i];
                var previousCustomDimension = PreviousCustomDimensions[i];

                if (gameAnalyticsCustomDimension.Any(value => !string.IsNullOrWhiteSpace(value)))
                {
                    if (! gameAnalyticsCustomDimension.SequenceEqual(previousCustomDimension ?? new List<string>())
                        && ! gameAnalyticsCustomDimension.SequenceEqual(customDimension ?? new List<string>()))
                    {
                        LastManuallySet = true;
                        return true;
                    }
                }
            }

            LastManuallySet = false;
            return false;
        }
    
        public void SaveCustomDimensions(List<string> customDimension1, List<string> customDimension2, List<string> customDimension3)
        {
            PreviousCustomDimension1 = customDimension1;
            PreviousCustomDimension2 = customDimension2;
            PreviousCustomDimension3 = customDimension3;
        
            Save(true);
        }
    
    
        #region Settings provider
    
        [InitializeOnLoadMethod]
        static void RegisterSettings()
        {
            Settings.RegisterSettings(new SettingsProvider());
        }

        private class SettingsProvider : ISettingsProvider
        {
            public int Order => 15;
            public string Name => "Game Analytics";
            public string Version => string.Empty;
            public void Draw()
            {
                bool newForceDisable = EditorGUILayout.Toggle(
                    new GUIContent("Disable CD Integration", "Prevents N-Testing from updating GameAnalytics' custom dimensions. Disables some N-Testing features."), 
                    instance.ForceDisableCustomDimensionIntegration);
            
                if (instance.ForceDisableCustomDimensionIntegration != newForceDisable)
                {
                    instance.ForceDisableCustomDimensionIntegration = newForceDisable;
                    instance.Save(true);
                }

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Toggle(new GUIContent("Dimensions manually set", "If custom dimensions are detected as manually set, the package will not update " +
                    "them with N-Testing values. This may disable some N-Testing features."), instance.LastManuallySet);
                EditorGUI.EndDisabledGroup();
                if (instance.LastManuallySet && ! instance.ForceDisableCustomDimensionIntegration)
                {
                    if (GUILayout.Button("Clear manually set dimensions"))
                    {
                        GameAnalytics.SettingsGA.CustomDimensions01 = new List<string>();
                        GameAnalytics.SettingsGA.CustomDimensions02 = new List<string>();
                        GameAnalytics.SettingsGA.CustomDimensions03 = new List<string>();

                        CompilationPipeline.RequestScriptCompilation();
                    }
                }
            }
        }
    
        #endregion
    }
}