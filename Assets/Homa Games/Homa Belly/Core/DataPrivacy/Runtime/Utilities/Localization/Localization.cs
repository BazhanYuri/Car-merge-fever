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
using System.Collections.Generic;
using HomaGames.HomaBelly.Utilities;
using UnityEngine;
using static HomaGames.HomaBelly.DataPrivacy.Settings;

namespace HomaGames.HomaBelly.DataPrivacy
{
    public static class Localization
    {
        #region Private properties

        /// <summary>
        /// Dictionary holding all key-value pairs representing the
        /// localized strings.
        /// </summary>
        private static Dictionary<string, string> loadedLocalizationDictionary;

        #endregion

        #region Public properties

        /// <summary>
        /// When fetching literals from NTesting is `true`, localization files
        /// will be ignored
        /// </summary>
        public static bool FETCH_LITERALS_FROM_NTESTING = true;

        /// <summary>
        /// Action to be invoked whenever the language has been loaded
        /// </summary>
        public static Action OnLocalizationLanguageLoaded;

        private static Settings.SupportedLanguages currentLanguage;

        /// <summary>
        /// Current configured language
        /// </summary>
        public static Settings.SupportedLanguages CurrentLanguage
        {
            get { return currentLanguage; }
            set
            {
                currentLanguage = FETCH_LITERALS_FROM_NTESTING ? Settings.SupportedLanguages.EN : value;
                LoadLocalizationFile(currentLanguage);
            }
        }

        #endregion

        static Localization()
        {
            // Load default file
            LoadLanguageFromSettings();
        }

        #region Public methods

        /// <summary>
        /// Loads the desired language file from Settings or from device (if configured)
        /// </summary>
        public static void LoadLanguageFromSettings()
        {
            DataPrivacy.Settings settings = DataPrivacy.Settings.Load();
            if (settings != null)
            {
                if (settings.UseDeviceLanguageWhenPossible)
                {
                    // Try to use device language
                    string languageIsoCode = Get2LetterISOCodeFromSystemLanguage(Application.systemLanguage);
                    Settings.SupportedLanguages parsedLanguage = Settings.SupportedLanguages.EN;
                    Enum.TryParse(languageIsoCode, true, out parsedLanguage);
                    CurrentLanguage = parsedLanguage;
                    Debug.Log($"[Homa Games DataPrivacy] Loading device language localization file: {CurrentLanguage}");
                }
                else
                {
                    // Use defined language in settings
                    CurrentLanguage = settings.Language;
                    Debug.Log($"[Homa Games DataPrivacy] Loading settings localization file: {CurrentLanguage}");
                }
            }
            else
            {
                Debug.Log($"[Homa Games DataPrivacy] Could not load settings. Loading default language");
                CurrentLanguage = Settings.SupportedLanguages.EN;
            }
        }

        /// <summary>
        /// Obtain the localized string given a localization key
        /// </summary>
        /// <param name="localizationKey">The key identifying the localization string</param>
        /// <param name="defaultValue">(Optional) a default value, if key is not found</param>
        /// <returns></returns>
        public static string GetLocalizedValueForKey(string localizationKey, string defaultValue = "")
        {
            // First, try to localize with Remote values if enabled
            string localizedString = "";
            
            // Then try to localize with local translation files if remote did not translate
            if (string.IsNullOrEmpty(localizedString)
                && loadedLocalizationDictionary != null
                && loadedLocalizationDictionary.ContainsKey(localizationKey))
            {
                localizedString = loadedLocalizationDictionary[localizationKey];
            }

            if (string.IsNullOrEmpty(localizedString))
            {
                Debug.LogWarning(
                $"[Homa Games DataPrivacy] Trying to load localization value for key {localizationKey}, but dictionary is not loaded or key is not present");
                localizedString = defaultValue;
            }

            if (!string.IsNullOrEmpty(localizedString) && localizedString.Contains("<game_name>"))
            {
                Settings settings = Load();
                if (settings != null)
                {
                    localizedString = localizedString.Replace("<game_name>", settings.GameName);
                }
            }

            return System.Text.RegularExpressions.Regex.Unescape(localizedString);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Loads a localization file into `loadedLocalizationDictionary`
        /// if found. If not, does nothing.
        /// </summary>
        /// <param name="language">The language (two letter iso code) to be loaded</param>
        private static void LoadLocalizationFile(Settings.SupportedLanguages language)
        {
            TextAsset localizationTextAsset =
                (TextAsset) Resources.Load($"homagames.dataprivacy_{language.ToString().ToLower()}", typeof(TextAsset));
            if (localizationTextAsset != null)
            {
                Dictionary<string, object> localizationDictionary =
                    Json.Deserialize(localizationTextAsset.text) as Dictionary<string, object>;

                // Dump loaded json into localization dictionary
                loadedLocalizationDictionary = new Dictionary<string, string>();
                foreach (KeyValuePair<string, object> entry in localizationDictionary)
                {
                    loadedLocalizationDictionary.Add(entry.Key, (string) entry.Value);
                }

                // Notify any intersted subscriber
                if (OnLocalizationLanguageLoaded != null)
                {
                    OnLocalizationLanguageLoaded.Invoke();
                }
            }
            else
            {
                Debug.LogWarning($"[Homa Games DataPrivacy] Langugage file for {language} not found");
            }
        }

        /// <summary>
        /// Helps to convert Unity's Application.systemLanguage to a 
        /// 2 letter ISO country code. There is unfortunately not more
        /// countries available as Unity's enum does not enclose all
        /// countries.
        /// </summary>
        /// <returns>The 2-letter ISO code from system language.</returns>
        public static string Get2LetterISOCodeFromSystemLanguage(SystemLanguage lang)
        {
            string res = "EN";
            switch (lang)
            {
                case SystemLanguage.Afrikaans:
                    res = "AF";
                    break;
                case SystemLanguage.Arabic:
                    res = "AR";
                    break;
                case SystemLanguage.Basque:
                    res = "EU";
                    break;
                case SystemLanguage.Belarusian:
                    res = "BY";
                    break;
                case SystemLanguage.Bulgarian:
                    res = "BG";
                    break;
                case SystemLanguage.Catalan:
                    res = "CA";
                    break;
                case SystemLanguage.Chinese:
                    res = "ZH";
                    break;
                case SystemLanguage.Czech:
                    res = "CS";
                    break;
                case SystemLanguage.Danish:
                    res = "DA";
                    break;
                case SystemLanguage.Dutch:
                    res = "NL";
                    break;
                case SystemLanguage.English:
                    res = "EN";
                    break;
                case SystemLanguage.Estonian:
                    res = "ET";
                    break;
                case SystemLanguage.Faroese:
                    res = "FO";
                    break;
                case SystemLanguage.Finnish:
                    res = "FI";
                    break;
                case SystemLanguage.French:
                    res = "FR";
                    break;
                case SystemLanguage.German:
                    res = "DE";
                    break;
                case SystemLanguage.Greek:
                    res = "EL";
                    break;
                case SystemLanguage.Hebrew:
                    res = "IW";
                    break;
                case SystemLanguage.Hungarian:
                    res = "HU";
                    break;
                case SystemLanguage.Icelandic:
                    res = "IS";
                    break;
                case SystemLanguage.Indonesian:
                    res = "IN";
                    break;
                case SystemLanguage.Italian:
                    res = "IT";
                    break;
                case SystemLanguage.Japanese:
                    res = "JA";
                    break;
                case SystemLanguage.Korean:
                    res = "KO";
                    break;
                case SystemLanguage.Latvian:
                    res = "LV";
                    break;
                case SystemLanguage.Lithuanian:
                    res = "LT";
                    break;
                case SystemLanguage.Norwegian:
                    res = "NO";
                    break;
                case SystemLanguage.Polish:
                    res = "PL";
                    break;
                case SystemLanguage.Portuguese:
                    res = "PT";
                    break;
                case SystemLanguage.Romanian:
                    res = "RO";
                    break;
                case SystemLanguage.Russian:
                    res = "RU";
                    break;
                case SystemLanguage.SerboCroatian:
                    res = "SH";
                    break;
                case SystemLanguage.Slovak:
                    res = "SK";
                    break;
                case SystemLanguage.Slovenian:
                    res = "SL";
                    break;
                case SystemLanguage.Spanish:
                    res = "ES";
                    break;
                case SystemLanguage.Swedish:
                    res = "SV";
                    break;
                case SystemLanguage.Thai:
                    res = "TH";
                    break;
                case SystemLanguage.Turkish:
                    res = "TR";
                    break;
                case SystemLanguage.Ukrainian:
                    res = "UK";
                    break;
                case SystemLanguage.Unknown:
                    res = "EN";
                    break;
                case SystemLanguage.Vietnamese:
                    res = "VI";
                    break;
            }

            if (!Enum.IsDefined(typeof(Settings.SupportedLanguages), res))
            {
                res = "EN";
            }

            return res.ToLower();
        }

        #endregion
    }
}