using System;
using System.Collections.Generic;
using HomaGames.HomaBelly;
using UnityEngine;

namespace HomaGames.HomaBelly.DataPrivacy
{
    /// <summary>
    /// This class allows delaying Homa Belly initialization
    /// after iOS 14.5+ IDFA decision (if applied)
    /// </summary>
    public static class InitializationHelper
    {
        private static readonly Queue<string> analyticsDesignEventsToTrack = new Queue<string>();
        private static readonly Queue<string> attributionEventsToTrack = new Queue<string>();
        
        [RuntimeInitializeOnLoadMethod]
        private static void RegisterHomaBellyEvent()
        {
            Events.onInitialized += OnHomaBellyInitialized;
        }

        /// <summary>
        /// Track a design event. If iOS 14.5+, will be queued until
        /// the users makes an IDFA decision. Othwerise will trigger the event.
        /// </summary>
        /// <param name="eventString"></param>
        public static void TrackDesignEvent(string eventString)
        {
            // For iOS 14.5+ enqueue and wait for IDFA decision
            if (Manager.Instance.IsiOS14_5OrHigher)
            {
                analyticsDesignEventsToTrack.Enqueue(eventString);
            }
            else
            {
                HomaBelly.Instance.TrackDesignEvent(eventString);
            }
        }

        /// <summary>
        /// Track an attribution event. If iOS 14.5+, will be queued until
        /// the users makes an IDFA decision. Othwerise will trigger the event.
        /// </summary>
        /// <param name="eventString"></param>
        public static void TrackAttributionEvent(string eventString)
        {
            // For iOS 14.5+ enqueue and wait for IDFA decision
            if (Manager.Instance.IsiOS14_5OrHigher)
            {
                attributionEventsToTrack.Enqueue(eventString);
            }
            else
            {
                HomaBelly.Instance.TrackAttributionEvent(eventString);
            }
        }

        /// <summary>
        /// Callback invoked after Homa Belly initialization
        /// </summary>
        private static void OnHomaBellyInitialized()
        {
            // Upon initialization, inform with data privacy decisions
            HomaBelly.Instance.SetUserIsAboveRequiredAge(Manager.Instance.IsAboveRequiredAge());
            HomaBelly.Instance.SetTermsAndConditionsAcceptance(Manager.Instance.IsTermsAndConditionsAccepted());
            HomaBelly.Instance.SetAnalyticsTrackingConsentGranted(Manager.Instance.IsAnalyticsGranted());
            HomaBelly.Instance.SetTailoredAdsConsentGranted(Manager.Instance.IsTailoredAdsGranted());

#if UNITY_IOS

            // Tracking authorization status upon start for lower iOS 14.5 devices
            if (!Manager.Instance.IsiOS14_5OrHigher)
            {
                switch (AppTrackingTransparency.TrackingAuthorizationStatus)
                {
                    case AppTrackingTransparency.AuthorizationStatus.NOT_DETERMINED:
                        HomaBelly.Instance.TrackDesignEvent("app_start_tracking_not_determined");
                        HomaBelly.Instance.TrackAttributionEvent("app_start_tracking_not_determined");
                        break;
                    case AppTrackingTransparency.AuthorizationStatus.AUTHORIZED:
                        HomaBelly.Instance.TrackDesignEvent("app_start_tracking_allowed");
                        HomaBelly.Instance.TrackAttributionEvent("app_start_tracking_allowed");
                        break;
                    case AppTrackingTransparency.AuthorizationStatus.DENIED:
                        HomaBelly.Instance.TrackDesignEvent("app_start_tracking_denied");
                        HomaBelly.Instance.TrackAttributionEvent("app_start_tracking_denied");
                        break;
                    case AppTrackingTransparency.AuthorizationStatus.RESTRICTED:
                        HomaBelly.Instance.TrackDesignEvent("app_start_tracking_restricted");
                        HomaBelly.Instance.TrackAttributionEvent("app_start_tracking_restricted");
                        break;
                }
            }

#if HOMA_STORE
            try
            {
                // Update user subscription attributes after IDFA accepted
                Purchases purchases = GameObject.FindObjectOfType<Purchases>();
                if (purchases != null)
                {
                    purchases.CollectDeviceIdentifiers();
                }
                else
                {
                    HomaGamesLog.Debug("Could not collect device identifiers for Purchases: Game Object not found");
                }
            }
            catch (Exception e)
            {
                HomaGamesLog.Debug($"Could not collect device identifiers for Purchases: {e.Message}");
            }
#endif
#endif

            // Track 'gdpr_first_accept' as the very first event after initialization
            HomaBelly.Instance.TrackDesignEvent("gdpr_first_accept");
            HomaBelly.Instance.TrackAttributionEvent("gdpr_first_accept");

            if (Manager.Instance.IsiOS14_5OrHigher)
            {
                // Send all the tracking events cached
                TriggerAnalyticDesignEvents();
                TriggerAttributionEvents();
            }

            // Deregister event (just in case)
            Events.onInitialized -= OnHomaBellyInitialized;
        }

        /// <summary>
        /// Triggers al the queued analytic events
        /// </summary>
        private static void TriggerAnalyticDesignEvents()
        {
            if (analyticsDesignEventsToTrack == null || analyticsDesignEventsToTrack.Count == 0)
            {
                return;
            }

            while (analyticsDesignEventsToTrack.Count > 0)
            {
                string eventString = analyticsDesignEventsToTrack.Dequeue();
                HomaBelly.Instance.TrackDesignEvent(eventString);
            }
        }

        /// <summary>
        /// Triggers all the queued attribution events
        /// </summary>
        private static void TriggerAttributionEvents()
        {
            if (attributionEventsToTrack == null || attributionEventsToTrack.Count == 0)
            {
                return;
            }

            while (attributionEventsToTrack.Count > 0)
            {
                string eventString = attributionEventsToTrack.Dequeue();
                HomaBelly.Instance.TrackAttributionEvent(eventString);
            }
        }
    }
}