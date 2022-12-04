#if HOMA_BELLY_EDITOR_ANALYTICS_ENABLED
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public class UsageReportQueryModel : EditorAnalyticsModelBase
    {
        private static readonly string PARAM_NAMESPACES = "namespaces";
        private static readonly string PARAM_NAMESPACES_BATCH_ID = "namespaces_batch_id";

        public UsageReportQueryModel(string eventName,string batchId,string usedFunction) : base(eventName)
        {
            EventValues.Add(PARAM_NAMESPACES_BATCH_ID, batchId);
            EventValues.Add(PARAM_NAMESPACES, usedFunction);
        }
    }
}
#endif