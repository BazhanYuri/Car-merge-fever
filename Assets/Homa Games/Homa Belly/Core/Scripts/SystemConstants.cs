#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Provides a thread-safe way to access system constants.
    /// </summary>
    public static class SystemConstants
    {
#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        public static void EditorInit()
        {
            Populate();
        }
#endif
        public static void Populate()
        {
            ApplicationVersion = Application.version;
            ApplicationIdentifier = Application.identifier;
            OperatingSystem = SystemInfo.operatingSystem;
            DeviceType = SystemInfo.deviceType;
            DeviceModel = SystemInfo.deviceModel;
#if UNITY_ANDROID
            UserAgent = "ANDROID";
#elif UNITY_IOS
            UserAgent = UnityEngine.iOS.Device.generation.ToString().Contains("iPad") ? "IPAD" : "IPHONE";
#endif
            DeviceIdentifier = SystemInfo.deviceUniqueIdentifier;
        }
        public static string ApplicationVersion = "";
        public static string ApplicationIdentifier = "";
        public static string OperatingSystem = "";
        public static DeviceType DeviceType = DeviceType.Unknown;
        public static string DeviceModel = "";
        public static string UserAgent = "INVALID";
        public static string DeviceIdentifier = "";
    }
}