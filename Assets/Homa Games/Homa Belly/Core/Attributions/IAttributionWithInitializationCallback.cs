using System;

namespace HomaGames.HomaBelly
{
    public interface IAttributionWithInitializationCallback : IAttribution
    {
        void Initialize(string appSubversion = "", Action onInitialized = null);
    }
}
