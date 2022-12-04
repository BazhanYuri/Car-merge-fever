using System.Collections.Generic;

namespace HomaGames.HomaBelly
{
    public class ProgressionAnalyticsEvent : RuntimeAnalyticsEvent
    {
        private const string EVENT_NAME = "progression_event";
        private const string STATUS_KEY = "status";
        private const string PROGRESSION1_KEY = "progression_01";
        private const string PROGRESSION2_KEY = "progression_02";
        private const string PROGRESSION3_KEY = "progression_03";
        private const string SCORE_KEY = "score";
        
        public ProgressionAnalyticsEvent(ProgressionStatus progressionStatus, string progression01, int score) 
            : base(EVENT_NAME,HomaAnalytics.PROGRESSION_CATEGORY)
        {
            if (EventValues == null)
            {
                EventValues = new Dictionary<string, object>(3);
            }
            
            EventValues.Add(STATUS_KEY,progressionStatus);
            EventValues.Add(PROGRESSION1_KEY,progression01);
            EventValues.Add(SCORE_KEY,score);
        }
        
        public ProgressionAnalyticsEvent(ProgressionStatus progressionStatus, string progression01,string progression02,int score) 
            : this(progressionStatus,progression01,score)
        {
            EventValues.Add(PROGRESSION2_KEY,progression02);
        }
        
        public ProgressionAnalyticsEvent(ProgressionStatus progressionStatus,
            string progression01,
            string progression02,
            string progression03,
            int score) 
            : this(progressionStatus,progression01,progression02,score)
        {
            EventValues.Add(PROGRESSION3_KEY,progression03);
        }
    }
}