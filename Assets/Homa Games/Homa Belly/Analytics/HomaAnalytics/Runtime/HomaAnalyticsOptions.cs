using System;
using System.Threading.Tasks;

namespace HomaGames.HomaBelly
{
    public class HomaAnalyticsOptions
    {
        /// <summary>
        /// Print logs when events are sent and responses are received from the server.
        /// </summary>
        public bool VerboseLogs { get; private set; }
        
        /// <summary>
        /// We will do validations before sending the event. Disable it in release builds to improve performance.
        /// </summary>
        public bool EventValidation { get; private set; }
        
        /// <summary>
        /// Used for events requests to the server.
        /// </summary>
        public string EndPointUrl { get; private set; }
        
        /// <summary>
        /// A new session id will be generated if the time since last opened session is major than this value.
        /// </summary>
        public float SecondsToGenerateNewSessionId { get; private set; }
        
        /// <summary>
        /// Homa Belly manifest version
        /// </summary>
        public string ManifestVersionId { get; private set; }

        /// <summary>
        /// Application token to access to Homa Belly manifest
        /// </summary>
        public string TokenIdentifier { get; private set; }
        
        /// <summary>
        /// Send Fps data.
        /// </summary>
        public bool SendFpsEvents { get; private set; }

        /// <summary>
        /// If true, we will save sent events in a CSV file. Useful to debug sent events.
        /// </summary>
        public bool RecordAllEventsInCsv { get; }

        public bool RecordLogs { get; set; } = true;

        /// <summary>
        /// We have to fetch this data from Geryon. We need Geryon to be initialized to gather this ID.
        /// </summary>
        public string NTestingId { get; private set; } = "NotReady";
        
        /// <summary>
        /// We have to fetch this data from Geryon. We need Geryon to be initialized to gather this ID.
        /// </summary>
        public string NTestingOverrideId { get; private set; } = "NotReady";

        public HomaAnalyticsOptions(bool verboseLogs,
            bool eventValidation,
            string endPointUrl,
            float secondsToGenerateNewSessionId,
            string tokenIdentifier,
            string manifestVersionId,
            bool sendFpsEvents,
            bool recordAllEventsInCsv,
            string nTestingId,
            string nTestingOverrideId)
        {
            VerboseLogs = verboseLogs;
            EventValidation = eventValidation;
            EndPointUrl = endPointUrl;
            SecondsToGenerateNewSessionId = secondsToGenerateNewSessionId;
            TokenIdentifier = tokenIdentifier;
            ManifestVersionId = manifestVersionId;
            SendFpsEvents = sendFpsEvents;
            RecordAllEventsInCsv = recordAllEventsInCsv;
            NTestingId = nTestingId;
            NTestingOverrideId = nTestingOverrideId;
        }

        public override string ToString()
        {
            return $"{nameof(VerboseLogs)}: {VerboseLogs}, {nameof(EventValidation)}: {EventValidation}, {nameof(EndPointUrl)}: {EndPointUrl}, {nameof(SecondsToGenerateNewSessionId)}: {SecondsToGenerateNewSessionId}, {nameof(ManifestVersionId)}: {ManifestVersionId}, {nameof(TokenIdentifier)}: {TokenIdentifier}, Fps: {SendFpsEvents} NTestingId: {NTestingId} NTestingOverrideId: {NTestingOverrideId}";
        }
    }
}