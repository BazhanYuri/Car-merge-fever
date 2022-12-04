using System;
using HomaGames.HomaBelly.Utilities;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public static class RemoteConfigurationConstants
    {
        [Obsolete("This variable will be removed in near future, please use TRACKING_FILE_COMPLETE_PATH by loading it from Resources")]
        public static string TRACKING_FILE = Application.streamingAssetsPath + "/Homa Games/Homa Belly/config.json";

        /// <summary>
        /// Use this path to load from resources.
        /// </summary>
        public static readonly string TRACKING_FILE_RESOURCES_PATH = "Homa Games/Homa Belly/config";
        
        /// <summary>
        /// Use this path to work with System.IO
        /// </summary>
        public static readonly string TRACKING_FILE_COMPLETE_PATH = $"{Application.dataPath}/Resources/{TRACKING_FILE_RESOURCES_PATH}.json";
        
        
        public static readonly string FIRST_TIME_ALREADY_REQUESTED = "homagames.homabelly.remoteconfiguration.first_time_already_requested";
    }
}