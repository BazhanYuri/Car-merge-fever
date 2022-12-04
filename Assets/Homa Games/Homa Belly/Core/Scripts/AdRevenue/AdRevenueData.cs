using UnityEngine;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Class holding all required information for an
    /// Ad Revenue tracking event
    /// </summary>
    public class AdRevenueData
    {
        public string AdPlatform = "";
        public string Currency = "";
        public double Revenue;
        public string NetworkName = "";
        public string AdType = "";
        public string AdGroupType = "";
        public string ImpressionId = "";
        public string AdPlacamentName = "";
        public string AdUnitId = "";
        public string AdUnitName = "";
        public string AdGroupId = "";
        public string AdGroupName = "";
        public string AdGroupPriority = "";
        public string Precision = "";
        public string PlacementId = "";

        public override string ToString()
        {
            return "AdRevenueData: " + AdPlatform + " - " + Revenue + " " + Currency;
        }
    }
}