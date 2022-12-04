using System;
using HomaGames.Geryon;
using HomaGames.HomaBelly.Utilities;
using JetBrains.Annotations;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public class ConfigurationResponseDeserializer : IModelDeserializer<GeryonConfigurationModel>
    {
        [NotNull]
        public GeryonConfigurationModel Deserialize(string json)
        {
            try
            {
                if (string.IsNullOrEmpty(json))
                {
                    // Return empty but not null model
                    return new GeryonConfigurationModel();
                }
                
                JsonObject jsonObject = Json.DeserializeObject(json);
                if (jsonObject != null && jsonObject.TryGetJsonObject("res", out var resultObject))
                {

                    if (resultObject.TryGetJsonObject("o_geryon", out var geryonData))
                    {
                        return GeryonConfigurationModel.FromServerResponse(geryonData);
                    }
                }
            }
            catch (Exception e)
            {
                HomaGamesLog.Warning($"[N-Testing] Could not obtain N-Testing data: {e}");
            }
        
            // Return empty but not null model
            return new GeryonConfigurationModel();
        }

        public GeryonConfigurationModel LoadFromCache()
        {
            throw new System.NotImplementedException();
        }
    }
}