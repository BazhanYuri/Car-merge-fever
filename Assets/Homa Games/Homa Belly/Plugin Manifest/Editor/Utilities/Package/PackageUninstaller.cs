using System;
using System.Collections.Generic;
using UnityEditor;
using HomaGames.HomaBelly.Installer;

namespace HomaGames.HomaBelly.Utilities
{
    /// <summary>
    /// Utility class to uninstall any package not present
    /// in the new plugin manifest but installed in previous integrations.
    /// </summary>
    public class PackageUninstaller
    {
        public void FindPackagesToUninstall(PluginManifest oldPluginManifest, PluginManifest newPluginManifest)
        {
            HomaBellyEditorLog.Debug($"Checking packages to uninstall...");
            UninstallPackages(oldPluginManifest.Packages.CorePackages, newPluginManifest, EditorPackageType.CORE_PACKAGE);
            UninstallPackages(oldPluginManifest.Packages.MediationLayers, newPluginManifest, EditorPackageType.MEDIATION_LAYER);
            UninstallPackages(oldPluginManifest.Packages.AttributionPlatforms, newPluginManifest, EditorPackageType.ATTRIBUTION_PLATFORM);
            UninstallPackages(oldPluginManifest.Packages.AdNetworks, newPluginManifest, EditorPackageType.AD_NETWORK);
            UninstallPackages(oldPluginManifest.Packages.AnalyticsSystems, newPluginManifest, EditorPackageType.ANALYTICS_SYSTEM);
            UninstallPackages(oldPluginManifest.Packages.Others, newPluginManifest, EditorPackageType.OTHER);
            AssetDatabase.Refresh();
            HomaBellyEditorLog.Debug($"Done");
        }

        public void UninstallAllPackages(PluginManifest manifest)
        {
            if (manifest != null && manifest.Packages != null)
            {
                List<PackageComponent> packages = manifest.Packages.GetAllPackages();
                for (int i = 0; i < packages.Count; i++)
                {
                    UninstallPackage(packages[i]);
                }
            }
        }
        private void UninstallPackages(List<PackageComponent> oldPackages, PluginManifest newPluginManifest, EditorPackageType type)
        {
            if (oldPackages != null)
            {
                for (int i = 0; i < oldPackages.Count; i++)
                {
                    PackageComponent oldPackage = oldPackages[i];

                    // Determine if the new plugin manifest does not contain the old package. If so, uninstall
                    bool shouldUninstall = newPluginManifest.Packages.GetPackageComponent(oldPackage.Id, oldPackage.Version, type) == null;
                    if (shouldUninstall && PackageCommon.ShouldUninstallPackage(oldPackage))
                    {
                        UninstallPackage(oldPackage);
                    }
                }
            }
        }

        private void UninstallPackage(PackageComponent packageComponent)
        {
            HomaBellyEditorLog.Debug($"Uninstalling package: {packageComponent.GetName()}");
            if (packageComponent.Files != null)
            {
                foreach (string filePath in packageComponent.Files)
                {
                    DeleteAsset(filePath);
                }
            }
        }

        private void DeleteAsset(string assetPath)
        {
            string assetWithoutPrefix = PackageCommon.GetAssetWithoutPrefix(assetPath);
            HomaBellyEditorLog.Debug($"Deleting file/directory at {assetWithoutPrefix}");
            try
            {
                // Delete file and .meta
                bool result = FileUtil.DeleteFileOrDirectory(assetWithoutPrefix);
                result &= FileUtil.DeleteFileOrDirectory(assetWithoutPrefix + ".meta");
                if (result)
                {
                    HomaBellyEditorLog.Debug($"{assetWithoutPrefix} deleted");
                }
                else
                {
                    HomaBellyEditorLog.Warning($"Could not delete {assetWithoutPrefix}");
                }
            }
            catch (Exception e)
            {
                HomaBellyEditorLog.Warning($"Could not delete file/directory. Reason: {e.Message}");
            }
        }
    }
}
