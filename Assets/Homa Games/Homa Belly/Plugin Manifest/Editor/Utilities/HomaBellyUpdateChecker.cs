using System;
using System.Threading.Tasks;
using HomaGames.HomaBelly.Installer.Utilities;
using UnityEditor;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Update Checker for Homa Belly manifest updates. This class
    /// will automatically look for a manifest update when Unity is relaunched
    /// or a script refresh happens, but only if 15 minutes passed after last fetch.
    ///
    /// It also shows an Update Popup if:
    /// - The fetched manifest is brand new (the exact same one has not been fetched before)
    /// - Or 24 hours passed before a new manifest was detected and it has not been installed
    /// </summary>
    public class HomaBellyUpdateChecker
    {
        private static string LAST_UPDATE_CHECK = "homagames.homabelly.last_update_check";
        private static string LAST_UPDATE_POPUP_SHOW = "homagames.homabelly.last_update_popup_show";
        private static string LAST_MANIFEST_AVAILABLE_STRING = "homagames.homabelly.last_manifest_available_string";
        private static int HOURS_UNTIL_NEXT_UPDATE_POPUP = 24;
        private static int MINUTES_UNTIL_NEXT_UPDATE_CHECK = 15;

        private static PluginManifest _latestFetchedPluginManifest;

        private static bool _updateAvailable = false;

        /// <summary>
        /// Determines if a manifest update is available at Homa Belly servers
        /// </summary>
        public static bool UpdateAvailable => _updateAvailable;

        [InitializeOnLoadMethod]
        static void CheckUpdateUponLoad()
        {
            if (!ShouldCheckForUpdate())
            {
                return;
            }

            // Obtain old plugin manifest before fetching the new one
            PluginManifest oldPluginManifest = PluginManifest.LoadFromLocalFile();
            if (oldPluginManifest != null)
            {
                FetchPluginManifest(oldPluginManifest.AppToken).ContinueWith((manifestResult) =>
                {
                    PluginManifest newPluginManifest = manifestResult.Result;

                    // Homa Belly manifest fetched and cached
                    if (newPluginManifest != null)
                    {
                        // Cache last fetched manifest and if an update is available
                        _latestFetchedPluginManifest = newPluginManifest;
                        _updateAvailable = !oldPluginManifest.Equals(newPluginManifest);

                        if (_updateAvailable)
                        {
                            EditorAnalyticsProxy.TrackEditorAnalyticsEvent("manifest_update_available");
                            HomaBellyEditorLog.Debug("Homa Belly manifest update available");

                            // Try to show the update popup
                            TryShowUpdatePopup();
                        }
                        else
                        {
                            HomaBellyEditorLog.Debug("Homa Belly manifes up to date");
                        }

                        // Update last fetched manifest string
                        EditorPrefs.SetString(LAST_MANIFEST_AVAILABLE_STRING, newPluginManifest.ToString());
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }
        
        #region Private helpers

        /// <summary>
        /// Fetches current manifest from servers with the given token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static Task<PluginManifest> FetchPluginManifest(string token)
        {
            try
            {
                return new EditorHttpCaller<PluginManifest>().Get(string.Format(HomaBellyInstallerConstants.API_APP_BASE_URL, token), new PluginManifestDeserializer(false));
            }
            catch (EditorHttpCallerException e)
            {
                HomaBellyEditorLog.Warning($"Exception detected while fetching Homa Belly Manifest for update: [{e.Status}] {e.Message}");
            }

            return default;
        }

        /// <summary>
        /// Tries to show update popup if conditions are met
        /// </summary>
        private static void TryShowUpdatePopup()
        {
            if (ShouldShowUpdatePopup())
            {
                // Wait 10 seconds before prompting the user to update HB on Unity load
                Task.Delay(10000).ContinueWith((_) =>
                {
                    EditorAnalyticsProxy.TrackEditorAnalyticsEvent("manifest_update_available_popup_prompted");
                    bool updateHomaBelly = EditorUtility.DisplayDialog("Homa Belly Update Available", "Your Homa Belly installation is not up to date. " +
                        "Having it up to date allows your game to benefit from the latest features and enhancements." +
                        "\n\nDo you want to update it now?", "Update", "Remind me later");

                    if (updateHomaBelly)
                    {
                        EditorAnalyticsProxy.TrackEditorAnalyticsEvent("manifest_update_available_popup_accept");
                        
                        // If HomaBellyEditorWindow is null means it is not created. Create it
                        HomaBellyEditorWindow window = EditorWindow.GetWindow<HomaBellyEditorWindow>();
                        if (window == null)
                        {
                            EditorWindow.GetWindow(typeof(HomaBellyEditorWindow), false, "Homa Belly", true);
                            window = EditorWindow.GetWindow<HomaBellyEditorWindow>();
                        }
                        
                        // Focus HomaBellyEditorWindow and start manifest update
                        if (window != null)
                        {
                            window.Focus();

                            // Delay installation start 500ms to allow Window to properly focus and setup
                            Task.Delay(500).ContinueWith((unused) =>
                            {
                                window.ShowWindow(HomaBellyBaseWindow.ID.INSTALLATION_PROGRESS);
                                window.InstallPluginManifest(_latestFetchedPluginManifest);                                
                            }, TaskScheduler.FromCurrentSynchronizationContext());
                        }
                    }
                    else
                    {
                        EditorAnalyticsProxy.TrackEditorAnalyticsEvent("manifest_update_available_popup_deny");
                        HomaBellyEditorLog.Debug($"Homa Belly update will be reminded within next {HOURS_UNTIL_NEXT_UPDATE_POPUP} hours");
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        /// <summary>
        /// Determines if the update popup should be shown. This is:
        /// 
        /// - If a new manifest is available and has not already been prompted for update
        /// - If a new manifest is available and was prompted for update +24h ago
        /// 
        /// </summary>
        /// <returns>True if popup should be shown, false otherwise</returns>
        private static bool ShouldShowUpdatePopup()
        {
            // If a manifest was previously fetched and the latest one is not the same, force show the popup.
            // This guarantees showing the popup for every manifest update available the very first time
            // it is detected
            if (EditorPrefs.HasKey(LAST_MANIFEST_AVAILABLE_STRING)
                && EditorPrefs.GetString(LAST_MANIFEST_AVAILABLE_STRING) != _latestFetchedPluginManifest.ToString())
            {
                return true;
            }

            // Otherwise, if the the same updated manifest was previously fetched and not updated,
            // show the popup only after 24h
            DateTime now = DateTime.Now;
            if (EditorPrefs.HasKey(LAST_UPDATE_POPUP_SHOW))
            {
                long.TryParse(EditorPrefs.GetString(LAST_UPDATE_POPUP_SHOW), out var lastUpdatePopupShowFileTime);

                // Show popup after HOURS_UNTIL_NEXT_UPDATE_POPUP hours of latest shown
                TimeSpan deltaTimeSpan = now.Subtract(DateTime.FromFileTime(lastUpdatePopupShowFileTime));
                if (deltaTimeSpan.TotalHours > HOURS_UNTIL_NEXT_UPDATE_POPUP)
                {
                    EditorPrefs.SetString(LAST_UPDATE_POPUP_SHOW, now.ToFileTime().ToString());
                    return true;
                }
            }
            else
            {
                // Set LAST_UPDATE_POPUP_SHOW as `now`
                EditorPrefs.SetString(LAST_UPDATE_POPUP_SHOW, now.ToFileTime().ToString());
            }

            return false;
        }

        /// <summary>
        /// Determines if a new manifest fetch should happen
        /// </summary>
        /// <returns></returns>
        private static bool ShouldCheckForUpdate()
        {
            DateTime now = DateTime.Now;
            if (EditorPrefs.HasKey(LAST_UPDATE_CHECK))
            {
                long.TryParse(EditorPrefs.GetString(LAST_UPDATE_CHECK), out var lastUpdateCheckFileTime);

                // Check new version after MINUTES_UNTIL_NEXT_UPDATE_CHECK minutes of last update check
                TimeSpan deltaTimeSpan = now.Subtract(DateTime.FromFileTime(lastUpdateCheckFileTime));
                if (deltaTimeSpan.TotalMinutes > MINUTES_UNTIL_NEXT_UPDATE_CHECK)
                {
                    EditorPrefs.SetString(LAST_UPDATE_CHECK, now.ToFileTime().ToString());
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                // Set first time update check as `now`
                EditorPrefs.SetString(LAST_UPDATE_CHECK, now.ToFileTime().ToString());
            }

            // If there is no manifest downloaded, means first install, return false
            // Otherwise return true
            return PluginManifest.LoadFromLocalFile() != null;
        }

        #endregion
    }
}
