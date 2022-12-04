#if HOMA_BELLY_EDITOR_ANALYTICS_ENABLED
#endif
using System.Globalization;
using System.Net.NetworkInformation;
using UnityEditor;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public abstract class EditorAnalyticsModelBase : EventApiQueryModel
    {
        private static readonly string UNITY_VERSION = "unity_version";
        private static readonly string UNITY_TARGET = "unity_target";
        private static readonly string DEVICE_IDENTIFIER = "device_identifier";

        public EditorAnalyticsModelBase(string eventName)
        {
            EventCategory = "unity_editor_event";
            InstallId = EditorAnalyticsSessionInfo.userId;
            SessionId = EditorAnalyticsSessionInfo.id.ToString(CultureInfo.InvariantCulture);
            EventName = eventName;
            EventValues.Add(UNITY_VERSION, Application.unityVersion);
            EventValues.Add(UNITY_TARGET, EditorUserBuildSettings.activeBuildTarget);
            EventValues.Add(DEVICE_IDENTIFIER,SystemConstants.DeviceIdentifier);
        }
    }
}