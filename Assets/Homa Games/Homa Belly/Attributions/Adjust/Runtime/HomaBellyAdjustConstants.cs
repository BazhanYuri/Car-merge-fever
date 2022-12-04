namespace HomaGames.HomaBelly
{
    public static class HomaBellyAdjustConstants
    {
        public const string ID = "adjust";
        public const string CONFIG_FILE_PATH_IN_RESOURCES = "AdjustConfig";
        public const string APP_VERSION_AT_INSTALL = "AdjustAppVersionAtInstall";
        public const string IN_APP_PURCHASE_EVENT_NAME = "in_app_purchase";
        public static readonly string CONFIG_FILE_PATH = $"Assets/Homa Games/Homa Belly/Attributions/Adjust/Resources/{CONFIG_FILE_PATH_IN_RESOURCES}.asset";
        public static readonly string SIGNATURE_IOS_PATH = $"Assets/Adjust/iOS/AdjustSigSdk.a";
        public static readonly string SIGNATURE_ANDROID_PATH = $"Assets/Adjust/Android/AdjustSigSdk.aar";
    }
}