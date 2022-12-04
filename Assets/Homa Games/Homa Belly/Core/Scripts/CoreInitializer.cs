using System;
using System.Threading.Tasks;
using HomaGames.HomaBelly.DataPrivacy;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public static class CoreInitializer
    {
        private static Task InitializationTask;

#if UNITY_2019_3_OR_NEWER
        // According to tests identifier fetching does not start before the splash screen is finished, 
        // but the behaviour is not guaranteed.  
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        private static void Initialize()
        {
            // Populate SystemConstants from Unity Main Thread in order
            // to access Application and SystemInfo APIs
            SystemConstants.Populate();
            
            // Always initialize Homa Belly after Data Privacy flow has been completed
            DataPrivacyFlowNotifier.OnFlowCompleted += () => InitializationTask = InitializeAsync();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnGameStarts()
        {
        }

        private static async Task InitializeAsync()
        {
            try
            {
                HomaBellyManifestConfiguration.Initialise();
                var geryonInitTask = 
#if HOMA_BELLY_GERYON_ENABLED
                    GeryonConfigProtectedAccessor.OnGameLaunch();
#else
                    Task.CompletedTask;
#endif
                await Identifiers.Initialize();

                RemoteConfiguration.PrepareRemoteConfigurationFetching();
                
                var firstTimeConfigFetch = RemoteConfiguration.FirstTimeConfigurationNeededThisSession() ?
                    RemoteConfiguration.GetFirstTimeConfiguration() :
                    Task.CompletedTask;
                var everyTimeConfigFetch = RemoteConfiguration.GetEveryTimeConfiguration();

                async Task GeryonEveryTimeAfter(params Task[] previousTasks)
                {
                    await Task.WhenAll(previousTasks);
#if HOMA_BELLY_GERYON_ENABLED
                    await GeryonConfigProtectedAccessor.OnEveryTimeConfigurationFetched();
#endif
                }
                var geryonEveryTimeTask = GeryonEveryTimeAfter(geryonInitTask, everyTimeConfigFetch);

                async Task ForceUpdatePopupAfter(params Task[] previousTasks)
                {
                    await Task.WhenAll(previousTasks);
                    ForceUpdatePopup.ShowPopupIfRequired();
                }
                var forceUpdatePopupTask = ForceUpdatePopupAfter(everyTimeConfigFetch);

                await Task.WhenAll(firstTimeConfigFetch, geryonInitTask);
                
#if HOMA_BELLY_GERYON_ENABLED
                if (RemoteConfiguration.FirstTimeConfigurationNeededThisSession())
                    await GeryonConfigProtectedAccessor.OnFirstTimeConfigurationFetched();
#endif
                await HomaBellyProtectedAccessor.Initialize();
                
                await everyTimeConfigFetch;
                await HomaBellyProtectedAccessor.InitializeRemoteConfigurationDependantComponents(RemoteConfiguration
                    .EveryTime);

                await Task.WhenAll(geryonEveryTimeTask, forceUpdatePopupTask);
            }
            catch (Exception e)
            {
                Debug.LogError("Error while Initializing Homa Belly Core:\n" + e);
                // Just in case someone wants to read the task to check for errors
                throw;
            }
        }

        #region HomaBelly Private Accessor

        // This bit of code is supposed to be removed once Homa Belly is in its own assembly.
        
        // We don't want users to be able to access initialization methods (to prevent errors
        // and confusion). But we cannot use internal because HB is in the main assembly. So we 
        // Use this trick where we define initialization methods protected static, and we use internal
        // child classes to access them. Inheritance VPN.
        
        private class HomaBellyProtectedAccessor : HomaBelly
        {
            private HomaBellyProtectedAccessor()
            { }
            
            public new static Task Initialize() => HomaBelly.Initialize();

            public new static Task InitializeRemoteConfigurationDependantComponents(
                RemoteConfiguration.RemoteConfigurationModelEveryTime remoteConfigurationModel) =>
                HomaBelly.InitializeRemoteConfigurationDependantComponents(remoteConfigurationModel);
        }
        
#if HOMA_BELLY_GERYON_ENABLED
        private class GeryonConfigProtectedAccessor : Geryon.Config
        {
            private GeryonConfigProtectedAccessor()
            { }

            public new static Task OnGameLaunch() => Geryon.Config.OnGameLaunch();

            public new static Task OnFirstTimeConfigurationFetched() => Geryon.Config.OnFirstTimeConfigurationFetched();
            public new static Task OnEveryTimeConfigurationFetched() => Geryon.Config.OnEveryTimeConfigurationFetched();
        }
#endif
#endregion
    }
}
