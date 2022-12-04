using UnityEditor;
using System;
using System.Collections.Generic;

#if UNITY_2018_4_OR_NEWER
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
#endif
using UnityEngine;

namespace HomaGames.HomaBelly.DataPrivacy
{
    public class PackageManagerDependencies
    {
        // legacy names
        private const string DEPENDENCIES_MET_KEY = "homagames.gdpr.dependencies_met";
        private const string HOMA_GAMES_DATA_PRIVACY_TMP_DEFINE = "homagames_gdpr_textmeshproavailable";
        private const string TMP_PACKAGE_ID = "com.unity.textmeshpro";

#if UNITY_2018_4_OR_NEWER
        private static AddRequest installationRequest;
        private static ListRequest listRequest;

        [InitializeOnLoadMethod]
        static void CheckDependencies()
        {
            bool dependenciesMet = EditorPrefs.GetBool(DEPENDENCIES_MET_KEY, false);
            if (!dependenciesMet)
            {
                listRequest = Client.List();
                EditorApplication.update += ListProgress;

                EditorPrefs.SetBool(DEPENDENCIES_MET_KEY, true);
            }
            else
            {
                // Ensure define is always present
                DefineSymbolsUtility.SetDefineSymbolValue(HOMA_GAMES_DATA_PRIVACY_TMP_DEFINE, true);
            }
        }

        static void ListProgress()
        {
            if (listRequest.IsCompleted)
            {
                if (listRequest.Status == StatusCode.Success)
                {
                    bool installed = false;
                    foreach (var package in listRequest.Result)
                    {
                        if (package.name == TMP_PACKAGE_ID)
                        {
                            // TMP is installed, do nothing
                            Debug.Log("Detected text mesh pro package");
                            DefineSymbolsUtility.SetDefineSymbolValue(HOMA_GAMES_DATA_PRIVACY_TMP_DEFINE, true);
                            installed = true;
                        }
                    }

                    // If it is not installed, add it
                    if (!installed)
                    {
                        RequestInstallation();
                    }
                }
                else if (listRequest.Status >= StatusCode.Failure)
                {
                    Debug.Log(listRequest.Error.message);
                }

                EditorApplication.update -= ListProgress;
            }
        }

        static void InstallationProgress()
        {
            if (installationRequest.IsCompleted)
            {
                if (installationRequest.Status == StatusCode.Success)
                {
                    Debug.Log("Installed: " + installationRequest.Result.packageId);
                    DefineSymbolsUtility.SetDefineSymbolValue(HOMA_GAMES_DATA_PRIVACY_TMP_DEFINE, true);
                }
                else if (installationRequest.Status >= StatusCode.Failure)
                {
                    Debug.Log(installationRequest.Error.message);
                    RequestInstallation();
                }

                EditorApplication.update -= InstallationProgress;
            }
        }

        private static void RequestInstallation()
        {
            if (Application.isBatchMode)
            {
                return;
            }
            
            bool result = EditorUtility.DisplayDialog("Homa Games DataPrivacy", "Text Mesh Pro is required for DataPrivacy module. It will be automatically imported now", "Accept", "Cancel");
            if (result)
            {
                installationRequest = Client.Add(TMP_PACKAGE_ID);
                EditorApplication.update += InstallationProgress;
            }
            else
            {
                Debug.LogError("Homa Games DataPrivacy module requires Text Mesh Pro package. Please install it through Package Manager to avoid compilation errors");
            }
        }
#endif
    }
}
