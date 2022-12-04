using HomaGames.HomaBelly.Utilities;
using UnityEditor;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Helper class to automate migrations from legacy Homa Belly
    /// versions to latest ones
    /// </summary>
    public class MigrationHelper
    {
        private const string LATEST_VERSION_INSTALLED_KEY = "com.homagames.homabelly.latest_installed_version";

        [InitializeOnLoadMethod]
        static void MigrateToLatest()
        {
            string currentVersion = HomaBellyConstants.PRODUCT_VERSION;
            string latestInstalledVersion = EditorPrefs.GetString(LATEST_VERSION_INSTALLED_KEY, "");

            // If the same version, do nothing
            if (!string.IsNullOrEmpty(latestInstalledVersion) && latestInstalledVersion == currentVersion)
            {
                return;
            }

            // If latest installed version is not available, HB is prior v1.2.0
            if (string.IsNullOrEmpty(latestInstalledVersion))
            {
                // Migrate from prior 1.2.0
                HomaGamesLog.Debug("Migrating Homa Belly from prior v1.2 version");

                // Move Android and iOS Utilities to core package
                AssetDatabase.MoveAsset("Assets/Homa Games/Homa Belly/Plugin Manifest/Editor/Utilities/Android", "Assets/Homa Games/Homa Belly/Core/Utilities/Editor/Android");
                AssetDatabase.MoveAsset("Assets/Homa Games/Homa Belly/Plugin Manifest/Editor/Utilities/iOS", "Assets/Homa Games/Homa Belly/Core/Utilities/Editor/iOS");
                AssetDatabase.MoveAsset("Assets/Homa Games/Homa Belly/Plugin Manifest/Editor/InitialConfiguration.cs", "Assets/Homa Games/Homa Belly/Core/Utilities/Editor/InitialConfiguration.cs");
                AssetDatabase.Refresh();

                // Completed
                HomaGamesLog.Debug("Migration completed");
            }

            // Save latest installed version as current one
            EditorPrefs.SetString(LATEST_VERSION_INSTALLED_KEY, currentVersion);
        }
    }
}
