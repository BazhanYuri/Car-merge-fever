using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameAnalyticsSDK;
using GameAnalyticsSDK.Setup;
using UnityEditor;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public class GameAnalyticsPostprocessor : AssetPostprocessor
    {
        private const string LAST_CUSTOM_DIMENSIONS_CONFIGURATION_CHECK_TIMESTAMP_KEY = "homagames.homabelly.gameanalytics.custom_dimensions_configuration";
        private const int HOURS_UNTIL_NEXT_CUSTOM_DIMENSIONS_CHECK = 12;
        private const string SETTINGS_PATH = "Assets/Resources/GameAnalytics/Settings.asset";
        private static bool is_importing = false;

        private static int androidPlatformIndex;
        private static int iosPlatformIndex;

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (importedAssets.Contains(SETTINGS_PATH))
            {
                if (!is_importing)
                {
                    is_importing = true;
                    Configure();
                }
                else
                {
                    is_importing = false;
                }
            }
        }

        [InitializeOnLoadMethod]
        public static void Configure()
        {
            // If the asset file is here, but not yet loaded in the asset database, 
            // we do not want to try to configure GameAnalytics, or the SDK will erase
            // the existing file. If there is no settings file in the folder, we still configure
            // GameAnalytics, and the SDK will create the asset for us.
            //
            // Be careful that to get settings asset, GameAnalytics uses the Resources class and 
            // not the AssetDatabase. Apparently the Resources class is updated later than the AssetDatabase.
            // That is why for this test to be consistent with GA's way of doing things, we check
            // in the resources. 
            if (File.Exists(SETTINGS_PATH) && !Resources.Load<GameAnalyticsSDK.Setup.Settings>("GameAnalytics/Settings")) {
                return;
            } 
            
            // If Unity is executed in batch mode (CI or build machines), do not
            // modify nor configure settings, as this is only intended
            // when the developer integrates the SDK for the very first time
            if (UnityEditorInternal.InternalEditorUtility.inBatchMode)
            {
                return;
            }

            HomaBellyEditorLog.Debug($"Configuring {HomaBellyGameAnalyticsConstants.ID}");
            PluginManifest pluginManifest = PluginManifest.LoadFromLocalFile();

            if (pluginManifest != null)
            {
                PackageComponent packageComponent = pluginManifest.Packages
                    .GetPackageComponent(HomaBellyGameAnalyticsConstants.ID, HomaBellyGameAnalyticsConstants.TYPE);
                if (packageComponent != null)
                {
                    Dictionary<string, string> configurationData = packageComponent.Data;

                    // Setup Game Analytics Settings
                    GameAnalytics.SettingsGA.UsePlayerSettingsBuildNumber = true;
                    GatherPlatformIndexes();

                    try
                    {
                        // Update keys
                        if (configurationData.ContainsKey("s_android_game_key"))
                        {
                            GameAnalyticsSDK.Setup.Settings.UpdateKeys(androidPlatformIndex,
                                configurationData["s_android_game_key"], 
                                configurationData["s_android_secret_key"]);
                        }

                        if (configurationData.ContainsKey("s_ios_game_key"))
                        {
                            GameAnalyticsSDK.Setup.Settings.UpdateKeys(iosPlatformIndex,
                                configurationData["s_ios_game_key"], 
                                configurationData["s_ios_secret_key"]);
                        }

                        // Determine if GA must submit FPS events or not
                        if (configurationData.ContainsKey("b_submit_fps"))
                        {
                            bool submitFps = true;
                            bool.TryParse(configurationData["b_submit_fps"], out submitFps);
                            GameAnalytics.SettingsGA.SubmitFpsAverage = submitFps;
                            GameAnalytics.SettingsGA.SubmitFpsCritical = submitFps;
                        }

                        // Configure Custom Dimensions 01 and 02 for automated Homa Games NTesting values
                        ConfigureCustomDimensionsForNTesting(configurationData);

                        if (!EditorApplication.isPlaying && !is_importing)
                        {
                            // Mark Game Analytics to dirty to force save
                            EditorUtility.SetDirty(GameAnalytics.SettingsGA);
                            EditorApplication.delayCall += AssetDatabase.SaveAssets;
                        }
                    }
                    catch (System.Exception e)
                    {
                        HomaBellyEditorLog.Error($"Error configuring Game Analytics: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Try to configure GA Custom Dimensions to automatically work with NTesting. If an update is required,
        /// this will ask for developers permission.
        /// </summary>
        /// <param name="configurationData">The Homa Belly configuration data for GA</param>
        private static void ConfigureCustomDimensionsForNTesting(Dictionary<string, string> configurationData)
        {
            var gameAnalyticsCustomDimensionData = GameAnalyticsCustomDimensionData.instance;
            
            if (gameAnalyticsCustomDimensionData.ForceDisableCustomDimensionIntegration)
                return;
            
            // Only configure custom dimensions if Geryon is detected within this project
            Type geryonConfig = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                 from type in assembly.GetTypes()
                                 where type.Namespace == "HomaGames.Geryon" && type.Name == "Config"
                                 select type).FirstOrDefault();
            if (geryonConfig == null)
            {
                HomaGamesLog.Debug("Current Homa Belly installation does not contain NTesting. No Game Analytics Custom Dimensions setup is required");
                return;
            }

            // Gather incoming configuration values
            List<string> configurationCustomDimensions01 = GetConfigurationCustomDimensions(configurationData, "s_custom_dimensions_01_csv", new List<string>()
            {
                "001", "002", "003", "004", "005",
                "006", "007", "008", "009", "010",
                "011", "012", "013", "014", "015",
                "016", "017", "018", "019", "020",
            });

            List<string> configurationCustomDimensions02 = GetConfigurationCustomDimensions(configurationData, "s_custom_dimensions_02_csv", new List<string>()
            {
                "Z", "A", "B", "C", "D",
                "E", "F", "G", "H", "I",
                "J", "K", "L", "M", "N",
                "O", "P", "Q", "R", "S",
            });

            List<string> configurationCustomDimensions03 = GetConfigurationCustomDimensions(configurationData, "s_custom_dimensions_03_csv", new List<string>()
            {
                "V1", "V2", "V3", "V4", "V5",
                "V6", "V7", "V8", "V9", "V10",
                "V11", "V12", "V13", "V14", "V15",
                "V16", "V17", "V18", "V19", "V20",
            });
            
            if (! gameAnalyticsCustomDimensionData.AreCustomDimensionsManuallySet(
                    configurationCustomDimensions01, configurationCustomDimensions02, configurationCustomDimensions03))
            {
                if (ShouldUpdateCustomDimensions(GameAnalytics.SettingsGA.CustomDimensions01,
                        configurationCustomDimensions01)
                    || ShouldUpdateCustomDimensions(GameAnalytics.SettingsGA.CustomDimensions02,
                        configurationCustomDimensions02)
                    || ShouldUpdateCustomDimensions(GameAnalytics.SettingsGA.CustomDimensions03,
                        configurationCustomDimensions03))
                {
                    GameAnalytics.SettingsGA.CustomDimensions01 = configurationCustomDimensions01;
                    GameAnalytics.SettingsGA.CustomDimensions02 = configurationCustomDimensions02;
                    GameAnalytics.SettingsGA.CustomDimensions03 = configurationCustomDimensions03;

                    gameAnalyticsCustomDimensionData.SaveCustomDimensions(configurationCustomDimensions01,
                        configurationCustomDimensions02, configurationCustomDimensions03);
                }
            }
        }

        /// <summary>
        /// Determines if the current GA configuration already has custom dimensions set
        /// and if they are the required ones for NTesting.
        /// </summary>
        /// <param name="customDimensions">List of current configured custom dimensions</param>
        /// <param name="configurationCustomDimensions">List of custom dimensions Homa Belly requires</param>
        /// <returns>True if the custom dimensions need to be updated</returns>
        private static bool ShouldUpdateCustomDimensions(List<string> customDimensions, List<string> configurationCustomDimensions)
        {
            bool alreadyHasCustomDimensions = customDimensions != null && customDimensions.Count > 0;

            // If Custom Dimensions are already in place, check if an update is required
            bool customDimensionsUpdateRequired = true;
            if (alreadyHasCustomDimensions)
            {
                bool areEqual = customDimensions.All(configurationCustomDimensions.Contains)
                    && customDimensions.Count == configurationCustomDimensions.Count;
                customDimensionsUpdateRequired = !areEqual;
            }

            return customDimensionsUpdateRequired;
        }

        /// <summary>
        /// Obtains a list of Custom Dimensions from the incoming Homa Belly configuration (if any).
        /// Otherwise, returns the `defaultValues` list
        /// </summary>
        /// <param name="configurationData">The Homa Belly configuration data for GA</param>
        /// <param name="jsonFieldKey">The json key to look for the list</param>
        /// <param name="defaultValues">Default values as fallback</param>
        /// <returns></returns>
        private static List<string> GetConfigurationCustomDimensions(Dictionary<string, string> configurationData, string jsonFieldKey, List<string> defaultValues)
        {
            // If we receive values from configuration data, use those
            if (configurationData.ContainsKey(jsonFieldKey))
            {
                try
                {
                    string[] values = configurationData[jsonFieldKey].ToString().Split(',');
                    return new List<string>(values);
                }
                catch (Exception e)
                {
                    HomaGamesLog.Warning(string.Format("Could not deserialize GA Custom Dimensions: {0}", e.Message));
                }
            }

            return defaultValues;
        }


        /// <summary>
        /// Obtains ANDROID and IOS Game Analytics platform indexed. If no
        /// platforms found, this method creates them.
        /// </summary>
        private static void GatherPlatformIndexes()
        {
            bool androidPlatformFound = false;
            bool iosPlatformFound = false;

            // Platforms already available in settings. Gather their indexes
            if (GameAnalytics.SettingsGA.Platforms != null)
            {
                for (int i = 0; i < GameAnalytics.SettingsGA.Platforms.Count; i++)
                {
                    RuntimePlatform platform = GameAnalytics.SettingsGA.Platforms[i];
                    if (platform == RuntimePlatform.Android)
                    {
                        androidPlatformIndex = i;
                        androidPlatformFound = true;
                    }

                    if (platform == RuntimePlatform.IPhonePlayer)
                    {
                        iosPlatformIndex = i;
                        iosPlatformFound = true;
                    }
                }
            }

            // If no ANDROID found, create and update index
            if (!androidPlatformFound)
            {
                HomaBellyEditorLog.Debug($"Creating Game Analytics Android platform");
                GameAnalytics.SettingsGA.AddPlatform(UnityEngine.RuntimePlatform.Android);
                androidPlatformIndex = GameAnalytics.SettingsGA.Platforms.Count - 1;
            }

            // If no IOS found, create and update index
            if (!iosPlatformFound)
            {
                HomaBellyEditorLog.Debug($"Creating Game Analytics iOS platform");
                GameAnalytics.SettingsGA.AddPlatform(UnityEngine.RuntimePlatform.IPhonePlayer);
                iosPlatformIndex = GameAnalytics.SettingsGA.Platforms.Count - 1;
            }
        }
    }
}
