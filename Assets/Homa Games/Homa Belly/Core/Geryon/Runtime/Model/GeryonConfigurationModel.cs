using System.Collections.Generic;
using HomaGames.HomaBelly;
using HomaGames.HomaBelly.Utilities;

namespace HomaGames.Geryon
{
    /// <summary>
    /// Model class holding server response for all Geryon requests.
    /// Some of this properties might be empty depending on the
    /// request (hgappbase, hgappfirsttime or hgappeerytime)
    ///
    /// This model represents the content of the "o_geryon_results" object within
    /// the server response
    /// </summary>
    public class GeryonConfigurationModel
    {
        private const string GERYON_RESULTS = "o_geryon_results";
        private const string STATUS = "status";
        private const string MESSAGE = "message";
        private const string RESULT_FIELD = "res";
        private const string S_SCOPE_ID = "s_scope_id";
        private const string S_VARIANT_ID = "s_variant_id";
        private const string S_OVERRIDE_ID = "s_override_id";
        private const string O_CONF = "o_conf";
        private const string S_TOKEN_0 = "s_token_0";
        private const string S_TOKEN_1 = "s_token_1";
        private const string S_TOKEN_2 = "s_token_2";
        private const string S_TOKEN_3 = "s_token_3";
        private const string S_TOKEN_4 = "s_token_4";
        private const string E_GERYON_STATUS = "e_geryon_status";


        public string Status { get; private set; }
        //[JsonProperty("s_scope_id")]
        public string ScopeId { get; private set; }
        //[JsonProperty("s_variant_id")]
        public string VariantId { get; private set; }
        //[JsonProperty("s_override_id")]
        public string OverrideId { get; private set; }
        //[JsonProperty("o_conf")]
        public Dictionary<string, object> Configuration { get; private set; }

        #region External tokens
        //[JsonProperty("s_token_0")]
        public string ExternalToken0 { get; private set; }
        //[JsonProperty("s_token_1")]
        public string ExternalToken1 { get; private set; }
        //[JsonProperty("s_token_2")]
        public string ExternalToken2 { get; private set; }
        //[JsonProperty("s_token_3")]
        public string ExternalToken3 { get; private set; }
        //[JsonProperty("s_token_4")]
        public string ExternalToken4 { get; private set; }
        #endregion

        public static GeryonConfigurationModel FromServerResponse(JsonObject deserializedJson)
        {
            GeryonConfigurationModel geryonConfigurationModel = new GeryonConfigurationModel();
     
            ////-
#if HOMA_BELLY_GERYON_ENABLED
            if (deserializedJson == null)
            {
                HomaGamesLog.Warning($"[N-Testing] Status unknown, data is null.");
                return geryonConfigurationModel;
            }

            if (deserializedJson.TryGetString(E_GERYON_STATUS, out string status))
            {
                geryonConfigurationModel.Status = status;

                if (geryonConfigurationModel.Status == "OK")
                {
                    HomaGamesLog.Debug($"[N-Testing] Status OK, reading data");
                }
                else
                {
                    string resultStatus = null;
                    string resultMessageCode = null;
                    if (deserializedJson.TryGetJsonObject(GERYON_RESULTS, out var result))
                    {
                        if (!result.TryGetString(STATUS, out resultStatus))
                            resultStatus = null;
                        
                        if (!result.TryGetString(MESSAGE, out resultMessageCode))
                            resultMessageCode = null;
                    }

                    HomaGamesLog.Error(
                        $"[N-Testing] Error while fetching data. Status \"{resultStatus}\", " +
                        $"with message: \"{DamysusApiUtils.GetMessageFromCode(resultMessageCode) ?? resultMessageCode}\"");
                    return geryonConfigurationModel;
                }
            }

            if (deserializedJson.TryGetJsonObject(GERYON_RESULTS, out var geryonResults))
            {
                if (geryonResults != null && geryonResults.TryGetJsonObject(RESULT_FIELD, out var resJson))
                {
                    if (resJson != null && resJson.TryGetJsonObject(O_CONF, out var configuration))
                    {
                        geryonConfigurationModel.Configuration = configuration.ToRawData();
                        
                        // Obtain IDs
                        resJson.TryGetString(S_SCOPE_ID, 
                            id => geryonConfigurationModel.ScopeId = id);
                        resJson.TryGetString(S_VARIANT_ID, 
                            id => geryonConfigurationModel.VariantId = id);
                        resJson.TryGetString(S_OVERRIDE_ID, 
                            id => geryonConfigurationModel.OverrideId = id);

                        // Obtain external tokens
                        resJson.TryGetString(S_TOKEN_0, 
                            token => geryonConfigurationModel.ExternalToken0 = token);
                        resJson.TryGetString(S_TOKEN_1, 
                            token => geryonConfigurationModel.ExternalToken1 = token);
                        resJson.TryGetString(S_TOKEN_2, 
                            token => geryonConfigurationModel.ExternalToken2 = token);
                        resJson.TryGetString(S_TOKEN_3, 
                            token => geryonConfigurationModel.ExternalToken3 = token);
                        resJson.TryGetString(S_TOKEN_4, 
                            token => geryonConfigurationModel.ExternalToken4 = token);

                        return geryonConfigurationModel;
                    }

                    HomaGamesLog.Error(
                        $"Configuration field \"{O_CONF}\" could not be found in the Geryon data.");
                    return geryonConfigurationModel;
                }

                HomaGamesLog.Error(
                    $"Result field \"{RESULT_FIELD}\" could not be found in the Geryon data.");
                return geryonConfigurationModel;
            }

            HomaGamesLog.Error(
                $"Result field \"{GERYON_RESULTS}\" could not be found in the Geryon data.");
#endif
            return geryonConfigurationModel;
        }
    }
}