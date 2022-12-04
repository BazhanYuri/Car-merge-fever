using System.IO;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Callbacks;
using UnityEngine;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// By the time we update Facebook to its v13.2.0 version, Facebook's Client Token is mandatory
    /// and needs to be added to AndroidManifest.xml and Info.plist.
    ///
    /// This class implements and automates those additions while the new Facebook's version is released with
    /// the fix. More info here: https://github.com/facebook/facebook-sdk-for-unity/issues/618
    /// </summary>
    public class FacebookPreProcessBuild : IPostGenerateGradleAndroidProject
    {
        [PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) 
        {
#if UNITY_IOS
            if (target == BuildTarget.iOS)
            {
                if (Facebook.Unity.Settings.FacebookSettings.ClientTokens?.Count == 0
                    || string.IsNullOrEmpty(Facebook.Unity.Settings.FacebookSettings.ClientTokens?[0]))
                {
                    Debug.LogError("The Facebook Client Token has not been set in your Homa Belly manifest. " +
                                   "Please update the Client Token field of your manifest and refresh Homa Belly.");
                    return;
                }
                
                PlistDocument plist = new PlistDocument();
                plist.ReadFromString(File.ReadAllText(pathToBuiltProject + "/Info.plist"));
                plist.root.values.Add("FacebookClientToken", new PlistElementString(Facebook.Unity.Settings.FacebookSettings.ClientTokens[0]));
                // Write to file
                File.WriteAllText(pathToBuiltProject + "/Info.plist", plist.WriteToString());
            }
#endif
        }
        
        public int callbackOrder => 99;
        public void OnPostGenerateGradleAndroidProject(string path)
        {
#if UNITY_ANDROID
            if (Facebook.Unity.Settings.FacebookSettings.ClientTokens?.Count == 0
                || string.IsNullOrEmpty(Facebook.Unity.Settings.FacebookSettings.ClientTokens?[0]))
            {
                Debug.LogError("The Facebook Client Token has not been set in your Homa Belly manifest. " +
                               "Please update the Client Token field of your manifest and refresh Homa Belly.");
                return;
            }
            
            AndroidManifestUtils.AddMetaDataToManifest(Path.Combine(path, "src/main/AndroidManifest.xml"), "com.facebook.sdk.ClientToken", Facebook.Unity.Settings.FacebookSettings.ClientTokens[0]);
#endif
        }
    }
}
