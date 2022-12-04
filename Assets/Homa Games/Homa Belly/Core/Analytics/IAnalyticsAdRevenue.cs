using System;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Interface to allow tracking ad revenue for analytic systems
    /// </summary>
    public interface IAnalyticsAdRevenue
    {
        void TrackAdRevenue(AdRevenueData adRevenueData);
    }
}
