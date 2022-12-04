using System;

namespace HomaGames.HomaBelly
{
    // Obsolete since 2021-07-18
    public interface IMediator
    {
        // Base methods
        void Initialize();
        void OnApplicationPause(bool pause);
        void ValidateIntegration();

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

        /// <summary>
        /// Register all events and callbacks required for the
        /// mediation implementation
        /// </summary>
        void RegisterEvents();

        // Rewarded Video Ads
        void ShowRewardedVideoAd(string placement = null);
        bool IsRewardedVideoAdAvailable(string placement = null);

        // Banners
        void LoadBanner(BannerSize size, BannerPosition position, string placement = null, UnityEngine.Color bannerBackgroundColor = default);
        void ShowBanner(string placement = null);
        void HideBanner(string placement = null);
        void DestroyBanner(string placement = null);
        
        // Interstitial
        void ShowInterstitial(string placement = null);
        bool IsInterstitialAvailable(string placement = null);
    }
}