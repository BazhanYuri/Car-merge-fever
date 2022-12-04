using System;
using System.Collections.Generic;
using System.Text;
using HomaGames.HomaBelly.Utilities;

namespace HomaGames.HomaBelly
{
    [Serializable]
    public class HomaAnalyticsEventResponse
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
        
        public static HomaAnalyticsEventResponse Deserialize(string json)
        {
            HomaAnalyticsEventResponse responseModel = new HomaAnalyticsEventResponse();

            // Return empty manifest if json string is not valid
            if (string.IsNullOrEmpty(json))
            {
                return responseModel;
            }

            // Basic info
            Dictionary<string, object> dictionary = Json.Deserialize(json) as Dictionary<string, object>;
            if (dictionary != null)
            {
                responseModel.AppToken = (string)dictionary["ti"];
                responseModel.AppIdentifier = (string)dictionary["ai"];

                // Res dictionary
                Dictionary<string, object> resDictionary = dictionary["res"] as Dictionary<string, object>;
                if (resDictionary != null)
                {
                    
                    if (resDictionary.ContainsKey("s_event_name"))
                    {
                        responseModel.EventName = resDictionary["s_event_name"] as string;    
                    }
                    
                    // Gather all incoming "s_event_value_N"
                    int valueIndex = 0;
                    string eventValueKey = $"s_event_value_{valueIndex}";
                    while (resDictionary.ContainsKey(eventValueKey))
                    {
                        responseModel.EventValues.Add(resDictionary[eventValueKey] as string);
                        valueIndex++;
                        eventValueKey = $"s_event_value_{valueIndex}";
                    }
                }
            }

            return responseModel;
        }
    }
}