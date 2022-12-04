using System;
using System.Collections.Generic;
using System.Diagnostics;
using com.adjust.sdk;
#if HOMA_STORE
using HomaGames.HomaBelly.IAP;
#endif
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public class AdjustAttributionAdapter : IAttributionWithInitializationCallback
    {
        private const string N_TESTING_VERSION = "s_ntesting_version";
        private const string INSTALL_APP_VERSION = "s_install_app_version";
        private const string HOMA_GAMES_ID = "s_homa_games_id";

        private bool m_initialized = false;

        public void Initialize(string appSubversion = "")
        {
            Initialize(appSubversion, null);
        }

        public void Initialize(string appSubversion, Action onInitialized)
        {
            try
            {
                if (m_initialized)
                {
                    return;
                }
                
                m_initialized = true;
                
                var adjustGameObject = new GameObject("Adjust");
                adjustGameObject.SetActive(false);
            
                var adjust = adjustGameObject.AddComponent<Adjust>();
                adjust.startManually = true;
                
                var manifestAdjustConfig = ManifestAdjustConfig.Instance;

                if (!manifestAdjustConfig.GetTargetPlatformConfig(out var platformAdjustConfig))
                {
                    HomaGamesLog.Error($"[ERROR] Current platform: {Application.platform} isn't compatible with Adjust SDK.");
                    return;
                }

                if (manifestAdjustConfig.IsReferralSystemEnabled)
                {
                    ReferralSystem.Instance.Initialize(manifestAdjustConfig.GetSchemaWithPath());
                }
                
                var adjustConfig = new AdjustConfig(
                    platformAdjustConfig.AppToken,
                    IsDevelopmentBuild() ? AdjustEnvironment.Sandbox : AdjustEnvironment.Production,false);

                adjustConfig.setLogLevel(IsDevelopmentBuild() ? AdjustLogLevel.Verbose : AdjustLogLevel.Warn);
                adjustConfig.setLogDelegate(UnityEngine.Debug.Log);
                adjustConfig.setSendInBackground(true);
                HomaGamesLog.Debug($"Adjust {adjustConfig.appToken} Log Level: {adjustConfig.logLevel} Environment: {adjustConfig.environment}");

                if (!string.IsNullOrEmpty(appSubversion) && appSubversion != "0")
                {
                    Adjust.addSessionCallbackParameter(N_TESTING_VERSION, appSubversion);
                }
                
                Adjust.addSessionCallbackParameter(HOMA_GAMES_ID,Identifiers.HomaGamesId);

                // We need to send the version in which the game was installed.
                var currentVersion = Application.version;
                if (!PlayerPrefs.HasKey(HomaBellyAdjustConstants.APP_VERSION_AT_INSTALL))
                {
                    PlayerPrefs.SetString(HomaBellyAdjustConstants.APP_VERSION_AT_INSTALL, currentVersion);
                    // We only have to add it once
                    Adjust.addSessionCallbackParameter(INSTALL_APP_VERSION, currentVersion);
                    PlayerPrefs.Save();
                }
                
                adjustConfig.setEventSuccessDelegate(EventSuccessCallback);
                adjustConfig.setEventFailureDelegate(EventFailureCallback);
                adjustConfig.setSessionSuccessDelegate(SessionSuccessCallback);
                adjustConfig.setSessionFailureDelegate(SessionFailureCallback);
                adjustConfig.setAttributionChangedDelegate(AttributionChangedCallback);
                
                adjustConfig.setLaunchDeferredDeeplink(false);

                adjustGameObject.SetActive(true);
                Adjust.start(adjustConfig);
                HomaGamesLog.Debug($"Adjust SDK initialized: {Adjust.getAdid()}");

                RevenueCatSetup();
            }
            finally
            {
                onInitialized?.Invoke();
            }
        }

        private bool IsDevelopmentBuild()
        {
            #if HOMA_DEVELOPMENT || UNITY_EDITOR
            return true;
            #else
            return false;
            #endif
        }

        [Conditional("HOMA_STORE")]
        private void RevenueCatSetup()
        {
#if HOMA_STORE
            if (HomaStore.Initialized)
            {
                SetAdjustId();
            }
            else
            {
                HomaStore.OnInitialized += delegate
                {
                    SetAdjustId();
                };
            }
#endif
        }

        [Conditional("HOMA_STORE")]
        private void SetAdjustId()
        {
#if HOMA_STORE
            var purchases = GameObject.FindObjectOfType<Purchases>(); 
            if (purchases != null)
            {
                purchases.SetAdjustID(Adjust.getAdid());
            }
            else
            {
                HomaGamesLog.Warning($"[WARNING] Can't set Adjust ID because Purchases wasn't found.");
            }
#endif
        }
        
        #region Adjust Callbacks
        
        public void EventSuccessCallback(AdjustEventSuccess eventSuccessData)
        {
            if (!IsDevelopmentBuild())
            {
                return;
            }
            
            HomaGamesLog.Debug("[Adjust] Event tracked successfully!");

            if (eventSuccessData.Message != null)
            {
                HomaGamesLog.Debug("[Adjust] Message: " + eventSuccessData.Message);
            }
            if (eventSuccessData.Timestamp != null)
            {
                HomaGamesLog.Debug("[Adjust] Timestamp: " + eventSuccessData.Timestamp);
            }
            if (eventSuccessData.Adid != null)
            {
                HomaGamesLog.Debug("[Adjust] Adid: " + eventSuccessData.Adid);
            }
            if (eventSuccessData.EventToken != null)
            {
                HomaGamesLog.Debug("[Adjust] EventToken: " + eventSuccessData.EventToken);
            }
            if (eventSuccessData.CallbackId != null)
            {
                HomaGamesLog.Debug("[Adjust] CallbackId: " + eventSuccessData.CallbackId);
            }
            if (eventSuccessData.JsonResponse != null)
            {
                HomaGamesLog.Debug("[Adjust] JsonResponse: " + eventSuccessData.GetJsonResponse());
            }
        }

        public void EventFailureCallback(AdjustEventFailure eventFailureData)
        {
            if (!IsDevelopmentBuild())
            {
                return;
            }
            
            HomaGamesLog.Debug("[Adjust] Event tracking failed!");

            if (eventFailureData.Message != null)
            {
                HomaGamesLog.Debug("[Adjust] Message: " + eventFailureData.Message);
            }
            if (eventFailureData.Timestamp != null)
            {
                HomaGamesLog.Debug("[Adjust] Timestamp: " + eventFailureData.Timestamp);
            }
            if (eventFailureData.Adid != null)
            {
                HomaGamesLog.Debug("[Adjust] Adid: " + eventFailureData.Adid);
            }
            if (eventFailureData.EventToken != null)
            {
                HomaGamesLog.Debug("[Adjust] EventToken: " + eventFailureData.EventToken);
            }
            if (eventFailureData.CallbackId != null)
            {
                HomaGamesLog.Debug("[Adjust] CallbackId: " + eventFailureData.CallbackId);
            }
            if (eventFailureData.JsonResponse != null)
            {
                HomaGamesLog.Debug("[Adjust] JsonResponse: " + eventFailureData.GetJsonResponse());
            }

            HomaGamesLog.Debug("[Adjust] WillRetry: " + eventFailureData.WillRetry.ToString());
        }
        
        public void SessionSuccessCallback(AdjustSessionSuccess sessionSuccessData)
        {
            HomaGamesLog.Debug("[Adjust] Session tracked successfully!");
            
            if (!IsDevelopmentBuild())
            {
                return;
            }

            if (sessionSuccessData.Message != null)
            {
                HomaGamesLog.Debug("[Adjust] Message: " + sessionSuccessData.Message);
            }
            if (sessionSuccessData.Timestamp != null)
            {
                HomaGamesLog.Debug("[Adjust] Timestamp: " + sessionSuccessData.Timestamp);
            }
            if (sessionSuccessData.Adid != null)
            {
                HomaGamesLog.Debug("[Adjust] Adid: " + sessionSuccessData.Adid);
            }
            if (sessionSuccessData.JsonResponse != null)
            {
                HomaGamesLog.Debug("[Adjust] JsonResponse: " + sessionSuccessData.GetJsonResponse());
            }
        }

        public void SessionFailureCallback(AdjustSessionFailure sessionFailureData)
        {
            HomaGamesLog.Error("[Adjust] Session tracking failed!");
            
            if (!IsDevelopmentBuild())
            {
                return;
            }

            if (sessionFailureData.Message != null)
            {
                HomaGamesLog.Debug("[Adjust] Message: " + sessionFailureData.Message);
            }
            if (sessionFailureData.Timestamp != null)
            {
                HomaGamesLog.Debug("[Adjust] Timestamp: " + sessionFailureData.Timestamp);
            }
            if (sessionFailureData.Adid != null)
            {
                HomaGamesLog.Debug("[Adjust] Adid: " + sessionFailureData.Adid);
            }
            if (sessionFailureData.JsonResponse != null)
            {
                HomaGamesLog.Debug("[Adjust] JsonResponse: " + sessionFailureData.GetJsonResponse());
            }

            HomaGamesLog.Debug("[Adjust] WillRetry: " + sessionFailureData.WillRetry.ToString());
        }
        
        public void AttributionChangedCallback(AdjustAttribution attributionData)
        {
            HomaGamesLog.Debug("[Adjust] Attribution changed!");
            
            if (!IsDevelopmentBuild())
            {
                return;
            }

            if (attributionData.trackerName != null)
            {
                HomaGamesLog.Debug("[Adjust] Tracker name: " + attributionData.trackerName);
            }
            if (attributionData.trackerToken != null)
            {
                HomaGamesLog.Debug("[Adjust] Tracker token: " + attributionData.trackerToken);
            }
            if (attributionData.network != null)
            {
                HomaGamesLog.Debug("[Adjust] Network: " + attributionData.network);
            }
            if (attributionData.campaign != null)
            {
                HomaGamesLog.Debug("[Adjust] Campaign: " + attributionData.campaign);
            }
            if (attributionData.adgroup != null)
            {
                HomaGamesLog.Debug("[Adjust] Adgroup: " + attributionData.adgroup);
            }
            if (attributionData.creative != null)
            {
                HomaGamesLog.Debug("[Adjust] Creative: " + attributionData.creative);
            }
            if (attributionData.clickLabel != null)
            {
                HomaGamesLog.Debug("[Adjust] Click label: " + attributionData.clickLabel);
            }
            if (attributionData.adid != null)
            {
                HomaGamesLog.Debug("[Adjust] ADID: " + attributionData.adid);
            }
        }
        
        #endregion
        
        #region IAttribution

        public void OnApplicationPause(bool pause)
        {
            // Adjust.cs already has OnApplicationPause logic
        }

        public void ValidateIntegration()
        {
            
        }

        public void SetUserIsAboveRequiredAge(bool consent)
        {
            
        }

        public void SetTermsAndConditionsAcceptance(bool consent)
        {
            
        }

        public void SetAnalyticsTrackingConsentGranted(bool consent)
        {
            // I am not sure if setEnabled is enough, but I think it is more secure to also
            // disabling Adjust sharing user data with third party partners
            var adjustThirdPartySharing = new AdjustThirdPartySharing(consent);
            Adjust.trackThirdPartySharing(adjustThirdPartySharing);
            
            // Adjust remembers the state after setEnabled is called
            // https://github.com/adjust/unity_sdk#disable-tracking
            Adjust.setEnabled(consent);
        }

        public void SetTailoredAdsConsentGranted(bool consent)
        {
            
        }

#if UNITY_PURCHASING
        public void TrackInAppPurchaseEvent(UnityEngine.Purchasing.Product product, bool isRestored = false)
        {
            TrackInAppPurchaseEvent(product.definition.id, 
                product.metadata.isoCurrencyCode,
                (double)product.metadata.localizedPrice, 
                product.transactionID);
        }
#endif
        
        public void TrackInAppPurchaseEvent(string productId, string currencyCode, double unitPrice, string transactionId = null,
            string payload = null, bool isRestored = false)
        {
            // https://github.com/adjust/unity_sdk#track-revenue
            // Note: Instead of tracking a token, we track with a fixed event name.
            // we are doing this because Adjust supports using event names if you create the app using the automation API
            // In case you need to test this feature with a manually created app, use an event token instead of an event name

            var adjustEvent = new AdjustEvent(HomaBellyAdjustConstants.IN_APP_PURCHASE_EVENT_NAME);
            adjustEvent.setRevenue(unitPrice,currencyCode);
            if (!string.IsNullOrEmpty(transactionId))
            {
                adjustEvent.setTransactionId(transactionId);
            }

            if (!string.IsNullOrEmpty(productId))
            {
                adjustEvent.addCallbackParameter("product_id",productId);
            }
            
            Adjust.trackEvent(adjustEvent);
        }

        public void TrackAdRevenue(AdRevenueData adRevenueData)
        {
            // https://github.com/adjust/unity_sdk#ad-revenue-tracking
            string lowerAdPlatform = "";
            if (adRevenueData.AdPlatform != null)
            {
                lowerAdPlatform = adRevenueData.AdPlatform.ToLower();
            }
            
            string adSource;
            if (lowerAdPlatform.Contains("applovin"))
            {
                adSource = AdjustConfig.AdjustAdRevenueSourceAppLovinMAX;
            }
            else if (lowerAdPlatform.Contains("mopub"))
            {
                adSource = AdjustConfig.AdjustAdRevenueSourceMopub;
            }
            else if (lowerAdPlatform.Contains("admob"))
            {
                adSource = AdjustConfig.AdjustAdRevenueSourceAdMob;
            }
            else if (lowerAdPlatform.Contains("ironsource"))
            {
                adSource = AdjustConfig.AdjustAdRevenueSourceIronSource;
            }
            else if (lowerAdPlatform.Contains("admost"))
            {
                adSource = AdjustConfig.AdjustAdRevenueSourceAdmost;
            }
            else if (lowerAdPlatform.Contains("unity"))
            {
                adSource = AdjustConfig.AdjustAdRevenueSourceUnity;
            }
            else if (lowerAdPlatform.Contains("chartboost"))
            {
                adSource = AdjustConfig.AdjustAdRevenueSourceHeliumChartboost;
            }
            else
            {
                adSource = adRevenueData.AdPlatform;
                HomaGamesLog.Warning($"[WARNING] Adjust unrecognized ad source for ad platform: {adRevenueData.AdPlatform}");
            }
            
            var adjustAdRevenue = new AdjustAdRevenue(adSource);
            adjustAdRevenue.setRevenue(adRevenueData.Revenue,adRevenueData.Currency);

            if (!string.IsNullOrEmpty(adRevenueData.NetworkName))
            {
                adjustAdRevenue.setAdRevenueNetwork(adRevenueData.NetworkName);
            }

            if (!string.IsNullOrEmpty(adRevenueData.AdUnitId))
            {
                adjustAdRevenue.setAdRevenueUnit(adRevenueData.AdUnitId);
            }
            
            if (!string.IsNullOrEmpty(adRevenueData.PlacementId))
            {
                adjustAdRevenue.setAdRevenuePlacement(adRevenueData.PlacementId);
            }
            
            if (IsDevelopmentBuild())
            {
                HomaGamesLog.Debug("[Adjust] TrackAdRevenue, source: " + adjustAdRevenue.source + " revenue: " + adjustAdRevenue.revenue + " currency: " + adjustAdRevenue.currency + " adImpressionsCount: " + adjustAdRevenue.adImpressionsCount + " adRevenueNetwork: " + adjustAdRevenue.adRevenueNetwork + " adRevenuePlacement: " + adjustAdRevenue.adRevenuePlacement+ " adRevenueUnit: "+adjustAdRevenue.adRevenueUnit);
            }

            Adjust.trackAdRevenue(adjustAdRevenue);
        }

        
        public void TrackEvent(string eventName, Dictionary<string, object> arguments = null)
        {
            // Although Adjust works with event tokens instead of event names, 
            // we can send the event name and then it will be bound on the server side with an event token.
            var adjustEvent = new AdjustEvent(eventName);
            if (arguments != null)
            {
                foreach (KeyValuePair<string,object> keyValuePair in arguments)
                {
                    adjustEvent.addCallbackParameter(keyValuePair.Key,keyValuePair.Value.ToString());
                }
            }

            if (IsDevelopmentBuild())
            {
                HomaGamesLog.Debug("[Adjust] TrackEvent, eventName: " + eventName);
            }

            Adjust.trackEvent(adjustEvent);
        }
        #endregion
    }
}
