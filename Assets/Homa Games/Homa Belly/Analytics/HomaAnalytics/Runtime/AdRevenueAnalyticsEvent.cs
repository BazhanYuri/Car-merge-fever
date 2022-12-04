using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public class AdRevenueAnalyticsEvent : RuntimeAnalyticsEvent
    {
        private const string EVENT_NAME = "ad_revenue";
        private const string AD_PLATFORM = "ad_platform";
        private const string CURRENCY = "currency";
        private const string REVENUE = "revenue";
        private const string NETWORK_NAME = "network_name";
        private const string AD_TYPE = "ad_type";
        private const string AD_GROUP_TYPE = "ad_group_type";
        private const string IMPRESSION_ID = "impression_id";
        private const string AD_PLACEMENT_NAME = "ad_placement_name";
        private const string AD_UNIT_ID = "ad_unit_id";
        private const string AD_UNIT_NAME = "ad_unit_name";
        private const string AD_GROUP_ID = "ad_group_id";
        private const string AD_GROUP_NAME = "ad_group_name";
        private const string AD_GROUP_PRIORITY = "ad_group_priority";
        private const string PRECISION = "precision";
        private const string PLACEMENT_ID = "placement_id";

        public AdRevenueAnalyticsEvent(AdRevenueData adRevenueData) : base(EVENT_NAME,HomaAnalytics.REVENUE_CATEGORY)
        {
            if (EventValues == null)
            {
                EventValues = new Dictionary<string, object>(15);
            }
            
            EventValues.Add(AD_PLATFORM,adRevenueData.AdPlatform);
            EventValues.Add(CURRENCY,adRevenueData.Currency);
            var revenueString = ConvertDoubleToString(adRevenueData.Revenue);
            EventValues.Add(REVENUE,revenueString);
            EventValues.Add(NETWORK_NAME,adRevenueData.NetworkName);
            EventValues.Add(AD_TYPE,adRevenueData.AdType);
            EventValues.Add(AD_GROUP_TYPE,adRevenueData.AdGroupType);
            EventValues.Add(IMPRESSION_ID,adRevenueData.ImpressionId);
            EventValues.Add(AD_PLACEMENT_NAME,adRevenueData.AdPlacamentName);
            EventValues.Add(AD_UNIT_ID,adRevenueData.AdUnitId);
            EventValues.Add(AD_UNIT_NAME,adRevenueData.AdUnitName);
            EventValues.Add(AD_GROUP_ID,adRevenueData.AdGroupId);
            EventValues.Add(AD_GROUP_NAME,adRevenueData.AdGroupName);
            EventValues.Add(AD_GROUP_PRIORITY,adRevenueData.AdGroupPriority);
            EventValues.Add(PRECISION,adRevenueData.Precision);
            EventValues.Add(PLACEMENT_ID,adRevenueData.PlacementId);
        }

        public static string ConvertDoubleToString(double revenueValue)
        {
            string stringValue = "0";
            try
            {
                // This will prevent the System.OverflowException
                decimal decimalValue = 0;
                if (revenueValue < (double)decimal.MinValue)
                    decimalValue = decimal.MinValue;
                else if (revenueValue > (double)decimal.MaxValue)
                    decimalValue = decimal.MaxValue;
                else
                {
                    // // Convert to decimal to avoid having double scientific notations. This line is used in singular
                    decimalValue = Convert.ToDecimal(revenueValue,CultureInfo.InvariantCulture);
                }
                
                // Convert to string using InvariantCulture to avoid having issues with the decimal number separator
                stringValue = decimalValue.ToString(CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                Debug.LogError($"[ERROR] Error converting double to decimal value: {revenueValue} {e}");
            }
            
            return stringValue;
        }
    }
}