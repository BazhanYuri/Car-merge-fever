using System;

namespace HomaGames.HomaBelly
{
    public interface IAnalyticsWithInitializationCallback : IAnalytics
    {
        void Initialize(Action onInitialized = null);
    }
}
