using System.Collections.Generic;

namespace HomaGames.HomaBelly
{
    public class ResourceAnalyticsEvent : RuntimeAnalyticsEvent
    {
        private const string  EVENT_NAME = "resource_event";
        
        private const string  FLOW_TYPE_KEY = "flow_type";
        private const string  CURRENCY_KEY = "currency";
        private const string  AMOUNT_KEY = "amount";
        private const string  ITEM_TYPE_KEY = "item_type";
        private const string  ITEM_ID_KEY = "item_id";
        
        public ResourceAnalyticsEvent(ResourceFlowType flowType,string currency,float amount,string itemType,string itemId)
            : base(EVENT_NAME, HomaAnalytics.REVENUE_CATEGORY)
        {
            if (EventValues == null)
            {
                EventValues = new Dictionary<string, object>(5);
            }
            
            EventValues.Add(FLOW_TYPE_KEY,flowType);
            EventValues.Add(CURRENCY_KEY,currency);
            EventValues.Add(AMOUNT_KEY,amount);
            EventValues.Add(ITEM_TYPE_KEY,itemType);
            EventValues.Add(ITEM_ID_KEY,itemId);
        }
    }
}