using System;

namespace HomaGames.HomaBelly
{
    public interface IAnalytics
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
        void TrackResourceEvent(ResourceFlowType flowType, string currency, float amount, string itemType, string itemId);
        void TrackProgressionEvent(ProgressionStatus progressionStatus, string progression01, int score = 0);
        void TrackProgressionEvent(ProgressionStatus progressionStatus, string progression01, string progression02, int score = 0);
        void TrackProgressionEvent(ProgressionStatus progressionStatus, string progression01, string progression02, string progression03, int score = 0);
        void TrackErrorEvent(ErrorSeverity severity, string message);
        void TrackDesignEvent(string eventName, float eventValue = 0f);
        void TrackAdEvent(AdAction adAction, AdType adType, string adNetwork, string adPlacementId);
    }
}
