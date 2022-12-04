#if HOMA_BELLY_EDITOR_ANALYTICS_ENABLED
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public class PerformanceReportQueryModel : EditorAnalyticsModelBase
    {
        private static readonly string PARAM_PERFORMANCE = "perf";
        private static readonly string PARAM_PERFORMANCE_TOP_CONTRIBUTORS = "top";
        private static readonly string PARAM_SESSION_TIME = "session_time";
        private static readonly string PARAM_FRAME_COUNT = "frame_count";

        public PerformanceReportQueryModel(string eventName, float sessionTime, int frameCount,
            Dictionary<string, float> performanceData, Dictionary<string, float> topContributors) : base(eventName)
        {
            EventValues.Add(PARAM_SESSION_TIME, sessionTime);
            EventValues.Add(PARAM_FRAME_COUNT, frameCount);
            EventValues.Add(PARAM_PERFORMANCE, performanceData);
            EventValues.Add(PARAM_PERFORMANCE_TOP_CONTRIBUTORS, topContributors);
        }
    }
}
#endif