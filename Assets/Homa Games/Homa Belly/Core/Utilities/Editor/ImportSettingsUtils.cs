using System.IO;
using UnityEditor;
using UnityEngine;

namespace HomaGames.HomaBelly.Utilities
{
    /// <summary>
    /// Utility class to update Import Settings for plugins
    /// </summary>
    public class ImportSettingsUtils
    {
        /// <summary>
        /// Update Import Settings for a given plugin at 'filePath' and 'target' platform
        /// </summary>
        /// <param name="filePath">The file path (from Assets) to be updated. ie: Assets/Homa Games/Homa Belly/Core/DataPrivacy/Plugins/iOS/AppTrackingTransparency.h</param>
        /// <param name="target">The BuildTarget platform: BuildTarget.Android or BuildTarget.iOS</param>
        /// <param name="enabled">True if plugin should be added to the build, false otherwise</param>
        public static void UpdateImportSettings(string filePath, BuildTarget target, bool enabled = true)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError("[Import Settings Utils] Plugin doesn't exist: "+filePath);
                return;
            }
            
            var pluginImporter = AssetImporter.GetAtPath(filePath) as PluginImporter;
            if (pluginImporter == null)
            {
                Debug.LogError("[Import Settings Utils] The file isn't a plugin: "+filePath);
                return;
            }
            
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            pluginImporter.SetCompatibleWithPlatform(target, enabled);

            EditorUtility.SetDirty(pluginImporter);
            pluginImporter.SaveAndReimport();
        }
    }
}