using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using HomaGames.HomaBelly;
using HomaGames.HomaBelly.Utilities;
using UnityEngine;

namespace HomaGames.HomaBelly.DataPrivacy
{
    public class PrivacyResponse
    {
        private const string API_PRIVACY_REGION_ENDPOINT = HomaBellyConstants.API_HOST + "/privacy";

        //[JsonProperty("s_country_code")]
        public string CountryCode;
        //[JsonProperty("s_region")]
        public string Region;
        //[JsonProperty("s_law_name")]
        public string LawName;

        /// <summary>
        /// By default, never protected
        /// </summary>
        //[JsonProperty("b_protected")]
        public bool Protected = false;

        private static string GeneratePrivacyRegionUrl(string ti, string userAgent)
        {
            return UriHelper.AddGetParameters(API_PRIVACY_REGION_ENDPOINT, new Dictionary<string, string>
            {
                {"cv", HomaBellyConstants.API_VERSION},
                {"sv", HomaBellyConstants.PRODUCT_VERSION},
                {"ti", ti},
                {"ai", Application.identifier},
                {"ua", userAgent}
            });
        }
        
        public static async Task<PrivacyResponse> FetchPrivacyForCurrentRegion()
        {
            PrivacyResponse privacyResponse = new PrivacyResponse();

            // If network is not reachable, return default PrivacyResponse, which is
            // protected
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                return privacyResponse;
            }
            
            using (HttpClient client = HttpCaller.GetHttpClient())
            {
                HomaBellyManifestConfiguration.TryGetString(out var ti, HomaBellyManifestConfiguration.MANIFEST_TOKEN_KEY);
                string url = GeneratePrivacyRegionUrl(ti, GetUserAgent());
                HttpResponseMessage response = null;
                string resultString = "";
                try
                {
                    Task<HttpResponseMessage> getTask = client.GetAsync(url);
                    var firstTaskToFinish = await Task.WhenAny(getTask, Task.Delay(3000));
                    
                    if (firstTaskToFinish == getTask)
                    {
                        response = getTask.Result;
                        if (response != null && response.IsSuccessStatusCode)
                        {
                            resultString = await response.Content.ReadAsStringAsync();
                        }
                    }
                    else
                    {
                        HomaGamesLog.Warning("[Privacy] Privacy region configuration fetching took " +
                                             "too long. Aborting.");
                        return privacyResponse;
                    }
                }
                catch (Exception e)
                {
                    HomaGamesLog.Warning($"[Privacy] Could not fetch privacy region configuration. ERROR: {e.Message}");
                    return privacyResponse;
                }

                // Return empty manifest if json string is not valid
                if (string.IsNullOrEmpty(resultString))
                {
                    return privacyResponse;
                }

                // Basic info
                JsonObject resultObject = await Task.Run(() => Json.DeserializeObject(resultString));
                if (resultObject.TryGetJsonObject("res", out var responseObject))
                {
                    responseObject.TryGetString("s_country_code", s => privacyResponse.CountryCode = s);
                    responseObject.TryGetString("s_region", s => privacyResponse.Region = s);
                    responseObject.TryGetString("s_law_name", s => privacyResponse.LawName = s);
                    responseObject.TryGetBool("b_protected", b => privacyResponse.Protected = b);
                }

                return privacyResponse;
            }
        }

        /// <summary>
        /// Obtain the User Agent to be sent within the requests
        /// </summary>
        /// <returns></returns>
        private static string GetUserAgent()
        {
            string userAgent = "ANDROID";
#if UNITY_IOS
            userAgent = "IPHONE";
            try
            {
                if (UnityEngine.iOS.Device.generation.ToString().Contains("iPad"))
                {
                    userAgent = "IPAD";
                }
            }
            catch (Exception e)
            {
                HomaGamesLog.Warning($"Could not determine iOS device generation: ${e.Message}");
            }
            
#endif
            return userAgent;
        }
    }
}
