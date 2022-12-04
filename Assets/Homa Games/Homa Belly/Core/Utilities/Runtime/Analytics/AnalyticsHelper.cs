#if UNITY_2019_4 ||  UNITY_2020_3_OR_NEWER
#define AUDIO_MOBILE_API_AVAILABLE 
#endif
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Internal class to automatically track Analytics (and some Attribution) events
    /// </summary>
    internal class AnalyticsHelper
    {
        // PlayerPrefs Keys
        private const string LIFETIME_REWARDED_VIDEO_ADS_WATCHED_KEY = "com.homagames.homabelly.lifetime_rw_watched";
        private const string LIFETIME_INTERSTITIAL_ADS_WATCHED_KEY = "com.homagames.homabelly.lifetime_is_watched";
        private const string FIRST_TIME_GAME_LAUNCH_KEY = "com.homagames.homabelly.first_time_game_launch";
        private const string SESSION_NUMBER_KEY = "com.homagames.homabelly.session_number";
        private const string LAST_APPLICATION_PAUSE_TIMESTAMP_KEY = "com.homagames.homabelly.last_application_pause_timestamp";
        private const string LAST_APPLICATION_RESUME_TIMESTAMP_KEY = "com.homagames.homabelly.last_application_resume_timestamp";

        /// <summary>
        /// Max number of sessions to be tracked
        /// </summary>
        private const int MAX_SESSION_COUNT = 100;

        /// <summary>
        /// We consider a new session any game run after 10 minutes of the
        /// last game exit
        /// </summary>
        private const int OFFLINE_MINUTES_TO_ASSUME_NEW_SESSION = 10;

        private NetworkHelper networkHelper = new NetworkHelper();
        
#pragma warning disable CS0414
        private static long beforeSceneLoadTimestampInSeconds = 0;
        private static long _currentApplicationResumeTimestamp = 0;
#pragma warning restore CS0414
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void BeforeSceneLoad()
        {
            if (beforeSceneLoadTimestampInSeconds == 0)
            {
                beforeSceneLoadTimestampInSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }

#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += PlayModeStateChanged;
#endif

        }

#if UNITY_EDITOR
        private static void PlayModeStateChanged(PlayModeStateChange playModeState)
        {
            switch (playModeState)
            {
                case PlayModeStateChange.ExitingPlayMode:
                    // Save last pause timestamp
                    PlayerPrefs.SetString(LAST_APPLICATION_PAUSE_TIMESTAMP_KEY, GetUtcNowInSeconds());
                    PlayerPrefs.Save();
                    break;
            }
        }
#endif

        public void Start()
        {
            if (!HomaBelly.Instance.IsInitialized)
            {
                Events.onInitialized += OnHomaBellyInitialized;
            }
            else
            {
                OnHomaBellyInitialized();
            }
        }

        public static string GetUtcNowInSeconds()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        }

        /// <summary>
        /// Start network and audio tracking after Homa Belly is initialized
        /// </summary>
        private void OnHomaBellyInitialized()
        {
#if HOMA_BELLY_DEFAULT_ANALYTICS_ENABLED
            // Track Homa Belly initialization time
            long elapsedSecondsSinceBeforeSceneLoad = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - beforeSceneLoadTimestampInSeconds;
            HomaBelly.Instance.TrackDesignEvent("HomaBelly_Initialized", Mathf.Max(0, elapsedSecondsSinceBeforeSceneLoad));

            // Listen network reachability events
            networkHelper.OnInitialNetworkReachabilityFetched += OnInitialNetworkReachabilityFetched;
            networkHelper.OnNetworkReachabilityChange += OnNetworkReachabilityChanged;
            
#if AUDIO_MOBILE_API_AVAILABLE
            // Track mute status and start listening to changes
            TrackMuteStatus();
            AudioSettings.Mobile.OnMuteStateChanged += OnMuteStateChanged;
#endif
#endif
        }

        /// <summary>
        /// Tracks default session start events:
        /// - Session:N:Started. Value: OfflineTimeInMinutes
        /// </summary>
        private void TrackSessionStarted()
        {
#if HOMA_BELLY_DEFAULT_ANALYTICS_ENABLED
            // Track first time game launch
            if (PlayerPrefs.GetInt(FIRST_TIME_GAME_LAUNCH_KEY, 0) == 0)
            {
                HomaBelly.Instance.TrackDesignEvent("GameLaunched");
                PlayerPrefs.SetInt(FIRST_TIME_GAME_LAUNCH_KEY, 1);
                PlayerPrefs.Save();
            }

            // Gather last timestamps
            long utcNowInSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long lastApplicationPauseTimestamp = long.Parse(PlayerPrefs.GetString(LAST_APPLICATION_PAUSE_TIMESTAMP_KEY, utcNowInSeconds.ToString()));

            // Session times
            float offlineTimeInMinutes = utcNowInSeconds == lastApplicationPauseTimestamp ? 0 : (float) (utcNowInSeconds - lastApplicationPauseTimestamp) / 60.0f;

            // Only track a new session started if the offline time is more than OFFLINE_MINUTES_TO_ASSUME_NEW_SESSION
            if (offlineTimeInMinutes == 0 || offlineTimeInMinutes > OFFLINE_MINUTES_TO_ASSUME_NEW_SESSION)
            {
                if (offlineTimeInMinutes > OFFLINE_MINUTES_TO_ASSUME_NEW_SESSION)
                {
                    // Track session ended when a new one is started
                    TrackSessionEnded();
                }

                // Track session number (increase by one for this session -- up to 100)
                int sessionNumber = PlayerPrefs.GetInt(SESSION_NUMBER_KEY, 0);
                sessionNumber += 1;
                PlayerPrefs.SetInt(SESSION_NUMBER_KEY, sessionNumber);
                PlayerPrefs.Save();
                if (sessionNumber < MAX_SESSION_COUNT)
                {
                    HomaBelly.Instance.TrackDesignEvent("Session:" + sessionNumber + ":Started", offlineTimeInMinutes);
                }
            }
#endif
        }

        /// <summary>
        /// Tracks default session end events:
        /// - Session:N:Played. Value: SessionLengthInSeconds
        /// </summary>
        private void TrackSessionEnded()
        {
#if HOMA_BELLY_DEFAULT_ANALYTICS_ENABLED
            // Gather last timestamps
            long utcNowInSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long lastApplicationResumeTimestamp = long.Parse(PlayerPrefs.GetString(LAST_APPLICATION_RESUME_TIMESTAMP_KEY, utcNowInSeconds.ToString()));
            long lastApplicationPauseTimestamp = long.Parse(PlayerPrefs.GetString(LAST_APPLICATION_PAUSE_TIMESTAMP_KEY, utcNowInSeconds.ToString()));

            // Session times
            long sessionLengthInSeconds = lastApplicationPauseTimestamp - lastApplicationResumeTimestamp;

            // Track session number
            int sessionNumber = PlayerPrefs.GetInt(SESSION_NUMBER_KEY, 0);
            if (sessionNumber < MAX_SESSION_COUNT)
            {
                HomaBelly.Instance.TrackDesignEvent("Session:" + sessionNumber + ":Played", sessionLengthInSeconds);
            }
#endif
        }

        /// <summary>
        /// Determine when the application is paused to save the timestamps
        /// </summary>
        /// <param name="pause"></param>
        public void OnApplicationPause(bool pause)
        {
#if HOMA_BELLY_DEFAULT_ANALYTICS_ENABLED
            // Call DefaultAnalytics to keep track of gameplay times
            DefaultAnalytics.OnApplicationPause(pause);

            // Game is paused
            if (pause)
            {
                // Save last pause and resume timestamp when pausing the game
                PlayerPrefs.SetString(LAST_APPLICATION_PAUSE_TIMESTAMP_KEY, GetUtcNowInSeconds());
                PlayerPrefs.SetString(LAST_APPLICATION_RESUME_TIMESTAMP_KEY, _currentApplicationResumeTimestamp.ToString());
                PlayerPrefs.Save();

                networkHelper.StopListening();
            }
            // Game is resumed
            else
            {
                _currentApplicationResumeTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                
                // Always track session started BEFORE saving LAST_APPLICATION_RESUME_TIMESTAMP_KEY
                TrackSessionStarted();
                networkHelper.StartListening();
            }
#endif
        }

        #region Ads

        /// <summary>
        /// When a Rewarded Video is watched, track some Attribution events
        /// </summary>
        /// <param name="placementId">(Optional) The placement id</param>
        public void OnRewardedVideoAdWatched(string placementId = null)
        {
            HomaBelly.Instance.TrackAttributionEvent("count_rv-is_watched");
            HomaBelly.Instance.TrackDesignEvent("count_rv-is_watched");

            // Increase RV lifetime watched count
            int lifetimeRvWatched = PlayerPrefs.GetInt(LIFETIME_REWARDED_VIDEO_ADS_WATCHED_KEY, 0);
            lifetimeRvWatched++;
            PlayerPrefs.SetInt(LIFETIME_REWARDED_VIDEO_ADS_WATCHED_KEY, lifetimeRvWatched);
            PlayerPrefs.Save();

            // Track all the pair times the user watched a rv
            if (lifetimeRvWatched > 0 && lifetimeRvWatched % 2 == 0)
            {
                HomaBelly.Instance.TrackAttributionEvent("count_2rv_watched");
                HomaBelly.Instance.TrackDesignEvent("count_2rv_watched");
            }
        }

        /// <summary>
        /// When an Interstitial is watched, track some Attribution events
        /// </summary>
        /// <param name="placementId">(Optional) The placement id</param>
        public void OnInterstitialAdWatched(string placementId = null)
        {
            HomaBelly.Instance.TrackAttributionEvent("count_rv-is_watched");
            HomaBelly.Instance.TrackDesignEvent("count_rv-is_watched");

            // Increase IS lifetime watched count
            int lifetimeIsWatched = PlayerPrefs.GetInt(LIFETIME_INTERSTITIAL_ADS_WATCHED_KEY, 0);
            lifetimeIsWatched++;
            PlayerPrefs.SetInt(LIFETIME_INTERSTITIAL_ADS_WATCHED_KEY, lifetimeIsWatched);
            PlayerPrefs.Save();

            // Track all the pair times the user watched an is
            if (lifetimeIsWatched > 0 && lifetimeIsWatched % 2 == 0)
            {
                HomaBelly.Instance.TrackAttributionEvent("count_2is_watched");
                HomaBelly.Instance.TrackDesignEvent("count_2is_watched");
            }
        }

        #endregion

        #region Network Detection

        private void OnInitialNetworkReachabilityFetched(NetworkReachability reachability)
        {
            if (Application.isPlaying)
            {
                TrackNetworkReachability(reachability);
            }
        }
        
        /// <summary>
        /// Callback invoked when a network reachability change is detected
        /// </summary>
        /// <param name="reachability"></param>
        private void OnNetworkReachabilityChanged(NetworkReachability reachability)
        {
            if (Application.isPlaying)
            {
                TrackNetworkReachability(reachability);
            }
        }

        /// <summary>
        /// Tracks network reachability (only once state per session)
        /// </summary>
        /// <param name="reachability"></param>
        private void TrackNetworkReachability(NetworkReachability reachability)
        {
#if HOMA_BELLY_DEFAULT_ANALYTICS_ENABLED
            switch (reachability)
            {
                case NetworkReachability.NotReachable:
                    // Internet not reachable
                    HomaBelly.Instance.TrackDesignEvent("NetworkReachability:NotReachable");

                    break;
                case NetworkReachability.ReachableViaCarrierDataNetwork:
                case NetworkReachability.ReachableViaLocalAreaNetwork:
                    // Internet reachable
                    HomaBelly.Instance.TrackDesignEvent("NetworkReachability:Reachable");

                    break;
            }
#endif
        }

        #endregion

#if AUDIO_MOBILE_API_AVAILABLE
        #region Audio Detection
        /// <summary>
        /// Invoked when the device changes its muted status 
        /// </summary>
        /// <param name="muted"></param>
        private void OnMuteStateChanged(bool muted)
        {
            TrackMuteStatus();
        }

        /// <summary>
        /// Tracks current mute status
        /// </summary>
        private void TrackMuteStatus()
        {
#if HOMA_BELLY_DEFAULT_ANALYTICS_ENABLED
            HomaBelly.Instance.TrackDesignEvent("Audio:" + (AudioSettings.Mobile.muteState ? "Muted" : "Unmuted"));
#endif
        }

        #endregion
#endif
    }
}
