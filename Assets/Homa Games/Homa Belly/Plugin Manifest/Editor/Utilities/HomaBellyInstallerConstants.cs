namespace HomaGames.HomaBelly.Installer.Utilities
{
    public static class HomaBellyInstallerConstants
    {
        public static string PRODUCT_NAME = "Homa Belly";
        public static string PRODUCT_VERSION = "1.4.3";
#if !HOMA_BELLY_DEV_ENV
        public static string API_HOST = "https://damysus-engine.homagames.com";
#else
        public static string API_HOST = "http://damysus-engine.oh.stage.homagames.com";
#endif
        public static string API_VERSION = "V1";
        public static string API_APP_BASE_URL = API_HOST + "/appbase?cv=" + API_VERSION + "&sv=" + PRODUCT_VERSION + "&ti={0}";
    }
}
