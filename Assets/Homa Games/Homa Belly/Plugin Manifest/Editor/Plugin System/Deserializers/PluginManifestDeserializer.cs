using System;
using System.Collections.Generic;
using System.IO;
using HomaGames.HomaBelly.Installer;
using HomaGames.HomaBelly.Installer.Utilities;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public class PluginManifestDeserializer : IModelDeserializer<PluginManifest>
    {
        public static string LOCAL_PLUGIN_MANIFEST_FILE = Application.dataPath + "/Homa Games/Homa Belly/Plugin Manifest/Editor/Plugin System/PluginManifest.json";

        private bool cacheEnabled = true;

        public PluginManifestDeserializer(bool cacheEnabled = true)
        {
            this.cacheEnabled = cacheEnabled;
        }

        public PluginManifest Deserialize(string json)
        {
            PluginManifest manifest = new PluginManifest();

            // Return empty manifest if json string is not valid
            if (string.IsNullOrEmpty(json))
            {
                return manifest;
            }

            // Basic info
            Dictionary<string, object> dictionary = InstallerJson.Deserialize(json) as Dictionary<string, object>;
            if (dictionary != null)
            {
                manifest.AppToken = (string)dictionary["ti"];

                // Packages
                Dictionary<string, object> packagesDictionary = dictionary["res"] as Dictionary<string, object>;
                AvailablePackages availablePackages = new AvailablePackages();
                availablePackages.Id = Convert.ToInt32(packagesDictionary["i_manifest_id"]);
                availablePackages.Name = Convert.ToString(packagesDictionary["s_manifest_name"]);

                if (packagesDictionary.ContainsKey("s_manifest_version_id"))
                {
                    availablePackages.ManifestVersionId = Convert.ToString(packagesDictionary["s_manifest_version_id"]);
                }

                // Core packages
                availablePackages.CorePackages = new List<PackageComponent>();
                if (packagesDictionary.ContainsKey("ao_core_packages"))
                {
                    List<object> corePackages = packagesDictionary["ao_core_packages"] as List<object>;
                    for (int i = 0; corePackages != null && i < corePackages.Count; i++)
                    {
                        Dictionary<string, object> dict = corePackages[i] as Dictionary<string, object>;
                        availablePackages.CorePackages.Add(PackageComponent.FromDictionary(dict));
                    }
                }

                // Mediation layers
                List<object> mediationLayers = packagesDictionary["ao_mediation_layers"] as List<object>;
                availablePackages.MediationLayers = new List<PackageComponent>();
                for (int i = 0; mediationLayers != null && i < mediationLayers.Count; i++)
                {
                    Dictionary<string, object> dict = mediationLayers[i] as Dictionary<string, object>;
                    availablePackages.MediationLayers.Add(PackageComponent.FromDictionary(dict));
                }

                // Attribution platforms
                List<object> attributionPlatforms = packagesDictionary["ao_attribution_platforms"] as List<object>;
                availablePackages.AttributionPlatforms = new List<PackageComponent>();
                for (int i = 0; attributionPlatforms != null && i < attributionPlatforms.Count; i++)
                {
                    Dictionary<string, object> dict = attributionPlatforms[i] as Dictionary<string, object>;
                    availablePackages.AttributionPlatforms.Add(PackageComponent.FromDictionary(dict));
                }

                // Analytics systems
                List<object> analyticsSystems = packagesDictionary["ao_analytics_systems"] as List<object>;
                availablePackages.AnalyticsSystems = new List<PackageComponent>();
                for (int i = 0; analyticsSystems != null && i < analyticsSystems.Count; i++)
                {
                    Dictionary<string, object> dict = analyticsSystems[i] as Dictionary<string, object>;
                    availablePackages.AnalyticsSystems.Add(PackageComponent.FromDictionary(dict));
                }

                // Ad networks
                List<object> adNetworks = packagesDictionary["ao_ad_networks"] as List<object>;
                availablePackages.AdNetworks = new List<PackageComponent>();
                for (int i = 0; adNetworks != null && i < adNetworks.Count; i++)
                {
                    Dictionary<string, object> dict = adNetworks[i] as Dictionary<string, object>;
                    availablePackages.AdNetworks.Add(PackageComponent.FromDictionary(dict));
                }

                // Others
                List<object> others = packagesDictionary["ao_others"] as List<object>;
                availablePackages.Others = new List<PackageComponent>();
                for (int i = 0; others != null && i < others.Count; i++)
                {
                    Dictionary<string, object> dict = others[i] as Dictionary<string, object>;
                    availablePackages.Others.Add(PackageComponent.FromDictionary(dict));
                }

                manifest.Packages = availablePackages;

                if (cacheEnabled)
                {
                    // Finally, cache successfull deserialization
                    CacheSerialization(json);
                }
            }
            
            // Store mandatory values for ApiQueries
            EditorAnalyticsProxy.SetTokenIdentifier(manifest.AppToken);
            EditorAnalyticsProxy.SetManifestVersionId(manifest.Packages?.ManifestVersionId);

            return manifest;
        }

        public PluginManifest LoadFromCache()
        {
            try
            {
                string legacyManifestPath = Path.Combine(Application.dataPath + "/../Library/Homa Belly/plugin_manifest.json");
                if (File.Exists(LOCAL_PLUGIN_MANIFEST_FILE))
                {
                    string manifestJson = File.ReadAllText(LOCAL_PLUGIN_MANIFEST_FILE);
                    return this.Deserialize(manifestJson);
                }
                // Check if legacy JSON file exists
                else if (File.Exists(legacyManifestPath))
                {
                    // Dump to new file and remove old one
                    File.WriteAllText(LOCAL_PLUGIN_MANIFEST_FILE, File.ReadAllText(legacyManifestPath));
                    File.Delete(legacyManifestPath);
                    // Deserialize manifest
                    string manifestJson = File.ReadAllText(LOCAL_PLUGIN_MANIFEST_FILE);
                    return this.Deserialize(manifestJson);
                }
            }
            catch (Exception e)
            {
                HomaBellyEditorLog.Warning($"[Plugin Manifest] Could not load plugin_manifest.json. Reason: {e.Message}");
            }

            return null;
        }

        #region Private helpers

        private void CacheSerialization(string pluginManifestJson)
        {
            try
            {
                EditorFileUtilities.CreateIntermediateDirectoriesIfNecessary(LOCAL_PLUGIN_MANIFEST_FILE);
                File.WriteAllText(LOCAL_PLUGIN_MANIFEST_FILE, pluginManifestJson);
            }
            catch (Exception e)
            {
                HomaBellyEditorLog.Warning($"[Plugin Manifest] Could not save plugin_manifest.json. Reason: {e.Message}");
            }
        }

        #endregion
    }
}
