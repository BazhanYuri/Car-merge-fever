using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public static class ForceUpdatePopup
    {
        private const string LAST_UPDATE_SHOWN_TIME_KEY = "homagames.last_force_update_time";

        private static DateTime LastShownPopupTime
        {
            get
            {
                if (PlayerPrefs.HasKey(LAST_UPDATE_SHOWN_TIME_KEY))
                    return DateTime.FromBinary(Convert.ToInt64(PlayerPrefs.GetString(LAST_UPDATE_SHOWN_TIME_KEY)));
                
                return new DateTime(0);
            }

            set => PlayerPrefs.SetString(LAST_UPDATE_SHOWN_TIME_KEY, value.ToBinary().ToString());
        }

        public static void ShowPopupIfRequired()
        {
            ForceUpdateConfigurationModel forceUpdateConfig = RemoteConfiguration.EveryTime.ForceUpdateConfigurationModel;

            if (forceUpdateConfig?.Enabled == true)
            {
                if (!forceUpdateConfig.ForceUpdate)
                    ForceUpdatePopupWakeUpProxy.SetupProxy();
                
                if (VersionCompare(Application.version, forceUpdateConfig.MinRequiredVersion) < 0)
                    ShowDialog(forceUpdateConfig);
            }
        }

        private static void ShowDialog(ForceUpdateConfigurationModel forceUpdateConfig)
        {
            ForceUpdateLocale locale = ForceUpdateLocale.GetLocaleFor(Application.systemLanguage);
            
            if (forceUpdateConfig.ForceUpdate)
            {
                MobileDialog.Create(locale.PopupTitle, locale.PopupText,
                    new MobileDialog.ButtonWithCallback{Message = locale.PopupOkayButton, Callback = () =>
                    {
                        Application.OpenURL(forceUpdateConfig.StoreLink);
                        ShowDialog(forceUpdateConfig);
                    }}, false);
            }
            else
            {
                DateTime now = DateTime.Now;
                if (now.Subtract(LastShownPopupTime).TotalDays >= 1)
                {
                    MobileDialog.Create(locale.PopupTitle, locale.PopupText,
                        new MobileDialog.ButtonWithCallback
                        {
                            Message = locale.PopupOkayButton, Callback = () =>
                            {
                                Application.OpenURL(forceUpdateConfig.StoreLink);
                            }
                        },
                        new MobileDialog.ButtonWithCallback {Message = locale.PopupDismissButton});
                    
                    LastShownPopupTime = now;
                }
            }
        }

        /// <summary>
        /// Compares two version strings.
        /// </summary>
        /// <param name="version1">The first version string to compare</param>
        /// <param name="version2">The second version string to compare</param>
        /// <returns><ul>
        /// <li><b>0</b> if the two versions are equal</li>
        /// <li><b>A positive integer</b> if the first version is newer than the second one</li>
        /// <li><b>A negative integer</b> if the first version is older than the second</li></ul></returns>
        public static int VersionCompare(string version1, string version2)
        {
            List<int> versionNumbers1 = ToVersionNumbers(version1);
            List<int> versionNumbers2 = ToVersionNumbers(version2);

            int maxIndex = Mathf.Min(versionNumbers1.Count, versionNumbers2.Count);
            for (int i = 0; i < maxIndex; i++)
            {
                if (versionNumbers1[i] != versionNumbers2[i])
                    return versionNumbers1[i] - versionNumbers2[i];
            }

            return versionNumbers1.Count - versionNumbers2.Count;
        }

        private static List<int> ToVersionNumbers(string version)
        {
            if (version == null) return new List<int>();
            
            return version.Split('.').Select(numberString =>
            {
                numberString = new string(numberString.Where(char.IsDigit).ToArray());
                if (int.TryParse(numberString, out int number))
                    return number;
                else
                    return 0;
            }).ToList();
        }
    }
}