using System.Threading.Tasks;

namespace HomaGames.HomaBelly
{
    public interface IHomaBellyWindowController
    {
        void ShowWindow(HomaBellyBaseWindow.ID id);
        Task<PluginManifest> RequestPluginManifest(string appToken);
        void InstallPluginManifest(PluginManifest newPluginManifest);
        float GetInstallationProgress();
        bool IsInstalling();
    }
}
