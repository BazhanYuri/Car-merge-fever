#if UNITY_ANDROID || UNITY_IOS
#define HOMA_BELLY_INSTALL_SUPPORTED
#endif
using System;
using System.Threading.Tasks;
using HomaGames.HomaBelly;
using HomaGames.HomaBelly.Installer.Utilities;
using UnityEditor;
using UnityEngine;

public class HomaBellyInstallWindow : HomaBellyBaseWindow
{
    private string appToken;
    private bool requestingManifest = false;
    private PluginManifest manifest;
    private EditorHttpCallerException editorHttpCallerException;

    public HomaBellyInstallWindow() : base()
    {
        // Initialize app token
        manifest = PluginManifest.LoadFromLocalFile();
        appToken = manifest != null ? manifest.AppToken : "";
    }

    protected override void Draw(Rect windowPosition)
    {
        Vector2 originalIconSize = EditorGUIUtility.GetIconSize();
        EditorGUIUtility.SetIconSize(new Vector2(32, 32));
#if !HOMA_BELLY_INSTALL_SUPPORTED
        var guiPreviouslyEnabled = GUI.enabled;
        GUI.enabled = false;
#endif
        GUILayout.Label("Enter your app token:", HomaGamesStyle.CenteredBoldText);

        // Input field
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        appToken = GUILayout.TextField(appToken, HomaGamesStyle.BigInputFieldStyle);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        // `Setup` button
        GUIContent buttonContent = new GUIContent();
        buttonContent.image = HomaGamesStyle.DownloadIcon;
        buttonContent.text = manifest == null ? "Install Packages" : (HomaBellyUpdateChecker.UpdateAvailable ? "Update Packages" : "Refresh Packages");

        if (requestingManifest)
        {
            GUILayout.Label("Fetching...", HomaGamesStyle.SecondaryLabelStyle);
        }
        else
        {
            if (GUILayout.Button(buttonContent, HomaGamesStyle.ButtonStyleTexts))
            {
                if (IsValidAppToken(appToken))
                {
                    requestingManifest = true;
                    
                    editorHttpCallerException = null;
                    EditorAnalyticsProxy.SetTokenIdentifier(appToken);
                    EditorAnalyticsProxy.TrackEditorAnalyticsEvent("manifest_fetching_started");
                    homaBellyWindowController.RequestPluginManifest(appToken).ContinueWith((taskResult) =>
                    {
                        try
                        {
                            if (taskResult.Result != null)
                            {
                                EditorAnalyticsProxy.TrackEditorAnalyticsEvent("manifest_fetching_completed");

                                // Clear previous log traces
                                HomaBellyEditorLog.ResetLog();
                                homaBellyWindowController.ShowWindow(ID.INSTALLATION_PROGRESS);
                                homaBellyWindowController.InstallPluginManifest(taskResult.Result);
                            }
                            else
                            {
                                EditorAnalyticsProxy.TrackEditorAnalyticsEvent("manifest_fetching_error", "Error deserializing. Manifest is null");
                                
                                // Show generic error
                                editorHttpCallerException = new EditorHttpCallerException("Error", $"Could not fetch manifest for app token '{appToken}'\nEnsure your app token is valid");
                            }
                        }
                        catch (AggregateException ae)
                        {
                            foreach (var e in ae.InnerExceptions)
                            {
                                EditorAnalyticsProxy.TrackEditorAnalyticsEvent("manifest_fetching_error", e.Message);
                                
                                // Handle the custom exception.
                                if (e is EditorHttpCallerException)
                                {
                                    editorHttpCallerException = (EditorHttpCallerException) e;
                                }
                                // Rethrow any other exception.
                                else
                                {
                                    throw e;
                                }
                            }
                        }

                        requestingManifest = false;
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                }
                else
                {
                    editorHttpCallerException = new EditorHttpCallerException("Error", "Invalid app token");
                }
            }
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
#if !HOMA_BELLY_INSTALL_SUPPORTED
        GUI.enabled = guiPreviouslyEnabled;
#endif
        // Display error (if any)
        DisplayError();
        
        EditorGUIUtility.SetIconSize(originalIconSize);
    }

    private void DisplayError()
    {
        // Status message (error)
        if (editorHttpCallerException != null)
        {
            // Print error
            string errorMessage;
            if (editorHttpCallerException.Message == "MANIFEST_SDK_VERSION_NOT_ALLOWED")
            {
                errorMessage = "";
            }
            else
            {
                // Print default error message
                errorMessage = editorHttpCallerException.Message;
            }

            GUILayout.Label(errorMessage, HomaGamesStyle.ErrorLabelStyle);

            // Print any consecuent action related to the error
            if (editorHttpCallerException.Message == "MANIFEST_SDK_VERSION_NOT_ALLOWED")
            {
                GUIStyle downloadHomaBellyFromHomaLabLinkStyle = new GUIStyle()
                {
                    margin = new RectOffset(10, 10, 10, 10),
                    font = (Font)Resources.Load("Poppins-Bold.ttf", typeof(Font)),
                    alignment = TextAnchor.MiddleCenter,
                };
                downloadHomaBellyFromHomaLabLinkStyle.normal.textColor = Color.red;
                downloadHomaBellyFromHomaLabLinkStyle.fontStyle = FontStyle.Italic;
                downloadHomaBellyFromHomaLabLinkStyle.fontSize = 12;

                // Add a button to Homa Lab download page
                if (GUILayout.Button(new GUIContent("There is a new Homa Belly version.\nPlease download it from Homa Lab", HomaGamesStyle.ExternalLinkIcon), downloadHomaBellyFromHomaLabLinkStyle, GUILayout.Height(20)))
                {
                    Application.OpenURL("https://lab.homagames.com/damysus/info?manifestListFilters[name_generic]=nerf&page=1");
                }
            }
        }
#if !HOMA_BELLY_INSTALL_SUPPORTED
        GUILayout.Label("Homa Belly is only supported for Android and iOS targets.\n Please change the target in Unity Build Settings.",
            HomaGamesStyle.ErrorLabelStyle);
#endif
    }

    private bool IsValidAppToken(string appToken)
    {
        return !string.IsNullOrEmpty(appToken)
            && !appToken.Contains(" ");
    }

    protected override void OnVisibleFocus()
    {
        // NO-OP
    }
}
