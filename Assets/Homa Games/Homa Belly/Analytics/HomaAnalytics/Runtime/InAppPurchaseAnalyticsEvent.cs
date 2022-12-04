using System.Collections.Generic;

namespace HomaGames.HomaBelly
{
    public class InAppPurchaseAnalyticsEvent : RuntimeAnalyticsEvent
    {
        private const string EVENT_NAME = "in_app_purchase";
        
        private const string CURRENCY_CODE = "currency_code";
        private const string UNIT_PRICE = "unit_price_cents";
        private const string PRODUCT_ID = "product_id";
        private const string TRANSACTION_ID = "transaction_id";
        private const string GOOGLE_PLAY_SIGNATURE = "google_play_signature";
            
        public InAppPurchaseAnalyticsEvent(
            string currencyCode,
            double unitPriceInCents,
            string productId,
            string transactionId,
            string googlePlaySignature = null) : base(EVENT_NAME, HomaAnalytics.REVENUE_CATEGORY)
        {
            if (EventValues == null)
            {
                EventValues = new Dictionary<string, object>(5);
            }
            
            EventValues.Add(CURRENCY_CODE,currencyCode);
            EventValues.Add(UNIT_PRICE,unitPriceInCents);
            EventValues.Add(PRODUCT_ID,productId);
            EventValues.Add(TRANSACTION_ID,transactionId);
            if (!string.IsNullOrEmpty(googlePlaySignature))
            {
                EventValues.Add(GOOGLE_PLAY_SIGNATURE,googlePlaySignature);
            }
        }
    }
}