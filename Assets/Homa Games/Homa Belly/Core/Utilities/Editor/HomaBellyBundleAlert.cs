using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Utility class showing a best practice advise popup to unify
    /// Android & iOS application identifiers. It will show only once
    /// and it is recommended to be lowecase and the same for both
    /// platforms
    /// </summary>
    public class HomaBellyBundleAlert
    {
        private static string BUNDLE_BEST_PRACTICE_ALERT_SHOWN = "homagames.homabelly.bundle_best_practice_alert_shown";

        [InitializeOnLoadMethod]
        public static void InitializeOnLoad()
        {
            if (Application.isBatchMode)
            {
                return;
            }
            
            if (!EditorPrefs.HasKey(BUNDLE_BEST_PRACTICE_ALERT_SHOWN))
            {
                string androidBundleId = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
                string iosBundleId = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS);

                if (androidBundleId != iosBundleId
                    || androidBundleId.Any(char.IsUpper)
                    || iosBundleId.Any(char.IsUpper))
                {
                    // Bundle IDs are different or contain Upper Case characters
                    int updateBundleIds = EditorUtility.DisplayDialogComplex("Homa Belly Best Pracite Tip",
                        "Your Android & iOS application identifiers differ or have upper case characters:" +
                        $"\n\n• Android: {androidBundleId}\n• iOS: {iosBundleId}\n\n" +
                        "It is recommended to be the same for both platforms and use only lower case characters.\n\n" +
                        "Do you want to unfiy them now?", "Use Android", "Use iOS", "Will do it manually");

                    switch(updateBundleIds)
                    {
                        // Use Android
                        case 0:
                            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, androidBundleId.ToLower());
                            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, androidBundleId.ToLower());
                            EditorUtility.DisplayDialog("Homa Belly Best Practice Tip",
                                $"Application identifiers updated to: {androidBundleId.ToLower()}", "Ok");
                            break;
                        // Use iOS
                        case 1:
                            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, iosBundleId.ToLower());
                            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, iosBundleId.ToLower());
                            EditorUtility.DisplayDialog("Homa Belly Best Practice Tip",
                                $"Application identifiers updated to: {iosBundleId.ToLower()}", "Ok");
                            break;
                        // Do it manually
                        case 2:
                            // Do nothing automatically
                            break;
                    }

                    // Avoid showing this in the future
                    EditorPrefs.SetBool(BUNDLE_BEST_PRACTICE_ALERT_SHOWN, true);
                }
            }
        }
    }
}
