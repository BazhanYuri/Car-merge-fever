using System.Collections.Generic;

namespace HomaGames.HomaBelly
{
    public class ErrorAnalyticsEvent : RuntimeAnalyticsEvent
    {
        private const string EVENT_NAME = "error_event";

        private const string SEVERITY_KEY = "severity";
        private const string MESSAGE_KEY = "message";
            
        public ErrorAnalyticsEvent(ErrorSeverity severity, string message) : base(EVENT_NAME,HomaAnalytics.SYSTEM_CATEGORY)
        {
            if (EventValues == null)
            {
                EventValues = new Dictionary<string, object>(2);
            }
            
            EventValues.Add(SEVERITY_KEY,severity);
            EventValues.Add(MESSAGE_KEY,message);
        }
    }
}