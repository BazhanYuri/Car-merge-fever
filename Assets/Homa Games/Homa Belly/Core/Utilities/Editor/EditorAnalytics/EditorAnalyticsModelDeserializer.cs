#if HOMA_BELLY_EDITOR_ANALYTICS_ENABLED
using System;
using System.Collections.Generic;
using System.Text;
using HomaGames.HomaBelly.Utilities;

namespace HomaGames.HomaBelly
{
    public class EditorAnalyticsModelDeserializer
    {
        public EditorAnalyticsResponseModel Deserialize(string json)
        {
            EditorAnalyticsResponseModel responseModel = new EditorAnalyticsResponseModel();

            // Return empty manifest if json string is not valid
            if (string.IsNullOrEmpty(json))
            {
                return responseModel;
            }

            // Basic info
            JsonObject inputObject = Json.DeserializeObject(json);
            
            inputObject.TryGetString("ti", out responseModel.AppToken);
            inputObject.TryGetString("ai", out responseModel.AppIdentifier);

            // Res dictionary
            if (inputObject.TryGetJsonObject("res", out var resultObject))
            {
                if (resultObject.TryGetString("s_event_name", out var eventName))
                    responseModel.EventName = eventName;
                
                // Gather all incoming "s_event_value_N"
                int valueIndex = 0;
                string eventValueKey = $"s_event_value_{valueIndex}";
                while (resultObject.TryGetString(eventValueKey, out var eventValue))
                {
                    responseModel.EventValues.Add(eventValue);
                    valueIndex++;
                    eventValueKey = $"s_event_value_{valueIndex}";
                }
            }

            return responseModel;
        }
    }

    [Serializable]
    public class EditorAnalyticsResponseModel
    {
        //[JsonProperty("ti")]
        public string AppToken;
        //[JsonProperty("ai")]
        public string AppIdentifier;
        //[JsonProperty("res/s_event_name")]
        public string EventName;
        //[JsonProperty("res/s_event_value_N")]
        public List<string> EventValues = new List<string>();

        public override string ToString()
        {
            return new StringBuilder().AppendLine()
                .Append("App ID: ").AppendLine(AppIdentifier)
                .Append("Token: ").AppendLine(AppToken)
                .Append("Event Name: ").AppendLine(EventName)
                .Append("Event Values: ").AppendLine(Json.Serialize(EventValues))
                .ToString();
        }
    }
}
#endif