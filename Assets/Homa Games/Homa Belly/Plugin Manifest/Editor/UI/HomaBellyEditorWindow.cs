using System;
using System.IO;
using System.Threading.Tasks;
using HomaGames.HomaBelly.Installer.Utilities;
using HomaGames.HomaBelly.Utilities;
using UnityEditor;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    [Serializable]
    internal sealed class HomaBellyEditorWindow : EditorWindow, IHomaBellyWindowController
    {
        #region Private properties
        private HomaBellyBaseWindow installWindow;
        private HomaBellyBaseWindow installationProgressWindow;
        private static PluginController pluginController;
        private Vector2 scrollPosition;
        [SerializeField]
        private HomaBellyBaseWindow.ID currentWindowId = HomaBellyBaseWindow.ID.INSTALL;
        #endregion

        [MenuItem("Window/Homa Games/Homa Belly/Setup", false, 1)]
        internal static void CreateSettingsAndFocus()
        {
            EditorAnalyticsProxy.TrackEditorAnalyticsEvent("open_homa_belly_setup_from_menu");
            GetWindow(typeof(HomaBellyEditorWindow), false, "Homa Belly", true);
        }
        
        [MenuItem("Window/Homa Games/Homa Belly/Delete All Packages", false, 12)]
        internal static void DeleteAllPackages()
        {
            // Determine if PluginManifest is available, which means some packages are installed
            PluginManifest manifest = PluginManifest.LoadFromLocalFile();
            if (manifest != null)
            {
                bool ensureDeletion = EditorUtility.DisplayDialog("Homa Belly",
                    "You are about to delete all installed packages from Homa Belly.\n\nAre you sure?", "Yes", "No");
                if (ensureDeletion)
                {
                    EditorAnalyticsProxy.TrackEditorAnalyticsEvent("delete_all_packages");
                    
                    // Uninstall packages
                    if (pluginController == null)
                    {
                        pluginController = new PluginController();
                    }
                    
                    pluginController.UninstallAllPackages();
                    // Delete manifest
                    File.Delete(PluginManifestDeserializer.LOCAL_PLUGIN_MANIFEST_FILE);
                    File.Delete(PluginManifestDeserializer.LOCAL_PLUGIN_MANIFEST_FILE + ".meta");
                    // Refresh Assets Database
                    AssetDatabase.Refresh();
                    // Open INSTALL window
                    HomaBellyEditorWindow window = GetWindow<HomaBellyEditorWindow>();
                    if (window != null) window.currentWindowId = HomaBellyBaseWindow.ID.INSTALL;
                    
                    HomaBellyEditorLog.Debug("All packages deleted");
                }   
            }
            else
            {
                HomaBellyEditorLog.Warning("Homa Belly Manifest not found");
            }
        }

        [MenuItem("Window/Homa Games/Homa Belly/Full Implementation Guide", false, 23)]
        internal static void OpenHomaBellyDocumentation()
        {
            EditorAnalyticsProxy.TrackEditorAnalyticsEvent("open_homa_belly_implementation_guide");
            Application.OpenURL("https://www.notion.so/homagames/Full-implementation-guide-13eb58192648433bb78707be3537e521");
        }

        [MenuItem("Window/Homa Games/Homa Belly/Troubleshooting", false, 23)]
        internal static void OpenHomaBellyTroubleshooting()
        {
            EditorAnalyticsProxy.TrackEditorAnalyticsEvent("open_homa_belly_troubleshooting");
            Application.OpenURL("https://www.notion.so/homagames/Troubleshooting-Known-Issues-98747034856b41c79ae3caed102dd12f");
        }

        private void Awake()
        {
            // When opening new window, if there is a manifest stored,
            // show installation result window
            PluginManifest oldPluginManifest = PluginManifest.LoadFromLocalFile();
            if (oldPluginManifest != null)
            {
                currentWindowId = HomaBellyBaseWindow.ID.INSTALLATION_PROGRESS;
            }
            else
            {
                currentWindowId = HomaBellyBaseWindow.ID.INSTALL;
            }
        }

        void OnEnable()
        {
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
        }

        void OnDisable()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
        }

        private void Update()
        {
            if (IsInstalling())
            {
                Repaint();
            }
        }

        private void OnFocus()
        {
            if (installWindow != null)
            {
                installWindow.OnFocus();
            }

            if (installationProgressWindow != null)
            {
                installationProgressWindow.OnFocus();
            }
        }

        public void OnBeforeAssemblyReload()
        {
            HomaBellyEditorLog.BeforeAssemblyReload();
        }

        public void OnAfterAssemblyReload()
        {
            HomaBellyEditorLog.AfterAssemblyReload();
        }

        private void Initialize()
        {
            if (installWindow == null)
            {
                installWindow = new HomaBellyInstallWindow();
                installWindow.SetWindowController(this);
            }

            if (installationProgressWindow == null)
            {
                installationProgressWindow = new HomaBellyInstallationProgressWindow();
                installationProgressWindow.SetWindowController(this);
            }

            if (pluginController == null)
            {
                pluginController = new PluginController();
            }

            // Show current window
            ShowWindow(currentWindowId);
        }

        void OnGUI()
        {
            // Global Defaults
            GUISkin originalSkin = GUI.skin;
            GUI.skin = HomaGamesStyle.HomaStyle;
            EditorGUIUtility.SetIconSize(new Vector2(32, 32));

            Initialize();

            // Draw background color
            GUI.DrawTexture(new Rect(0, 0, maxSize.x, maxSize.y), HomaGamesStyle.BackgroundTexture, ScaleMode.StretchToFill);
            // Settings button
            if (SettingsReflection.SettingsAPIExists() &&
                GUILayout.Button(EditorGUIUtility.IconContent("_Popup@2x"), GUIStyle.none))
            {
                SettingsReflection.OpenSettings();
            }

            // ####################################
            // HEADER
            // ####################################

            // Draw Homa Games logo
            float homaGameLogoXPosition = position.width / 2 - HomaGamesStyle.HOMA_GAMES_LOGO_WITH / 2;
            GUI.DrawTexture(new Rect(homaGameLogoXPosition, 40,
                HomaGamesStyle.HOMA_GAMES_LOGO_WITH, HomaGamesStyle.HOMA_GAMES_LOGO_HEIGHT),
                HomaGamesStyle.LogoTexture, ScaleMode.ScaleToFit, true);
            GUILayout.Space(HomaGamesStyle.HOMA_GAMES_LOGO_HEIGHT + 80);


            // ####################################
            // WINDOWS
            // ####################################
            scrollPosition = GUILayout.BeginScrollView(scrollPosition,GUILayout.Height(position.height - 160 - HomaGamesStyle.HOMA_GAMES_LOGO_HEIGHT));
            installWindow.OnGUI(position);
            installationProgressWindow.OnGUI(position);
            GUILayout.EndScrollView();

            // ####################################
            // FOOTER
            // ####################################

            // Product name and version
            GUILayout.BeginArea(new Rect(0, position.height - 40, position.width, position.height));
            GUILayout.Label($"{HomaBellyInstallerConstants.PRODUCT_NAME} v{HomaBellyInstallerConstants.PRODUCT_VERSION}", HomaGamesStyle.SecondaryLabelStyle);
            GUILayout.EndArea();

            GUI.skin = originalSkin;
        }

        #region IHomaBellyWindowController

        public async Task<PluginManifest> RequestPluginManifest(string appToken)
        {
            if (pluginController != null)
            {
                try
                {
                    return await pluginController.RequestPluginManifest(appToken);
                }
                catch (EditorHttpCallerException e)
                {
                    // Forward Exception
                    throw e;
                }
            }

            return default;
        }

        public void InstallPluginManifest(PluginManifest newPluginManifest)
        {
            if (newPluginManifest != null && pluginController != null)
            {
                pluginController.InstallPluginManifest(newPluginManifest);
            }
        }

        public float GetInstallationProgress()
        {
            float progress = 0f;
            if (pluginController != null)
            {
                progress = pluginController.GetInstallationProgress();
            }

            return progress;
        }

        public void ShowWindow(HomaBellyBaseWindow.ID id)
        {
            currentWindowId = id;
            installWindow.SetVisible(id == HomaBellyBaseWindow.ID.INSTALL);
            installationProgressWindow.SetVisible(id == HomaBellyBaseWindow.ID.INSTALLATION_PROGRESS);
        }

        public bool IsInstalling()
        {
            if (pluginController != null)
            {
                return pluginController.IsInstalling();
            }

            return false;
        }

        #endregion
    }
}