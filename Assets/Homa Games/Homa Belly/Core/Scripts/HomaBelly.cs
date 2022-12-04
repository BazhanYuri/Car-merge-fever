 using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

namespace HomaGames.HomaBelly
{
    public class HomaBelly : IHomaBellyBridge
    {
        #region Constants
        /// <summary>
        /// If this define symbol is set in the build settings, logs will be enabled.
        /// </summary>
        public const string LOGS_ENABLED_DEFINE_SYMBOL = "HOMA_DEVELOPMENT";
        #endregion

        #region Public properties
        public bool IsInitialized
        {
            get
            {
                return homaBridge.IsInitialized;
            }
        }
        #endregion

        #region Singleton pattern

        private static HomaBelly _instance;
        public static HomaBelly Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new HomaBelly();
                }

                return _instance;
            }
        }

#endregion

#region Private properties
#if UNITY_EDITOR && UNITY_2019_1_OR_NEWER
        private static HomaDummyBridge homaBridge = new HomaDummyBridge();
#else
        private static HomaBridge homaBridge = new HomaBridge();
#endif
        private Events events = new Events();

        /// <summary>
        /// If network is not reachable, API calls will be stored in
        /// this queue. All Actions will be triggered when network is
        /// reachable again
        /// </summary>
        private static Queue<Action> unreachableNetworkActionQueue = new Queue<Action>();
        private const int REACHABILITY_WAIT_MS = 3000;
        private static Task unreachableNetworkTaskDelay;
        private static bool IsNetworkReachable
        {
            get
            {
                return Application.internetReachability != NetworkReachability.NotReachable;
            }
        }
#endregion

        // Accessed in CoreInitializer
        protected static async Task Initialize()
        {            
#if !HOMA_DEVELOPMENT
            UnityEngine.Debug.Log("<color=#41CBA5>[Homa Belly] All logs have been disabled in this build to improve the performance. If you want to have logs, you have to toggle ON 'Development Build' in Homa Belly window.</color>");
#endif
            await ThreadUtils.RunOnMainThreadAsync(() =>
                CheckNetworkReachabilityBeforeAction(() => homaBridge.Initialize()));
            SetupComponentProxy();
        }

        private static void SetupComponentProxy()
        {
            var proxyGameObject = new GameObject("HomaBellyProxy");
            HomaBellyComponentProxy componentProxy = proxyGameObject.AddComponent<HomaBellyComponentProxy>();
            componentProxy.SetOnApplicationPause(Instance.OnApplicationPause);
        }

        protected static Task InitializeRemoteConfigurationDependantComponents(RemoteConfiguration.RemoteConfigurationModelEveryTime remoteConfigurationModel)
        {
            CheckNetworkReachabilityBeforeAction(() => homaBridge.InitializeRemoteConfigurationDependantComponents(remoteConfigurationModel));
            return Task.CompletedTask;
        }

        private void OnApplicationPause(bool pause)
        {
            homaBridge.OnApplicationPause(pause);
        }

        /// <summary>
        /// Method filtering API calls on network reachabiliy.
        /// If reachable, executes it. If not, stores in in the
        /// #unreachableNetworkActionQueue to be executed when
        /// network is reachable again
        /// </summary>
        /// <param name="action">The action to be executed</param>
        private static void CheckNetworkReachabilityBeforeAction(Action action)
        {
            if (IsNetworkReachable)
            {
                action.Invoke();
            }
            else
            {
                HomaGamesLog.Warning($"[Homa Belly] Network not reachable. Deferring call to {action} until network available again.");
                unreachableNetworkActionQueue.Enqueue(action);
                WaitForReachability();
            }
        }

        /// <summary>
        /// Method triggered when an API call is done and network is not reachable.
        /// This method will loop until network is reachable and execute
        /// all the deferred actions
        /// </summary>
        private static void WaitForReachability()
        {
            // If network recovery task is already running
            if (unreachableNetworkTaskDelay == null
                || unreachableNetworkTaskDelay.IsCanceled
                || unreachableNetworkTaskDelay.IsCompleted
                || unreachableNetworkTaskDelay.IsFaulted)
            {
                // Task waiting REACHABILITY_WAIT_MS to retry any API call
                // If network is not reachable after that time, a new task
                // will be scheduled to try again, until network is reachable
                unreachableNetworkTaskDelay = Task.Delay(REACHABILITY_WAIT_MS)
                    .ContinueWithOnMainThread(result =>
                    {
                        // If network is reachable and actions need to be
                        // triggered, proceed
                        if (IsNetworkReachable
                            && unreachableNetworkActionQueue != null
                            && unreachableNetworkActionQueue.Count > 0)
                        {
                            HomaGamesLog.Debug($"[Homa Belly] Network recovered. Executing deferred actions");
                            while (unreachableNetworkActionQueue.Count > 0)
                            {
                                Action action = unreachableNetworkActionQueue.Dequeue();
                                if (action != null)
                                {
                                    action.Invoke();
                                }
                            }

                            HomaGamesLog.Debug($"[Homa Belly] Deferred actions successfully invoked");
                        }
                    })
                    .ContinueWithOnMainThread(result =>
                    {
                        // Network recovery task is done
                        unreachableNetworkTaskDelay = null;

                        // If network is still not reachable, reset task
                        if (!IsNetworkReachable)
                        { 
                            WaitForReachability();
                        }
                    });
            }
        }

        #region IHomaBellyBridge

        public void ValidateIntegration()
        {
            homaBridge.ValidateIntegration();
        }

        // Rewarded Video Ads
        public void LoadExtraRewardedVideoAd(string placementId)
        {
            homaBridge.LoadExtraRewardedVideoAd(placementId);
        }

        public void LoadHighValueRewardedVideoAd()
        {
            homaBridge.LoadHighValueRewardedVideoAd();
        }

        public void ShowRewardedVideoAd(string placementName, string placementId = null)
        {
            homaBridge.ShowRewardedVideoAd(placementName, placementId);
        }

        public void ShowHighValueRewardedVideoAd(string placementName)
        {
            homaBridge.ShowHighValueRewardedVideoAd(placementName);
        }

        public bool IsRewardedVideoAdAvailable(string placementId = null)
        {
            return homaBridge.IsRewardedVideoAdAvailable(placementId);
        }

        // Banners
        public void LoadBanner(string placementId)
        {
            LoadBanner(BannerSize.BANNER, BannerPosition.BOTTOM, placementId);
        }

        public void LoadBanner(UnityEngine.Color bannerBackgroundColor, string placementId = null)
        {
            LoadBanner(BannerSize.BANNER, BannerPosition.BOTTOM, placementId, bannerBackgroundColor);
        }

        public void LoadBanner(BannerPosition position, string placementId = null)
        {
            LoadBanner(BannerSize.BANNER, position, placementId);
        }

        public void LoadBanner(BannerSize size, string placementId = null)
        {
            LoadBanner(size, BannerPosition.BOTTOM, placementId);
        }

        public void LoadBanner(BannerSize size = BannerSize.BANNER, BannerPosition position = BannerPosition.BOTTOM, string placementId = null, Color bannerBackgroundColor = default)
        {
            homaBridge.LoadBanner(size, position, placementId, bannerBackgroundColor);
        }

        public void ShowBanner(string placementId = null)
        {
            homaBridge.ShowBanner(placementId);
        }

        public void HideBanner(string placementId = null)
        {
            homaBridge.HideBanner(placementId);
        }

        public void DestroyBanner(string placementId = null)
        {
            homaBridge.DestroyBanner(placementId);
        }

        public int GetBannerHeight(string placementId = null)
        {
            return homaBridge.GetBannerHeight(placementId);
        }

        public void SetBannerPosition(BannerPosition position, string placementId = null)
        {
            homaBridge.SetBannerPosition(position,placementId);
        }

        public void SetBannerBackgroundColor(Color color, string placementId = null)
        {
            homaBridge.SetBannerBackgroundColor(color,placementId);
        }

        public void LoadExtraInterstitial(string placementId)
        {
            homaBridge.LoadExtraInterstitial(placementId);
        }

        public void LoadHighValueInterstitial()
        {
            homaBridge.LoadHighValueInterstitial();
        }

        public void ShowInterstitial(string placementName, string placementId = null)
        {
            homaBridge.ShowInterstitial(placementName, placementId);
        }

        public void ShowHighValueInterstitial(string placementName)
        {
            homaBridge.ShowHighValueInterstitial(placementName);
        }

        public bool IsInterstitialAvailable(string placementId = null)
        {
            return homaBridge.IsInterstitialAvailable(placementId);
        }

        [Preserve]
        public void SetUserIsAboveRequiredAge(bool consent)
        {
            homaBridge.SetUserIsAboveRequiredAge(consent);
        }

        [Preserve]
        public void SetTermsAndConditionsAcceptance(bool consent)
        {
            homaBridge.SetTermsAndConditionsAcceptance(consent);
        }

        [Preserve]
        public void SetAnalyticsTrackingConsentGranted(bool consent)
        {
            homaBridge.SetAnalyticsTrackingConsentGranted(consent);
        }

        [Preserve]
        public void SetTailoredAdsConsentGranted(bool consent)
        {
            homaBridge.SetTailoredAdsConsentGranted(consent);
        }

#if UNITY_PURCHASING
        public void TrackInAppPurchaseEvent(UnityEngine.Purchasing.Product product, bool isRestored = false)
        {
            homaBridge.TrackInAppPurchaseEvent(product);
        }
#endif

        public void TrackInAppPurchaseEvent(string productId, string currencyCode, double unitPrice, string transactionId = null, string payload = null, bool isRestored = false)
        {
            homaBridge.TrackInAppPurchaseEvent(productId, currencyCode, unitPrice, transactionId, payload);
        }

        public void TrackResourceEvent(ResourceFlowType flowType, string currency, float amount, string itemType, string itemId)
        {
            homaBridge.TrackResourceEvent(flowType, currency, amount, itemType, itemId);
        }

        public void TrackProgressionEvent(ProgressionStatus progressionStatus, string progression01, int score = 0)
        {
            homaBridge.TrackProgressionEvent(progressionStatus, progression01, score);
        }

        public void TrackProgressionEvent(ProgressionStatus progressionStatus, string progression01, string progression02, int score = 0)
        {
            homaBridge.TrackProgressionEvent(progressionStatus, progression01, progression02, score);
        }

        public void TrackProgressionEvent(ProgressionStatus progressionStatus, string progression01, string progression02, string progression03, int score = 0)
        {
            homaBridge.TrackProgressionEvent(progressionStatus, progression01, progression02, progression03, score);
        }

        public void TrackErrorEvent(ErrorSeverity severity, string message)
        {
            homaBridge.TrackErrorEvent(severity, message);
        }

        public void TrackDesignEvent(string eventName, float eventValue = 0)
        {
            homaBridge.TrackDesignEvent(eventName, eventValue);
        }

        public void TrackAdEvent(AdAction adAction, AdType adType, string adNetwork, string adPlacementId)
        {
            homaBridge.TrackAdEvent(adAction, adType, adNetwork, adPlacementId);
        }

        public void TrackAdRevenue(AdRevenueData adRevenueData)
        {
            homaBridge.TrackAdRevenue(adRevenueData);
        }

        public void TrackAttributionEvent(string eventName, Dictionary<string, object> arguments = null)
        {
            homaBridge.TrackAttributionEvent(eventName, arguments);
        }

        public void SetCustomDimension01(string customDimension)
        {
            homaBridge.SetCustomDimension01(customDimension);
        }

        public void SetCustomDimension02(string customDimension)
        {
            homaBridge.SetCustomDimension02(customDimension);
        }

        public void SetCustomDimension03(string customDimension)
        {
            homaBridge.SetCustomDimension03(customDimension);
        }

        #endregion
    }
}
