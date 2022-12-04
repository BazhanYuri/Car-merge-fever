using UnityEditor;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public class EditorSettingsProvider : AssetPostprocessor, ISettingsProvider
    {
        private static readonly string DEFINE_SYMBOL = "HOMA_BELLY_EDITOR_ANALYTICS_ENABLED"; 
        
        [InitializeOnLoadMethod]
        static void RegisterSettings()
        {
            DefineSymbolsUtility.TrySetInitialValue(DEFINE_SYMBOL, true, "homagames.editor_analytics_enabled_on_first_install");
            
            Settings.RegisterSettings(new EditorSettingsProvider());
        }

        public int Order => 2;
        public string Name => "Unity Editor";
        public string Version => "";
        
        public void Draw()
        {
            bool value = DefineSymbolsUtility.GetDefineSymbolValue(DEFINE_SYMBOL);
            bool newValue = EditorGUILayout.Toggle(new GUIContent("Editor Analytics", "Enable Unity Editor Analytics to be sent to Homa Games to improve its products"), value);
                
            if (value != newValue)
                DefineSymbolsUtility.SetDefineSymbolValue(DEFINE_SYMBOL, newValue);
        }
        
#if HOMA_BELLY_EDITOR_ANALYTICS_ENABLED
        /// <summary>
        /// To fully support Homa Belly's Delete All Packages feature, if HOMA_BELLY_EDITOR_ANALYTICS_ENABLED is enabled,
        /// we do need to disable it due to the Installer dependency on Core and this define (see EditorAnalyticsProxy.cs).
        /// To do so, we intercept this class (EditorSettingsProvider) deletion and disable the define symbol if enabled.
        /// </summary>
        /// <param name="importedAssets"></param>
        /// <param name="deletedAssets"></param>
        /// <param name="movedAssets"></param>
        /// <param name="movedFromAssetPaths"></param>
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            for (int i = 0; i < deletedAssets.Length; i++)
            {
                if (deletedAssets[i].EndsWith(nameof(EditorSettingsProvider) + ".cs"))
                {
                    // Deleting Core package. Remove HOMA_BELLY_EDITOR_ANALYTICS_ENABLED define symbol
                    HomaBellyEditorLog.Debug($"Disabling {DEFINE_SYMBOL}");
                    DefineSymbolsUtility.SetDefineSymbolValue(DEFINE_SYMBOL, false);
                }
            }
        }
#endif
    }
}