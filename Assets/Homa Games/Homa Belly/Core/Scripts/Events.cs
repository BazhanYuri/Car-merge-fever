using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public class Events
    {
        private static event Action _onInitialized;
        /// <summary>
        /// Invoked when Homa Belly has been fully initialized
        /// </summary>
        public static event Action onInitialized
        {
            add
            {
                if (_onInitialized == null || !_onInitialized.GetInvocationList().Contains(value))
                {
                    _onInitialized += value;
                }
            }

            remove
            {
                if (_onInitialized.GetInvocationList().Contains(value))
                {
                    _onInitialized -= value;
                }
            }
        }
        public void OnInitialized()
        {
            if (_onInitialized != null)
            {
                _onInitialized();
            }
        }

        #region Rewarded Video Ad Events
        private static event Action<AdInfo> _onRewardedVideoAdClosedEvent;
        /// <summary>
        /// Invoked when the RewardedVideo ad view is about to be closed.
        /// <typeparam name="AdInfo">See <see cref="AdInfo"/></typeparam>
        /// </summary>
        public static event Action<AdInfo> onRewardedVideoAdClosedEvent
        {
            add
            {
                if (_onRewardedVideoAdClosedEvent == null || !_onRewardedVideoAdClosedEvent.GetInvocationList().Contains(value))
                {
                    _onRewardedVideoAdClosedEvent += value;
                }
            }

            remove
            {
                if (_onRewardedVideoAdClosedEvent.GetInvocationList().Contains(value))
                {
                    _onRewardedVideoAdClosedEvent -= value;
                }
            }
        }
        public void OnRewardedVideoAdClosedEvent(AdInfo adInfo)
        {
            if (_onRewardedVideoAdClosedEvent != null)
            {
                _onRewardedVideoAdClosedEvent(adInfo);
            }
        }

        private static event Action<bool,AdInfo> _onRewardedVideoAvailabilityChangedEvent;
        /// <summary>
        /// Invoked when there is a change in the ad availability status.
        /// <typeparam name="bool">If the video is available</typeparam>
        /// <typeparam name="AdInfo">See <see cref="AdInfo"/></typeparam>
        /// </summary>
        public static event Action<bool,AdInfo> onRewardedVideoAvailabilityChangedEvent
        {
            add
            {
                if (_onRewardedVideoAvailabilityChangedEvent == null || !_onRewardedVideoAvailabilityChangedEvent.GetInvocationList().Contains(value))
                {
                    _onRewardedVideoAvailabilityChangedEvent += value;
                }
            }

            remove
            {
                if (_onRewardedVideoAvailabilityChangedEvent.GetInvocationList().Contains(value))
                {
                    _onRewardedVideoAvailabilityChangedEvent -= value;
                }
            }
        }
        public void OnRewardedVideoAvailabilityChangedEvent(bool available, AdInfo adInfo)
        {
            if (_onRewardedVideoAvailabilityChangedEvent != null)
            {
                _onRewardedVideoAvailabilityChangedEvent(available,adInfo);
            }
        }

        private static event Action<AdInfo> _onRewardedVideoAdStartedEvent;
        /// <summary>
        /// Invoked when the video ad has opened. 
        /// <typeparam name="AdInfo">See <see cref="AdInfo"/></typeparam>
        /// </summary>
        public static event Action<AdInfo> onRewardedVideoAdStartedEvent
        {
            add
            {
                if (_onRewardedVideoAdStartedEvent == null || !_onRewardedVideoAdStartedEvent.GetInvocationList().Contains(value))
                {
                    _onRewardedVideoAdStartedEvent += value;
                }
            }

            remove
            {
                if (_onRewardedVideoAdStartedEvent.GetInvocationList().Contains(value))
                {
                    _onRewardedVideoAdStartedEvent -= value;
                }
            }
        }
        public void OnRewardedVideoAdStartedEvent(AdInfo adInfo)
        {
            if (_onRewardedVideoAdStartedEvent != null)
            {
                _onRewardedVideoAdStartedEvent(adInfo);
            }
        }

        private static event Action<VideoAdReward,AdInfo> _onRewardedVideoAdRewardedEvent;
        /// <summary>
        /// Invoked when the user completed the video and should be rewarded.
        /// <typeparam name="VideoAdReward">See <see cref="VideoAdReward"/></typeparam>
        /// <typeparam name="AdInfo">See <see cref="AdInfo"/></typeparam>
        /// </summary>
        public static event Action<VideoAdReward,AdInfo> onRewardedVideoAdRewardedEvent
        {
            add
            {
                if (_onRewardedVideoAdRewardedEvent == null || !_onRewardedVideoAdRewardedEvent.GetInvocationList().Contains(value))
                {
                    _onRewardedVideoAdRewardedEvent += value;
                }
            }

            remove
            {
                if (_onRewardedVideoAdRewardedEvent.GetInvocationList().Contains(value))
                {
                    _onRewardedVideoAdRewardedEvent -= value;
                }
            }
        }
        public void OnRewardedVideoAdRewardedEvent(VideoAdReward videoAdReward, AdInfo adInfo)
        {
            if (_onRewardedVideoAdRewardedEvent != null)
            {
                _onRewardedVideoAdRewardedEvent(videoAdReward,adInfo);
            }
        }

        private static event Action<AdInfo> _onRewardedVideoAdShowFailedEvent;
        /// <summary>
        /// Invoked when the Rewarded Video failed to show
        /// <typeparam name="AdInfo">See <see cref="AdInfo"/></typeparam>
        /// </summary>
        public static event Action<AdInfo> onRewardedVideoAdShowFailedEvent
        {
            add
            {
                if (_onRewardedVideoAdShowFailedEvent == null || !_onRewardedVideoAdShowFailedEvent.GetInvocationList().Contains(value))
                {
                    _onRewardedVideoAdShowFailedEvent += value;
                }
            }

            remove
            {
                if (_onRewardedVideoAdShowFailedEvent.GetInvocationList().Contains(value))
                {
                    _onRewardedVideoAdShowFailedEvent -= value;
                }
            }
        }
        public void OnRewardedVideoAdShowFailedEvent(AdInfo adInfo)
        {
            if (_onRewardedVideoAdShowFailedEvent != null)
            {
                _onRewardedVideoAdShowFailedEvent(adInfo);
            }
        }

        private static event Action<AdInfo> _onRewardedVideoAdClicked;
        /// <summary>
        /// Invoked when the video ad is clicked.
        /// <typeparam name="AdInfo">See <see cref="AdInfo"/></typeparam>
        /// </summary>
        public static event Action<AdInfo> onRewardedVideoAdClickedEvent
        {
            add
            {
                if (_onRewardedVideoAdClicked == null || !_onRewardedVideoAdClicked.GetInvocationList().Contains(value))
                {
                    _onRewardedVideoAdClicked += value;
                }
            }

            remove
            {
                if (_onRewardedVideoAdClicked.GetInvocationList().Contains(value))
                {
                    _onRewardedVideoAdClicked -= value;
                }
            }
        }
        public void OnRewardedVideoAdClickedEvent(AdInfo adInfo)
        {
            if (_onRewardedVideoAdClicked != null)
            {
                _onRewardedVideoAdClicked(adInfo);
            }
        }
        #endregion

        #region Interstitial Events
        private static event Action<AdInfo> _onInterstitialAdReadyEvent;
        /// <summary>
        /// Invoked when the Interstitial is Ready to shown after load function is called
        /// <typeparam name="AdInfo">See <see cref="AdInfo"/></typeparam>
        /// </summary>
        public static event Action<AdInfo> onInterstitialAdReadyEvent
        {
            add
            {
                if (_onInterstitialAdReadyEvent == null || !_onInterstitialAdReadyEvent.GetInvocationList().Contains(value))
                {
                    _onInterstitialAdReadyEvent += value;
                }
            }

            remove
            {
                if (_onInterstitialAdReadyEvent.GetInvocationList().Contains(value))
                {
                    _onInterstitialAdReadyEvent -= value;
                }
            }
        }
        public void OnInterstitialAdReadyEvent(AdInfo adInfo)
        {
            if (_onInterstitialAdReadyEvent != null)
            {
                _onInterstitialAdReadyEvent(adInfo);
            }
        }

        private static event Action<AdInfo> _onInterstitialAdLoadFailedEvent;
        /// <summary>
        /// Invoked when the initialization process has failed.
        /// <typeparam name="AdInfo">See <see cref="AdInfo"/></typeparam>
        /// </summary>
        public static event Action<AdInfo> onInterstitialAdLoadFailedEvent
        {
            add
            {
                if (_onInterstitialAdLoadFailedEvent == null || !_onInterstitialAdLoadFailedEvent.GetInvocationList().Contains(value))
                {
                    _onInterstitialAdLoadFailedEvent += value;
                }
            }

            remove
            {
                if (_onInterstitialAdLoadFailedEvent.GetInvocationList().Contains(value))
                {
                    _onInterstitialAdLoadFailedEvent -= value;
                }
            }
        }
        public void OnInterstitialAdLoadFailedEvent(AdInfo adInfo)
        {
            if (_onInterstitialAdLoadFailedEvent != null)
            {
                _onInterstitialAdLoadFailedEvent(adInfo);
            }
        }

        private static event Action<AdInfo> _onInterstitialAdShowSucceededEvent;
        /// <summary>
        /// Invoked right before the Interstitial screen is about to open.
        /// <typeparam name="AdInfo">See <see cref="AdInfo"/></typeparam>
        /// </summary>
        public static event Action<AdInfo> onInterstitialAdShowSucceededEvent
        {
            add
            {
                if (_onInterstitialAdShowSucceededEvent == null || !_onInterstitialAdShowSucceededEvent.GetInvocationList().Contains(value))
                {
                    _onInterstitialAdShowSucceededEvent += value;
                }
            }

            remove
            {
                if (_onInterstitialAdShowSucceededEvent.GetInvocationList().Contains(value))
                {
                    _onInterstitialAdShowSucceededEvent -= value;
                }
            }
        }
        public void OnInterstitialAdShowSucceededEvent(AdInfo adInfo)
        {
            if (_onInterstitialAdShowSucceededEvent != null)
            {
                _onInterstitialAdShowSucceededEvent(adInfo);
            }
        }

        private static event Action<AdInfo> _onInterstitialAdShowFailedEvent;
        /// <summary>
        /// Invoked when the ad fails to show.
        /// <typeparam name="AdInfo">See <see cref="AdInfo"/></typeparam>
        /// </summary>
        public static event Action<AdInfo> onInterstitialAdShowFailedEvent
        {
            add
            {
                if (_onInterstitialAdShowFailedEvent == null || !_onInterstitialAdShowFailedEvent.GetInvocationList().Contains(value))
                {
                    _onInterstitialAdShowFailedEvent += value;
                }
            }

            remove
            {
                if (_onInterstitialAdShowFailedEvent.GetInvocationList().Contains(value))
                {
                    _onInterstitialAdShowFailedEvent -= value;
                }
            }
        }
        public void OnInterstitialAdShowFailedEvent(AdInfo adInfo)
        {
            if (_onInterstitialAdShowFailedEvent != null)
            {
                _onInterstitialAdShowFailedEvent(adInfo);
            }
        }

        private static event Action<AdInfo> _onInterstitialAdClickedEvent;
        /// <summary>
        /// Invoked when end user clicked on the interstitial ad
        /// <typeparam name="AdInfo">See <see cref="AdInfo"/></typeparam>
        /// </summary>
        public static event Action<AdInfo> onInterstitialAdClickedEvent
        {
            add
            {
                if (_onInterstitialAdClickedEvent == null || !_onInterstitialAdClickedEvent.GetInvocationList().Contains(value))
                {
                    _onInterstitialAdClickedEvent += value;
                }
            }

            remove
            {
                if (_onInterstitialAdClickedEvent.GetInvocationList().Contains(value))
                {
                    _onInterstitialAdClickedEvent -= value;
                }
            }
        }
        public void OnInterstitialAdClickedEvent(AdInfo adInfo)
        {
            if (_onInterstitialAdClickedEvent != null)
            {
                _onInterstitialAdClickedEvent(adInfo);
            }
        }

        private static event Action<AdInfo> _onInterstitialAdOpenedEvent;
        /// <summary>
        /// Invoked when the Interstitial Ad Unit has opened
        /// <typeparam name="AdInfo">See <see cref="AdInfo"/></typeparam>
        /// </summary>
        public static event Action<AdInfo> onInterstitialAdOpenedEvent
        {
            add
            {
                if (_onInterstitialAdOpenedEvent == null || !_onInterstitialAdOpenedEvent.GetInvocationList().Contains(value))
                {
                    _onInterstitialAdOpenedEvent += value;
                }
            }

            remove
            {
                if (_onInterstitialAdOpenedEvent.GetInvocationList().Contains(value))
                {
                    _onInterstitialAdOpenedEvent -= value;
                }
            }
        }
        public void OnInterstitialAdOpenedEvent(AdInfo adInfo)
        {
            if (_onInterstitialAdOpenedEvent != null)
            {
                _onInterstitialAdOpenedEvent(adInfo);
            }
        }

        private static event Action<AdInfo> _onInterstitialAdClosedEvent;
        /// <summary>
        /// Invoked when the interstitial ad closed and the user goes back to the application screen.
        /// <typeparam name="AdInfo">See <see cref="AdInfo"/></typeparam>
        /// </summary>
        public static event Action<AdInfo> onInterstitialAdClosedEvent
        {
            add
            {
                if (_onInterstitialAdClosedEvent == null || !_onInterstitialAdClosedEvent.GetInvocationList().Contains(value))
                {
                    _onInterstitialAdClosedEvent += value;
                }
            }

            remove
            {
                if (_onInterstitialAdClosedEvent.GetInvocationList().Contains(value))
                {
                    _onInterstitialAdClosedEvent -= value;
                }
            }
        }
        public void OnInterstitialAdClosedEvent(AdInfo adInfo)
        {
            if (_onInterstitialAdClosedEvent != null)
            {
                _onInterstitialAdClosedEvent(adInfo);
            }
        }
        #endregion

        #region Banner Events
        private static event Action _onBannerAdLeftApplicationEvent;
        /// <summary>
        /// Invoked when the user leaves the app
        /// </summary>
        public static event Action onBannerAdLeftApplicationEvent
        {
            add
            {
                if (_onBannerAdLeftApplicationEvent == null || !_onBannerAdLeftApplicationEvent.GetInvocationList().Contains(value))
                {
                    _onBannerAdLeftApplicationEvent += value;
                }
            }

            remove
            {
                if (_onBannerAdLeftApplicationEvent.GetInvocationList().Contains(value))
                {
                    _onBannerAdLeftApplicationEvent -= value;
                }
            }
        }
        public void OnBannerAdLeftApplicationEvent()
        {
            if (_onBannerAdLeftApplicationEvent != null)
            {
                _onBannerAdLeftApplicationEvent();
            }
        }

        private static event Action _onBannerAdScreenDismissedEvent;
        /// <summary>
        /// Notifies the presented screen has been dismissed
        /// </summary>
        public static event Action onBannerAdScreenDismissedEvent
        {
            add
            {
                if (_onBannerAdScreenDismissedEvent == null || !_onBannerAdScreenDismissedEvent.GetInvocationList().Contains(value))
                {
                    _onBannerAdScreenDismissedEvent += value;
                }
            }

            remove
            {
                if (_onBannerAdScreenDismissedEvent.GetInvocationList().Contains(value))
                {
                    _onBannerAdScreenDismissedEvent -= value;
                }
            }
        }
        public void OnBannerAdScreenDismissedEvent()
        {
            if (_onBannerAdScreenDismissedEvent != null)
            {
                _onBannerAdScreenDismissedEvent();
            }
        }

        private static event Action<string> _onBannerAdScreenPresentedEvent;
        /// <summary>
        /// Notifies the presentation of a full screen content following user click
        /// </summary>
        public static event Action<string> onBannerAdScreenPresentedEvent
        {
            add
            {
                if (_onBannerAdScreenPresentedEvent == null || !_onBannerAdScreenPresentedEvent.GetInvocationList().Contains(value))
                {
                    _onBannerAdScreenPresentedEvent += value;
                }
            }

            remove
            {
                if (_onBannerAdScreenPresentedEvent.GetInvocationList().Contains(value))
                {
                    _onBannerAdScreenPresentedEvent -= value;
                }
            }
        }
        public void OnBannerAdScreenPresentedEvent(string placement)
        {
            if (_onBannerAdScreenPresentedEvent != null)
            {
                _onBannerAdScreenPresentedEvent(placement);
            }
        }

        private static event Action<AdInfo> _onBannerAdClickedEvent;
        /// <summary>
        /// Invoked when end user clicks on the banner ad
        /// <typeparam name="AdInfo">See <see cref="AdInfo"/></typeparam>
        /// </summary>
        public static event Action<AdInfo> onBannerAdClickedEvent
        {
            add
            {
                if (_onBannerAdClickedEvent == null || !_onBannerAdClickedEvent.GetInvocationList().Contains(value))
                {
                    _onBannerAdClickedEvent += value;
                }
            }

            remove
            {
                if (_onBannerAdClickedEvent.GetInvocationList().Contains(value))
                {
                    _onBannerAdClickedEvent -= value;
                }
            }
        }
        public void OnBannerAdClickedEvent(AdInfo adInfo)
        {
            if (_onBannerAdClickedEvent != null)
            {
                _onBannerAdClickedEvent(adInfo);
            }
        }

        private static event Action<AdInfo> _onBannerAdLoadFailedEvent;
        /// <summary>
        /// Invoked when the banner loading process has failed.
        /// <typeparam name="AdInfo">See <see cref="AdInfo"/></typeparam>
        /// </summary>
        public static event Action<AdInfo> onBannerAdLoadFailedEvent
        {
            add
            {
                if (_onBannerAdLoadFailedEvent == null || !_onBannerAdLoadFailedEvent.GetInvocationList().Contains(value))
                {
                    _onBannerAdLoadFailedEvent += value;
                }
            }

            remove
            {
                if (_onBannerAdLoadFailedEvent.GetInvocationList().Contains(value))
                {
                    _onBannerAdLoadFailedEvent -= value;
                }
            }
        }
        public void OnBannerAdLoadFailedEvent(AdInfo adInfo)
        {
            if (_onBannerAdLoadFailedEvent != null)
            {
                _onBannerAdLoadFailedEvent(adInfo);
            }
        }

        private static event Action<AdInfo> _onBannerAdLoadedEvent;
        /// <summary>
        /// Invoked once the banner has loaded
        /// <typeparam name="AdInfo">See <see cref="AdInfo"/></typeparam>
        /// </summary>
        public static event Action<AdInfo> onBannerAdLoadedEvent
        {
            add
            {
                if (_onBannerAdLoadedEvent == null || !_onBannerAdLoadedEvent.GetInvocationList().Contains(value))
                {
                    _onBannerAdLoadedEvent += value;
                }
            }

            remove
            {
                if (_onBannerAdLoadedEvent.GetInvocationList().Contains(value))
                {
                    _onBannerAdLoadedEvent -= value;
                }
            }
        }
        public void OnBannerAdLoadedEvent(AdInfo adInfo)
        {
            if (_onBannerAdLoadedEvent != null)
            {
                _onBannerAdLoadedEvent(adInfo);
            }
        }
        #endregion
    }
}
