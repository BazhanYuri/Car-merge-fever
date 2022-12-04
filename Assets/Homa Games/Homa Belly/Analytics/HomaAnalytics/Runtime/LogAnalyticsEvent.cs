using System.Collections.Generic;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public class LogAnalyticsEvent : RuntimeAnalyticsEvent
    {
        private const string EVENT_NAME = "Log";
    
        public LogAnalyticsEvent (string condition, string stacktrace, LogType type) 
            : base(
                EVENT_NAME, HomaAnalytics.SYSTEM_CATEGORY, 
                new Dictionary<string, object>
                {
                    {"content", condition},
                    {"stacktrace", stacktrace},  
                    {"Log_type", GetLogTypeString(type)}
                })
        { }

        private static string GetLogTypeString(LogType type)
        {
            switch (type)
            {
                case LogType.Log:
                    return "Log";
                case LogType.Warning:
                    return "warning";
                case LogType.Assert:
                case LogType.Error:
                    return "error";
                case LogType.Exception:
                    return "exception";
                default:
                    return "unknown";
            }
        }
    }
}