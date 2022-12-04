using System;
using System.Diagnostics;
using System.Reflection;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace HomaGames.HomaBelly
{
    public static class EditorAnalyticsProxy
    {
#pragma warning disable 169
        private static FieldInfo _setTokenIdentifier;
        private static FieldInfo _setManifestVersionId;
        private static MethodInfo _trackEditorAnalyticsEvent;
#pragma warning restore 169

        [Conditional("HOMA_BELLY_EDITOR_ANALYTICS_ENABLED")]
        public static void SetTokenIdentifier(string tokenIdentifier)
        {
#if HOMA_BELLY_EDITOR_ANALYTICS_ENABLED
            if (_setTokenIdentifier == null)
            {
                Type t = Type.GetType("HomaGames.HomaBelly.ApiQueryModel, Assembly-CSharp");
                _setTokenIdentifier
                    = t?.GetField("TokenIdentifier", BindingFlags.Static | BindingFlags.Public);
            }

            _setTokenIdentifier?.SetValue(null, tokenIdentifier);
#endif
        }

        [Conditional("HOMA_BELLY_EDITOR_ANALYTICS_ENABLED")]
        public static void SetManifestVersionId(string manifestVersionId)
        {
#if HOMA_BELLY_EDITOR_ANALYTICS_ENABLED
            if (_setManifestVersionId == null)
            {
                Type t = Type.GetType("HomaGames.HomaBelly.ApiQueryModel, Assembly-CSharp");
                _setManifestVersionId
                    = t?.GetField("ManifestVersionId", BindingFlags.Static | BindingFlags.Public);
            }

            _setManifestVersionId?.SetValue(null, manifestVersionId);
#endif
        }

        [Conditional("HOMA_BELLY_EDITOR_ANALYTICS_ENABLED")]
        public static void TrackEditorAnalyticsEvent(string eventName, string eventDescription = null,
            string eventStackTrace = null, float eventValue = 0, float eventFps = 0)
        {
#if HOMA_BELLY_EDITOR_ANALYTICS_ENABLED
            if (_trackEditorAnalyticsEvent == null)
            {
                Type t = Type.GetType("HomaGames.HomaBelly.EditorAnalytics, Assembly-CSharp-Editor");
                _trackEditorAnalyticsEvent
                    = t?.GetMethod("TrackEditorAnalyticsEvent", BindingFlags.Static | BindingFlags.Public);
            }

            _trackEditorAnalyticsEvent?.Invoke(null,new object[]{eventName,eventDescription,eventStackTrace,eventValue,eventFps});
#endif
        }
    }
}