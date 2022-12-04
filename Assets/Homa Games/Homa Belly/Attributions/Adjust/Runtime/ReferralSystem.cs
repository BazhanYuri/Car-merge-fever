using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using HomaGames.HomaBelly.Utilities;
using UnityEngine;
using System.Linq;
using JetBrains.Annotations;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CheckNamespace
namespace HomaGames.HomaBelly
{
    public class ReferralSystem
    {
        private readonly string REFERRAL_API_ENDPOINT = HomaBellyConstants.API_HOST + "/referral?cv=" + HomaBellyConstants.API_VERSION
                                                               + "&av=" + Application.version
                                                               + "&ti={0}"
                                                               + "&ai=" + Application.identifier
                                                               + "&ua={1}"
                                                               + "&lruid={2}"
                                                               + "&oruid={3}";

        /// <summary>
        /// Oruid stand for original referral user id. The user which created and shared the deep link.
        /// </summary>
        private const string USER_ID_PARAM_NAME = "oruid=";
        private static readonly string USER_ID_SEARCH_PATTERN = $"{USER_ID_PARAM_NAME}oruid";
        private static readonly string USER_ID_REPLACE_PATTERN = $"{USER_ID_PARAM_NAME}{{0}}";
        private static string PREF_KEY_REFERRAL_INSTALL = "homagames_referral_install";
        private readonly TimeSpan m_queryTimeOutSeconds = TimeSpan.FromSeconds(10);
        private readonly TimeSpan m_initTimeOutSeconds = TimeSpan.FromSeconds(3);

        
        /// <summary>
        /// Cached Referral URL ready to be shared to invite users
        /// </summary>
        private string _referralUrl;
        
        /// <summary>
        /// Only links that match this schema://path will be processed.
        /// </summary>
        private string m_referralSchemaAndPath = null;
        private Task<Dictionary<string, object>> m_readingTrackingDataTask = null;
        private bool m_initialized = false; 

        public static event Action _referralInstallDetectedAction;
        
        /// <summary>
        /// Event invoked at any time if this install is attributed
        /// to Homas' Referral System. The time on determining
        /// its attribution is unknown and might happen within the first 60-120 seconds of the first
        /// game run <para />
        /// This will only be invoked <b>the very first time the game is run and the referral is detected</b>.
        /// To know if it was a referred install on successive runs, use <see cref="ReferralSystem.IsReferralAttributedInstall"/>
        /// </summary>
        public static event Action ReferralInstallDetectedAction
        {
            add
            {
                if (_referralInstallDetectedAction == null || !_referralInstallDetectedAction.GetInvocationList().Contains(value))
                {
                    _referralInstallDetectedAction += value;

                    // If already detected, invoke the callback upon registering
                    if (_referralAttributedInstall && _referralInstallDetectedAction != null)
                    {
                        _referralInstallDetectedAction();
                    }
                }
            }

            remove
            {
                if (_referralInstallDetectedAction != null && _referralInstallDetectedAction.GetInvocationList().Contains(value))
                {
                    _referralInstallDetectedAction -= value;
                }
            }
        }
        
        private static bool _referralAttributedInstall;
        
        /// <summary>
        /// True if this install is attributed to Homas' Referral System. The time on determining
        /// its attribution is unknown and might happen within the first 60-120 seconds of the first
        /// game run.<para />
        ///
        /// This will return true even in successive game runs
        /// </summary>
        public static bool IsReferralAttributedInstall => _referralAttributedInstall || PlayerPrefs.GetInt(PREF_KEY_REFERRAL_INSTALL) == 1;
        
        #region Singleton pattern

        private static ReferralSystem _instance;
        public static ReferralSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    // We don't want developers having null reference exceptions when they try to access the instance
                    // but if they query the referral data before the system is initialized, the tasks will wait for it internally
                    _instance = new ReferralSystem();
                }

                return _instance;
            }
        }

        private ReferralSystem()
        {
        }

        #endregion

        #region Public API
        
        public void Initialize(string schemaAndPath)
        {
            if (m_initialized)
            {
                return;
            }
            
            m_initialized = true;

            m_referralSchemaAndPath = schemaAndPath;

            // Read tracking data ASAP so when we do a query the data is available
            m_readingTrackingDataTask = ReadTrackingData();

            // Upon initialization, determine if current install was previously detected as referral
            _referralAttributedInstall = PlayerPrefs.GetInt(PREF_KEY_REFERRAL_INSTALL, 0) == 1;
            
            HomaGamesLog.Debug($"[ReferralSystem] Initialized. Previously Installed: {_referralAttributedInstall}");

            if (_referralAttributedInstall)
            {
                return;
            }
            
            Application.deepLinkActivated += OnDeepLinkResolved;
            if (!string.IsNullOrEmpty(Application.absoluteURL))
            {
                // Cold start and Application.absoluteURL not null so process Deep Link.
                OnDeepLinkResolved(Application.absoluteURL);
            }
        }
        
        /// <summary>
        /// This method will do an API call on every invocation.
        /// Return how many times other users installed the application when this user shared the deep link.
        /// If the referral system isn't enabled, it will return 0.
        /// </summary>
        public async Task<int> FetchTotalAttributedInstalls()
        {
            var model = await QueryAPI();
            if (model.Enabled)
            {
                return model.TotalAttributedInstalls;
            }

            return 0;
        }

        /// <summary>
        /// This method will do an API call only the very first time if the Referral System
        /// was not queried previously. Otherwise will return the cached value.
        /// Can return null or empty if the referral system is disabled.
        /// </summary>
        [CanBeNull]
        public async Task<string> GetReferralUrl()
        {
            // If any API query was done previously, _referralUrl will already be populated
            if (string.IsNullOrWhiteSpace(_referralUrl))
            {
                var model = await QueryAPI();
                if (model.Enabled)
                {
                    _referralUrl = model.ReferralUrlToShare; 
                }
            }

            return _referralUrl;
        }
        
        #endregion
 
        /// <summary>
        /// This should be called only when the application is opened after the installation when a deep link
        /// was used to get to the store.
        /// </summary>
        /// <param name="url"></param>
       private void OnDeepLinkResolved(string url) 
       {
           try
           {
               if (!url.Contains(m_referralSchemaAndPath))
                {
                    HomaGamesLog.Warning("[ReferralSystem] Invalid deep link. Expected schema: " + m_referralSchemaAndPath+" received: "+url);
                    return;
                }

                // Add code here to process the deeplink
                HomaGamesLog.Debug($"[ReferralSystem] Deep Link Resolved: {url}");

                string parameters = url.Substring(url.IndexOf('?'));

                if (string.IsNullOrEmpty(parameters))
                {
                    HomaGamesLog.Warning("[ReferralSystem] Invalid deep link. No parameters found.");
                    return;
                }

                // Extract from url the user id.
                // Example of adjust deeplink skr://referral?oruid=7df76edc-59a8-4934-8dee-b3b585952ec7&adjust_no_sdkclick=1
                string urlAfterUserIdParam = parameters.Substring(parameters.IndexOf(USER_ID_PARAM_NAME) + USER_ID_PARAM_NAME.Length);
                // 7df76edc-59a8-4934-8dee-b3b585952ec7&adjust_no_sdkclick=1
                var urlPieces = urlAfterUserIdParam.Split('&');
                // [0]=7df76edc-59a8-4934-8dee-b3b585952ec7
                // [1]=adjust_no_sdkclick=1
                string originalUserId = null;
                if (urlPieces != null && urlPieces.Length > 0)
                {
                    originalUserId = urlPieces[0];
                }
                else
                {
                    originalUserId = urlAfterUserIdParam;
                }

                if(string.IsNullOrEmpty(originalUserId))
                {
                    HomaGamesLog.Warning("[ReferralSystem] Invalid deep link. No original user id found.");
                    return;
                }

                if (!Guid.TryParse(originalUserId,out _))
                {
                    HomaGamesLog.Warning("[ReferralSystem] Valid user id with format GUID not found in the deep link.");
                    return;
                }
                
                HomaGamesLog.Warning($"[ReferralSystem] Referral original user: {originalUserId}");
                HomaBelly.Instance.TrackDesignEvent($"ReferralResolved:{originalUserId}");
                
                NotifyReferredInstall(originalUserId);
                _referralAttributedInstall = true;
                _referralInstallDetectedAction?.Invoke();
                
                // Save this install is a referral one for further executions
                PlayerPrefs.SetInt(PREF_KEY_REFERRAL_INSTALL, 1);
                PlayerPrefs.Save();
           }
           catch (Exception exception)
           {
               HomaGamesLog.Warning("[ReferralSystem] Error while processing deep link: " + exception);
           }
       }

        private async void NotifyReferredInstall(string originReferralUid)
        {
            await QueryAPI(originReferralUid);
        }
        
        private async Task<Dictionary<string, object>> ReadTrackingData()
        {
            var result =
                await FileUtilities.LoadAndDeserializeJsonFromResources<Dictionary<string, object>>(RemoteConfigurationConstants.TRACKING_FILE_RESOURCES_PATH);

            return result;
        }

        #region Query Methods

        private async Task<ReferralModel> QueryAPI(string originReferralUid = "")
        {
            var referralModel = await WaitFor(() => Identifiers.HomaGamesId != null,
                "[ReferralSystem] HomaGamesId is null or empty.");

            if (referralModel?.Error != null)
            {
                return referralModel;
            }
            
            referralModel = await WaitFor(() => m_initialized,
                $"[ReferralSystem] Couldn't complete query because the {nameof(ReferralSystem)} isn't initialized.");

            if (referralModel?.Error != null)
            {
                return referralModel;
            }

            using (HttpClient client = HttpCaller.GetHttpClient())
            {
                client.Timeout = m_queryTimeOutSeconds;
                string homaBellyToken = "";

                // Just in case the user does the query very quickly, wait until tracking data is read
                var trackingData = await m_readingTrackingDataTask;
                
                if (trackingData != null && trackingData.ContainsKey("ti"))
                {
                    homaBellyToken = trackingData["ti"] as string;
                }
                
                if(string.IsNullOrEmpty(homaBellyToken))
                {
                    HomaGamesLog.Error($"[ReferralSystem] homaBellyToken is null or empty. {trackingData} {trackingData?.ContainsKey("ti")}");
                }

                string url = string.Format(REFERRAL_API_ENDPOINT, homaBellyToken, UserAgent.GetUserAgent(), Identifiers.HomaGamesId, originReferralUid);
                HomaGamesLog.Debug($"[ReferralSystem] Querying API {url}");
                
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    return await DeserializeReferralModel(response);
                }
                catch (Exception e)
                {
                    HomaGamesLog.Warning($"[ReferralSystem] ERROR: {e}");
                    ReferralModel model = new ReferralModel();
                    ReferralModel.ReferralError error = new ReferralModel.ReferralError();
                    error.Code = 001;
                    error.Message = e.Message;
                    model.Error = error;
                    return model;
                }
            }
        }

        private async Task<ReferralModel> WaitFor(Func<bool> conditionToWait,string errorMessage)
        {
            var timeStamp = DateTime.Now;
            while (!conditionToWait())
            {
                await Task.Delay(50);
                
                if(DateTime.Now - timeStamp > m_initTimeOutSeconds)
                {
                    HomaGamesLog.Warning($"[ReferralSystem] ERROR: Time waiting timeout: {errorMessage}");
                    var model = new ReferralModel();
                    var error = new ReferralModel.ReferralError
                    {
                        Code = 002,
                        Message = errorMessage
                    };
                    model.Error = error;
                    return model;
                }
            }

            return null;
        }
        
        private async Task<ReferralModel> DeserializeReferralModel(HttpResponseMessage response)
        {
            var model = new ReferralModel();
            ReferralModel.ReferralError error = new ReferralModel.ReferralError();
            
            // Failure
            if (response == null)
            {
                error.Code = 000;
                error.Message = "Response is empty";
                model.Error = error;
                HomaGamesLog.Warning($"[ReferralSystem] ERROR: {error.Message}");
                return model;
            }

            if (!response.IsSuccessStatusCode)
            {
                error.Code = (int) response.StatusCode;
                error.Message = response.ReasonPhrase;
                model.Error = error;
                HomaGamesLog.Warning($"[ReferralSystem] ERROR: {error.Message}");
                return model;
            }

            // Success
            string resultString = await response.Content.ReadAsStringAsync();
            HomaGamesLog.Debug($"[ReferralSystem] API queried successfully. Response: {resultString}");
            
            Dictionary<string, object> dictionary = Json.Deserialize(resultString) as Dictionary<string, object>;
            if (dictionary != null)
            {
                if (dictionary.ContainsKey("res"))
                {
                    Dictionary<string, object> resDictionary = (Dictionary<string, object>)dictionary["res"];
                    if (resDictionary != null)
                    {

                        Dictionary<string, object> referralDictionary = (Dictionary<string, object>) resDictionary["o_ref"];
                        if (referralDictionary != null)
                        {
                            if (referralDictionary.ContainsKey("b_enabled"))
                            {
                                bool enabled = Convert.ToBoolean(referralDictionary["b_enabled"].ToString(),
                                    CultureInfo.InvariantCulture);
                                model.Enabled = enabled;
                            }

                            if (referralDictionary.ContainsKey("s_referral_url_with_macros"))
                            {
                                model.ReferralUrlWithMacros = referralDictionary["s_referral_url_with_macros"] as string;
                                
                                if (!string.IsNullOrWhiteSpace(model.ReferralUrlWithMacros))
                                {
                                    var unescapedUrl = Uri.UnescapeDataString(model.ReferralUrlWithMacros);
                                    if(unescapedUrl.Contains(USER_ID_SEARCH_PATTERN))
                                    {
                                        model.ReferralUrlToShare = unescapedUrl.Replace(USER_ID_SEARCH_PATTERN,
                                            string.Format(USER_ID_REPLACE_PATTERN, Identifiers.HomaGamesId));
                                    }
                                    else
                                    {
                                        HomaGamesLog.Warning($"[ReferralSystem] Warning: No pattern found in the referral url to replace with the user id. The deeplink will work but install won't be added to the user.");
                                    }
                                }
                            }

                            if (referralDictionary.ContainsKey("i_total_attributed_installs"))
                            {
                                model.TotalAttributedInstalls =
                                    Convert.ToInt32(referralDictionary["i_total_attributed_installs"]);
                            }
                        }
                    }
                }
            }
            
            return model;
        }
        
        #endregion
      
        
        public class ReferralModel
        {
            public bool Enabled { get; set; }
            [CanBeNull]
            public string ReferralUrlWithMacros { get; set; }
            public string ReferralUrlToShare { get; set; }
            public int TotalAttributedInstalls { get; set; }
            [CanBeNull]
            public ReferralError Error { get; set; }
            
            public class ReferralError
            {
                public string Message { get; set; }
                public int Code { get; set; }

                public override string ToString()
                {
                    return $"{nameof(Message)}: {Message}, {nameof(Code)}: {Code}";
                }
            }

            public override string ToString()
            {
                return $"{nameof(Enabled)}: {Enabled}, {nameof(ReferralUrlWithMacros)}: {ReferralUrlWithMacros}, {nameof(ReferralUrlToShare)}: {ReferralUrlToShare}, {nameof(TotalAttributedInstalls)}: {TotalAttributedInstalls}, {nameof(Error)}: {Error}";
            }
        }
    }
}
