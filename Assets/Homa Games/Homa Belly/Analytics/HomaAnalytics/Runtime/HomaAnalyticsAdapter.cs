using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using HomaGames.HomaBelly.Utilities;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Adapter for IAnalytics interface
    /// </summary>
    public class HomaAnalyticsAdapter : IAnalyticsWithInitializationCallback, IAnalyticsAdRevenue
    {
        private const string ANALYTICS_CONSENT_GRANTED_KEY = "analytics_consent";
        private const string END_POINT_FORMAT = "{0}/appevent";
        private const float SECONDS_FOR_NEW_SESSION = 600f;
        private HomaAnalytics m_homaAnalytics = null;
         
        #region IAnalyticsWithInitializationCallback
        
        public void Initialize()
        {
            Initialize(null);
        }
        
        public void Initialize(Action onInitialized = null)
        {
            var validateEvents = false;
            bool useCsvTool = false;
            #if HOMA_DEVELOPMENT
                validateEvents = true;
                useCsvTool = true;
            #endif
            string url = HomaBellyConstants.API_HOST;
            string endPoint = string.Format(END_POINT_FORMAT, url);
            
            
            // Gather token and manifest version id
            HomaBellyManifestConfiguration.TryGetString(out var tokenIdentifier,
                HomaBellyManifestConfiguration.MANIFEST_TOKEN_KEY);
            HomaBellyManifestConfiguration.TryGetString(out var manifestVersionId,
                HomaBellyManifestConfiguration.MANIFEST_VERSION_ID_KEY);
            
            // We create a temporal options object so we can start storing 
            // event prior to HomaAnalytics initialization
            var temporalHomaAnalyticsOptions = new HomaAnalyticsOptions(false, 
                validateEvents,
                endPoint,
                SECONDS_FOR_NEW_SESSION,
                tokenIdentifier,
                manifestVersionId,
                true,
                useCsvTool,
                "NTestingIdNotSet",
                "NTestingOverrideIdNotSet");
            
            m_homaAnalytics = new HomaAnalytics(temporalHomaAnalyticsOptions);

            Geryon.Config.OnInitialized += delegate
            {
                // Gather token and manifest version id
                HomaBellyManifestConfiguration.TryGetString(out var tokenIdentifier,
                    HomaBellyManifestConfiguration.MANIFEST_TOKEN_KEY);
                HomaBellyManifestConfiguration.TryGetString(out var manifestVersionId,
                    HomaBellyManifestConfiguration.MANIFEST_VERSION_ID_KEY);

                // Override options with token identifier and manifest version id
                var finalHomaAnalyticsOptions = new HomaAnalyticsOptions(false,
                    validateEvents,
                    endPoint,
                    SECONDS_FOR_NEW_SESSION,
                    tokenIdentifier,
                    manifestVersionId,
                    true,
                    useCsvTool,
                    Geryon.Config.NTESTING_ID,
                    Geryon.Config.OverrideId);

                m_homaAnalytics.Initialize(finalHomaAnalyticsOptions);

                if (PlayerPrefs.HasKey(ANALYTICS_CONSENT_GRANTED_KEY))
                {
                    bool analyticsConsentGranted = PlayerPrefs.GetInt(ANALYTICS_CONSENT_GRANTED_KEY) == 1;
                    m_homaAnalytics.ToggleAnalytics(analyticsConsentGranted);
                }

                onInitialized?.Invoke();
            };
        }

        public void OnApplicationPause(bool pause)
        {
            // NO-OP. Unity lifecycle callbacks are handled in HomaAnalytics automatically
        }

        public void ValidateIntegration()
        {
            // Do nothing
        }

        public void SetUserIsAboveRequiredAge(bool consent)
        {
            // ¿?
        }

        public void SetTermsAndConditionsAcceptance(bool consent)
        {
            // ¿?
        }

        public void SetAnalyticsTrackingConsentGranted(bool consent)
        {
            m_homaAnalytics.ToggleAnalytics(consent);
            
            PlayerPrefs.SetInt(ANALYTICS_CONSENT_GRANTED_KEY,consent ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void SetTailoredAdsConsentGranted(bool consent)
        {
            // ¿?
        }
        
#if UNITY_PURCHASING
        public void TrackInAppPurchaseEvent(UnityEngine.Purchasing.Product product, bool isRestored = false)
        {
            TrackInAppPurchaseEvent(product.definition.id, 
                product.metadata.isoCurrencyCode,
                Convert.ToDouble(product.metadata.localizedPrice, CultureInfo.InvariantCulture),
                product.transactionID,
                product.receipt,
                isRestored);
        }
#endif

        public void TrackInAppPurchaseEvent(string productId,
            string currencyCode,
            double unitPrice,
            string transactionId = null,
            string payload = null, bool isRestored = false)
        {
            int unitPriceInCents = isRestored ? 0 : (int)(unitPrice * 100);

            string googlePlaySignature = null;
            
#if UNITY_ANDROID
            googlePlaySignature = ExtractGooglePlaySignature(payload);
#endif

            var inAppPurchaseAnalyticsEvent = new InAppPurchaseAnalyticsEvent(
                currencyCode,
                unitPriceInCents,
                productId,
                transactionId,
                googlePlaySignature);

            m_homaAnalytics.TrackEvent(inAppPurchaseAnalyticsEvent);
        }

        public void TrackResourceEvent(ResourceFlowType flowType, string currency, float amount, string itemType, string itemId)
        {
            var resourcesEvent = new ResourceAnalyticsEvent(flowType, currency, amount, itemType, itemId);
            
            m_homaAnalytics.TrackEvent(resourcesEvent);
        }

        public void TrackProgressionEvent(ProgressionStatus progressionStatus, string progression01, int score = 0)
        {
            var progressionEvent = new ProgressionAnalyticsEvent(progressionStatus, progression01, score);
            
            m_homaAnalytics.TrackEvent(progressionEvent);
        }

        public void TrackProgressionEvent(ProgressionStatus progressionStatus, string progression01, string progression02,
            int score = 0)
        {
            var progressionEvent = new ProgressionAnalyticsEvent(progressionStatus, progression01,progression02, score);
            
            m_homaAnalytics.TrackEvent(progressionEvent);
        }

        public void TrackProgressionEvent(ProgressionStatus progressionStatus, string progression01, string progression02,
            string progression03, int score = 0)
        {
            var progressionEvent = new ProgressionAnalyticsEvent(progressionStatus,progression01,progression02,progression03,score);
            
            m_homaAnalytics.TrackEvent(progressionEvent);
        }

        public void TrackErrorEvent(ErrorSeverity severity, string message)
        {
            var errorEvent = new ErrorAnalyticsEvent(severity, message);
            
            m_homaAnalytics.TrackEvent(errorEvent);
        }

        /// <summary>
        /// Track an event using the format from GameAnalytics [category]:[sub_category]:[outcome]
        /// We will convert it to our HomaAnalytics json format.
        /// https://gameanalytics.com/docs/s/article/Designing-Custom-Events-Best-Practice
        /// </summary>
        public void TrackDesignEvent(string gameAnalyticsHierarchyName, float eventValue = 0)
        {
            if (string.IsNullOrWhiteSpace(gameAnalyticsHierarchyName))
            {
                Debug.LogError("[ERROR] Can't use null or empty hierarchy name");
                return;
            }

            var eventName = ConvertFromGameAnalyticsFormatToDictionary(gameAnalyticsHierarchyName, eventValue, out var values);

            TrackDesignEvent(eventName,values);
        }

        public void TrackDesignEvent(string eventName, Dictionary<string,object> values)
        {
            var designEvent = new RuntimeAnalyticsEvent(eventName, HomaAnalytics.DESIGN_CATEGORY, values);
            
            m_homaAnalytics.TrackEvent(designEvent);
        }

        public void TrackAdEvent(AdAction adAction, AdType adType, string adNetwork, string adPlacementId)
        {
            var adEvent = new AdAnalyticsEvent(adAction, adType, adNetwork, adPlacementId);
            
            m_homaAnalytics.TrackEvent(adEvent);
        }
        
        #endregion

        #region IAnalyticsRevenue
        public void TrackAdRevenue(AdRevenueData adRevenueData)
        {
            var adRevenueEvent = new AdRevenueAnalyticsEvent(adRevenueData);
            
            m_homaAnalytics.TrackEvent(adRevenueEvent);
        }
        
        #endregion

        #region Utils
        public static string ConvertFromGameAnalyticsFormatToDictionary(string gameAnalyticsHierarchyName, float eventValue,
            out Dictionary<string, object> values)
        {
            // https://gameanalytics.com/docs/s/article/Designing-Custom-Events-Best-Practice
            // [category]:[sub_category]:[outcome]
            // Tutorial:Step01:Start
            // Tutorial:Step01:Complete
            // newUserTutorial:namedCharacter:complete
            // guiClick:volume:on
            // ButtonClickedEvent:{buttonAnalyticsId}:{parentWindowId}
            // WindowOpenedEvent:{windowId}:{source}:{location}

            string[] hierarchyParts = gameAnalyticsHierarchyName.Split(':');
            string eventName = hierarchyParts.Length == 0 ? gameAnalyticsHierarchyName : hierarchyParts[0];

            values = new Dictionary<string, object> {
                // Store the complete event name in GA format
                {"ga_raw", gameAnalyticsHierarchyName}
            };

            // Split each level of the hierarchy in a different value
            for (int i = 1; i < hierarchyParts.Length; i++)
            {
                values.Add($"ga_hierarchy_{i}",hierarchyParts[i]);
            }
            
            // Add the float value
            if (eventValue != 0f)
            {
                values.Add("ga_float", eventValue.ToString(CultureInfo.InvariantCulture));
            }

            return eventName;
        }
        
        private static string ExtractGooglePlaySignature(string payload)
        {
            var googlePlaySignature = "";
            if (payload != null)
            {
                if (Json.Deserialize(payload) is Dictionary<string, object> receiptDictionary 
                    && receiptDictionary.TryGetValue("Payload",out object payloadObject))
                {
                    var googlePlayPayloadString = payloadObject as string;
                    if (Json.Deserialize(googlePlayPayloadString) is Dictionary<string, object> googlePlayPayloadDictionary)
                    {
                        if (googlePlayPayloadDictionary.TryGetValue("signature",out var signatureObject))
                        {
                            googlePlaySignature = signatureObject as string;
                        }
                    }
                }
            }

            return googlePlaySignature;
        }
        
        #endregion
    }
}