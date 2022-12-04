using UnityEngine;
using UnityEditor;
using HomaGames.HomaBelly.Utilities;
using System;
using System.Collections.Generic;
using JetBrains.Annotations;

#if UNITY_IOS
using System.Threading;
using System.Threading.Tasks;
using UnityEditor.iOS.Xcode;
using UnityEditor.Callbacks;
#endif

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Postprocessor executed upon iOS build. It fetches any
    /// configuration from servers and applies it to the build:
    /// 
    /// - List of SkAdNetwork IDs to be added to Info.plist
    /// </summary>
    public class iOSPostProcessBuild
    {
#if UNITY_IOS
        [MenuItem("Tools/Core/iOS/Post Process Build")]
        public static void Test()
        {
            OnPostprocessBuild(BuildTarget.iOS, System.IO.Path.Combine(Application.dataPath + "/../Build/ios_fix/"));
        }
        
        private static readonly string APP_POST_BUILD_ENDPOINT = HomaBellyConstants.API_HOST + "/appbuild";

        private static string GeneratePostBuildUrl(string ti, string userAgent)
        {
            return UriHelper.AddGetParameters(APP_POST_BUILD_ENDPOINT, new Dictionary<string, string>
            {
                {"cv", HomaBellyConstants.API_VERSION},
                {"sv", HomaBellyConstants.PRODUCT_VERSION},
                {"av", Application.version},
                {"ti", ti},
                {"ai", Application.identifier},
                {"ua", userAgent}
            });
        }

        [PostProcessBuild(int.MaxValue)]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string buildPath)
        {
            try
            {
                var project = iOSPbxProjectUtils.LoadPbxProject(buildPath);
                project.SetBuildProperty(project.GetUnityFrameworkTargetGuid(), "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");
                project.SetBuildProperty(project.GetUnityMainTargetGuid(), "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");
                iOSPbxProjectUtils.SavePbxProjectFile(buildPath,project);
                
                // Fetch Post Build model
                PostBuildModel model = FetchPostBuildModel();

                // If Homa Belly manifest contains any mediator, try to add SkAdNetworkIDs
                PluginManifest pluginManifest = PluginManifest.LoadFromLocalFile();
                if (pluginManifest != null
                    && pluginManifest.Packages.MediationLayers != null
                    && pluginManifest.Packages.MediationLayers.Count > 0
                    && model.SkAdNetworkIds != null
                    && model.SkAdNetworkIds.Length > 0)
                {
                    UnityEngine.Debug.Log($"[iOS Post Process Build] Adding SkAdNetworkIDs to Info.plist: {Json.Serialize(model.SkAdNetworkIds)}");
                    iOSPlistUtils.AppendAdNetworkIds(buildTarget, buildPath, model.SkAdNetworkIds);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning($"[iOS Post Process Build] Exception thrown while post build processing: {e.Message}");
            }
        }

        #region Private helpers
        
        [NotNull]
        private static PostBuildModel FetchPostBuildModel()
        {
            EditorHttpCaller<PostBuildModel> editorHttpCaller = new EditorHttpCaller<PostBuildModel>();

            string appBuild = GeneratePostBuildUrl(PluginManifest.LoadFromLocalFile().AppToken, "IPHONE");
            return editorHttpCaller.GetSynchronous(appBuild, new PostBuildModelDeserializer());
        }

        #endregion
#endif
    }
}
