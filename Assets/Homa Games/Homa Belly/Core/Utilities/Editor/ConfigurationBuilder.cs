using UnityEditor;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public static class ConfigurationBuilder
    {
        [InitializeOnLoadMethod]
        public static void Build()
        {
            HomaBellyManifestConfiguration homaBellyManifestConfiguration = GetOrCreateAsset();
            var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(
                FileUtil.GetProjectRelativePath(PluginManifestDeserializer.LOCAL_PLUGIN_MANIFEST_FILE));
            var currentPluginManifestHash = textAsset ? textAsset.text.GetHashCode() : 0;
            if (currentPluginManifestHash == homaBellyManifestConfiguration.PluginManifestHash) return;
            var manifest = PluginManifest.LoadFromLocalFile();
            if (manifest == null) return;
            
            homaBellyManifestConfiguration.SetEntry(manifest.AppToken, HomaBellyManifestConfiguration.MANIFEST_TOKEN_KEY);
            homaBellyManifestConfiguration.SetEntry(manifest.Packages?.ManifestVersionId, HomaBellyManifestConfiguration.MANIFEST_VERSION_ID_KEY);
            foreach (var component in manifest.Packages.GetAllPackages())
            {
                // Not all components may have Data, so ignore empty/null ones
                if (component.Data != null && component.Data.Count > 0)
                {
                    foreach (var data in component.Data)
                    {
                        homaBellyManifestConfiguration.SetEntry(data.Value, component.Id, data.Key);
                    }
                }
            }

            EditorUtility.SetDirty(homaBellyManifestConfiguration);
            EditorApplication.delayCall += AssetDatabase.SaveAssets;
            homaBellyManifestConfiguration.PluginManifestHash = currentPluginManifestHash;
        }

        private static HomaBellyManifestConfiguration GetOrCreateAsset()
        {
            var asset = AssetDatabase.LoadAssetAtPath<HomaBellyManifestConfiguration>(
                HomaBellyManifestConfiguration.CONFIG_FILE_PROJECT_PATH);
            if (asset)
                return asset;
            FileUtilities.CreateIntermediateDirectoriesIfNecessary(HomaBellyManifestConfiguration.CONFIG_FILE_PROJECT_PATH);
            var newAsset = ScriptableObject.CreateInstance<HomaBellyManifestConfiguration>();
            AssetDatabase.CreateAsset(newAsset, HomaBellyManifestConfiguration.CONFIG_FILE_PROJECT_PATH);
            return newAsset;
        }
    }
}