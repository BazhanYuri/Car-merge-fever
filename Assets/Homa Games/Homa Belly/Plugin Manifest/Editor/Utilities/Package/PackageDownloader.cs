using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace HomaGames.HomaBelly.Utilities
{
    /// <summary>
    /// Utility class to download packages to a library folder
    /// </summary>
    public class PackageDownloader
    {
        private HomaGames.HomaBelly.Installer.Utilities.EditorHttpCaller<string> httpCaller;
        private float numberOfPackagesToBeDownloaded = 0;
        private float numberOfPackagesAlreadyDownloaded = 0;

        public PackageDownloader()
        {
            httpCaller = new Installer.Utilities.EditorHttpCaller<string>();
        }

        /// <summary>
        /// Download all packages defined in the given Plugin Manifest
        /// </summary>
        /// <param name="pluginManifest"></param>
        /// <returns></returns>
        public async Task DownloadPackages(PluginManifest pluginManifest)
        {
            GatherNumberOfPackagesToBeDownloaded(pluginManifest);

            HomaBellyEditorLog.Debug($"Downloading core packages...");
            await DownloadPackages(pluginManifest.Packages.CorePackages);
            HomaBellyEditorLog.Debug($"Downloading mediation packages...");
            await DownloadPackages(pluginManifest.Packages.MediationLayers);
            HomaBellyEditorLog.Debug($"Downloading attribution packages...");
            await DownloadPackages(pluginManifest.Packages.AttributionPlatforms);
            HomaBellyEditorLog.Debug($"Downloading ad network packages...");
            await DownloadPackages(pluginManifest.Packages.AdNetworks);
            HomaBellyEditorLog.Debug($"Downloading analytics packages...");
            await DownloadPackages(pluginManifest.Packages.AnalyticsSystems);
            HomaBellyEditorLog.Debug($"Downloading other packages...");
            await DownloadPackages(pluginManifest.Packages.Others);
        }

        /// <summary>
        /// Given a list of packages, download the main package and the adapter (if any)
        /// </summary>
        /// <param name="packages"></param>
        /// <returns></returns>
        private async Task DownloadPackages(List<PackageComponent> packages)
        {
            for (int i = 0; packages != null && i < packages.Count; i++)
            {
                PackageComponent package = packages[i];
                await DownloadPackage(package.Url, package.GetMainPackageLocalFilePath());
                if (!string.IsNullOrEmpty(package.AdapterUrl))
                {
                    await DownloadPackage(package.AdapterUrl, package.GetAdapterPackageLocalFilePath());
                }

                numberOfPackagesAlreadyDownloaded++;
            }
        }

        public float GetProgress()
        {
            return numberOfPackagesToBeDownloaded == 0 ? 0.01f : (Mathf.Max(1f, numberOfPackagesAlreadyDownloaded) / numberOfPackagesToBeDownloaded);
        }

        private void GatherNumberOfPackagesToBeDownloaded(PluginManifest pluginManifest)
        {
            numberOfPackagesAlreadyDownloaded = 0;
            numberOfPackagesToBeDownloaded = 0;
            numberOfPackagesToBeDownloaded += pluginManifest.Packages.CorePackages != null ? pluginManifest.Packages.CorePackages.Count : 0;
            numberOfPackagesToBeDownloaded += pluginManifest.Packages.MediationLayers != null ? pluginManifest.Packages.MediationLayers.Count : 0;
            numberOfPackagesToBeDownloaded += pluginManifest.Packages.AttributionPlatforms != null ? pluginManifest.Packages.AttributionPlatforms.Count : 0;
            numberOfPackagesToBeDownloaded += pluginManifest.Packages.AdNetworks != null ? pluginManifest.Packages.AdNetworks.Count : 0;
            numberOfPackagesToBeDownloaded += pluginManifest.Packages.AnalyticsSystems != null ? pluginManifest.Packages.AnalyticsSystems.Count : 0;
            numberOfPackagesToBeDownloaded += pluginManifest.Packages.Others != null ? pluginManifest.Packages.Others.Count : 0;
        }

        /// <summary>
        /// Asynchronously downloads the available package in <paramref name="packageUri"/>, saving
        /// it in <see cref="TMP_PACKAGE_DOWNLOAD_DIR"/> folder
        /// </summary>
        /// <param name="packageUri">The uri hosting the package</param>
        /// <param name="forceDownload">(Optional) True to force download</param>
        /// <returns>A Task with the saved file path</returns>
        private async Task<string> DownloadPackage(string packageUri, string outputFilePath, bool forceDownload = true)
        {
            if (!File.Exists(outputFilePath) || forceDownload)
            {
                EditorFileUtilities.CreateIntermediateDirectoriesIfNecessary(outputFilePath);
                HomaBellyEditorLog.Debug($"Downloading package: {packageUri}");
                try
                {
                    // Trigger package download
                    string packagePath = await httpCaller.DownloadFile(packageUri, outputFilePath).ConfigureAwait(false);
                    HomaBellyEditorLog.Debug($"Package available at: {packagePath}");
                    return packagePath;
                }
                catch (Exception e)
                {
                    EditorAnalyticsProxy.TrackEditorAnalyticsEvent("error_downloading_package", e.Message);
                    HomaBellyEditorLog.Warning($"Could not download package at: {packageUri}. Error: {e.Message}");
                    return "";
                }
            }
            else
            {
                // Package already downloaded, notify callback
                HomaBellyEditorLog.Debug($"Package already downloaded. Available at: {outputFilePath}");
                return outputFilePath;
            }
        }
    }
}
