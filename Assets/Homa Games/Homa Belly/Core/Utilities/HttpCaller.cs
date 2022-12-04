using System.Net.Http;

namespace HomaGames.HomaBelly
{
    public static class HttpCaller
    {
        public static HttpClient GetHttpClient()
        {
#if UNITY_EDITOR && CHARLES_PROXY
            var httpClientHandler = new HttpClientHandler()
            {
                Proxy = new System.Net.WebProxy("http://localhost:8888", false),
                UseProxy = true
            };

            HttpClient client = new HttpClient(httpClientHandler);
            return client;
#else
            HttpClient client = new HttpClient();
            return client;
#endif
        }
    }
}
