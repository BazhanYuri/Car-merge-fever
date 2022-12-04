using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HomaGames.HomaBelly
{
    public static class HomaAnalyticsEventRecorderDebugTool
    {
         #region CSV Debugging Tool
        
        // Temp code to generate debug fields

        private const char SEPARATOR = ',';
        private const int EVENT_VALUES_MAX_QUANTITY = 50;
        private static readonly HashSet<string> m_eventsIdRegister = new HashSet<string>();

        public static void RecordInCsv(string persistentDataPath,
            string sessionId,
            string eventId,
            in Dictionary<string, object> dictionary)
        {
            try
            {
                if (m_eventsIdRegister.Contains(eventId))
                {
                    return;
                }
            
                m_eventsIdRegister.Add(eventId);

                // Create a copy of the dictionary, we are going to modify it
                var dictionaryCopy = new Dictionary<string, object>(dictionary);

                var fileName = $"HomaAnalyticsEvents_Session_{sessionId}.csv";
                var pathToCsv = Path.Combine(persistentDataPath, fileName);

                StreamWriter stream = null;
                if (!File.Exists(pathToCsv))
                {
                    // Because the content of the field evv varies for each type of event,
                    // we add generic headers that will work for all events
                    var headers = dictionaryCopy.Select(d => d.Key);
                    var headersList = new List<string>(headers);
                    for (int i = 0; i < EVENT_VALUES_MAX_QUANTITY; i++)
                    {
                        headersList.Add("evv" + i);
                    }
                    
                    stream = new StreamWriter(pathToCsv,true,Encoding.UTF8);
                    var headerRow = string.Join(SEPARATOR.ToString(), headersList);
                    stream.WriteLine(headerRow);
                }
                else
                {
                    stream = new StreamWriter(pathToCsv,true,Encoding.UTF8);
                }
                
                // Unroll the evv dictionary 
                if (dictionaryCopy.TryGetValue("evv", out var evv))
                {
                    dictionaryCopy["evv"] = "Unrolled";
                    
                    var valuesDictionary = evv as Dictionary<string, object>;
                    
                    if (valuesDictionary.Count > EVENT_VALUES_MAX_QUANTITY)
                    {
                        HomaGamesLog.Warning("HomaAnalyticsEventRecorderDebugTool "+
                            $"The event {eventId} has more than {EVENT_VALUES_MAX_QUANTITY} evv values, " +
                            $"the rest will be added without header.");    
                    }

                    // Add values to dictionary
                    foreach (var value in valuesDictionary)
                    {
                        dictionaryCopy.Add(value.Key, value.Value);
                    }
                }

                // Create CSV line
                var values = dictionaryCopy.Select(d => d.Value != null ? d.Value.ToString().Replace(',','.') : "null");
                var csvLine = string.Join(SEPARATOR.ToString(), values);
                stream.WriteLine(csvLine);
                // Guarantee that the file is written to disk in the editor
                stream.Close();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[ERROR] Exception in the events record tool: " + e);
            }
        }
        
        #endregion
    }
}