using System;

namespace HomaGames.HomaBelly
{
    [Obsolete("Use MediatorBase instead")] // Since 2021-07-18
    public interface IMediatorWithInitializationCallback : IMediator
    {
        void Initialize(Action onInitialized = null);
    }
}