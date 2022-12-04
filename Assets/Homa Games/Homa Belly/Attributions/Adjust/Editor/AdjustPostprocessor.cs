using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using HomaGames.HomaBelly.Installer;
using HomaGames.HomaBelly.Utilities;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
#if UNITY_IOS
    using UnityEditor.iOS.Xcode;
#endif
using UnityEngine;
using UnityEngine.Networking;

namespace HomaGames.HomaBelly
{
    public static class AdjustPostprocessor
    {
        [InitializeOnLoadMethod]
        static void InitializeOnLoad()
        {
            HomaBellyEditorLog.Debug($"Configuring {HomaBellyAdjustConstants.ID}");

            ConfigureAdjustManifestConfig();

            if (AdjustSettings.NullableInstance != null)
            {
                // This framework is needed so that the SDK can automatically handle attribution for ASA campaigns you are running.
                // If your app is targeting the "Kids" category, you should not implement this framework.
                AdjustSettings.iOSFrameworkiAd = true;
                // For devices running iOS 14.3 or higher, this framework allows the SDK to automatically handle attribution for ASA campaigns. It is required when leveraging the Apple Ads Attribution API.
                AdjustSettings.iOSFrameworkAdServices = true;
                // This framework is needed so that the SDK can access the IDFA value and – prior to iOS 14 – LAT information.
                // If your app is targeting the "Kids" category, you should not implement this framework.
                AdjustSettings.iOSFrameworkAdSupport = true;
                
                EditorUtility.SetDirty(AdjustSettings.NullableInstance);
            }

            BuildPlayerHandlerWrapper.AddBuildFilter(ManifestAdjustConfigDoesntExist);
            BuildPlayerHandlerWrapper.AddBuildFilter(ValidConfiguration);
        }

        private static void ConfigureAdjustManifestConfig()
        {
            var pluginManifest = PluginManifest.LoadFromLocalFile();
            
            var packageComponent = pluginManifest?.Packages
                .GetPackageComponent(HomaBellyAdjustConstants.ID, EditorPackageType.ATTRIBUTION_PLATFORM);

            if (packageComponent == null)
            {
                return;
            }
            
            var configurationData = packageComponent.Data;
            ManifestAdjustConfig.Instance.FillWithValuesFromManifestDictionary(configurationData);
        }

        private static bool ManifestAdjustConfigDoesntExist(BuildReport options)
        {
            if (ManifestAdjustConfig.Instance == null)
            {
                // This should never happen, on Editor time the AdjustConfig is recreated everytime InitializeOnLoad is called.
                Debug.LogError($"[AdjustPostprocessor] Can't find AdjustConfig scriptable in: {HomaBellyAdjustConstants.CONFIG_FILE_PATH_IN_RESOURCES}");
                return false;
            }
            
            return true;
        }
        
        private static bool ValidConfiguration(BuildReport options)
        {
            ConfigureAdjustManifestConfig();
            if (!ManifestAdjustConfig.Instance.ConfigurationForCurrentPlatformIsValid())
            {
                // Log an error but don't cancel the build
                Debug.LogError($"[AdjustPostprocessor] Adjust configuration is not valid. Please check your configuration file: {HomaBellyAdjustConstants.CONFIG_FILE_PATH_IN_RESOURCES}");
                return false;
            }

            return true;
        }

        private class AndroidManifestModification : IPostGenerateGradleAndroidProject
        {
            public int callbackOrder { get; }
            public void OnPostGenerateGradleAndroidProject(string path)
            {
                Android12AdIdPermission(path);
                AddProGuardRules(path);
            }

            private static void Android12AdIdPermission(string path)
            {
                // Add AD ID permission if target SDK is >= Android 12
                // https://github.com/adjust/unity_sdk#add-permission-to-gather-google-advertising-id

                var appManifestPath = Path.Combine(path, "src/main/AndroidManifest.xml");

                var targetSdkVersion = PlayerSettings.Android.targetSdkVersion;
                var add = (int)targetSdkVersion >= 31 || targetSdkVersion == AndroidSdkVersions.AndroidApiLevelAuto;
                TempAndroidManifestUtilsUntilNextCoreRelease.UpdatePermissionInManifest(appManifestPath,
                    "com.google.android.gms.permission.AD_ID",
                    add);
            }
            
            private static void AddProGuardRules(string path)
            {
                string adjustProGuardRulesPath = $"{Application.dataPath}/Homa Games/Homa Belly/Attributions/Adjust/Editor/AdjustProGuardRules.txt";
                
                if (!File.Exists(adjustProGuardRulesPath))
                {
                    Debug.LogError($"[AdjustPostprocessor] Can't find AdjustProGuardRules.txt file in: {adjustProGuardRulesPath}");
                    return;
                }
                
                var proGuardUnityFile = Path.Combine(path, "proguard-unity.txt");
                if (!File.Exists(proGuardUnityFile))
                {
                    Debug.LogError($"[AdjustPostprocessor] Can't find proguard-unity.txt file in: {proGuardUnityFile}");
                    return;
                }
                
                string proGuardRules = File.ReadAllText(adjustProGuardRulesPath);
                AndroidProguardUtils.AddProguardRules(proGuardRules,proGuardUnityFile);
            }
        }
        
        #region Adjust Signature V2 Libraries

        private class AdjustSignatureV2SetupBuild : IPreprocessBuildWithReport
        {
            // I want to call it after default callbacks
            public int callbackOrder { get; } = 10;

            public void OnPreprocessBuild(BuildReport report)
            {
                GetSignatureV2Libraries();
            }
        }

        /// <summary>
        /// To use Adjust Signature V2 to have safest attribution, we need to download a library (iOS & Android)
        /// and include it in the project in release builds.
        /// Adjust will use automatically those libraries if they are present in the project.
        /// </summary>
        [MenuItem("Assets/Adjust/Manually Fetch Signature Libraries")]
        public static void GetSignatureV2Libraries()
        {
            Debug.Log("[AdjustPostprocessor] Checking Adjust Signature Libraries");
            UpdateProgressBar(0f);

            try
            {
                var pluginManifest = PluginManifest.LoadFromLocalFile();

                if (pluginManifest == null)
                {
                    return;
                }
                
                var appToken = pluginManifest.AppToken;

                if (string.IsNullOrEmpty(appToken))
                {
                    return;
                }
                
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    Debug.LogWarning($"[AdjustPostProcessor] AdjustSignatureV2ConfigDeserializer could not be fetched in offline mode");
                    return;
                }
                
                var appBaseUrl = string.Format(PostBuildConstants.API_APP_POST_BUILD,
                    appToken,
                    UserAgent.GetUserAgent());

                var editorHttpCaller = new EditorHttpCaller<AdjustSignatureV2ConfigDeserializer.ConfigModel>();
                EditorAnalytics.TrackEditorAnalyticsEvent("Adjust Signing V2 - Appbase query fired");
                
                
                AdjustSignatureV2ConfigDeserializer.ConfigModel signatureConfig;

                try
                {
                    signatureConfig = editorHttpCaller.GetSynchronous(appBaseUrl, new AdjustSignatureV2ConfigDeserializer());
                }
                catch (EditorHttpCallerException e)
                {
                    // Error 408: Request timeout https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/408
                    if (e.Status != "408") throw;
                    
                    Debug.LogError(
                        "[AdjustPostprocessor] Timeout requesting appbuild info to Damysus. Can't update Adjust signing libraries.");
                    return;
                }

                UpdateProgressBar(0.3f);
                 
                 if (signatureConfig != null)
                 {
                     EditorAnalytics.TrackEditorAnalyticsEvent("Adjust Signing V2 - Signature Config deserialized.");

                     if (!string.IsNullOrWhiteSpace(signatureConfig.IosLibUrl))
                     {
                         DownloadLibraryIfNecessary(signatureConfig.IosLibUrl, HomaBellyAdjustConstants.SIGNATURE_IOS_PATH,
                             signatureConfig.IosMd5Sum,
                             0.3f,
                             0.33f);   
                     }
                     else
                     {
                         Debug.Log($"[AdjustPostprocessor] iOS Adjust Signature Library not available to download.");
                     }

                     if (!string.IsNullOrWhiteSpace(signatureConfig.AndroidLibUrl))
                     {
                         DownloadLibraryIfNecessary(signatureConfig.AndroidLibUrl,
                             HomaBellyAdjustConstants.SIGNATURE_ANDROID_PATH,
                             signatureConfig.AndroidMd5Sum,
                             0.66f,
                             0.33f);
                     }
                     else
                     {
                         Debug.Log($"[AdjustPostprocessor] Android Adjust Signature Library not available to download.");
                     }
                 }
                 else
                 {
                     Debug.LogWarning($"[AdjustPostprocessor] AdjustSignatureV2ConfigDeserializer config object is null.");
                     EditorAnalytics.TrackEditorAnalyticsEvent("Adjust Signing V2 - Signature Config deserialize error.");
                 }
                 
                ChangeImportSettings(HomaBellyAdjustConstants.SIGNATURE_ANDROID_PATH, true);
                ChangeImportSettings(HomaBellyAdjustConstants.SIGNATURE_IOS_PATH, false);
            }
            catch (Exception e)
            {
                Debug.LogError("[AdjustPostprocessor] Can't get Adjust signature config. " + e.Message);
                EditorAnalytics.TrackEditorAnalyticsEvent("Adjust Signing V2 - Exception downloading Signature V2",e.Message,e.StackTrace);
            }
            finally
            {
                UpdateProgressBar(1f);
            }
        }
        
        private static void DownloadLibraryIfNecessary(string url, 
            string pathAndFileName,
            string remoteMd5Checksum,
            float currentProgress,
            float progressStep)
        {
            try
            {
                bool hasToDownload = false;
                
                var directory = Path.GetDirectoryName(pathAndFileName);
                if (directory != null && !Directory.Exists(directory))
                {
                    hasToDownload = true;
                    Directory.CreateDirectory(directory);
                }
                else if (!File.Exists(pathAndFileName))
                {
                    hasToDownload = true;
                }
                else
                {
                    var md5Checksum = GetSignatureFileMd5(pathAndFileName);

                    if (md5Checksum != remoteMd5Checksum)
                    {
                        hasToDownload = true;
                    }
                }

                string fileName = Path.GetFileName(pathAndFileName);
                EditorAnalytics.TrackEditorAnalyticsEvent($"Adjust Signing V2. Has to download:{hasToDownload}",fileName);
                if (!hasToDownload)
                {
                    Debug.Log($"[AdjustPostprocessor] Adjust signature library already downloaded: {pathAndFileName}");
                    return;
                }

                using (var unityWebRequest = new UnityWebRequest(url))
                {
                    var downloadHandlerFile = new DownloadHandlerFile(pathAndFileName, false);
                    downloadHandlerFile.removeFileOnAbort = true;
                    unityWebRequest.downloadHandler = downloadHandlerFile;
                    unityWebRequest.timeout = 15;
                    var request = unityWebRequest.SendWebRequest();

                    while (!request.isDone)
                    {
                        // Block the main thread while downloading. I need this request to be sync to be executed before build starts.
                        UpdateProgressBar(currentProgress + (request.progress * progressStep));
                    }

                    if (string.IsNullOrEmpty(unityWebRequest.error))
                    {
                        Debug.Log($"[AdjustPostprocessor] Downloaded Adjust Signature Library: {pathAndFileName}");
                        EditorAnalytics.TrackEditorAnalyticsEvent($"Adjust Signing V2. Download succeeded.",fileName);
                        AssetDatabase.Refresh();
                    }
                    else
                    {
                        Debug.LogError(
                            $"[AdjustPostprocessor] Can't download library. {unityWebRequest.error} to path: {pathAndFileName}");
                        EditorAnalytics.TrackEditorAnalyticsEvent($"Adjust Signing V2. Download failed.",fileName);
                    }
                }
            }
            finally
            {
                UpdateProgressBar(currentProgress + progressStep);
            }
        }

        private static string GetSignatureFileMd5(string pathAndFileName)
        {
            string md5String = "";
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(pathAndFileName))
                {
                    byte[] md5ByteArray = md5.ComputeHash(stream);
                    md5String = BitConverter.ToString(md5ByteArray).Replace("-", "").ToLowerInvariant();
                }
            }

            return md5String;
        }

        private static void UpdateProgressBar(float progress)
        {
            if (progress < 1f)
            {
                EditorUtility.DisplayProgressBar("Adjust Signature Config", "Requesting signature libraries", progress);
            }
            else
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static void ChangeImportSettings(string filePath,bool isAndroid)
        {
            if (!File.Exists(filePath))
            {
                Debug.Log($"[AdjustPostprocessor] Signature Library not found: {filePath}. This feature is optional" +
                          $" so you may be good with this log. You can proceed as build won't be affected.");
                return;
            }
            
            var pluginImporter = AssetImporter.GetAtPath(filePath) as PluginImporter;

            if (pluginImporter == null)
            {
                Debug.LogError("[AdjustPostprocessor] The file isn't a plugin: "+filePath);
                return;
            }

            #if HOMA_DEVELOPMENT
            var enabled = false;
            #else
            var enabled = true;
            #endif
            
            pluginImporter.SetCompatibleWithAnyPlatform(false);
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            pluginImporter.SetCompatibleWithPlatform(isAndroid ? BuildTarget.Android : BuildTarget.iOS,enabled);

            EditorUtility.SetDirty(pluginImporter);
            pluginImporter.SaveAndReimport();
        }

        [PostProcessBuild]
        public static void RemoveSigningLibraryIfNecessary(BuildTarget target, string projectPath)
        {
            if (target != BuildTarget.iOS)
            {
                return;
            }

            #if !HOMA_DEVELOPMENT
                 return;       
            #endif
            
            #if UNITY_IOS
                // ReSharper disable once HeuristicUnreachableCode
                string xcodeProjectPath = projectPath + "/Unity-iPhone.xcodeproj/project.pbxproj";

                PBXProject xcodeProject = new PBXProject();
                xcodeProject.ReadFromFile(xcodeProjectPath);

                string path = HomaBellyAdjustConstants.SIGNATURE_IOS_PATH.Replace("Assets", "Libraries");
                var fileGUid = xcodeProject.FindFileGuidByProjectPath(path);
                
                if (!string.IsNullOrEmpty(fileGUid))
                {
                    Debug.Log("AdjustSigSdk.a isn't longer needed for development builds. Removing file in: " + path);
                    xcodeProject.RemoveFile(fileGUid);
                    xcodeProject.WriteToFile(xcodeProjectPath);
                }
            #endif
        }
            
        #endregion
        
        #region Referral System Postprocess
        
        private class SetupReferralInAdjust : IPreprocessBuildWithReport
        {
            public int callbackOrder { get; } = 0;
            public void OnPreprocessBuild(BuildReport report)
            {
                SetupReferralSchemas();
            }
        }

        
        [MenuItem("Assets/Adjust/Manually Setup Referral Schemas")]
        public static void SetupReferralSchemas()
        {
            if (ManifestAdjustConfig.Instance.IsReferralSystemEnabled)
            {
#if UNITY_ANDROID
                var exitingSchemas = AdjustSettings.AndroidUriSchemes;
                var newSchemas = AddSchemaToAdjustSettings(exitingSchemas);
                AdjustSettings.AndroidUriSchemes = newSchemas;
#elif UNITY_IOS
                AdjustSettings.iOSUrlIdentifier = Application.identifier;
                
                var existingSchemas = AdjustSettings.iOSUrlSchemes;
                var newSchemas = AddSchemaToAdjustSettings(existingSchemas);
                AdjustSettings.iOSUrlSchemes = newSchemas;
#endif
            }
            else
            {
#if UNITY_ANDROID
                var newSchemas = RemoveSchemaFromAdjustSettings(AdjustSettings.AndroidUriSchemes);
                AdjustSettings.AndroidUriSchemes = newSchemas;
#elif UNITY_IOS
                    AdjustSettings.iOSUrlIdentifier = "";
                    var newSchemas = RemoveSchemaFromAdjustSettings(AdjustSettings.iOSUrlSchemes);
                    AdjustSettings.iOSUrlSchemes = newSchemas;
#endif
                EditorUtility.SetDirty(AdjustSettings.Instance);
            }
        }
        
        private static string[] AddSchemaToAdjustSettings(string[] exitingSchemas)
        {
            var referralSystemSchema = ManifestAdjustConfig.Instance.GetSchemaForCurrentPlatformToSetupPostprocess();
            if (exitingSchemas == null)
            {
                exitingSchemas = new string[1]{referralSystemSchema.Trim()};
                Debug.Log($"Added schema to Adjust: {exitingSchemas[exitingSchemas.Length - 1]}");
            }
            else
            {
                if (!Array.Exists(exitingSchemas, existingSchema => existingSchema == referralSystemSchema))
                {
                    var newSchemasArray = new string[exitingSchemas.Length + 1];
                    Array.Copy(exitingSchemas, newSchemasArray, exitingSchemas.Length);
                    exitingSchemas = newSchemasArray;
                    exitingSchemas[exitingSchemas.Length - 1] = referralSystemSchema.Trim();
                    Debug.Log($"Added schema to Adjust: {exitingSchemas[exitingSchemas.Length - 1]}");
                }
            }

            return exitingSchemas;
        }
            
        private static string[] RemoveSchemaFromAdjustSettings(string[] schemas)
        {
            var referralSchema = ManifestAdjustConfig.Instance.GetSchemaForCurrentPlatformToSetupPostprocess();
            var validReferralSchema = referralSchema.Trim();
            if (schemas != null
                && Array.Exists(schemas, existingSchema => existingSchema.Trim() == validReferralSchema))
            {
                schemas= schemas.Where(schema => schema.Trim() != validReferralSchema).ToArray();
                Debug.Log($"Removed schema from Adjust: {validReferralSchema}");
            }
                
            return schemas;
        }

        #endregion
    }
}