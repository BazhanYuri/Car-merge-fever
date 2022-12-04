using System;
using System.Collections.Generic;
using System.Globalization;
using HomaGames.HomaBelly.Utilities;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Base class for all Homa Analytics API queries: editor and runtime.
    /// </summary>
    public abstract class EventApiQueryModel : ApiQueryModel
    {
        public static readonly string EVENT_API_ENDPOINT = HomaBellyConstants.API_HOST + "/appevent";

        /// <summary>
        /// Install id, doesn't change while the game remains installed: 4567abcd
        /// </summary>
        private static readonly string INSTALL_ID = "iid";
        
        /// <summary>
        /// Session Id. It would change every time the game opens or after returning from a period in background. 
        /// </summary>
        private static readonly string SESSION_ID = "sid";
        
        /// <summary>
        /// Event unique identifier
        /// </summary>
        private static readonly string EVENT_UNIQUE_ID = "eid";
        
        /// <summary>
        /// N-testing id 
        /// </summary>
        private static readonly string NTESTING_ID = "ntesting_id";
        
        /// <summary>
        /// N-testing override id
        /// </summary>
        private static readonly string NTESTING_OVERRIDE_ID = "ntesting_oid";
        
        /// <summary>
        /// Event client timestamp
        /// </summary>
        private static readonly string CLIENT_TIME_STAMP = "ect";
        
        /// <summary>
        /// Event client timezone 
        /// </summary>
        private static readonly string CLIENT_TIME_ZONE = "ectz";
        
        /// <summary>
        /// Event category
        /// </summary>
        private static readonly string EVENT_CATEGORY = "evc";
        
        /// <summary>
        /// Event name
        /// </summary>
        private static readonly string EVENT_NAME = "evn";
        
        /// <summary>
        /// We will store generic information in this field in the way of key:value json.
        /// </summary>
        private static readonly string EVENT_VALUES = "evv";

        public string InstallId { get; set; }
        public string SessionId { get; set; }
        public string NTestingId { get; set; }
        public string NTestingOverrideId { get; set; }
        /// <summary>
        /// Unique event id
        /// </summary>
        public string EventId { get; }
        
        // Frequently changed from event to event
        public string EventCategory { get; protected set; }
        public string EventName { get; protected set; }
        
        /// <summary>
        /// Stores utc timestamp when the event was created.
        /// </summary>
        public string CreatedTimeStamp { get; }
        
        protected Dictionary<string, object> EventValues = new Dictionary<string, object>();

        public EventApiQueryModel()
        {
            EventId = Guid.NewGuid().ToString();
            CreatedTimeStamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",CultureInfo.InvariantCulture);
        }

        public override string ToString()
        {
            return Json.Serialize(ToDictionary());
        }

        public override Dictionary<string, object> ToDictionary()
        {
            // Set base dictionary values
            Dictionary<string, object> dictionary = base.ToDictionary();
            
            // Set specific dictionary values
            dictionary.Add(INSTALL_ID, InstallId);
            dictionary.Add(SESSION_ID, SessionId);
            dictionary.Add(EVENT_UNIQUE_ID, EventId);
            dictionary.Add(NTESTING_ID, NTestingId);
            dictionary.Add(NTESTING_OVERRIDE_ID, NTestingOverrideId);
            dictionary.Add(CLIENT_TIME_STAMP,CreatedTimeStamp);
            dictionary.Add(CLIENT_TIME_ZONE, TimeZoneInfo.Local.StandardName);
            dictionary.Add(EVENT_NAME, Sanitize(EventName));
            dictionary.Add(EVENT_CATEGORY, Sanitize(EventCategory));
            dictionary.Add(EVENT_VALUES, EventValues);
            
            return dictionary;
        }
    }
}