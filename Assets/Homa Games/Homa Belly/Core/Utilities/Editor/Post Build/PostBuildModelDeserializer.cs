using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HomaGames.HomaBelly.Utilities;
using JetBrains.Annotations;

namespace HomaGames.HomaBelly
{
    public class PostBuildModelDeserializer : IModelDeserializer<PostBuildModel>
    {
        [NotNull]
        public PostBuildModel Deserialize(string json)
        {
            PostBuildModel model = new PostBuildModel();

            // Return empty manifest if json string is not valid
            if (string.IsNullOrEmpty(json))
            {
                return model;
            }

            // Basic info
            JsonObject inputObject = Json.DeserializeObject(json);
            
            if (inputObject.TryGetString("ti", out var token)) 
                model.AppToken = token;

            
            if (inputObject.TryGetJsonObject("res", out var resultObject))
            {
                if (resultObject.TryGetJsonList("as_skadnetwork_ids", out var adNetworkIdJsonList))
                {
                    model.SkAdNetworkIds = adNetworkIdJsonList.ToRawData()
                        .Select(x => x.ToString())
                        .ToArray();
                }
            }

            return model;
        }

        public PostBuildModel LoadFromCache()
        {
            // NO-OP
            return default;
        }
    }

    [Serializable]
    public class PostBuildModel
    {
        //[JsonProperty("as_skadnetwork_ids")]
        public string[] SkAdNetworkIds;
        //[JsonProperty("ti")]
        public string AppToken;
    }
}
