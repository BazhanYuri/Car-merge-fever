#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomaGames.HomaBelly.Utilities;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Homa Bridge is the main connector between the public facade (HomaBelly)
    /// and all the inner behaviour of the Homa Belly library. All features
    /// and callbacks will be centralized within this class.
    /// </summary>
    public class HomaDummyBridge : IHomaBellyBridge
    {
        private const string RES_PATH = "Assets/Homa Games/Homa Belly/Core/Prefabs/DummyAds/";
        #region Private properties
        private bool BannerLoaded = false;
        private readonly Dictionary<string, bool> RewardedVideoLoaded = new Dictionary<string, bool>();
        private GameObject DummyBanner;
        private Events m_events = new Events();
        private bool initialized = false;
        private AnalyticsHelper analyticsHelper = new AnalyticsHelper();

        public bool IsInitialized
        {
            get
            {
                return initialized;
            }
        }
        #endregion

        public void Initialize()
        {
            TaskScheduler currentContext = TaskScheduler.FromCurrentSynchronizationContext();
            HomaGamesLog.Debug("[Homa Belly] Initializing Homa Belly Dummy for Unity Editor");

            InitializeRemoteConfigurationIndependentComponents();
        }

        /// <summary>
        /// Initializes all those components that can be initialized
        /// before the Remote Configuration data is fetched
        /// </summary>
        private void InitializeRemoteConfigurationIndependentComponents()
        {
            analyticsHelper.Start();
            LoadRewardedVideoAd();

            // Notify initialized after some dummy delay
            ExecuteWithDelay(3, () =>
            {
                initialized = true;
                m_events.OnInitialized();
            });
        }

        /// <summary>
        /// Initializes all those components that require from Remote Configuration
        /// data in order to initialize
        /// </summary>
        public void InitializeRemoteConfigurationDependantComponents(RemoteConfiguration.RemoteConfigurationModelEveryTime remoteConfigurationModel)
        {
            HomaGamesLog.Debug("[Homa Belly] Initializing Homa Belly after Remote Configuration fetch");
            CrossPromotionManager.Initialize(remoteConfigurationModel);
        }

        public void ValidateIntegration()
        {

        }

        public void OnApplicationPause(bool pause)
        {
            // Analytics Helper
            analyticsHelper.OnApplicationPause(pause);
        }

        #region IHomaBellyBridge

        public void LoadExtraRewardedVideoAd(string placementId)
        {
            LoadRewardedVideoAd(placementId);
        }

        public void LoadHighValueRewardedVideoAd()
        {
            // NO-OP
        }

        public void ShowRewardedVideoAd(string placementName, string placementId = null)
        {
            if (!IsRewardedVideoAdLoaded(placementId))
            {
                HomaGamesLog.Warning("[Homa Belly] Rewarded Video Ad not yet loaded.");
            }
            else
            {
                DefaultAnalytics.RewardedAdTriggered(placementName,placementId==null? AdPlacementType.Default : AdPlacementType.User);
                m_events.OnRewardedVideoAdStartedEvent(new AdInfo(placementId,AdType.RewardedVideo,AdPlacementType.Default));
                ShowDummyRewardedAd(placementId);
            }
        }

        public void ShowHighValueRewardedVideoAd(string placementName)
        {
            // NO-OP
        }

        public bool IsRewardedVideoAdAvailable(string placementId = null)
        {
            return IsRewardedVideoAdLoaded(placementId);
        }

        private void LoadRewardedVideoAd(string placementId = null)
        {
            ExecuteWithDelay(1f, () =>
            {
                RewardedVideoLoaded[placementId ?? String.Empty] = true;
                m_events.OnRewardedVideoAvailabilityChangedEvent(true,new AdInfo(placementId,AdType.RewardedVideo,AdPlacementType.Default));
            });
        }

        private void ShowDummyRewardedAd(string placementId)
        {
            GameObject rewardedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(RES_PATH + "Rewarded.prefab");
            GameObject dummyRewardedAd = InstantiateDummyAd(rewardedPrefab, Vector3.zero, Quaternion.identity);
            bool grantedReward = false;
            DummyRewardBehaviour dummy = dummyRewardedAd.GetComponent<DummyRewardBehaviour>();
            dummy.HomaRewardedCloseButton.onClick.AddListener(() =>
            {
                if (grantedReward)
                {
                    var reward = placementId != null ? 
                        new VideoAdReward(placementId,dummy.NameInput.text, int.Parse(dummy.QuantityInput.text)) 
                        : new VideoAdReward(dummy.NameInput.text, int.Parse(dummy.QuantityInput.text));
                    m_events.OnRewardedVideoAdRewardedEvent(reward,new AdInfo(placementId,AdType.RewardedVideo,AdPlacementType.Default));
                }
                m_events.OnRewardedVideoAdClosedEvent(new AdInfo(placementId,AdType.RewardedVideo,AdPlacementType.Default));
                LoadRewardedVideoAd(placementId);
                UnityEngine.Object.Destroy(dummyRewardedAd);
            });
            dummy.HomaRewardButton.onClick.AddListener(() =>
            {
                grantedReward = true;
                dummy.HomaRewardStatus.text = "Reward granted. Will send reward callback on ad close.";
            });
            m_events.OnRewardedVideoAvailabilityChangedEvent(false, new AdInfo(placementId,AdType.RewardedVideo,AdPlacementType.Default));
            RewardedVideoLoaded[placementId ?? String.Empty] = false;

            analyticsHelper.OnRewardedVideoAdWatched();
        }

        // Banners
        public void LoadBanner(BannerSize size, BannerPosition position, string placementId = null, Color bannerBackgroundColor = default)
        {
            if (!BannerLoaded)
            {
                // Only support BottomCenter and TopCenter for now
                string bannerPrefabName = position == BannerPosition.BOTTOM ? "BannerBottom" : "BannerTop";
                GameObject bannerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(RES_PATH + bannerPrefabName + ".prefab");
                GameObject dummyBanner = InstantiateDummyAd(bannerPrefab, Vector3.zero, Quaternion.identity);
                dummyBanner.SetActive(false); // Hidden by default

                DummyBanner = dummyBanner;
                BannerLoaded = true;
                ExecuteWithDelay(0.1f, () => m_events.OnBannerAdLoadedEvent(new AdInfo(placementId,AdType.Banner,AdPlacementType.Default)));
            }
        }

        public void ShowBanner(string placementId = null)
        {
            if (!BannerLoaded)
            {
                HomaGamesLog.Warning("[Homa Belly] Banner was not created, can not show it");
            }
            else
            {
                if (DummyBanner)
                {
                    DummyBanner.SetActive(true);
                }
            }
        }

        public void HideBanner(string placementId = null)
        {
            if (DummyBanner)
            {
                DummyBanner.SetActive(false);
            }
        }

        public void DestroyBanner(string placementId = null)
        {
            if (DummyBanner)
            {
                UnityEngine.Object.Destroy(DummyBanner);
            }
        }

        public int GetBannerHeight(string placementId = null)
        {
            if (DummyBanner)
                return (int) Mathf.Abs(((RectTransform)DummyBanner.transform.GetChild(0)).rect.height);

            return 0;
        }

        public void SetBannerPosition(BannerPosition position, string placementId = null)
        {
            if (DummyBanner)
            {
                var active = DummyBanner.activeSelf;
                var color = DummyBanner.transform.GetChild(0).GetComponent<Image>().color;
                UnityEngine.Object.Destroy(DummyBanner);
                string bannerPrefabName = position == BannerPosition.BOTTOM ? "BannerBottom" : "BannerTop";
                GameObject bannerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(RES_PATH + bannerPrefabName + ".prefab");
                GameObject dummyBanner = InstantiateDummyAd(bannerPrefab, Vector3.zero, Quaternion.identity);
                dummyBanner.SetActive(active);
                dummyBanner.transform.GetChild(0).GetComponent<Image>().color = color;
                DummyBanner = dummyBanner;
            }
        }

        public void SetBannerBackgroundColor(Color color, string placementId = null)
        {
            if (DummyBanner)
            {
                var background = DummyBanner.transform.GetChild(0).GetComponent<Image>();
                background.color = color;
            }
        }

        public void LoadExtraInterstitial(string placementId)
        {
            // NO-OP
        }

        public void LoadHighValueInterstitial()
        {
            // NO-OP
        }

        public void ShowInterstitial(string placementName, string placementId = null)
        {
            m_events.OnInterstitialAdOpenedEvent(new AdInfo(placementId,AdType.Interstitial,AdPlacementType.Default));
            GameObject interstitialPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(RES_PATH + "/Interstitial.prefab");
            GameObject dummyInterstitial = InstantiateDummyAd(interstitialPrefab, Vector3.zero, Quaternion.identity);
            DummyInterstitialBehaviour dummy = dummyInterstitial.GetComponent<DummyInterstitialBehaviour>();

            dummy.HomaInterstitialCloseButton.onClick.AddListener(() =>
            {
                m_events.OnInterstitialAdClosedEvent(new AdInfo(placementId,AdType.Interstitial,AdPlacementType.Default));
                UnityEngine.Object.Destroy(dummyInterstitial);
            });
            DefaultAnalytics.InterstitialAdTriggered(placementName, placementId==null? AdPlacementType.Default : AdPlacementType.User);
            m_events.OnInterstitialAdShowSucceededEvent(new AdInfo(placementId,AdType.Interstitial,AdPlacementType.Default));

            analyticsHelper.OnInterstitialAdWatched();
        }

        public void ShowHighValueInterstitial(string placementName)
        {
            // NO-OP
        }

        public bool IsInterstitialAvailable(string placementId = null)
        {
            return true;
        }

        public void SetUserIsAboveRequiredAge(bool consent)
        {

        }

        public void SetTermsAndConditionsAcceptance(bool consent)
        {

        }

        public void SetAnalyticsTrackingConsentGranted(bool consent)
        {

        }

        public void SetTailoredAdsConsentGranted(bool consent)
        {

        }

#if UNITY_PURCHASING
        public void TrackInAppPurchaseEvent(UnityEngine.Purchasing.Product product, bool isRestored = false)
        {
            DummyEvent("InAppPurchase", "product=" + product, "restored=" + isRestored);
        }
#endif

        public void TrackInAppPurchaseEvent(string productId, string currencyCode, double unitPrice, string transactionId = null, string payload = null, bool isRestored = false)
        {
            DummyEvent("InAppPurchase", "productId=" + productId, "currencyCode=" + currencyCode, "unitPrice=" + unitPrice, "transactionId=" + transactionId, "payload=" + payload, "restored=" + isRestored);
        }

        public void TrackResourceEvent(ResourceFlowType flowType, string currency, float amount, string itemType, string itemId)
        {
            DummyEvent("ResourceEvent", "ResourceFlowType=" + flowType.ToString(), "currency=" + currency, "amount=" + amount, "itemType=" + itemType, "itemId=" + itemId);
        }

        public void TrackProgressionEvent(ProgressionStatus progressionStatus, string progression01, int score = 0)
        {
            DummyEvent("Progression", "ProgressionStatus=" + progressionStatus.ToString(), "progression01=" + progression01, "score=" + score);
        }

        public void TrackProgressionEvent(ProgressionStatus progressionStatus, string progression01, string progression02, int score = 0)
        {
            DummyEvent("Progression", "ProgressionStatus=" + progressionStatus.ToString(), "progression01=" + progression01, "progression02=" + progression02, "score=" + score);
        }

        public void TrackProgressionEvent(ProgressionStatus progressionStatus, string progression01, string progression02, string progression03, int score = 0)
        {
            DummyEvent("Progression", "ProgressionStatus=" + progressionStatus.ToString(), "progression01=" + progression01, "progression02=" + progression02, "progression03=" + progression03, "score=" + score);
        }

        public void TrackErrorEvent(ErrorSeverity severity, string message)
        {
            DummyEvent("Error", "ErrorSeverity=" + severity.ToString(), "message=" + message);
        }

        public void TrackDesignEvent(string eventName, float eventValue = 0f)
        {
            DummyEvent("Design", "eventName=" + eventName, "eventValue=" + eventValue);
        }

        public void TrackAdEvent(AdAction adAction, AdType adType, string adNetwork, string adPlacementId)
        {
            DummyEvent("Ad", "AdAction=" + adAction.ToString(), "AdType=" + adType.ToString(), "adNetwork=" + adNetwork, "adPlacementId=" + adPlacementId);
        }

        public void TrackAdRevenue(AdRevenueData adRevenueData)
        {
            DummyEvent("AdRevenue", "AdRevenueData=" + adRevenueData.ToString());
        }

        public void TrackAttributionEvent(string eventName, Dictionary<string, object> arguments = null)
        {
            DummyEvent(eventName, "Arguments=" + Json.Serialize(arguments));
        }

        public void SetCustomDimension01(string customDimension)
        {
            // NO-OP
        }

        public void SetCustomDimension02(string customDimension)
        {
            // NO-OP
        }

        public void SetCustomDimension03(string customDimension)
        {
            // NO-OP
        }

        private void DummyEvent(string eventName, params string[] p)
        {
            var str = "[Homa Belly] Tracking " + eventName + " Event : ";
            foreach (string param in p)
                str += " " + param;
            HomaGamesLog.Debug(str);
        }

        private bool IsRewardedVideoAdLoaded([CanBeNull] string placementId)
        {
            return RewardedVideoLoaded.TryGetValue(placementId ?? String.Empty, out var value) && value;
        }

        private void ExecuteWithDelay(float seconds, Action action)
        {
            Task.Delay((int)(seconds * 1000)).ContinueWithOnMainThread((result) =>
            {
                if (Application.isPlaying)
                {
                    action();
                }
            });
        }

        private GameObject InstantiateDummyAd(GameObject prefab, Vector3 position,Quaternion rotation)
        {
            GameObject ad = UnityEngine.Object.Instantiate(prefab, position, rotation);
            UnityEngine.Object.DontDestroyOnLoad(ad);
            return ad;
        }
        #endregion
    }
}
#endif