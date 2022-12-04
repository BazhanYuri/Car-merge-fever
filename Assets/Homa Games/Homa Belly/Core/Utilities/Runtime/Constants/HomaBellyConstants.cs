namespace HomaGames.HomaBelly.Utilities
{
    public static class HomaBellyConstants
    {
        public static string PRODUCT_NAME = "Homa Belly";
        public static string PRODUCT_VERSION = "1.7.2";
#if !HOMA_BELLY_DEV_ENV
        public const string API_HOST = "https://damysus-engine.homagames.com";
#else
        public const string API_HOST = "http://damysus-engine.oh.stage.homagames.com";
#endif
        
        public static string API_VERSION = "V2";
    }
}
