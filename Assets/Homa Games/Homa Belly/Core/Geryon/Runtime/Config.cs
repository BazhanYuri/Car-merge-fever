using System;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using HomaGames.HomaBelly;

namespace HomaGames.Geryon
{
    // Non static for CoreInitializer
    public class Config
    {
        // Weird names, but legacy
        private const string SCOPE_ID_PREF_KEY = "Homa Games_SCOPE_ID";
        private const string VARIANT_ID_PREF_KEY = "Homa Games_VARIANT_ID";
        private const string DEFAULT_ID_VALUE = "000";

        private static bool FirstTimeConfigurationParsed;
        private static event Action OnFirstTimeConfigurationParsed;
        
        private static event Action _onInitialized;
        
        [Obsolete("Use OnInitialized instead")]
        public static event Action onInitialized
        {
            add => OnInitialized += value;
            remove => OnInitialized -= value;
        }
        
        /// <summary>
        /// This event will be triggered once Geryon is initialized.<br /><br />
        /// Geryon can be disabled by the user, and thus this event
        /// may never be triggered. 
        /// </summary>
        public static event Action OnInitialized
        {
            add
            {
                // If Geryon is already initialized when setting 
                // the callback, invoke it directly
                if (initialized && value != null)
                {
                    value.Invoke();
                }
                else if (_onInitialized == null || !_onInitialized.GetInvocationList().Contains(value))
                {
                    _onInitialized += value;
                }
            }

            remove
            {
                if (_onInitialized != null && _onInitialized.GetInvocationList().Contains(value))
                {
                    _onInitialized -= value;
                }
            }
        }

        private static bool initialized
#if HOMA_BELLY_GERYON_ENABLED
            = false;
#else
            = true;
#endif
        
        /// <summary>
        /// Determines if Geryon is initialized
        /// </summary>
        public static bool Initialized => initialized;

        /// <summary>
        /// <para>
        /// This is the Homa Games Testing ID assigned to the
        /// game run. This value needs to be informed to any
        /// attribution platform integrated within the game.
        /// </para>
        /// </summary>
        public static string NTESTING_ID
        {
            get
            {
                if (!initialized)
                {
                    HomaGamesLog.Warning($"Reading {nameof(NTESTING_ID)} before fully initialized. No proper value is guaranteed. Please wait for {nameof(OnInitialized)}");
                }

                return scopeId + variantId;
            }
        }

        private static string scopeId = DEFAULT_ID_VALUE;
        public static string ScopeId
        {
            get
            {
                if (!initialized)
                {
                    HomaGamesLog.Warning($"Reading {nameof(ScopeId)} before fully initialized. No proper value is guaranteed. Please wait for {nameof(OnInitialized)}");
                }

                return scopeId;
            }
        }

        private static string variantId = DEFAULT_ID_VALUE;
        public static string VariantId
        {
            get
            {
                if (!initialized)
                {
                    HomaGamesLog.Warning($"Reading {nameof(VariantId)} before fully initialized. No proper value is guaranteed. Please wait for {nameof(OnInitialized)}");
                }

                return variantId;
            }
        }

        private static string overrideId = DEFAULT_ID_VALUE;
        public static string OverrideId
        {
            get
            {
                if (!initialized)
                {
                    HomaGamesLog.Warning("Reading OverrideId before fully initialized. No proper value is guaranteed. Please wait for onInitialized");
                }

                return overrideId;
            }
        }

        #region External tokens

        public static string ExternalToken0 { get; private set; }
        public static string ExternalToken1 { get; private set; }
        public static string ExternalToken2 { get; private set; }
        public static string ExternalToken3 { get; private set; }
        public static string ExternalToken4 { get; private set; }

        #endregion

        private static Task LoadingFirstTimeValuesFromPersistence = Task.CompletedTask;
        
        // Accessed in CoreInitializer
        protected static Task OnGameLaunch()
        {
            // Load any previous saved values. This needs to be done before
            // blocking Unity Main Thread in order to access PlayerPrefs
            scopeId = PlayerPrefs.GetString(SCOPE_ID_PREF_KEY, DEFAULT_ID_VALUE);
            variantId = PlayerPrefs.GetString(VARIANT_ID_PREF_KEY, DEFAULT_ID_VALUE);
            
            if (! RemoteConfiguration.FirstTimeConfigurationNeededThisSession())
                // On the second or more run, first time app open configuration needs to be restored from persistence
                LoadingFirstTimeValuesFromPersistence = Task.WhenAll(                
                    LoadExternalTokensFromPersistence(),
                    Persistence.LoadFirstTimeConfigurationFromPersistence()
                    .ContinueWith(task => JSONUtils.UpdateDynamicVariables(task.Result)));
            
            return Task.CompletedTask;
        }

        protected static async Task OnFirstTimeConfigurationFetched()
        {
            
            if (! RemoteConfiguration.FirstTimeConfigurationNeededThisSession()
                || RemoteConfiguration.FirstTime == null)
            {
                Debug.LogError($"First time configuration not accessible this session in {nameof(OnFirstTimeConfigurationFetched)}.");
                return;
            }
            
            Task persistenceTask = Task.CompletedTask;
            try
            {
                GeryonConfigurationModel firstTimeGeryonConfigurationModel =
                    RemoteConfiguration.FirstTime.GeryonConfigurationModel;
                if (firstTimeGeryonConfigurationModel != null)
                {
                    persistenceTask = Persistence.PersistFirstTimeConfigurationAsync(firstTimeGeryonConfigurationModel);
                    // Persist First Time App Open and update variables in memory.
                    // Persisted values will be load in subsequent game runs
                    UpdateExternalTokens(firstTimeGeryonConfigurationModel);
                    JSONUtils.UpdateDynamicVariables(firstTimeGeryonConfigurationModel.Configuration);
                    scopeId = firstTimeGeryonConfigurationModel.ScopeId;
                    variantId = firstTimeGeryonConfigurationModel.VariantId;
                }
                else
                {
                    HomaGamesLog.Warning("First time response is null");
                }
            }
            catch (Exception e)
            {
                HomaGamesLog.Error($"Exception while first time app open handling: {e.Message}");
            }

            PersistFirstTimeResponseIDs();
            
            FirstTimeConfigurationParsed = true;
            OnFirstTimeConfigurationParsed?.Invoke();

            await persistenceTask;
        }

        protected static async Task OnEveryTimeConfigurationFetched()
        {
            if (RemoteConfiguration.FirstTimeConfigurationNeededThisSession())
            {
                if (!FirstTimeConfigurationParsed)
                    await new EventTask(ref OnFirstTimeConfigurationParsed);
            }
            else
            {
                await LoadingFirstTimeValuesFromPersistence;
            }
            
            try
            {
                GeryonConfigurationModel everyTimeGeryonConfigurationModel =
                    RemoteConfiguration.EveryTime.GeryonConfigurationModel;
                if (everyTimeGeryonConfigurationModel != null)
                {
                    // Update external tokens with every time app open response
                    UpdateExternalTokens(everyTimeGeryonConfigurationModel);

                    overrideId = everyTimeGeryonConfigurationModel.OverrideId;
                    JSONUtils.UpdateDynamicVariables(
                        everyTimeGeryonConfigurationModel.Configuration);
                }
                else
                {
                    HomaGamesLog.Warning("Every time response is null");
                }
            }
            catch (Exception e)
            {
                HomaGamesLog.Error($"Exception while every time app open handling: {e.Message}");
            }

            NotifyInitializationCompleted();
        }

#region Private helpers

        /// <summary>
        /// Loads persisted external tokens from disk (if any)
        /// </summary>
        private static async Task LoadExternalTokensFromPersistence()
        {
            string[] externalTokens = await Persistence.LoadFirstTimeExternalTokensFromPersistence();

            // Security check: by design, we only expose 5 fixed external tokens
            if (externalTokens != null && externalTokens.Length == 5)
            {
                ExternalToken0 = externalTokens[0];
                ExternalToken1 = externalTokens[1];
                ExternalToken2 = externalTokens[2];
                ExternalToken3 = externalTokens[3];
                ExternalToken4 = externalTokens[4];
            }
        }

        /// <summary>
        /// Overrides external tokens with the provided ConfigurationResponse
        /// </summary>
        /// <param name="geryonConfigurationModel">The ConfigurationResponse with the new external tokens</param>
        private static void UpdateExternalTokens(GeryonConfigurationModel geryonConfigurationModel)
        {
            if (geryonConfigurationModel == null)
            {
                return;
            }

            string[] externalTokenResponse =
            {
                geryonConfigurationModel.ExternalToken0,
                geryonConfigurationModel.ExternalToken1,
                geryonConfigurationModel.ExternalToken2,
                geryonConfigurationModel.ExternalToken3,
                geryonConfigurationModel.ExternalToken4
            };

            Action<string>[] externalTokenSetters =
            {
                v => ExternalToken0 = v,
                v => ExternalToken1 = v,
                v => ExternalToken2 = v,
                v => ExternalToken3 = v,
                v => ExternalToken4 = v
            };

            for (int i = 0; i < externalTokenResponse.Length; i++)
            {
                if (!string.IsNullOrEmpty(externalTokenResponse[i]))
                    externalTokenSetters[i].Invoke(externalTokenResponse[i]);
            }
        }

        /// <summary>
        /// Persists first time app open ids for further game runs.
        /// This method needs to run in Unity Main Thread as it uses PlayerPrefs
        /// </summary>
        private static void PersistFirstTimeResponseIDs()
        {
            PlayerPrefs.SetString(SCOPE_ID_PREF_KEY, scopeId);
            PlayerPrefs.SetString(VARIANT_ID_PREF_KEY, variantId);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Mark Geryon as initialized and invoke any registered callback
        /// </summary>
        private static void NotifyInitializationCompleted()
        {
            initialized = true;
            _onInitialized?.Invoke();
        }

        #endregion
    }
}