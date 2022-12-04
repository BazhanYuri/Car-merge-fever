using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Interface exposed with Homa Belly to interact
    /// with any of its products: mediations, attributions or analytics.
    /// </summary>
    public interface IHomaBellyBridge : ICustomDimensions
    {
        #region Base

        /// <summary>
        /// Validate the SDKs integration
        /// </summary>
        void ValidateIntegration();

        /// <summary>
        /// Determines if Homa Belly is already initialized
        /// </summary>
        /// <returns></returns>
        bool IsInitialized
        {
            get;
        }

        #endregion

        #region Ads

        /// <summary>
        /// Asks the mediations to load an extra Rewarded Video with the given placement
        /// ID <b>on top of the default one, with the default placement ID</b> 
        /// </summary>
        /// <param name="placementId">THe placement ID of the new Rewarded Video</param>
        void LoadExtraRewardedVideoAd([NotNull] string placementId);
        /// <summary>
        /// Loads a high value rewarded ad, will do nothing if no high value ad is configured in the manifest.
        /// </summary>
        void LoadHighValueRewardedVideoAd();
        /// <summary>
        /// Requests to show a rewarded video ad
        /// </summary>
        /// <param name="placementName">The ad placement name for analytics</param>
        /// <param name="placementId">(optional) The ad placement</param>
        void ShowRewardedVideoAd(string placementName, string placementId = null);
        /// <summary>
        /// Requests to show a high value rewarded video ad
        /// </summary>
        void ShowHighValueRewardedVideoAd(string placementName);

        /// <summary>
        /// Determines if a rewarded video ad is available
        /// </summary>
        /// <returns></returns>
        bool IsRewardedVideoAdAvailable(string placementId = null);

        /// <summary>
        /// Loads a banner with the given size and position
        /// </summary>
        /// <param name="size">The banner's size</param>
        /// <param name="position">The banner's position</param>
        /// <param name="placementId">(optional) The ad placement</param>
        /// <param name="bannerBackgroundColor">(optional) The banner background color</param>
        void LoadBanner(BannerSize size, BannerPosition position, string placementId = null, UnityEngine.Color bannerBackgroundColor = default);

        /// <summary>
        /// Show the latest loaded banner
        /// </summary>
        void ShowBanner(string placementId = null);

        /// <summary>
        /// Hides the latest banner shown
        /// </summary>
        void HideBanner(string placementId = null);

        /// <summary>
        /// Destroys the latest loaded banner
        /// </summary>
        void DestroyBanner(string placementId = null);

        /// <summary>
        /// Returns the size of the banner on screen. Will return 0
        /// if there is no banner, or if the feature is not supported. 
        /// </summary>
        /// <param name="placementId"></param>
        int GetBannerHeight(string placementId = null);
        /// <summary>
        /// Sets the banner position.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="placementId"></param>
        void SetBannerPosition(BannerPosition position, string placementId = null);
        /// <summary>
        /// Sets the banner background color.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="placementId"></param>
        void SetBannerBackgroundColor(Color color, string placementId = null);

        /// <summary>
        /// Asks the mediations to load an extra interstitial with the given placement
        /// ID <b>on top of the default one, with the default placement ID</b> 
        /// </summary>
        /// <param name="placementId">THe placement ID of the new interstitial</param>
        void LoadExtraInterstitial([NotNull] string placementId);
        /// <summary>
        /// Loads a high value interstitial ad, will do nothing if no high value ad is configured in the manifest.
        /// </summary>
        void LoadHighValueInterstitial();
        
        /// <summary>
        /// Shows the latest interstitial loaded ad
        /// </summary>
        /// <param name="placementName">The ad placement name for analytics</param>
        /// <param name="placementId">(optional) The ad placement</param>
        void ShowInterstitial(string placementName, string placementId = null);
        /// <summary>
        /// Requests to show a high value interstitial ad
        /// </summary>
        void ShowHighValueInterstitial(string placementName);

        /// <summary>
        /// Determines if the interstitial ad is available
        /// </summary>
        /// <returns></returns>
        bool IsInterstitialAvailable(string placementId = null);

        #endregion

        #region GDPR/CCPA
        /// <summary>
        /// Specifies if the user asserted being above the required age
        /// </summary>
        /// <param name="consent">true if user accepted, false otherwise</param>
        void SetUserIsAboveRequiredAge(bool consent);

        /// <summary>
        /// Specifies if the user accepted privacy policy and terms and conditions
        /// </summary>
        /// <param name="consent">true if user accepted, false otherwise</param>
        void SetTermsAndConditionsAcceptance(bool consent);

        /// <summary>
        /// Specifies if the user granted consent for analytics tracking
        /// </summary>
        /// <param name="consent">true if user accepted, false otherwise</param>
        void SetAnalyticsTrackingConsentGranted(bool consent);

        /// <summary>
        /// Specifies if the user granted consent for showing tailored ads
        /// </summary>
        /// <param name="consent">true if user accepted, false otherwise</param>
        void SetTailoredAdsConsentGranted(bool consent);

        #endregion

        #region Analytics

#if UNITY_PURCHASING
        /// <summary>
        /// Tracks an In App Purchase event
        /// </summary>
        /// <param name="product">The Unity IAP Product purchased</param>
        /// <param name="isRestored">(Optional) If the purchase is restored. Default is false</param>
        void TrackInAppPurchaseEvent(UnityEngine.Purchasing.Product product, bool isRestored = false);
#endif

        /// <summary>
        /// Tracks an In App Purchase event. Purchase can be verified if
        /// `transactionId` and `payload` are informed for the corresponding platforms
        /// </summary>
        /// <param name="productId">The product id puchased</param>
        /// <param name="currencyCode">The currency code of the purchase</param>
        /// <param name="unitPrice">The unit price</param>
        /// <param name="transactionId">(Optional) The transaction id for the IAP validation</param>
        /// <param name="payload">(Optional - Only Android) Payload for Android IAP validation</param>
        /// <param name="isRestored">(Optional) If the purchase is restored. Default is false</param>
        void TrackInAppPurchaseEvent(string productId, string currencyCode, double unitPrice, string transactionId = null, string payload = null, bool isRestored = false);

        /// <summary>
        /// Track a resource flow (source/sink) event
        /// </summary>
        /// <param name="flowType">`Source` when user obtains some resource, or `Sink` when user spens some resource</param>
        /// <param name="currency">Resource name</param>
        /// <param name="amount">Resource amount</param>
        /// <param name="itemType">Resource type</param>
        /// <param name="itemId">Resource id</param>
        void TrackResourceEvent(ResourceFlowType flowType, string currency, float amount, string itemType, string itemId);

        /// <summary>
        /// Tracks a progression event
        /// </summary>
        /// <param name="progressionStatus">Start, Complete or Fail</param>
        /// <param name="progression">Progress description</param>
        /// <param name="score">(Optional) Score</param>
        void TrackProgressionEvent(ProgressionStatus progressionStatus, string progression01, int score = 0);
        void TrackProgressionEvent(ProgressionStatus progressionStatus, string progression01, string progression02, int score = 0);
        void TrackProgressionEvent(ProgressionStatus progressionStatus, string progression01, string progression02, string progression03, int score = 0);

        /// <summary>
        /// Tracks an error event
        /// </summary>
        /// <param name="severity">Debug, Info, Warning, Error or Critical</param>
        /// <param name="message">The error message</param>
        void TrackErrorEvent(ErrorSeverity severity, string message);

        /// <summary>
        /// Tracks a design event
        /// </summary>
        /// <param name="eventName">The design event name</param>
        /// <param name="eventValue">(Optional) Any event value</param>
        void TrackDesignEvent(string eventName, float eventValue = 0f);

        /// <summary>
        /// Tracks an Ad event
        /// </summary>
        /// <param name="adAction">Clicked, FailedShow, RewardReceived, Request or Loaded</param>
        /// <param name="adType">Ad type</param>
        /// <param name="adNetwork">Ad network</param>
        /// <param name="adPlacementId">Ad placement id</param>
        void TrackAdEvent(AdAction adAction, AdType adType, string adNetwork, string adPlacementId);

        /// <summary>
        /// Tracks an Ad Revenue event
        /// </summary>
        /// <param name="adRevenueData">Object holding all ad revenue data to be sent</param>
        void TrackAdRevenue(AdRevenueData adRevenueData);

        /// <summary>
        /// Tracks an event on the attribution platform
        /// </summary>
        /// <param name="eventName">The event name</param>
        /// <param name="arguments">(Optional) Additional arguments. Dictionary values must have one of these types: string, int, long, float, double, null, ArrayList, Dictionary<String,object></param>
        void TrackAttributionEvent(string eventName, Dictionary<string, object> arguments = null);

#endregion
    }
}
