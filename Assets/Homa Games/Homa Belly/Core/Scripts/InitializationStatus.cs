using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Component holding Homa Belly's initialization status at every time.
    /// This status can be accessed through the property InitializationStatus#IsInitialized
    /// or registering Events#onInitialized
    /// </summary>
    public class InitializationStatus
    {
        #region Private properties
        /// <summary>
        /// Gift INITIALIZATION_GRACE_PERIOD_MS for Homa Belly initialization.
        /// If time is elapsed, #OnInitialized is invoked to avoid any possible
        /// issue block Homa Belly usage
        /// </summary>
        private const int INITIALIZATION_GRACE_PERIOD_MS = 5000;
        private readonly object initializationLock = new object();
        private int totalComponentsToInitialize = 0;
        private int initializedComponents = 0;
        private bool initialized = false;
        private Events events = new Events();
        #endregion

        #region Public properties

        /// <summary>
        /// Determines is Homa Belly is initialized
        /// </summary>
        public bool IsInitialized
        {
            get
            {
                return initialized;
            }
        }
        #endregion

        public void SetComponentsToInitialize(int componentsToInitialize)
        {
            // Obtain total components to wait for initialization
            totalComponentsToInitialize = componentsToInitialize;

            HomaGamesLog.Debug($"[InitializationStatus] Components to initialize: Total: {totalComponentsToInitialize}");
        }

        #region Public methods

        /// <summary>
        /// Starts the timer for an initialization grace period. This will allow
        /// backwards compatibility for those components not implementing
        /// yet the initialization status callback
        /// </summary>
        public void StartInitializationGracePeriod()
        {
            Task.Delay(INITIALIZATION_GRACE_PERIOD_MS).ContinueWithOnMainThread((result) =>
            {
                // If Homa Belly is not initialized after INITIALIZATION_GRACE_PERIOD_MS, move forward
                if (!initialized)
                {
                    HomaGamesLog.Warning($"[InitializationStatus] Forcing initialization completed after grace period. Initialized: {initializedComponents} Total: {totalComponentsToInitialize}");
                    initialized = true;
                    events.OnInitialized();
                }
            });
        }

        /// <summary>
        /// Action invoked after each component is initialized
        /// </summary>
        public void OnInnerComponentInitialized()
        {
            lock (initializationLock)
            {
                initializedComponents++;
                HomaGamesLog.Debug($"[InitializationStatus] Component initialized. Total: {initializedComponents}");
            }

            if (initializedComponents >= totalComponentsToInitialize)
            {
                // Homa Belly initialization completed
                HomaGamesLog.Debug($"[InitializationStatus] Initialization completed");
                initialized = true;
                events.OnInitialized();
            }
        }

        #endregion
    }
}
