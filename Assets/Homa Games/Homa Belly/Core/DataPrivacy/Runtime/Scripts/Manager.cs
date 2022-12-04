/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_IOS
using UnityEngine.iOS;
#endif

namespace HomaGames.HomaBelly.DataPrivacy
{
    /// <summary>
    /// Entry point for Homa Games DataPrivacy features
    /// </summary>
    public sealed class Manager
    {
        #region Singleton pattern

        public static Manager Instance { get; } = new Manager();

        #endregion

        #region Public properties

        /// <summary>
        /// Callback invoked when the DataPrivacy UI is shown
        /// </summary>
        public static event Action OnShow;

        /// <summary>
        /// Callback invoked when the DataPrivacy UI is dismissed. When this
        /// method gets invoked, all user decisions can be retrieved
        /// through corresponding Manager accessors.
        /// </summary>
        public static event Action OnDismiss;

        private static PrivacyResponse privacyResponse;
        private GameObject DataPrivacySettingsPrefab;
        public static bool IsGdprProtectedRegion => PlayerPrefs.GetInt(Constants.PersistenceKey.IS_GDPR_PROTECTED,0) == 1;

        public bool IsiOS14_5OrHigher
        {
            get
            {
#if UNITY_IOS && !UNITY_EDITOR
                Version currentVersion = new Version(Device.systemVersion); // Parse the version of the current OS
                Version ios14_5 = new Version("14.5"); // Parse the iOS 14.5 version constant

                if (currentVersion >= ios14_5)
                {
                    return true;
                }

                return false;
#elif UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }
        #endregion

        #region Public methods


        public Task ShowDataPrivacySettings() => DoShowDataPrivacySettings();
        
        [Obsolete("Use \"ShowDataPrivacySettings()\" instead")] // 22/03/2022
        public Task Show() => DoShowDataPrivacySettings();
        [Obsolete("Use \"ShowDataPrivacySettings()\" instead")]
        public Task Show(bool internetReachable, bool forceDisableDataPrivacy) => DoShowDataPrivacySettings();
        [Obsolete("Use \"ShowDataPrivacySettings()\" instead")]
        public Task Show(bool internetReachable, DataPrivacy.Settings settings, bool forceDisableDataPrivacy) => DoShowDataPrivacySettings();

        /// <summary>
        /// Obtain either if user is above required age or not.
        /// </summary>
        /// <returns>True if user explicitly asserted being above the required age. False otherwise</returns>
        public bool IsAboveRequiredAge()
        {
            return PlayerPrefs.GetInt(Constants.PersistenceKey.ABOVE_REQUIRED_AGE, 0) == 1;
        }

        /// <summary>
        /// Obtain either if user accepted Terms & Conditions or not.
        /// </summary>
        /// <returns>True if user accepted Terms & Conditions. False otherwise</returns>
        public bool IsTermsAndConditionsAccepted()
        {
            return PlayerPrefs.GetInt(Constants.PersistenceKey.TERMS_AND_CONDITIONS, 0) == 1;
        }

        /// <summary>
        /// Obtain either if user granted Analytics tracking or not.
        /// </summary>
        /// <returns>True if user granted Analytics tracking. False otherwise</returns>
        public bool IsAnalyticsGranted()
        {
            return PlayerPrefs.GetInt(Constants.PersistenceKey.ANALYTICS_TRACKING, 0) == 1;
        }

        /// <summary>
        /// Obtain either if user granted Tailored Ads permission or not.
        /// </summary>
        /// <returns>True if user granted Tailored Ads permission. False otherwise</returns>
        public bool IsTailoredAdsGranted()
        {
            return PlayerPrefs.GetInt(Constants.PersistenceKey.TAILORED_ADS, 0) == 1;
        }

        /// <summary>
        /// Return if the IOS IDFA onboarding flow has been asked already.
        /// </summary>
        /// <returns>True if already asked.</returns>
        public bool IsIOSIDFAFlowDone()
        {
            return PlayerPrefs.GetInt(Constants.PersistenceKey.IOS_ADS_TRACKING_ASKED, 0) == 1;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Show the DataPrivacy settings UI
        /// </summary>
        private async Task DoShowDataPrivacySettings()
        {
            if(DataPrivacySettingsPrefab==null)
            {
                DataPrivacySettingsPrefab = await ResourcesUtils.LoadAsync<GameObject>(Constants.PREFAB_RESOURCE_NAME);
                DataPrivacySettings.OnDismiss += () => OnDismiss?.Invoke();
            }
            
            if(DataPrivacySettingsPrefab==null)
            {
                Debug.LogError(
                    $"[Homa Games DataPrivacy] Could not show DataPrivacy. Prefab {Constants.PREFAB_RESOURCE_NAME} is missing");
                return;
            }

            Object.Instantiate(DataPrivacySettingsPrefab);
            
            OnShow?.Invoke();
        }

        public async Task FetchIsGdprProtectedRegion()
        {
            privacyResponse = await PrivacyResponse.FetchPrivacyForCurrentRegion();
            Debug.Log($"[Homa Games DataPrivacy] Region detection. Protected region? {privacyResponse.Protected}");
            PlayerPrefs.SetInt(Constants.PersistenceKey.IS_GDPR_PROTECTED, privacyResponse.Protected ? 1 : 0);
            PlayerPrefs.Save();
        }

        #endregion
    }
}
