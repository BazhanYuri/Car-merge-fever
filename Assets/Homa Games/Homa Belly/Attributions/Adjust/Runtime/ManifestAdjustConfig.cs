#if UNITY_EDITOR
    using UnityEditor;
#endif
using System;
using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

namespace HomaGames.HomaBelly
{
    public class ManifestAdjustConfig : ScriptableObject
    {
        #region Manifest Keys

        private const string APP_TOKEN_IOS = "s_ios_app_token";
        private const string APP_TOKEN_ANDROID = "s_android_app_token";
        
        #endregion
        
        private static ManifestAdjustConfig m_config;

        public static ManifestAdjustConfig Instance
        {
            get
            {
                if (m_config == null)
                    m_config = Resources.Load<ManifestAdjustConfig>(HomaBellyAdjustConstants.CONFIG_FILE_PATH_IN_RESOURCES);
#if UNITY_EDITOR
                if (m_config == null)
                {
                    var newConfig = CreateInstance<ManifestAdjustConfig>();
                    newConfig.hideFlags = HideFlags.NotEditable;
                    FileUtilities.CreateIntermediateDirectoriesIfNecessary(HomaBellyAdjustConstants.CONFIG_FILE_PATH);
                    AssetDatabase.CreateAsset(newConfig, HomaBellyAdjustConstants.CONFIG_FILE_PATH);
                    AssetDatabase.SaveAssets();
                    m_config = newConfig;
                }
#endif
                return m_config;
            }
        }

        [SerializeField]
        private PlatformConfig m_IosConfig = null;
        [SerializeField]
        private PlatformConfig m_AndroidConfig = null;
        [SerializeField]
        private bool m_isReferralSystemEnabled = false;
        [SerializeField]
        private string m_referralSchema = null;
        [SerializeField]
        private string m_referralHost = null;

        public bool IsReferralSystemEnabled => m_isReferralSystemEnabled;
        
        public bool GetTargetPlatformConfig(out PlatformConfig platformConfig)
        {
            platformConfig = null;
#if UNITY_ANDROID
            platformConfig = m_AndroidConfig;
#elif UNITY_IOS
            platformConfig = m_IosConfig;
#endif
            return platformConfig != null;
        }

        /// <summary>
        /// Use it to fill the config from the Dictionary that e can get from the manifest 
        /// </summary>
        public void FillWithValuesFromManifestDictionary(Dictionary<string,string> manifestDictionary)
        {
            if (manifestDictionary.TryGetValue(APP_TOKEN_IOS, out string iosAppToken))
            {
                m_IosConfig.AppToken = iosAppToken;
            }

            if (manifestDictionary.TryGetValue(APP_TOKEN_ANDROID, out string androidAppToken))
            {
                m_AndroidConfig.AppToken = androidAppToken;
            }
            
            // TODO: Get referral data for deep linking if the feature is approved

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
            if (!ConfigurationForCurrentPlatformIsValid())
            {
                Debug.LogError("Adjust SDK Configuration is not valid for the current target platform.");
            }
        }

        public bool ConfigurationForCurrentPlatformIsValid()
        {
            if (GetTargetPlatformConfig(out var config))
            {
                if (string.IsNullOrEmpty(config.AppToken))
                {
                    Debug.LogError("Adjust SDK: App Token is not set in the manifest file");
                    return false;
                }

            }
            else
            {
                Debug.LogError("Adjust SDK Configuration is not valid. Current platform not supported.");
            }
            
            if (IsReferralSystemEnabled)
            {
                if (string.IsNullOrEmpty(m_referralSchema))
                {
                    Debug.LogError("Adjust SDK: Referral schema is not set in the manifest file.");
                    return false;
                }

                if (!m_referralSchema.All(char.IsLetterOrDigit))
                {
                    Debug.LogError($"Adjust SDK: Referral schema: {m_referralSchema} is not valid. Only letters and digits are allowed.");
                    return false;
                }
                
                if (string.IsNullOrEmpty(m_referralHost))
                {
                    Debug.LogError("Adjust SDK: Referral schema is not set in the manifest file.");
                    return false;
                }
                
                if (!m_referralHost.All(char.IsLetterOrDigit))
                {
                    Debug.LogError($"Adjust SDK: Referral host: {m_referralHost} is not valid. Only letters and digits are allowed.");
                    return false;
                }
            }

            return true;
        }
        
        /// <summary>
        /// Return the schema with the path/host
        /// schema://path
        /// </summary>
        public string GetSchemaWithPath()
        {
            return $"{m_referralSchema}://{m_referralHost}";
        }

        /// <summary>
        /// Return the schema for the current platform.
        /// Each platforms needs to have a slightly different schema.
        /// </summary>
        /// <returns>
        /// Android: schema://host
        /// iOS: schema
        /// </returns>
        public string GetSchemaForCurrentPlatformToSetupPostprocess()
        {
            string schema  = null;
#if UNITY_ANDROID
            schema = $"{m_referralSchema}://{m_referralHost}";
#elif UNITY_IOS
            schema = m_referralSchema;
#endif
            return schema;
        }

        [Serializable]
        public class PlatformConfig
        {
            [SerializeField]
            private string m_appToken = null;

            public string AppToken
            {
                get => m_appToken;
                set => m_appToken = value;
            }
        }
    }
}