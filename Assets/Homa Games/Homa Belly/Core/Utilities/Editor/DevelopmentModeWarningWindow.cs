using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;


// ReSharper disable once CheckNamespace
namespace HomaGames.HomaBelly {
    /// <summary>
    /// This class handles the warning windows for <c>HOMA_DEVELOPMENT</c>.
    /// There are two different kind of windows: the warning and consistency window.<br /><br />
    /// The warning window is shown when compiling a dev build with <c>HOMA_DEVELOPMENT</c>.
    /// Its goal is to warn the user that this mode alter settings without them knowing.
    /// This window is meant to be discarded once the user knows about it.<br /><br />
    /// The second consistency window warns the user about a much more dangerous issue:
    /// it shows if they try to compile a release build with the <c>HOMA_DEVELOPMENT</c> define
    /// enabled. This window can only be discarded for the current session. 
    /// </summary>
    public static class DevelopmentModeWarningWindow {
        
        // ReSharper disable UnusedMember.Local
        private const string DEVELOPMENT_MODE_SHOW_WARNING_PREF_KEY = "homagames.development_mode_show_warning";
        private const string DEVELOPMENT_MODE_CONSISTENCY_STORAGE_KEY = "homagames.development_mode_consistency_key";

        private static bool ShowWarningWindow {
            get => EditorPrefs.GetBool(DEVELOPMENT_MODE_SHOW_WARNING_PREF_KEY, true) && !Application.isBatchMode;
            set => EditorPrefs.SetBool(DEVELOPMENT_MODE_SHOW_WARNING_PREF_KEY, value); 
        }
        // ReSharper restore UnusedMember.Local

        
        [InitializeOnLoadMethod]
        private static void RegisterWarningWindowFilter() {
            BuildPlayerHandlerWrapper.AddInteractiveFilter(DevelopmentModeWarningFilter);
        }

        private static bool DevelopmentModeWarningFilter(BuildReport report) {
            
#if HOMA_DEVELOPMENT
            if (ShowWarningWindow) {
                int dialogOutput = EditorUtility.DisplayDialogComplex(
                    "Homa Development",
                    "Homa Development mode is enabled. \n" +
                    "You will not be allowed to push this build to the stores. \n\n" +
                    "Go to Window > Homa Games > Homa Belly > Settings for more details.\n",
                    "This is a debug build",
                    "Cancel",
                    "Don't show again");

                if (dialogOutput == 1) {
                    return false;
                }
                else if (dialogOutput == 2) {
                    ShowWarningWindow = false;
                }
            }
#endif
            return true;
        }
    }
}
