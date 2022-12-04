#if HOMA_BELLY_EDITOR_ANALYTICS_ENABLED
#endif
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Specific API model for Unity Editor Analytics events
    /// </summary>
    public class EditorAnalyticsEventModel : EditorAnalyticsModelBase
    {
        private static readonly string EVENT_DESCRIPTION = "description";
        private static readonly string STACK_TRACE = "stack_trace";
        private static readonly string EVENT_VALUE = "value";
        private static readonly string EVENT_FPS = "fps";

        public EditorAnalyticsEventModel(string eventName, string eventDescription, string eventStackTrace, float eventValue, float eventFps) : base(eventName)
        {
            EventValues.Add(EVENT_DESCRIPTION, Sanitize(eventDescription));
            EventValues.Add(STACK_TRACE, Sanitize(eventStackTrace));
            EventValues.Add(EVENT_VALUE, Sanitize(eventValue));
            EventValues.Add(EVENT_FPS, Sanitize(eventFps));
        }
    }
}
