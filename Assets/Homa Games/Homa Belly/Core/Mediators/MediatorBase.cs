using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public abstract class MediatorBase
    {
        // Helpers
        private readonly NetworkHelper _networkHelper = new NetworkHelper();
        private readonly Events _events = new Events();

        private const int MAX_RELOAD_RETRY_ATTEMPT = 5;

        public class AdState
        {
            public readonly AdInfo AdInfo;
            public AdState(AdPlacementType placementType, AdType type, string adPlacement)
            {
                AdInfo = new AdInfo(adPlacement,type,placementType);
            }

            private bool _loading = false;

            public bool Loading
            {
                get => _loading;
                set
                {
                    if (!value)
                        _retryCount = 0;
                    _loading = value;
                }
            }

            private int _retryCount = 0;
            public int LoadingRetryCount => _retryCount;
            public void Retry() => _retryCount++;
            public AdPlacementType PlacementType => AdInfo.AdPlacementType;
            public AdType Type => AdInfo.AdType;
            public string AdPlacement => AdInfo.PlacementId;
        }

        // Ad placement to ad state
        private readonly Dictionary<string, AdState> _adStates = new Dictionary<string, AdState>();

        private AdState GetOrCreateAdState(string placementId, AdType adType)
        {
            if (!_adStates.ContainsKey(placementId))
            {
                if (_defaultAdIds.ContainsValue(placementId))
                    _adStates.Add(placementId, new AdState(AdPlacementType.Default, adType, placementId));
                else if (_highValueAdIds.ContainsValue(placementId))
                    _adStates.Add(placementId, new AdState(AdPlacementType.HighValue, adType, placementId));
                else
                    _adStates.Add(placementId, new AdState(AdPlacementType.User, adType, placementId));
            }

            return _adStates[placementId];
        }

        // Banner
        private string CurrentBannerPlacementId { get; set; } = null;
        private Color CurrentBannerBackgroundColor { get; set; } = Color.white;
        private BannerPosition CurrentBannerPosition { get; set; } = BannerPosition.BOTTOM;
        private BannerSize CurrentBannerSize { get; set; }

        private bool _reportAdRevenue = false;


        private static readonly Dictionary<AdType, string> AddTypesToString = new Dictionary<AdType, string>
        {
            [AdType.Banner] = "banner",
            [AdType.Interstitial] = "interstitial",
            [AdType.RewardedVideo] = "rewarded_video",
        };

        /// <summary>
        /// Dictionary containing the default Ad IDs for the current platform.
        /// </summary>
        private readonly Dictionary<AdType, string> _defaultAdIds = new Dictionary<AdType, string>();

        /// <summary>
        /// Dictionary containing the high value Ad IDs for the current platform.
        /// </summary>
        private readonly Dictionary<AdType, string> _highValueAdIds = new Dictionary<AdType, string>();

        // Base methods
        public void Initialize(Action onInitialized = null)
        {
            // Gather default Ad placement IDs
            LoadDefaultAdIds();
            _networkHelper.OnNetworkReachabilityChange += OnNetworkReachabilityChange;
            _networkHelper.StartListening();
            if (HomaBellyManifestConfiguration.TryGetBool(out var shouldReport, MediatorPackageName,
                    "b_report_ad_revenue"))
            {
                _reportAdRevenue = shouldReport;
            }

            OnInitialised += () =>
            {
#if UNITY_IOS || UNITY_IPHONE
                InvokeFacebookAudienceNetworkAdvertiserFlag();
#endif
                // Preload interstitial and rewarded video ads to be cached
                LoadInterstitial(GetAdIdOrDefault(AdType.Interstitial));
                LoadRewardedVideoAd(GetAdIdOrDefault(AdType.RewardedVideo));
                LoadBanner(BannerSize.BANNER, BannerPosition.BOTTOM, GetAdIdOrDefault(AdType.Banner), Color.white);
                onInitialized?.Invoke();
                HomaGamesLog.Debug($"[{MediatorPackageName} Mediator]  Initialized successfully");
            };
            OnAdRevenuePaidEvent += OnAdRevenuePaid;
            OnBannerLoadedEvent += s =>
            {
                var adState = GetOrCreateAdState(s, AdType.Banner);
                adState.Loading = false;

                HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] OnBannerAdLoadedEvent {adState.AdInfo}");
                _events.OnBannerAdLoadedEvent(adState.AdInfo);
            };
            BannerAdClickedEvent += s =>
            {
                var adState = GetOrCreateAdState(s, AdType.Banner);
                HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] BannerAdClickedEvent {adState.AdInfo}");
                _events.OnBannerAdClickedEvent(adState.AdInfo);
            };
            BannerAdLoadFailedEvent += (s, errorCode, errorMessage) =>
            {
                var adState = GetOrCreateAdState(s, AdType.Banner);
                HomaGamesLog.Debug(
                    $"[{MediatorPackageName} Mediator] BannerAdLoadFailedEvent with error code {errorCode}: {errorMessage}.\n {adState.AdInfo}");
                _events.OnBannerAdLoadFailedEvent(adState.AdInfo);
                RetryLoadAd(adState).ListenForErrors();
            };
            OnRewardedAdReceivedRewardEvent += (s, reward) =>
            {
                var adState = GetOrCreateAdState(s, AdType.RewardedVideo);
                HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] OnRewardedAdReceivedRewardEvent {adState.AdInfo}");
                _events.OnRewardedVideoAdRewardedEvent(reward,adState.AdInfo);
            };
            OnRewardedAdDismissedEvent += s =>
            {
                LoadRewardedVideoAd(s);
                var adState = GetOrCreateAdState(s, AdType.RewardedVideo);
                HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] OnRewardedAdDismissedEvent {adState.AdInfo}");
                _events.OnRewardedVideoAdClosedEvent(adState.AdInfo);
            };
            OnRewardedAdClickedEvent += s =>
            {
                var adState = GetOrCreateAdState(s, AdType.RewardedVideo);
                HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] OnRewardedAdClickedEvent {adState.AdInfo}");
                _events.OnRewardedVideoAdClickedEvent(adState.AdInfo);
            };
            OnRewardedAdDisplayedEvent += s =>
            {
                var adState = GetOrCreateAdState(s, AdType.RewardedVideo);
                adState.Loading = false;
                HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] OnRewardedAdDisplayedEvent {adState.AdInfo}");
                _events.OnRewardedVideoAdStartedEvent(adState.AdInfo);
            };
            OnRewardedAdFailedToDisplayEvent += (s, errorCode, error) =>
            {
                var adState = GetOrCreateAdState(s, AdType.RewardedVideo);
                adState.Loading = false;

                HomaGamesLog.Debug(
                    $"[{MediatorPackageName} Mediator] OnRewardedAdFailedToDisplayEvent with error code {errorCode}: {error}.\n {adState.AdInfo}");
                RetryLoadAd(GetOrCreateAdState(s, AdType.RewardedVideo)).ListenForErrors();
                _events.OnRewardedVideoAdShowFailedEvent(adState.AdInfo);
            };
            OnRewardedAdFailedEvent += (s, errorCode, error) =>
            {
                var adState = GetOrCreateAdState(s, AdType.RewardedVideo);
                HomaGamesLog.Debug(
                    $"[{MediatorPackageName} Mediator] OnRewardedAdFailedEvent with error code {errorCode}: {error}.\n {adState.AdInfo}");
                RetryLoadAd(adState).ListenForErrors();
                _events.OnRewardedVideoAdShowFailedEvent(adState.AdInfo);
            };
            OnRewardedAdLoadedEvent += s =>
            {
                var adState = GetOrCreateAdState(s, AdType.RewardedVideo);
                adState.Loading = false;

                HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] OnRewardedAdLoadedEvent {adState.AdInfo}");
                _events.OnRewardedVideoAvailabilityChangedEvent(true,adState.AdInfo);
            };
            OnInterstitialDismissedEvent += s =>
            {
                var adState = GetOrCreateAdState(s, AdType.Interstitial);
                adState.Loading = false;

                HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] OnInterstitialDismissedEvent {adState.AdInfo}");
                LoadInterstitial(s);

                _events.OnInterstitialAdClosedEvent(adState.AdInfo);
            };
            InterstitialFailedToDisplayEvent += (s, errorCode, error) =>
            {
                var adState = GetOrCreateAdState(s, AdType.Interstitial);
                HomaGamesLog.Debug(
                    $"[{MediatorPackageName} Mediator] InterstitialAdShowFailedEvent with error code {errorCode}: {error}.\n {adState.AdInfo}");
                _events.OnInterstitialAdShowFailedEvent(adState.AdInfo);
                RetryLoadAd(adState).ListenForErrors();
            };
            OnInterstitialFailedEvent += (s, errorCode, error) =>
            {
                var adState = GetOrCreateAdState(s, AdType.Interstitial);
                HomaGamesLog.Debug(
                    $"[{MediatorPackageName} Mediator] OnInterstitialFailedEvent with error code {errorCode}: {error}.\n {adState.AdInfo}");
                _events.OnInterstitialAdLoadFailedEvent(adState.AdInfo);
                RetryLoadAd(adState).ListenForErrors();
            };
            OnInterstitialLoadedEvent += s =>
            {
                var adState = GetOrCreateAdState(s, AdType.Interstitial);
                adState.Loading = false;

                HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] OnInterstitialLoadedEvent {adState.AdInfo}");
                _events.OnInterstitialAdReadyEvent(adState.AdInfo);
            };
            OnInterstitialClickedEvent += s =>
            {
                var adState = GetOrCreateAdState(s, AdType.Interstitial);
                HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] InterstitialAdClickedEvent {adState.AdInfo}");
                _events.OnInterstitialAdClickedEvent(adState.AdInfo);
            };
            OnInterstitialShownEvent += s =>
            {
                var adState = GetOrCreateAdState(s, AdType.Interstitial);
                adState.Loading = false;
                HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] InterstitialAdShowSucceededEvent {adState.AdInfo}");
                _events.OnInterstitialAdShowSucceededEvent(adState.AdInfo);
            };
            InternalInitialize();
        }

        public virtual void OnApplicationPause(bool pause)
        {
        }

        public virtual void ValidateIntegration()
        {
        }

        #region GDPR/CCPA

        /// <summary>
        /// Specifies if the user asserted being above the required age
        /// </summary>
        /// <param name="consent">true if user accepted, false otherwise</param>
        public virtual void SetUserIsAboveRequiredAge(bool consent)
        {
        }

        /// <summary>
        /// Specifies if the user accepted privacy policy and terms and conditions
        /// </summary>
        /// <param name="consent">true if user accepted, false otherwise</param>
        public virtual void SetTermsAndConditionsAcceptance(bool consent)
        {
        }

        /// <summary>
        /// Specifies if the user granted consent for analytics tracking
        /// </summary>
        /// <param name="consent">true if user accepted, false otherwise</param>
        public virtual void SetAnalyticsTrackingConsentGranted(bool consent)
        {
        }

        /// <summary>
        /// Specifies if the user granted consent for showing tailored ads
        /// </summary>
        /// <param name="consent">true if user accepted, false otherwise</param>
        public virtual void SetTailoredAdsConsentGranted(bool consent)
        {
        }

        #endregion

        /// <summary>
        /// Try reloading the ad after one or multiple failed attempts
        /// </summary>
        /// <param name="state">The ad to reload</param>
        private async Task RetryLoadAd(AdState state)
        {
            if (!state.Loading)
                return;
            if (state.LoadingRetryCount > MAX_RELOAD_RETRY_ATTEMPT)
                state.Loading = false;
            state.Retry();
            int retryDelayInMs = (int) Math.Pow(2, state.LoadingRetryCount) * 1000;
            HomaGamesLog.Debug(
                $"[{MediatorPackageName} Mediator] Trying to reload ad {state.AdPlacement} in {retryDelayInMs}. Retry count : {state.LoadingRetryCount}");
            await Task.Delay(retryDelayInMs);
            switch (state.Type)
            {
                case AdType.Banner:
                    LoadBanner(CurrentBannerSize, CurrentBannerPosition, state.AdPlacement,
                        CurrentBannerBackgroundColor);
                    break;
                case AdType.Interstitial:
                    LoadInterstitial(state.AdPlacement);
                    break;
                case AdType.RewardedVideo:
                    LoadRewardedVideoAd(state.AdPlacement);
                    break;
            }
        }

        // Rewarded Video Ads
        public void LoadRewardedVideoAd([NotNull] string placement)
        {
            GetOrCreateAdState(placement, AdType.RewardedVideo).Loading = true;
            InternalLoadRewardedVideoAd(placement);
        }
        
        public void LoadHighValueRewardedVideoAd()
        {
            if (_highValueAdIds.TryGetValue(AdType.RewardedVideo,out var adPlacement))
            {
                LoadRewardedVideoAd(adPlacement);
            }
            else
            {
                HomaGamesLog.Error($"[{MediatorPackageName} Mediator] No High Value rewarded video ad in configuration.");
            }
        }

        public void ShowRewardedVideoAd(string placement = null)
        {
            string finalPlacement = GetAdIdOrDefault(AdType.RewardedVideo, placement);
            if (InternalIsInitialized)
            {
                // If rewarded video ad is ready, show it
                if (InternalIsRewardedVideoAdAvailable(finalPlacement))
                {
                    HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] Video Ad available. Showing...");
                    InternalShowRewardedVideoAd(finalPlacement);
                }
                else
                {
                    LoadRewardedVideoAd(finalPlacement);
                    HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] Rewarded video not available");
                    _events.OnRewardedVideoAdShowFailedEvent(GetOrCreateAdState(finalPlacement,AdType.RewardedVideo).AdInfo);
                }
            }
            else
            {
                HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] Not initialised");
            }
        }

        public void ShowHighValueRewardedVideoAd()
        {
            if(_highValueAdIds.TryGetValue(AdType.RewardedVideo,out var adPlacement))
            {
                ShowRewardedVideoAd(adPlacement);
            }
            else
            {
                HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] No High Value ad configured for rewarded video ads.");
            }
        }

        public bool IsRewardedVideoAdAvailable(string placement = null)
        {
            return InternalIsRewardedVideoAdAvailable(GetAdIdOrDefault(AdType.RewardedVideo, placement));
        }

        // Banners
        public void LoadBanner(BannerSize size, BannerPosition position, string placementId = null,
            Color bannerBackgroundColor = default)
        {
            placementId = GetAdIdOrDefault(AdType.Banner, placementId);
            bool mustDestroyBanner = position != CurrentBannerPosition || placementId != CurrentBannerPlacementId ||
                                     CurrentBannerBackgroundColor != bannerBackgroundColor || size != CurrentBannerSize;

            CurrentBannerSize = size;
            CurrentBannerPosition = position;
            CurrentBannerPlacementId = placementId;
            CurrentBannerBackgroundColor = bannerBackgroundColor == default ? Color.white : bannerBackgroundColor;

            if (InternalIsInitialized)
            {
                if (mustDestroyBanner)
                {
                    DestroyBanner(placementId);
                }

                GetOrCreateAdState(placementId, AdType.Banner).Loading = true;
                HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] Loading banner");
                InternalLoadBanner(size, position, placementId, CurrentBannerBackgroundColor);
            }
            else
            {
                HomaGamesLog.Warning(
                    $"[{MediatorPackageName} Mediator] Not initialized yet while trying to load banner");
            }
        }

        public void ShowBanner(string placement = null)
        {
            InternalShowBanner(GetAdIdOrDefault(AdType.Banner, placement));
        }

        public void HideBanner(string placement = null)
        {
            InternalHideBanner(GetAdIdOrDefault(AdType.Banner, placement));
        }

        public void DestroyBanner(string placement = null)
        {
            InternalDestroyBanner(GetAdIdOrDefault(AdType.Banner, placement));
        }

        public int GetBannerHeight(string placement = null)
        {
            return InternalGetBannerHeight(GetAdIdOrDefault(AdType.Banner, placement));
        }

        public void SetBannerPosition(BannerPosition position,string placementId = null)
        {
            var finalPlacement = GetAdIdOrDefault(AdType.Banner,placementId);
            CurrentBannerPosition = position;
            InternalSetBannerPosition(finalPlacement,position);
        }
        
        public void SetBannerBackgroundColor(Color color,string placementId = null)
        {
            var finalPlacement = GetAdIdOrDefault(AdType.Banner,placementId);
            CurrentBannerBackgroundColor = color;
            InternalSetBannerBackgroundColor(finalPlacement,color);
        }

        // Interstitial
        public void LoadInterstitial([NotNull] string placement)
        {
            GetOrCreateAdState(placement, AdType.Interstitial).Loading = true;
            InternalLoadInterstitial(placement);
        }
        
        public void LoadHighValueInterstitial()
        {
            if (_highValueAdIds.TryGetValue(AdType.Interstitial,out var adPlacement))
            {
                LoadInterstitial(adPlacement);
            }
            else
            {
                HomaGamesLog.Error($"[{MediatorPackageName} Mediator] No High Value interstitial ad in configuration.");
            }
        }

        public void ShowInterstitial(string placement = null)
        {
            var finalPlacement = GetAdIdOrDefault(AdType.Interstitial, placement);
            if (InternalIsInitialized)
            {
                HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] Request show interstitial");
                if (InternalIsInterstitialAvailable(finalPlacement))
                {
                    HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] Interstitial available");
                    InternalShowInterstitial(finalPlacement);
                }
                else
                {
                    LoadInterstitial(finalPlacement);
                    HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] Interstitial not available");
                    _events.OnInterstitialAdShowFailedEvent(GetOrCreateAdState(finalPlacement,AdType.Interstitial).AdInfo);
                }
            }
            else
            {
                HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] Not initialised");
            }
        }
        
        public void ShowHighValueInterstitial()
        {
            if(_highValueAdIds.TryGetValue(AdType.Interstitial,out var adPlacement))
            {
                ShowInterstitial(adPlacement);
            }
            else
            {
                HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] No High Value ad configured for interstitial ads.");
            }
        }

        public bool IsInterstitialAvailable(string placement = null)
        {
            return InternalIsInterstitialAvailable(GetAdIdOrDefault(AdType.Interstitial, placement));
        }

        private string GetAdIdOrDefault(AdType adType, string placement = "")
        {
            if (string.IsNullOrEmpty(placement) && _defaultAdIds.TryGetValue(adType, out string defaultPlacement))
                return defaultPlacement;
            return placement;
        }

        private void OnNetworkReachabilityChange(NetworkReachability reachability)
        {
            if (reachability != NetworkReachability.NotReachable)
            {
                HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] Internet reachable. Reloading ads if necessary");
                foreach (var adState in _adStates.Values)
                {
                    if (adState.Loading)
                    {
                        switch (adState.Type)
                        {
                            case AdType.Interstitial:
                                InternalLoadInterstitial(adState.AdPlacement);
                                break;
                            case AdType.RewardedVideo:
                                InternalLoadRewardedVideoAd(adState.AdPlacement);
                                break;
                            case AdType.Banner:
                                InternalLoadBanner(CurrentBannerSize, CurrentBannerPosition, adState.AdPlacement,
                                    CurrentBannerBackgroundColor);
                                break;
                            case AdType.Undefined:
                            case AdType.Video:
                            case AdType.Playable:
                            case AdType.OfferWall:
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Loads default and high_value ad units from HomaBellyManifestConfiguration.
        /// </summary>
        private void LoadDefaultAdIds()
        {
            foreach (var adType in _adTypeConfigName.Keys)
            {
                foreach (var adPlacement in _adPlacementConfigName.Keys)
                {
                    LoadAdPlacementIdFromConfiguration(adPlacement,adType);
                }
            }
        }

        private static readonly Dictionary<AdType, string> _adTypeConfigName = new Dictionary<AdType, string>()
        {
            {AdType.Banner,"banner_ad_unit_id"},
            {AdType.Interstitial,"interstitial_ad_unit_id"},
            {AdType.RewardedVideo,"rewarded_video_ad_unit_id"}
        };
        
        private static readonly Dictionary<AdPlacementType, string> _adPlacementConfigName = new Dictionary<AdPlacementType, string>()
        {
            {AdPlacementType.Default,"default"},
            {AdPlacementType.HighValue,"high_value"}
        };

        private void LoadAdPlacementIdFromConfiguration(AdPlacementType placementType,AdType adType)
        {
            if (!_adTypeConfigName.ContainsKey(adType) || !_adPlacementConfigName.ContainsKey(placementType))
                return;
            
#if UNITY_ANDROID
            var platform = "android";   
#elif UNITY_IOS
            var platform = "ios";
#else
            string platform;
            return;
#endif
            string configPath = $"s_{platform}_{_adPlacementConfigName[placementType]}_{_adTypeConfigName[adType]}";
            if (HomaBellyManifestConfiguration.TryGetString(out var adUnit,
                    MediatorPackageName, configPath) && !string.IsNullOrEmpty(adUnit))
            {
                switch (placementType)
                {
                    case AdPlacementType.Default:
                        _defaultAdIds.Add(adType, adUnit);
                        break;
                    case AdPlacementType.HighValue:
                        _highValueAdIds.Add(adType, adUnit);
                        break;
                }
                HomaGamesLog.Debug($"[{MediatorPackageName} Mediator] Loaded {placementType} placement id {adUnit}");
            }
        }

        /// <summary>
        /// Callback invoked for ULRD
        /// </summary>
        /// <param name="adPlacement"></param>
        /// <param name="adType"></param>
        /// <param name="data"></param>
        private void OnAdRevenuePaid(string adPlacement, AdType adType, AdRevenueData data)
        {
            if (!_reportAdRevenue)
            {
                return;
            }

            AddTypesToString.TryGetValue(adType, out data.AdType);
            HomaBelly.Instance.TrackAdRevenue(data);
        }

        #region Protected Interface

        protected abstract string MediatorPackageName { get; }

        // Ads
        protected abstract void InternalInitialize();
        protected abstract bool InternalIsInitialized { get; }

        protected abstract void InternalLoadBanner(BannerSize size, BannerPosition position,
            [NotNull] string placementId,
            Color backgroundColor);

        protected abstract void InternalShowBanner([NotNull] string placement);
        protected abstract void InternalHideBanner([NotNull] string placement);
        protected abstract void InternalDestroyBanner([NotNull] string placement);
        protected abstract int InternalGetBannerHeight([NotNull] string placement);
        protected abstract void InternalSetBannerPosition([NotNull] string placement,BannerPosition bannerPosition);
        protected abstract void InternalSetBannerBackgroundColor([NotNull] string placement,Color color);
        protected abstract void InternalLoadInterstitial([NotNull] string placement);
        protected abstract void InternalShowInterstitial([NotNull] string placement);
        protected abstract bool InternalIsInterstitialAvailable([NotNull] string placement);
        protected abstract void InternalLoadRewardedVideoAd([NotNull] string placement);
        protected abstract void InternalShowRewardedVideoAd([NotNull] string placement);

        protected abstract bool InternalIsRewardedVideoAdAvailable([NotNull] string placement);

        // Events
        public event Action OnInitialised;
        protected void InvokeOnInitialised() => OnInitialised?.Invoke();
        public event Action<string, AdType, AdRevenueData> OnAdRevenuePaidEvent;

        protected void InvokeOnAdRevenuePaidEvent(string placementId, AdType adType, AdRevenueData data) =>
            OnAdRevenuePaidEvent?.Invoke(placementId, adType, data);

        public event Action<string> OnBannerLoadedEvent;
        protected void InvokeOnBannerLoadedEvent(string placementId) => OnBannerLoadedEvent?.Invoke(placementId);
        public event Action<string> BannerAdClickedEvent;

        protected void InvokeBannerAdClickedEvent(string placementId) => BannerAdClickedEvent?.Invoke(placementId);

        // Ad placement, error code, error message
        public event Action<string, int, string> BannerAdLoadFailedEvent;

        protected void InvokeBannerAdLoadFailedEvent(string placementId, int errorCode, string error) =>
            BannerAdLoadFailedEvent?.Invoke(placementId, errorCode, error);

        public event Action<string> OnRewardedAdLoadedEvent;

        protected void InvokeOnRewardedAdLoadedEvent(string placementId) =>
            OnRewardedAdLoadedEvent?.Invoke(placementId);

        // Ad placement, error code, error message
        public event Action<string, int, string> OnRewardedAdFailedEvent;

        protected void InvokeOnRewardedAdFailedEvent(string placementId, int errorCode, string error) =>
            OnRewardedAdFailedEvent?.Invoke(placementId, errorCode, error);

        // Ad placement, error code, error message
        public event Action<string, int, string> OnRewardedAdFailedToDisplayEvent;

        protected void InvokeOnRewardedAdFailedToDisplayEvent(string placementId, int errorCode, string error) =>
            OnRewardedAdFailedToDisplayEvent?.Invoke(placementId, errorCode, error);

        public event Action<string> OnRewardedAdDisplayedEvent;

        protected void InvokeOnRewardedAdDisplayedEvent(string placementId) =>
            OnRewardedAdDisplayedEvent?.Invoke(placementId);

        public event Action<string> OnRewardedAdClickedEvent;

        protected void InvokeOnRewardedAdClickedEvent(string placementId) =>
            OnRewardedAdClickedEvent?.Invoke(placementId);

        public event Action<string> OnRewardedAdDismissedEvent;

        protected void InvokeOnRewardedAdDismissedEvent(string placementId) =>
            OnRewardedAdDismissedEvent?.Invoke(placementId);

        public event Action<string, VideoAdReward> OnRewardedAdReceivedRewardEvent;

        protected void InvokeOnRewardedAdReceivedRewardEvent(string placementId, VideoAdReward reward) =>
            OnRewardedAdReceivedRewardEvent?.Invoke(placementId, reward);

        public event Action<string> OnInterstitialClickedEvent;

        protected void InvokeOnInterstitialClickedEvent(string placementId) =>
            OnInterstitialClickedEvent?.Invoke(placementId);

        public event Action<string> OnInterstitialShownEvent;

        protected void InvokeOnInterstitialShownEvent(string placementId) =>
            OnInterstitialShownEvent?.Invoke(placementId);

        public event Action<string> OnInterstitialLoadedEvent;

        protected void InvokeOnInterstitialLoadedEvent(string placementId) =>
            OnInterstitialLoadedEvent?.Invoke(placementId);

        // Ad placement, error code, error message
        public event Action<string, int, string> OnInterstitialFailedEvent;

        protected void InvokeOnInterstitialFailedEvent(string placementId, int errorCode, string error) =>
            OnInterstitialFailedEvent?.Invoke(placementId, errorCode, error);

        // Ad placement, error code, error message
        public event Action<string, int, string> InterstitialFailedToDisplayEvent;

        protected void InvokeInterstitialFailedToDisplayEvent(string placementId, int errorCode, string error) =>
            InterstitialFailedToDisplayEvent?.Invoke(placementId, errorCode, error);

        public event Action<string> OnInterstitialDismissedEvent;

        protected void InvokeOnInterstitialDismissedEvent(string placementId) =>
            OnInterstitialDismissedEvent?.Invoke(placementId);

        #endregion

        /// <summary>
        /// Call AudienceNetwork.AdSettings.SetAdvertiserTrackingFlag by reflection
        /// to avoid crashes if the integration does not contain FacebookAudienceNetwork adapter
        /// </summary>
        private void InvokeFacebookAudienceNetworkAdvertiserFlag()
        {
            try
            {
                Type adSettingsType =
                    Type.GetType(
                        "AudienceNetwork.AdSettings, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                if (adSettingsType != null)
                {
                    MethodInfo methodInfo = adSettingsType.GetMethod("SetAdvertiserTrackingFlag",
                        BindingFlags.Static | BindingFlags.Public);
                    if (methodInfo != null)
                    {
                        methodInfo.Invoke(null, null);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(
                    $"AudienceNetwork.AdSettings.SetAdvertiserTrackingFlag() method failed to invoke: {e.Message}");
            }
        }

        public string DumpAdStates()
        {
            var s = $"AdStates for {MediatorPackageName} Mediator\n\n";
            foreach (var state in _adStates.Values)
            {
                s +=
                    $"Ad placement:{state.AdPlacement} Type:{state.Type} Loading:{state.Loading} Load retries {state.LoadingRetryCount} Placement Type {state.PlacementType}\n\n";
            }

            return s;
        }
    }
}