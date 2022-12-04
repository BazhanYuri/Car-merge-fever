using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HomaGames.Geryon;
using HomaGames.HomaBelly.Utilities;
using JetBrains.Annotations;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Class used to fetch Damysus Remote Configuration.
    ///
    /// By sending to the server some useful information about Damysus
    /// configuration (app token, app identifier and dependencies), the
    /// server will return a configuration for the app at runtime
    /// </summary>
    public static class RemoteConfiguration
    {
        private static readonly string FIRST_TIME_CONFIG_ENDPOINT = $"{HomaBellyConstants.API_HOST}/appfirsttime"; 
        private static readonly string EVERY_TIME_CONFIG_ENDPOINT = $"{HomaBellyConstants.API_HOST}/appeverytime";

        private const int TimeoutDelayMs = 3000;

        private static string advertisingID;

        /// <summary>
        /// Response model fetched from Remote Configuration endpoints
        /// </summary>
        public abstract class RemoteConfigurationModel
        {
            public string AppToken;
            public CrossPromotionConfigurationModel CrossPromotionConfigurationModel;
            public GeryonConfigurationModel GeryonConfigurationModel;
        }
        
        public class RemoteConfigurationModelFirstTime : RemoteConfigurationModel 
        { }

        public class RemoteConfigurationModelEveryTime : RemoteConfigurationModel
        {
            public ForceUpdateConfigurationModel ForceUpdateConfigurationModel;
            public AttributionConfigurationModel AttributionConfigurationModel;
        }

        public static RemoteConfigurationModelFirstTime FirstTime;
        public static RemoteConfigurationModelEveryTime EveryTime;

        #region Public methods

        private static bool? _firstTimeConfigurationNeededCache;
        public static bool FirstTimeConfigurationNeededThisSession()
        {
            if (_firstTimeConfigurationNeededCache == null)
                _firstTimeConfigurationNeededCache =
                    PlayerPrefs.GetInt(RemoteConfigurationConstants.FIRST_TIME_ALREADY_REQUESTED, 0) == 0;

            return _firstTimeConfigurationNeededCache.Value;
        }

        public static void PrepareRemoteConfigurationFetching()
        {
            try
            {
                if (FirstTimeConfigurationNeededThisSession())
                {
                    PlayerPrefs.SetInt(RemoteConfigurationConstants.FIRST_TIME_ALREADY_REQUESTED, 1);
                    PlayerPrefs.Save();
                }
            }
            catch (Exception e)
            {
                HomaGamesLog.Warning($"[Remote Configuration] Could not prepare for remote configuration fetching: {e}");
            }
            
            // At this point, Identifiers are set thanks to CoreInitializer.
#if UNITY_IOS
			advertisingID = Identifiers.Idfa;
#else
            advertisingID = Identifiers.Gaid;
#endif
        }

        #endregion

        #region Private helpers

        private static string GenerateRequestUri(string endpoint, string ti, string mvi)
        {
            return UriHelper.AddGetParameters(endpoint, new Dictionary<string, string>
            {
                {"cv", HomaBellyConstants.API_VERSION}, // Configuration version
                {"ti", ti}, // Token Identifier
                {"av", Application.version}, // Application version
                {"sv", HomaBellyConstants.PRODUCT_VERSION}, // SDK version
                {"ua", SystemConstants.UserAgent}, // User agent
                {"ai", SystemConstants.ApplicationIdentifier}, // Application identifier
                {"mvi", mvi}, // Manifest version ID
                {"di", advertisingID} // device advertising ID
            });
        }

        public static async Task GetFirstTimeConfiguration()
        {
            if (! FirstTimeConfigurationNeededThisSession())
            {
                Debug.LogError($"{nameof(GetFirstTimeConfiguration)} called in a non-first session.");
                return;
            }

            HomaBellyManifestConfiguration.TryGetString(out var ti, HomaBellyManifestConfiguration.MANIFEST_TOKEN_KEY);
            HomaBellyManifestConfiguration.TryGetString(out var mvi, HomaBellyManifestConfiguration.MANIFEST_VERSION_ID_KEY);
            string firstTimeUri = GenerateRequestUri(FIRST_TIME_CONFIG_ENDPOINT, ti, mvi);
            HomaGamesLog.Debug($"[Remote Configuration] Requesting first time config {firstTimeUri}...");

            FirstTime = ParseFirstTimeConfiguration(await Get(firstTimeUri));
            HomaGamesLog.Debug($"[Remote Configuration] first time config fetched");
        }
        
        public static async Task GetEveryTimeConfiguration()
        {
            HomaBellyManifestConfiguration.TryGetString(out var ti, HomaBellyManifestConfiguration.MANIFEST_TOKEN_KEY);
            HomaBellyManifestConfiguration.TryGetString(out var mvi, HomaBellyManifestConfiguration.MANIFEST_VERSION_ID_KEY);
            string everyTimeUri = GenerateRequestUri(EVERY_TIME_CONFIG_ENDPOINT, ti, mvi);
            HomaGamesLog.Debug($"[Remote Configuration] Requesting every time config {everyTimeUri}...");

            EveryTime = ParseEveryTimeConfiguration(await Get(everyTimeUri));
            HomaGamesLog.Debug($"[Remote Configuration] every time config fetched");
        }


        /// <summary>
        /// Asynchronous Http GET request
        /// </summary>
        /// <param name="uri">The URI to query</param>
        /// <returns></returns>
        private static async Task<JsonObject> Get(string uri)
        {
            using (HttpClient client = HttpCaller.GetHttpClient())
            {
                try
                {
                    HttpResponseMessage response = await GetAsyncWithRetries(client, uri, 3);

                    if (response == null)
                    {
                        return JsonObject.Empty;
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        string resultString = await response.Content.ReadAsStringAsync();

                        // Return empty manifest if json string is not valid
                        if (string.IsNullOrEmpty(resultString))
                        {
                            return default;
                        }

                        // Basic info
                        JsonObject jsonObject = await Task.Run(() => Json.DeserializeObject(resultString));

                        HomaGamesLog.Debug($"[Remote Configuration] Request result to {uri}\n {resultString}");
                        return jsonObject;
                    }
                }
                catch (Exception e)
                {
                    HomaGamesLog.Error($"[Remote Configuration] Exception while requesting {uri}: {e}");
                }
            }

            return default;
        }

        [ItemCanBeNull]
        private static async Task<HttpResponseMessage> GetAsyncWithRetries(HttpClient client, string uri, int attempts)
        {
            for (int i = 0; i < attempts; i++)
            {
                Task<HttpResponseMessage> request = client.GetAsync(uri);
                
                Task firstTaskToFinish = await Task.WhenAny(request, Task.Delay(TimeoutDelayMs));
                Exception error = firstTaskToFinish.Exception; // if any


                if (error == null && firstTaskToFinish == request)
                {
                    return request.Result;
                }
                else
                {
                    if (error != null)
                        HomaGamesLog.Error("Error while fetching remote configuration: " + error);
                    
                    if (i == attempts - 1)
                        HomaGamesLog.Error($"Could not fetch remote configuration at \"{uri}\". Moving on.");
                    else
                        HomaGamesLog.Warning($"Could not fetch remote configuration on attempt {i+1}. Retrying...");
                    
                    client.CancelPendingRequests();
                }
            }

            return null;
        }
        
        private static RemoteConfigurationModelFirstTime ParseFirstTimeConfiguration(JsonObject rawResponse)
        {
            var output = ParseConfigurationCommon<RemoteConfigurationModelFirstTime>(rawResponse);
            
            // First time specific parsing

            return output;
        }
        
        private static RemoteConfigurationModelEveryTime ParseEveryTimeConfiguration(JsonObject rawResponse)
        {
            var output = ParseConfigurationCommon<RemoteConfigurationModelEveryTime>(rawResponse);
            
            if (rawResponse != null)
            {
                if (rawResponse.TryGetJsonObject("res", out var resultObject))
                {

                    resultObject.TryGetJsonObject("o_force_update", forceUpdateData =>
                    {
                        output.ForceUpdateConfigurationModel =
                            ForceUpdateConfigurationModel.FromServerResponse(forceUpdateData);
                    });

                    resultObject.TryGetJsonObject("o_attributions", attributionConfigurationData =>
                    {
                        output.AttributionConfigurationModel =
                            AttributionConfigurationModel.FromRemoteConfigurationDictionary(
                                attributionConfigurationData);
                    });
                }
            }

            return output;
        }

        private static TModel ParseConfigurationCommon<TModel>(JsonObject rawResponse)
            where TModel : RemoteConfigurationModel, new()
        {
            var output = new TModel();

            if (rawResponse != null)
            {
                rawResponse.TryGetString("ti", s => output.AppToken = s);


                if (rawResponse.TryGetJsonObject("res", out var resultObject))
                {
                    resultObject.TryGetJsonObject("o_cross_promotion", crossPromotionData =>
                    {
                        output.CrossPromotionConfigurationModel = CrossPromotionConfigurationModel.FromRemoteConfigurationDictionary(crossPromotionData);
                    });
                    
                    resultObject.TryGetJsonObject("o_geryon", geryonData =>
                    {
                        output.GeryonConfigurationModel =
                            GeryonConfigurationModel.FromServerResponse(geryonData);
                    });
                }
            }

            return output;
        }
#endregion
    }
}
