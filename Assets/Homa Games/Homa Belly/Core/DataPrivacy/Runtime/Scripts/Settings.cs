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

using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace HomaGames.HomaBelly.DataPrivacy
{
    public class Settings : ScriptableObject
    {
        public static string LEGACY_DEFAULT_APPLE_MESSAGE = "This will only be used to keep the app free by serving ads. Please support us by allowing tracking. Thank you!";
        public static string DEFAULT_APPLE_MESSAGE = "We use your information in order to enhance your game experience, by serving you personalized ads and measuring the performance of our game.";

        private static DataPrivacy.Settings _settingsCache;
        
        public static async Task<DataPrivacy.Settings> LoadAsync()
        {
            if (! _settingsCache)
                _settingsCache = await ResourcesUtils.LoadAsync<DataPrivacy.Settings>(Constants.SETTINGS_RESOURCE_NAME);

            return _settingsCache;
        }
        
        public static DataPrivacy.Settings Load()
        {
            if (! _settingsCache)
                _settingsCache = Resources.Load<DataPrivacy.Settings>(Constants.SETTINGS_RESOURCE_NAME);

            return _settingsCache;
        }

        public enum SupportedLanguages
        {
            /// <summary>
            /// English
            /// </summary>
            EN,
            /// <summary>
            /// Spanish
            /// </summary>
            ES,
            /// <summary>
            /// German
            /// </summary>
            DE,
            /// <summary>
            /// French
            /// </summary>
            FR,
            /// <summary>
            /// Italian
            /// </summary>
            IT,
            /// <summary>
            /// Catalan (Spain)
            /// </summary>
            CA
        }

        #region Settings properties
        /// <summary>
        /// The game name to be visible in the DataPrivacy welcome page
        /// </summary>
        [Tooltip("The game name to be visible in the DataPrivacy welcome page")]
        public string GameName;

        /// <summary>
        /// Base background color for the DataPrivacy windows
        /// </summary>
        public Color BackgroundColor;

        /// <summary>
        /// Base font color for the DataPrivacy text
        /// </summary>
        public Color FontColor;
        
        /// <summary>
        /// Secondary font color for the DataPrivacy text
        /// </summary>
        public Color SecondaryFontColor;

        /// <summary>
        /// Base toggle color for the DataPrivacy toggle when ONs and CTA button
        /// </summary>
        public Color ToggleColor;

        /// <summary>
        /// Base font color for the DataPrivacy buttons
        /// </summary>
        public Color ButtonFontColor;

        /// <summary>
        /// Flag informing if DataPrivacy should be displayed in device language in runtime or not
        /// </summary>
        public bool UseDeviceLanguageWhenPossible;

        /// <summary>
        /// If `UseDeviceLanguageWhenPossible` is set to false, DataPrivacy will be always shown
        /// in this specific language
        /// </summary>
        public SupportedLanguages Language;

        /// <summary>
        /// The customizable iOS 14 IDFA native popup request message
        /// </summary>
        public string iOSIdfaPopupMessage;

        /// <summary>
        /// Locally forces the GDPR screen to never appear. This might be useful
        /// for very localized builds, like Chinese builds.
        /// </summary>
        public bool ForceDisableGdpr;

        /// <summary>
        /// Disables GDPR for iOS apps.
        /// This settings must be changed in the Homa belly Manifest.
        /// </summary>
        public bool GdprEnabledForIos;
        
        /// <summary>
        /// Disables DataPrivacy for Android apps.
        /// This settings must be changed in the Homa belly Manifest.
        /// </summary>
        public bool GdprEnabledForAndroid;

        /// <summary>
        /// Disables IDFA popup for iOS apps.
        /// This settings must be changed in the Homa belly Manifest.
        /// </summary>
        public bool ShowIdfa;
        
        /// <summary>
        /// Disables DataPrivacy's IDFA pre-popup for iOS apps.
        /// This settings must be changed in the Homa belly Manifest.
        /// </summary>
        public bool ShowIdfaPrePopup;
        #endregion
        
        public bool GdprEnabled => 
#if UNITY_IOS
                GdprEnabledForIos;
#else
                GdprEnabledForAndroid;
#endif

        #region Public methods

        public void ResetToDefaultValues()
        {
            GameName = Application.productName;
            ColorUtility.TryParseHtmlString(Constants.FONT_COLOR, out FontColor);
            ColorUtility.TryParseHtmlString(Constants.BACKGROUND_COLOR, out BackgroundColor);
            ColorUtility.TryParseHtmlString(Constants.SECONDARY_FONT_COLOR, out SecondaryFontColor);
            ColorUtility.TryParseHtmlString(Constants.TOGGLE_COLOR, out ToggleColor);
            ColorUtility.TryParseHtmlString(Constants.BUTTON_FONT_COLOR, out ButtonFontColor);
            Language = SupportedLanguages.EN;
            UseDeviceLanguageWhenPossible = true;
            iOSIdfaPopupMessage = DEFAULT_APPLE_MESSAGE;
            ForceDisableGdpr = false;

            // Update localization
            Localization.LoadLanguageFromSettings();
        }

#if UNITY_EDITOR
        public static DataPrivacy.Settings EditorCreateOrLoadDataPrivacySettings()
        {
            DataPrivacy.Settings asset = AssetDatabase.LoadAssetAtPath<DataPrivacy.Settings>(Constants.DATA_PRIVACY_SETTINGS_ASSET_PATH);
            if (asset == null)
            {
                // If asset is not found, create it
                asset = ScriptableObject.CreateInstance<DataPrivacy.Settings>();
                asset.ResetToDefaultValues();
                FileUtilities.CreateIntermediateDirectoriesIfNecessary(Constants.DATA_PRIVACY_SETTINGS_ASSET_PATH);
                AssetDatabase.CreateAsset(asset, Constants.DATA_PRIVACY_SETTINGS_ASSET_PATH);
                AssetDatabase.SaveAssets();
            }

            return asset;
        }
#endif
        #endregion
    }
}
