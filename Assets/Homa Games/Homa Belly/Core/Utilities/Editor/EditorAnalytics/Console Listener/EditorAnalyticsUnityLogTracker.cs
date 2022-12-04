#if HOMA_BELLY_EDITOR_ANALYTICS_ENABLED && UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Standalone class listening Unity's Console Logs to keep track of interesting
    /// runtime, editor and build errors
    /// </summary>
    public class EditorAnalyticsUnityLogTracker : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        /// <summary>
        /// Because how Android build errors are displayed (one log entry for each error sentence)
        /// we will collect 5 sequential log entries for tracking Android build error 
        /// </summary>
        private static readonly int NumberOfErrorsForAndroidBuild = 5;
        private static readonly StringBuilder AndroidBuildErrorStringBuilder = new StringBuilder();
        private static int _remainingErrorsClassifiedAsAndroidBuildErrors;
        
        /// <summary>
        /// Keep track of Unity Editor performing a build
        /// </summary>
        private static bool _building;
        
        /// <summary>
        ///  Callback order for IPreprocessBuildWithReport
        /// </summary>
        public int callbackOrder => 0;
        
        /// <summary>
        /// Keep track of latest log detected to avoid tracking the same log
        /// multiple times
        /// </summary>
        private static string _latestConditionMessageSent;

        [InitializeOnLoadMethod]
        static void Initialize()
        {
            // TODO: Use logMessageReceivedThreaded?
            // removing the callback first makes sure it is only added once
            Application.logMessageReceived -= LogMessageReceived;
            Application.logMessageReceived += LogMessageReceived;
            
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
        }

        #region Build callbacks
        
        private static void OnBeforeAssemblyReload()
        {
            _building = false;
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            _building = true;

#if UNITY_ANDROID
            // We might find useful Android Resolver settings when developers build (if they have resolution enabled)
            string androidResolverFilePath = Application.dataPath + "/../ProjectSettings/GvhProjectSettings.xml";
            string androidResolverSettings = null;
            if (File.Exists(androidResolverFilePath))
            {
                androidResolverSettings = File.ReadAllText(androidResolverFilePath);    
            }
            
            EditorAnalytics.TrackEditorAnalyticsEvent("android_build_started", androidResolverSettings);
#elif UNITY_IOS
            EditorAnalytics.TrackEditorAnalyticsEvent("ios_build_started");
#endif
        }
        
        /// <summary>
        /// This method is not invoked if the build process fails
        /// </summary>
        /// <param name="report"></param>
        public void OnPostprocessBuild(BuildReport report)
        {
            _building = false;
#if UNITY_ANDROID
            EditorAnalytics.TrackEditorAnalyticsEvent("android_build_succeeded");
#elif UNITY_IOS
            EditorAnalytics.TrackEditorAnalyticsEvent("ios_build_succeeded");
#endif
        }

        #endregion

        /// <summary>
        /// Determine if the received log should be skipped and not tracked
        /// </summary>
        /// <param name="condition">The log condition</param>
        /// <param name="type">The log type</param>
        /// <returns>true if it should be skipped</returns>
        private static bool ShouldSkipLog(string condition, LogType type)
        {
            bool isDesiredLogType = type == LogType.Error || type == LogType.Exception;
            return _latestConditionMessageSent == condition || !isDesiredLogType;
        }
        
        private static void LogMessageReceived(string condition, string stackTrace, LogType type)
        {
            // Avoid processing undesired logs
            if (ShouldSkipLog(condition, type))
            {
                return;
            }

            _latestConditionMessageSent = condition;
            
            // Handle a bit log string to ease content detection
            string lowerCaseCondition = condition.ToLower().Replace(" ", "");
            string lowerCaseStackTrace = stackTrace.ToLower().Replace("\n", " ").Replace(" ", "");

            if (_building)
            {
                // Log here building errors
                if (_remainingErrorsClassifiedAsAndroidBuildErrors > 0)
                {
                    AndroidBuildErrorStringBuilder.Append($" {condition}");
                    _remainingErrorsClassifiedAsAndroidBuildErrors--;

                    // Send the Android build error event
                    if (_remainingErrorsClassifiedAsAndroidBuildErrors == 0)
                    {
                        EditorAnalytics.TrackEditorAnalyticsEvent("android_build_error", AndroidBuildErrorStringBuilder.ToString());
                        AndroidBuildErrorStringBuilder.Clear();
                    }
                }
                // Upon Android build error, the most important messages will come after "exception was raised by workers"
                else if (lowerCaseCondition.Contains("exceptionwasraisedbyworkers"))
                {
                    // Next 3 errors will be sent all together as an android_build error
                    AndroidBuildErrorStringBuilder.Clear();
                    _remainingErrorsClassifiedAsAndroidBuildErrors = NumberOfErrorsForAndroidBuild;
                }
                // Cocoapods generation failure generates a log like "iOS framework addition failed"
                else if (lowerCaseCondition.Contains("iosframeworkadditionfailed"))
                {
                    EditorAnalytics.TrackEditorAnalyticsEvent("ios_cocoapods_error", condition, stackTrace);
                }
            }
            else
            {            
                // Important to know if it is a Homa related error or not
                bool isHomaRelatedError = lowerCaseCondition.Contains("dvr")
                                                   || lowerCaseCondition.Contains("homabelly")
                                                   || lowerCaseCondition.Contains("homagames")
                                                   || lowerCaseStackTrace.Contains("dvr")
                                                   || lowerCaseStackTrace.Contains("homabelly")
                                                   || lowerCaseStackTrace.Contains("homagames");
                
                // Always check Homa related error the last one to avoid being skipped in more accurate error logs
                if (isHomaRelatedError)
                {
                    EditorAnalytics.TrackEditorAnalyticsEvent(Application.isPlaying ? "homa_runtime_error" : "homa_editor_error", condition, stackTrace);
                }
                else
                {
                    EditorAnalytics.TrackEditorAnalyticsEvent(Application.isPlaying ? "generic_runtime_error" : "generic_editor_error", condition, stackTrace);
                }
            }
        }
    }
}
#endif