using System;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable CS0414

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Use this class to invoke default Analytic Events for your game. You will
    /// need to invoke the following methods accordingly:
    ///
    /// - LevelStarted(levelId)
    /// - LevelFailed()
    /// - LevelCompleted()
    ///
    /// - TutorialStepStarted(stepId)
    /// - TutorialStepFailed()
    /// - TutorialStepCompleted()
    ///
    /// - SuggestedRewardedAd(string placementName)
    /// - RewardedAdTriggered(string placementName)
    /// - InterstitialAdTriggered(string placementName)
    /// </summary>
    public static class DefaultAnalytics
    {
        private const string LEVEL_ATTEMPT_KEY = "com.homagames.homabelly.level_{0}_attempt";
        private const string LEVEL_STARTED_AT_GAMEPLAY_SECONDS_KEY = "com.homagames.homabelly.level_started_{0}_at_gameplay_seconds";
        private const string LEVEL_COMPLETED_KEY = "com.homagames.homabelly.level_completed_{0}_at_gameplay_seconds";
        private const string CURRENT_LEVEL_KEY = "com.homagames.homabelly.current_level";

        private const string TUTORIAL_STEP_STARTED_KEY = "com.homagames.homabelly.tutorial_step_started_{0}_at_gameplay_seconds";
        private const string TUTORIAL_STEP_COMPLETED_KEY = "com.homagames.homabelly.tutorial_step_completed_{0}_at_gameplay_seconds";
        private const string CURRENT_TUTORIAL_STEP_KEY = "com.homagames.homabelly.current_tutorial_step";

        private const string REWARDED_AD_FIRST_TIME_TAKEN_EVER_KEY = "com.homagames.homabelly.rewarded_ad_first_time_taken_ever_{0}";
        private const string INTERSTITIAL_AD_FIRST_TIME_KEY = "com.homagames.homabelly.interstitial_ad_first_time";

        private const string CURRENT_GAMEPLAY_TIME_KEY = "com.homagames.homabelly.current_gameplay_time_in_seconds";
        private const string MAIN_MENU_LOADED_KEY = "com.homagames.homabelly.main_menu_loaded";
        private const string GAMEPLAY_STARTED_KEY = "com.homagames.homabelly.gameplay_started";

        private static string _sanitizedCurrentLevelId = "1";
        private static string _sanitizedCurrentTutorialStep = "1";
        private static string currentGameplayTime;
        private static long gameplayResumedTimestamp;
        private static string _sanitizedCurrentRewardedAdName = "Default";
        private static string _sanitizedCurrentInterstitialAdName = "Default";
        private static Dictionary<string, bool> rewardedAdsTakenThisSession = new Dictionary<string, bool>();

        static DefaultAnalytics()
        {
            Start();
        }

        public static void Start()
        {
            // Recover any previous level or tutorial step stored
            _sanitizedCurrentLevelId = PlayerPrefs.GetString(CURRENT_LEVEL_KEY, "1");
            _sanitizedCurrentTutorialStep = PlayerPrefs.GetString(CURRENT_TUTORIAL_STEP_KEY, "1");

            if (HomaBelly.Instance.IsInitialized)
            {
                OnHomaBellyInitialized();
            }
            else
            {
                Events.onInitialized += OnHomaBellyInitialized;    
            }
        }

        private static void OnHomaBellyInitialized()
        {
            RegisterAdEvents();
        }

        public static void OnApplicationPause(bool pause)
        {
#if HOMA_BELLY_DEFAULT_ANALYTICS_ENABLED
            // Game is paused
            if (pause)
            {
                // Save new current gameplay time
                PlayerPrefs.SetString(CURRENT_GAMEPLAY_TIME_KEY, GetTotalGameplaySecondsAtThisExactMoment().ToString());
                PlayerPrefs.Save();
            }
            else
            {
                // Game is resumed
                currentGameplayTime = PlayerPrefs.GetString(CURRENT_GAMEPLAY_TIME_KEY, "");
                gameplayResumedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }
#endif
        }

        /// <summary>
        /// Obtains the current gameplay elapsed time in seconds at the
        /// moment of invoking this method.
        ///
        /// This takes into account only the time spent playing, not the time
        /// when the application is paused (the user minimized/exitted the game)
        /// </summary>
        /// <returns></returns>
        private static long GetTotalGameplaySecondsAtThisExactMoment()
        {
            // Calculate time since last resume
            long currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long gameplayTimeSinceApplicationResumed = currentTimestamp - gameplayResumedTimestamp;

            // Parse and add calculated time to current gameplay time
            long currentGameplayTimeLong = string.IsNullOrEmpty(currentGameplayTime) ? 0 : long.Parse(currentGameplayTime);
            currentGameplayTimeLong += gameplayTimeSinceApplicationResumed;

            return currentGameplayTimeLong;
        }

        #region Level Tracking

        /// <summary>
        /// Every time players start the level
        /// </summary>
        /// <param name="levelId"></param>
        public static void LevelStarted(int levelId)
        {
            LevelStarted(levelId.ToString());
        }

        /// <summary>
        /// Every time players start the level
        /// </summary>
        /// <param name="levelId"></param>
        public static void LevelStarted(string levelId)
        {
#if HOMA_BELLY_DEFAULT_ANALYTICS_ENABLED
            // Set current level
            _sanitizedCurrentLevelId = Sanitize(levelId);
            PlayerPrefs.SetString(CURRENT_LEVEL_KEY, _sanitizedCurrentLevelId);
            
            // Set level attempt
            int levelAttempt = PlayerPrefs.GetInt(string.Format(LEVEL_ATTEMPT_KEY, _sanitizedCurrentLevelId), 0);
            levelAttempt++;
            PlayerPrefs.SetInt(string.Format(LEVEL_ATTEMPT_KEY,_sanitizedCurrentLevelId), levelAttempt);
            
            // Save Player Prefs
            PlayerPrefs.Save();

            HomaBelly.Instance.TrackProgressionEvent(ProgressionStatus.Start, "Level_" + _sanitizedCurrentLevelId);

            // GameplayTime is the time spent in the game since the first launch, in seconds.
            long totalGameplaySecondsAtThisMoment = GetTotalGameplaySecondsAtThisExactMoment();
            
            // If is the very first time this level is started, track level reached
            if (string.IsNullOrEmpty(PlayerPrefs.GetString(string.Format(LEVEL_STARTED_AT_GAMEPLAY_SECONDS_KEY, _sanitizedCurrentLevelId), "")))
            {
                HomaBelly.Instance.TrackDesignEvent("Levels:Reached:" + _sanitizedCurrentLevelId, totalGameplaySecondsAtThisMoment);
            }
            
            // Save the GameplayTime of the level started attempt
            PlayerPrefs.SetString(string.Format(LEVEL_STARTED_AT_GAMEPLAY_SECONDS_KEY, _sanitizedCurrentLevelId), totalGameplaySecondsAtThisMoment.ToString());
            PlayerPrefs.Save();
#endif
        }

        /// <summary>
        /// Every time players fail the level
        /// </summary>
        public static void LevelFailed()
        {
#if HOMA_BELLY_DEFAULT_ANALYTICS_ENABLED
            HomaBelly.Instance.TrackProgressionEvent(ProgressionStatus.Fail, "Level_" + _sanitizedCurrentLevelId);
#endif
        }

        /// <summary>
        /// Every time players complete the level
        /// </summary>
        public static void LevelCompleted()
        {
#if HOMA_BELLY_DEFAULT_ANALYTICS_ENABLED
            HomaBelly.Instance.TrackProgressionEvent(ProgressionStatus.Complete, "Level_" + _sanitizedCurrentLevelId);

            // If is the very first time this level is started, track it
            if (string.IsNullOrEmpty(PlayerPrefs.GetString(string.Format(LEVEL_COMPLETED_KEY, _sanitizedCurrentLevelId), "")))
            {
                // LevelDuration is the time spent in the level from the start until the completion, in seconds.
                long levelStartAtGameplaySeconds = long.Parse(PlayerPrefs.GetString(string.Format(LEVEL_STARTED_AT_GAMEPLAY_SECONDS_KEY, _sanitizedCurrentLevelId), "0"));
                long levelDuration = Math.Max(0, GetTotalGameplaySecondsAtThisExactMoment() - levelStartAtGameplaySeconds);
                HomaBelly.Instance.TrackDesignEvent("Levels:Duration:" + _sanitizedCurrentLevelId, levelDuration);
                PlayerPrefs.SetString(string.Format(LEVEL_COMPLETED_KEY, _sanitizedCurrentLevelId), GetTotalGameplaySecondsAtThisExactMoment().ToString());
                PlayerPrefs.Save();
                
                // Track number of attempts before completing the level
                HomaBelly.Instance.TrackDesignEvent("Levels:Attempts:" + _sanitizedCurrentLevelId, PlayerPrefs.GetInt(string.Format(LEVEL_ATTEMPT_KEY, _sanitizedCurrentLevelId), 1));
            }
#endif
        }

        #endregion

        #region Tutorial Steps Tracking                                                                                                                                                                                                                                  

        /// <summary>
        /// Invoke this method everytime a tutorial step is started. Invoking
        /// it twice for the same step is harmless, as only the very first
        /// one will be taken into account.
        /// </summary>
        /// <param name="step">The tutorial step</param>
        public static void TutorialStepStarted(int step)
        {
            TutorialStepStarted(step.ToString());
        }

        /// <summary>
        /// Invoke this method everytime a tutorial step is started. Invoking
        /// it twice for the same step is harmless, as only the very first
        /// one will be taken into account.
        /// </summary>
        /// <param name="step">The tutorial step</param>
        public static void TutorialStepStarted(string step)
        {
#if HOMA_BELLY_DEFAULT_ANALYTICS_ENABLED
            // Set current level
            _sanitizedCurrentTutorialStep = Sanitize(step);
            PlayerPrefs.SetString(CURRENT_TUTORIAL_STEP_KEY, _sanitizedCurrentTutorialStep);
            PlayerPrefs.Save();

            // If is the very first time this tutorial step is started, track it
            if (string.IsNullOrEmpty(PlayerPrefs.GetString(string.Format(TUTORIAL_STEP_STARTED_KEY, _sanitizedCurrentTutorialStep), "")))
            {
                // GameplayTime is the time spent in the game since the first launch, in seconds.
                long totalGameplaySecondsAtThisMoment = GetTotalGameplaySecondsAtThisExactMoment();
                HomaBelly.Instance.TrackDesignEvent("Tutorial:" + _sanitizedCurrentTutorialStep + ":Started", totalGameplaySecondsAtThisMoment);
                PlayerPrefs.SetString(string.Format(TUTORIAL_STEP_STARTED_KEY, _sanitizedCurrentTutorialStep), totalGameplaySecondsAtThisMoment.ToString());
                PlayerPrefs.Save();
            }
#endif
        }

        /// <summary>
        /// When the player does not execute the asked behavior in the current tutorial step
        /// </summary>
        public static void TutorialStepFailed()
        {
#if HOMA_BELLY_DEFAULT_ANALYTICS_ENABLED
            HomaBelly.Instance.TrackDesignEvent("Tutorial:" + _sanitizedCurrentTutorialStep + ":Failed");
#endif
        }

        /// <summary>
        /// Invoke this method everytime a tutorial step is completed. Invoking
        /// it twice for the same step is harmless, as only the very first
        /// one will be taken into account.
        /// </summary>
        public static void TutorialStepCompleted()
        {
#if HOMA_BELLY_DEFAULT_ANALYTICS_ENABLED
            // If is the very first time this tutorial step is completed, track it
            if (string.IsNullOrEmpty(PlayerPrefs.GetString(string.Format(TUTORIAL_STEP_COMPLETED_KEY, _sanitizedCurrentTutorialStep), "")))
            {
                // StepDuration is the time spent to complete the step, in seconds.
                long tutorialStepStartAtGameplaySeconds = long.Parse(PlayerPrefs.GetString(string.Format(TUTORIAL_STEP_STARTED_KEY, _sanitizedCurrentTutorialStep), "0"));
                long tutorialStepDuration = Math.Max(0, GetTotalGameplaySecondsAtThisExactMoment() - tutorialStepStartAtGameplaySeconds);
                HomaBelly.Instance.TrackDesignEvent("Tutorial:" + _sanitizedCurrentTutorialStep + ":Completed", tutorialStepDuration);
                PlayerPrefs.SetString(string.Format(TUTORIAL_STEP_COMPLETED_KEY, _sanitizedCurrentTutorialStep), GetTotalGameplaySecondsAtThisExactMoment().ToString());
                PlayerPrefs.Save();
            }   
#endif
        }

        #endregion

        #region Ads

        /// <summary>
        /// Invoke this method whenever a rewarded offer is suggested to the player.
        /// </summary>
        /// <param name="placementName">Please follow the nomenclature in the relevant document</param>
        /// <param name="adPlacementType">The type of placement</param>
        public static void SuggestedRewardedAd(string placementName,AdPlacementType adPlacementType = AdPlacementType.Default)
        {
#if HOMA_BELLY_DEFAULT_ANALYTICS_ENABLED
            HomaBelly.Instance.TrackDesignEvent($"Rewarded:Suggested:{_sanitizedCurrentRewardedAdName}:{_sanitizedCurrentLevelId}:{adPlacementType}");
#endif
        }

        /// <summary>
        /// Internally invoked by Homa Belly upon RW showing
        /// </summary>
        /// <param name="placementName"></param>
        /// <param name="adPlacementType"></param>
        public static void RewardedAdTriggered(string placementName,AdPlacementType adPlacementType)
        {
#if HOMA_BELLY_DEFAULT_ANALYTICS_ENABLED
            _sanitizedCurrentRewardedAdName = Sanitize(placementName);
            HomaBelly.Instance.TrackDesignEvent($"Rewarded:Triggered:{_sanitizedCurrentRewardedAdName}:{_sanitizedCurrentLevelId}:{adPlacementType}");
#endif
        }

        /// <summary>
        /// Internally invoked by Homa Belly upon IS showing
        /// </summary>
        /// <param name="placementName"></param>
        /// <param name="adPlacementType"></param>
        public static void InterstitialAdTriggered(string placementName,AdPlacementType adPlacementType)
        {
#if HOMA_BELLY_DEFAULT_ANALYTICS_ENABLED
            _sanitizedCurrentInterstitialAdName = Sanitize(placementName);
            HomaBelly.Instance.TrackDesignEvent($"Interstitial:Triggered:{_sanitizedCurrentInterstitialAdName}:{_sanitizedCurrentLevelId}:{adPlacementType}");
#endif
        }

        /// <summary>
        /// Registers Ad Events to be tracked
        /// </summary>
        private static void RegisterAdEvents()
        {
#if HOMA_BELLY_DEFAULT_ANALYTICS_ENABLED
            // Banner
            Events.onBannerAdClickedEvent += (adInfo) =>
            {
                HomaBelly.Instance.TrackDesignEvent($"Banners:Clicked:{adInfo.AdPlacementType}");
            };

            Events.onBannerAdLoadFailedEvent += (adInfo) =>
            {
                HomaBelly.Instance.TrackDesignEvent($"Banners:LoadFailed:{adInfo.AdPlacementType}");
            };

            // Interstitial
            Events.onInterstitialAdReadyEvent += (adInfo) =>
            {
                HomaBelly.Instance.TrackDesignEvent($"Interstitials:Ready:{_sanitizedCurrentInterstitialAdName}:{_sanitizedCurrentLevelId}:{adInfo.AdPlacementType}");
            };

            Events.onInterstitialAdLoadFailedEvent += (adInfo) =>
            {
                HomaBelly.Instance.TrackDesignEvent($"Interstitials:LoadFailed:{_sanitizedCurrentInterstitialAdName}:{_sanitizedCurrentLevelId}:{adInfo.AdPlacementType}");
            };

            Events.onInterstitialAdClosedEvent += (adInfo) =>
            {
                HomaBelly.Instance.TrackDesignEvent($"Interstitials:Closed:{_sanitizedCurrentInterstitialAdName}:{_sanitizedCurrentLevelId}:{adInfo.AdPlacementType}");
            };

            Events.onInterstitialAdClickedEvent += (adInfo) =>
            {
                HomaBelly.Instance.TrackDesignEvent($"Interstitials:Clicked:{_sanitizedCurrentInterstitialAdName}:{_sanitizedCurrentLevelId}:{adInfo.AdPlacementType}");
            };

            Events.onInterstitialAdShowFailedEvent += (adInfo) =>
            {
                HomaBelly.Instance.TrackDesignEvent($"Interstitials:ShowFailed:{_sanitizedCurrentInterstitialAdName}:{_sanitizedCurrentLevelId}:{adInfo.AdPlacementType}");
            };

            Events.onInterstitialAdShowSucceededEvent += (adInfo) =>
            {
                HomaBelly.Instance.TrackDesignEvent($"Interstitials:ShowSucceeded:{_sanitizedCurrentInterstitialAdName}:{_sanitizedCurrentLevelId}:{adInfo.AdPlacementType}");

                if (PlayerPrefs.GetInt(INTERSTITIAL_AD_FIRST_TIME_KEY, 0) == 0)
                {
                    HomaBelly.Instance.TrackDesignEvent($"Interstitials:FirstWatched:{_sanitizedCurrentInterstitialAdName}:{_sanitizedCurrentLevelId}:{adInfo.AdPlacementType}", GetTotalGameplaySecondsAtThisExactMoment());
                    PlayerPrefs.SetInt(INTERSTITIAL_AD_FIRST_TIME_KEY, 1);
                    PlayerPrefs.Save();
                }
            };

            // Rewarded Video
            Events.onRewardedVideoAdRewardedEvent += (reward,adInfo) =>
            {
                if (!string.IsNullOrEmpty(_sanitizedCurrentRewardedAdName))
                {
                    // Current rewarded ad has not already been taken this session
                    if (!rewardedAdsTakenThisSession.ContainsKey(_sanitizedCurrentRewardedAdName))
                    {
                        rewardedAdsTakenThisSession.Add(_sanitizedCurrentRewardedAdName, true);
                        HomaBelly.Instance.TrackDesignEvent($"Rewarded:FirstWatchedSession:{_sanitizedCurrentRewardedAdName}:{adInfo.AdPlacementType}", GetTotalGameplaySecondsAtThisExactMoment());
                    }

                    // Current rewarded ad has not been taken ever
                    if (PlayerPrefs.GetInt(REWARDED_AD_FIRST_TIME_TAKEN_EVER_KEY, 0) == 0)
                    {
                        HomaBelly.Instance.TrackDesignEvent($"Rewarded:FirstWatched:{_sanitizedCurrentRewardedAdName}:{adInfo.AdPlacementType}", GetTotalGameplaySecondsAtThisExactMoment());
                        PlayerPrefs.SetInt(REWARDED_AD_FIRST_TIME_TAKEN_EVER_KEY, 1);
                        PlayerPrefs.Save();
                    }
                }
                
                HomaBelly.Instance.TrackDesignEvent($"Rewarded:Taken:{_sanitizedCurrentRewardedAdName}:{_sanitizedCurrentLevelId}:{adInfo.AdPlacementType}");
            };

            Events.onRewardedVideoAdShowFailedEvent += (adInfo) =>
            {
                HomaBelly.Instance.TrackDesignEvent($"Rewarded:Failed:{_sanitizedCurrentRewardedAdName}:{_sanitizedCurrentLevelId}:{adInfo.AdPlacementType}");
            };

            Events.onRewardedVideoAdStartedEvent += (adInfo) =>
            {
                HomaBelly.Instance.TrackDesignEvent($"Rewarded:Opened:{_sanitizedCurrentRewardedAdName}:{_sanitizedCurrentLevelId}:{adInfo.AdPlacementType}");
            };

            Events.onRewardedVideoAdClosedEvent += (adInfo) =>
            {
                HomaBelly.Instance.TrackDesignEvent($"Rewarded:Closed:{_sanitizedCurrentRewardedAdName}:{_sanitizedCurrentLevelId}:{adInfo.AdPlacementType}");
            };
#endif
        }

        #endregion

        #region Miscellaneous

        /// <summary>
        /// Invoke this method everytime your Main Menu screen is loaded
        /// </summary>
        public static void MainMenuLoaded()
        {
#if HOMA_BELLY_DEFAULT_ANALYTICS_ENABLED
            if (PlayerPrefs.GetInt(MAIN_MENU_LOADED_KEY, 0) == 0)
            {
                HomaBelly.Instance.TrackDesignEvent("MainMenu_Loaded", GetTotalGameplaySecondsAtThisExactMoment());
                PlayerPrefs.SetInt(MAIN_MENU_LOADED_KEY, 1);
                PlayerPrefs.Save();
            }
#endif
        }

        /// <summary>
        /// Invoke this method everytime the user starts the gameplay
        /// </summary>
        public static void GameplayStarted()
        {
#if HOMA_BELLY_DEFAULT_ANALYTICS_ENABLED
            if (PlayerPrefs.GetInt(GAMEPLAY_STARTED_KEY, 0) == 0)
            {
                HomaBelly.Instance.TrackDesignEvent("GamePlay_Started", GetTotalGameplaySecondsAtThisExactMoment());
                PlayerPrefs.SetInt(GAMEPLAY_STARTED_KEY, 1);
                PlayerPrefs.Save();
            }
#endif
        }

        #endregion

        /// <summary>
        /// Avoids sending an empty parameter to analytics
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private static string Sanitize(string parameter)
        {
            return string.IsNullOrWhiteSpace(parameter) ? "_" : parameter.Replace(" ", "_");
        }
    }
}
