using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HomaGames.HomaBelly.Utilities
{
    /// <summary>
    /// Utility class to install `unitypackage` files into current project
    /// </summary>
    public class PackageInstaller
    {
        private float numberOfPackagesToBeInstalled = 0;
        private float numberOfPackagesAlreadyInstalled = 0;

        public void InstallPackages(PluginManifest pluginManifest)
        {
            GatherNumberOfPackagesToBeInstalled(pluginManifest);

            AssetDatabase.importPackageCompleted += OnImportPackageCompleted;
            AssetDatabase.importPackageCancelled += OnImportPackageCompleted;
            AssetDatabase.importPackageFailed += OnImportPackageFailed;

            HomaBellyEditorLog.Debug($"Installing core packages...");
            InstallPackages(pluginManifest.Packages.CorePackages);
            HomaBellyEditorLog.Debug($"Installing mediation layers...");
            InstallPackages(pluginManifest.Packages.MediationLayers);
            HomaBellyEditorLog.Debug($"Installing attribution platforms...");
            InstallPackages(pluginManifest.Packages.AttributionPlatforms);
            HomaBellyEditorLog.Debug($"Installing ad networks...");
            InstallPackages(pluginManifest.Packages.AdNetworks);
            HomaBellyEditorLog.Debug($"Installing analytics systems...");
            InstallPackages(pluginManifest.Packages.AnalyticsSystems);
            HomaBellyEditorLog.Debug($"Installing others...");
            InstallPackages(pluginManifest.Packages.Others);
        }

        private void InstallPackages(List<PackageComponent> packages)
        {
            for (int i = 0; packages != null && i < packages.Count; i++)
            {
                PackageComponent package = packages[i];
                InstallPackage(package);
            }
        }

        /// <summary>
        /// Asynchronously installs the package represented by the given PackageComponent.
        /// If the package contains an adapter url, will also be installed.
        /// </summary>
        /// <param name="packageComponent">The PackageComponent to be installed</param>
        /// <returns>Void Task</returns>
        private void InstallPackage(PackageComponent packageComponent)
        {
            HomaBellyEditorLog.Debug($"Installing package: {packageComponent.GetName()}");
            string packagePath = packageComponent.GetMainPackageLocalFilePath();

            try
            {
                if (File.Exists(packagePath))
                {
                    // Import all package without showing editor import window
                    AssetDatabase.ImportPackage(packagePath, false);
                    HomaBellyEditorLog.Debug($"{packageComponent.GetName()} installed");
                }
                else
                {
                    HomaBellyEditorLog.Error($"Could not install package {packageComponent.GetName()}. File {packagePath} not found.");
                }
            }
            catch (Exception e)
            {
                HomaBellyEditorLog.Error($"Could not install package {packageComponent.GetName()}. Reason: {e.Message}");
            }

            InstallAdapter(packageComponent);
        }

        public float GetProgress()
        {
            return numberOfPackagesToBeInstalled == 0 ? 0.01f : (Mathf.Max(1, numberOfPackagesAlreadyInstalled) / numberOfPackagesToBeInstalled);
        }

        private void GatherNumberOfPackagesToBeInstalled(PluginManifest pluginManifest)
        {
            numberOfPackagesAlreadyInstalled = 0;
            numberOfPackagesToBeInstalled = 0;
            numberOfPackagesToBeInstalled += pluginManifest.Packages.CorePackages != null ? pluginManifest.Packages.CorePackages.Count : 0;
            numberOfPackagesToBeInstalled += pluginManifest.Packages.MediationLayers != null ? pluginManifest.Packages.MediationLayers.Count : 0;
            numberOfPackagesToBeInstalled += pluginManifest.Packages.AttributionPlatforms != null ? pluginManifest.Packages.AttributionPlatforms.Count : 0;
            numberOfPackagesToBeInstalled += pluginManifest.Packages.AdNetworks != null ? pluginManifest.Packages.AdNetworks.Count : 0;
            numberOfPackagesToBeInstalled += pluginManifest.Packages.AnalyticsSystems != null ? pluginManifest.Packages.AnalyticsSystems.Count : 0;
            numberOfPackagesToBeInstalled += pluginManifest.Packages.Others != null ? pluginManifest.Packages.Others.Count : 0;
        }

        private void OnImportPackageFailed(string packageName, string errorMessage)
        {
            numberOfPackagesAlreadyInstalled++;
            HomaBellyEditorLog.Warning($"Could not install package {packageName}. Error: {errorMessage}");

            if (numberOfPackagesAlreadyInstalled == numberOfPackagesToBeInstalled)
            {
                AssetDatabase.importPackageFailed -= OnImportPackageFailed;
            }
        }

        private void OnImportPackageCompleted(string packageName)
        {
            numberOfPackagesAlreadyInstalled++;

            if (numberOfPackagesAlreadyInstalled == numberOfPackagesToBeInstalled)
            {
                AssetDatabase.importPackageCompleted -= OnImportPackageCompleted;
                AssetDatabase.importPackageCancelled -= OnImportPackageCompleted;
            }
        }

        /// <summary>
        /// Installs the adapter (if any) linked to the given PackageComponent
        /// </summary>
        /// <param name="packageComponent">The main PackageComponent</param>
        /// <returns>Void Task</returns>
        private void InstallAdapter(PackageComponent packageComponent)
        {
            // Install adapter (if available)
            if (!string.IsNullOrEmpty(packageComponent.AdapterUrl))
            {
                string packagePath = packageComponent.GetAdapterPackageLocalFilePath();
                try
                {
                    if (File.Exists(packagePath))
                    {
                        // Import all package without showing editor import window
                        AssetDatabase.ImportPackage(packagePath, false);
                        HomaBellyEditorLog.Debug($"{packageComponent.GetName()} adapter installed");
                    }
                    else
                    {
                        HomaBellyEditorLog.Error($"Could not install {packageComponent.GetName()} adapter. File {packagePath} not found.");
                    }
                }
                catch (Exception e)
                {
                    HomaBellyEditorLog.Error($"Could not install {packageComponent.GetName()} adapter. Reason: {e.Message}");
                }
            }
        }
    }
}
