using System.Collections.Generic;

namespace HomaGames.HomaBelly
{
    
    public class AdAnalyticsEvent : RuntimeAnalyticsEvent
    {
        public const string EVENT_NAME = "ad_event";
        
        public const string AD_ACTION_KEY = "ad_action";
        public const string AD_TYPE_KEY = "ad_type";
        public const string AD_NETWORK_KEY = "ad_network";
        public const string AD_PLACEMENT_ID_KEY = "ad_placement_id";

        public AdAnalyticsEvent(AdAction adAction,
            AdType adType,
            string adNetwork,
            string adPlacementId) 
            : base(EVENT_NAME, HomaAnalytics.AD_CATEGORY)
        {
            if (EventValues == null)
            {
                EventValues = new Dictionary<string, object>(4);
            }
            
            EventValues.Add(AD_ACTION_KEY,adAction);
            EventValues.Add(AD_TYPE_KEY,adType);
            EventValues.Add(AD_NETWORK_KEY,adNetwork);
            EventValues.Add(AD_PLACEMENT_ID_KEY,adPlacementId);
        }
    }
}