using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Facebook.Unity;
using Facebook.Unity.Settings;
using UnityEngine;
using UnityEngine.Events;

namespace HomaGames.HomaBelly
{
    public class FacebookImplementation : IAnalyticsWithInitializationCallback
    {
        private Stack<UnityAction> delayedActionsUntilInitialization = new Stack<UnityAction>();

        #region Public methods
        public void Initialize()
        {
            Initialize(null);
        }

        public void Initialize(Action onInitialized = null)
        {
            FB.Init(() =>
            {
                FB.ActivateApp();
                if (delayedActionsUntilInitialization != null)
                {
                    HomaGamesLog.Debug($"FB Initialized. Invoking delayed actions");
                    while (delayedActionsUntilInitialization.Count > 0)
                    {
                        UnityAction delayedAction = delayedActionsUntilInitialization.Pop();
                        if (delayedAction != null)
                        {
                            HomaGamesLog.Debug($"Invoking delayed action {delayedAction.Method.Name}");
                            delayedAction.Invoke();
                        }
                    }
                }

                // Invoke initialization callback
                if (onInitialized != null)
                {
                    onInitialized.Invoke();
                }
            });
        }

        public void OnApplicationPause(bool pause)
        {
            if (!pause)
            {
                // App Resumed
                if (FB.IsInitialized)
                {
                    FB.ActivateApp();
                }
                else
                {
                    Initialize(null);
                }
            }
        }

        public void SetUserIsAboveRequiredAge(bool consent)
        {
            // NO-OP
        }

        public void SetTermsAndConditionsAcceptance(bool consent)
        {
            // NO-OP
        }

        public void SetAnalyticsTrackingConsentGranted(bool consent)
        {
            if (FB.IsInitialized)
            {
                FB.Mobile.SetAutoLogAppEventsEnabled(consent);
            }
            else
            {
                HomaGamesLog.Debug($"Delaying analytics tracking consent notification...");
                delayedActionsUntilInitialization.Push(() =>
                {
                    SetAnalyticsTrackingConsentGranted(consent);
                });
            }            
        }

        public void SetTailoredAdsConsentGranted(bool consent)
        {
            if (FB.IsInitialized)
            {
                FB.Mobile.SetAdvertiserTrackingEnabled(consent);
                FB.Mobile.SetAdvertiserIDCollectionEnabled(consent);
            }
            else
            {
                HomaGamesLog.Debug($"Delaying tailored ads consent notification...");
                delayedActionsUntilInitialization.Push(() =>
                {
                    SetTailoredAdsConsentGranted(consent);
                });
            }
        }

        public void ValidateIntegration()
        {
#if UNITY_EDITOR
            string facebookSettingsPath = Application.dataPath + "/FacebookSDK/SDK/Resources/FacebookSettings.asset";
            if (File.Exists(facebookSettingsPath))
            {
#endif
                if (FacebookSettings.AppIds != null)
                {
                    if (FacebookSettings.AppIds.Count > 0)
                    {
                        HomaGamesLog.Debug($"[Validate Integration] Facebook successfully integrated");
                    }
                }
                else
                {
                    HomaGamesLog.Warning($"[Validate Integration] Wrong configuration. Facebook APP_IDs not found");
                }
#if UNITY_EDITOR
            }
            else
            {
                HomaGamesLog.Warning($"[Validate Integration] Facebook Settings not found. Please see {facebookSettingsPath}");
            }
#endif
        }

#if UNITY_PURCHASING
        public void TrackInAppPurchaseEvent(UnityEngine.Purchasing.Product product, bool isRestored = false)
        {
            TrackInAppPurchaseEvent(product.definition.id, product.metadata.isoCurrencyCode, Convert.ToDouble(product.metadata.localizedPrice, CultureInfo.InvariantCulture), product.transactionID, product.receipt, isRestored);
        }
#endif

        public void TrackInAppPurchaseEvent(string productId, string currencyCode, double unitPrice, string transactionId = null, string payload = null, bool isRestored = false)
        {
            if (FB.IsInitialized)
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                AddParameterToDictionary(parameters, AppEventParameterName.NumItems, 1);
                AddParameterToDictionary(parameters, AppEventParameterName.ContentID, productId);
                FB.LogPurchase(Convert.ToDecimal(isRestored ? 0 : unitPrice), currencyCode, parameters);
            }
            else
            {
                HomaGamesLog.Debug($"Delaying TrackInAppPurchaseEvent...");
                delayedActionsUntilInitialization.Push(() =>
                {
                    TrackInAppPurchaseEvent(productId, currencyCode, unitPrice, transactionId, payload, isRestored);
                });
            }
        }

        public void TrackResourceEvent(ResourceFlowType flowType, string currency, float amount, string itemType, string itemId)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            AddParameterToDictionary(parameters, AppEventParameterName.NumItems, amount);
            AddParameterToDictionary(parameters, AppEventParameterName.Description, currency);
            AddParameterToDictionary(parameters, AppEventParameterName.ContentID, itemId);
            AddParameterToDictionary(parameters, AppEventParameterName.ContentType, itemType);
            LogAppEvent($"Resource_{flowType.ToString()}", amount, parameters);
        }

        public void TrackProgressionEvent(ProgressionStatus progressionStatus, string progression01, int score = 0)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            AddParameterToDictionary(parameters, AppEventParameterName.ContentType, progressionStatus.ToString());
            AddParameterToDictionary(parameters, AppEventParameterName.ContentID, progression01);
            LogAppEvent("Progression", score, parameters);
        }

        public void TrackProgressionEvent(ProgressionStatus progressionStatus, string progression01, string progression02, int score = 0)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            AddParameterToDictionary(parameters, AppEventParameterName.ContentType, progressionStatus.ToString());
            AddParameterToDictionary(parameters, AppEventParameterName.ContentID, progression01 + "_" + progression02);
            LogAppEvent("Progression", score, parameters);
        }

        public void TrackProgressionEvent(ProgressionStatus progressionStatus, string progression01, string progression02, string progression03, int score = 0)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            AddParameterToDictionary(parameters, AppEventParameterName.ContentType, progressionStatus.ToString());
            AddParameterToDictionary(parameters, AppEventParameterName.ContentID, progression01 + "_" + progression02 + "_" + progression03);
            LogAppEvent("Progression", score, parameters);
        }

        public void TrackErrorEvent(ErrorSeverity severity, string message)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            AddParameterToDictionary(parameters, AppEventParameterName.ContentType, severity.ToString());
            AddParameterToDictionary(parameters, AppEventParameterName.Description, message);
            LogAppEvent("ERROR", 0, parameters);
        }

        public void TrackDesignEvent(string eventName, float eventValue = 0f)
        {
            LogAppEvent(eventName, eventValue);
        }

        public void TrackAdEvent(AdAction adAction, AdType adType, string adNetwork, string adPlacementId)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            AddParameterToDictionary(parameters, AppEventParameterName.ContentID, adAction.ToString());
            AddParameterToDictionary(parameters, AppEventParameterName.ContentType, adType.ToString());
            AddParameterToDictionary(parameters, AppEventParameterName.Description, adPlacementId);
            AddParameterToDictionary(parameters, AppEventParameterName.RegistrationMethod, adNetwork);
            LogAppEvent("AdEvent", 0, parameters);
        }

        #endregion

        #region Privave Helpers

        /// <summary>
        /// Try to log an AppEvent if FB SDK is initialized. If not, tracking
        /// is delayed and triggered upon initialization
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="eventValue"></param>
        /// <param name="parameters"></param>
        private void LogAppEvent(string eventName, float eventValue, Dictionary<string, object> parameters = null)
        {
            Sanitize(ref eventName);
            if (FB.IsInitialized)
            {
                FB.LogAppEvent(eventName, eventValue, parameters);
            }
            else
            {
                HomaGamesLog.Debug($"Delaying analytics event...");
                delayedActionsUntilInitialization.Push(() =>
                {
                    FB.LogAppEvent(eventName, eventValue, parameters);
                });
            }
        }

        private void AddParameterToDictionary(Dictionary<string, object> dict, string key, string value)
        {
            if (dict != null && !string.IsNullOrEmpty(value))
            {
                dict[key] = value;
            }
        }

        private void AddParameterToDictionary(Dictionary<string, object> dict, string key, float value)
        {
            if (dict != null)
            {
                dict[key] = value;
            }
        }

        private void AddParameterToDictionary(Dictionary<string, object> dict, string key, int value)
        {
            if (dict != null)
            {
                dict[key] = value;
            }
        }
        
        private void Sanitize(ref string str)
        {
            str = str.Replace(':','_');
        }

        #endregion
    }
}

