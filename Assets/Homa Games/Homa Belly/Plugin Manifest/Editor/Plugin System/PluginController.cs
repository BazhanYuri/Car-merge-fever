using System.Threading.Tasks;
using HomaGames.HomaBelly.Installer.Utilities;
using HomaGames.HomaBelly.Utilities;
using UnityEditor;

namespace HomaGames.HomaBelly
{
    public class PluginController
    {
        private Installer.Utilities.EditorHttpCaller<PluginManifest> editorHttpCaller;
        private PluginManifestDeserializer pluginManifestDeserializer;
        private PackageInstaller packageInstaller;
        private PackageDownloader packageDownloader;
        private PackageUninstaller packageUninstaller;
        private PluginManifest latestInstalledManifest;
        private bool installing;

        public PluginController()
        {
            packageInstaller = new PackageInstaller();
            packageDownloader = new PackageDownloader();
            packageUninstaller = new PackageUninstaller();
            editorHttpCaller = new Installer.Utilities.EditorHttpCaller<PluginManifest>();
            pluginManifestDeserializer = new PluginManifestDeserializer();
            latestInstalledManifest = PluginManifest.LoadFromLocalFile();
        }

        #region Public methods

        public async Task<PluginManifest> RequestPluginManifest(string appToken)
        {
            try
            {
                return await editorHttpCaller.Get(string.Format(HomaBellyInstallerConstants.API_APP_BASE_URL, appToken), pluginManifestDeserializer);
            }
            catch(EditorHttpCallerException e)
            {
                // Forward Exception
                throw e;
            }
        }

        public async void InstallPluginManifest(PluginManifest newPluginManifest)
        {
            EditorAnalyticsProxy.TrackEditorAnalyticsEvent("manifest_installation_started");
            
            installing = true;

            if (newPluginManifest != null)
            {
                // Determine if old packages need to be uninstalled
                if (latestInstalledManifest != null)
                {
                    // As there is an old plugin manifest saved locally, proceed to uninstallation process
                    packageUninstaller.FindPackagesToUninstall(latestInstalledManifest, newPluginManifest);
                }

                // Update local manifest with the new one
                HomaBellyEditorLog.Debug($"{newPluginManifest.ToString()}");

                // Lock reload assemblies while installing packages
                EditorApplication.LockReloadAssemblies();

                // Download packages from new manifest
                await packageDownloader.DownloadPackages(newPluginManifest);

                // Install packages from new manifest
                packageInstaller.InstallPackages(newPluginManifest);

                // Unlock reload assemblies and refresh AssetDatabase
                EditorApplication.UnlockReloadAssemblies();

                // Update latest installed manifest
                latestInstalledManifest = newPluginManifest;
            }

            AssetDatabase.Refresh();

            installing = false;
        }

        public void UninstallAllPackages()
        {
            if (latestInstalledManifest != null)
            {
                packageUninstaller.UninstallAllPackages(latestInstalledManifest);    
            }
        }

        public float GetInstallationProgress()
        {
            return (packageDownloader.GetProgress() + packageInstaller.GetProgress()) / 2f;
        }

        public bool IsInstalling()
        {
            return installing || (GetInstallationProgress() > 0.01f && GetInstallationProgress() < 0.95f);
        }

        #endregion
    }
}