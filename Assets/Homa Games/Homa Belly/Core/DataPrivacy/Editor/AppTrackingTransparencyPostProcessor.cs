using UnityEditor.Callbacks;
using UnityEditor;
using System.IO;
using UnityEngine;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace HomaGames.HomaBelly.DataPrivacy
{
    /// <summary>
    /// PostProcessor script to automatically fill all required dependencies
    /// for App Tracking Transparency
    /// </summary>
    public class AppTrackingTransparencyPostProcessor
    {
#if UNITY_IOS
        [PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string buildPath)
        {
            if (buildTarget == BuildTarget.iOS)
            {
                Settings settings = DataPrivacy.Settings.EditorCreateOrLoadDataPrivacySettings();
                if (settings != null && !settings.ShowIdfa)
                    return;
                
                /*
                 * PBXProject
                 */
                PBXProject project = new PBXProject();
                string projectPath = PBXProject.GetPBXProjectPath(buildPath);
                project.ReadFromFile(projectPath);

                // If loaded, add `AppTrackingTransparency` Framework
                if (project != null)
                {
                    string targetId;
#if UNITY_2019_3_OR_NEWER
                    targetId = project.GetUnityFrameworkTargetGuid();
#else
                    targetId = project.TargetGuidByName("Unity-iPhone");
#endif

                    project.AddFrameworkToProject(targetId, "AppTrackingTransparency.framework", true);
                    project.AddFrameworkToProject(targetId, "AdSupport.framework", false);
                    project.AddFrameworkToProject(targetId, "StoreKit.framework", false);

                    project.WriteToFile(PBXProject.GetPBXProjectPath(buildPath));
                }

                /*
                 * PList
                 */
                PlistDocument plist = new PlistDocument();
                plist.ReadFromString(File.ReadAllText(buildPath + "/Info.plist"));
                if (plist != null)
                {
                    // Get root
                    PlistElementDict rootDict = plist.root;

                    // Add NSUserTrackingUsageDescription
                    string iOSIdfaPopupMessage = Settings.DEFAULT_APPLE_MESSAGE;
                    if (settings != null)
                    {
                        iOSIdfaPopupMessage = settings.iOSIdfaPopupMessage;
                    }

                    // Override previous legacy default message if found
                    if (iOSIdfaPopupMessage == Settings.LEGACY_DEFAULT_APPLE_MESSAGE)
                    {
                        iOSIdfaPopupMessage = Settings.DEFAULT_APPLE_MESSAGE;
                    }

                    rootDict.SetString("NSUserTrackingUsageDescription", iOSIdfaPopupMessage);

                    File.WriteAllText(buildPath + "/Info.plist", plist.WriteToString());
                }
            }
        }
#endif
    }
}