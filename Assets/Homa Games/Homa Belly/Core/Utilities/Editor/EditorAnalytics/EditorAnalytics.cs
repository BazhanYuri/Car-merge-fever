using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using HomaGames.HomaBelly.Utilities;
using JetBrains.Annotations;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Class to send Unity Editor Analytics to Homa Games servers.
    /// </summary>
    public static class EditorAnalytics
    {
#if HOMA_BELLY_EDITOR_ANALYTICS_ENABLED
        [InitializeOnLoadMethod]
        public static void Initialise()
        {
            PackageReporter packageReporter = new PackageReporter();
            UsageReporter usageReporter = new UsageReporter();
            PerformanceReporter performanceReporter = new PerformanceReporter();
            performanceReporter.OnDataReported += OnDataReported;
            packageReporter.OnDataReported += OnDataReported;
            usageReporter.OnDataReported += OnDataReported;
        }

        private static void OnDataReported(EventApiQueryModel eventApiQueryModel)
        {
#pragma warning disable 4014
            TrackGenericEditorEvent(eventApiQueryModel);
#pragma warning restore 4014
        }
#endif
        

        /// <summary>
        /// Track a Unity Editor Event with eventName and other optional values
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="eventDescription"></param>
        /// <param name="eventStackTrace"></param>
        /// <param name="eventValue"></param>
        /// <param name="eventFps"></param>
        [Conditional("HOMA_BELLY_EDITOR_ANALYTICS_ENABLED")]
#pragma warning disable 1998
        public static async void TrackEditorAnalyticsEvent(string eventName, string eventDescription = null, string eventStackTrace = null, float eventValue = 0, float eventFps = 0)
#pragma warning restore 1998
        {
#if HOMA_BELLY_EDITOR_ANALYTICS_ENABLED
            if (string.IsNullOrWhiteSpace(eventName))
            {
                Debug.LogError($"[Editor Analytics] Tracking empty event name");
                return;
            }

            EditorAnalyticsEventModel eventModel = new EditorAnalyticsEventModel(eventName, eventDescription, eventStackTrace, eventValue, eventFps);

            await TrackGenericEditorEvent(eventModel);
#endif
        }

        [PublicAPI]
#pragma warning disable CS1998
        public static async Task TrackGenericEditorEvent(EventApiQueryModel eventModel)
#pragma warning restore CS1998
        {
#if HOMA_BELLY_EDITOR_ANALYTICS_ENABLED
            string editorEventUrl = EventApiQueryModel.EVENT_API_ENDPOINT;

#if HOMA_BELLY_DEV_ENV
            Debug.Log($"[Editor Analytics] Tracking: {editorEventUrl}. With body: {eventModel}");
#endif
            
            try
            {
                EditorAnalyticsResponseModel responseModel = await Post(editorEventUrl, eventModel.ToDictionary(), new EditorAnalyticsModelDeserializer());
                if (responseModel != null)
                {
#if HOMA_BELLY_DEV_ENV
                    Debug.Log($"[Editor Analytics] Response: {responseModel}");
#endif
                }
            }
#pragma warning disable CS0168
            catch (Exception e)
#pragma warning restore CS0168
            {
#if HOMA_BELLY_DEV_ENV
                Debug.LogError($"[Editor Analytics] Error while sending the event. Reason: {e.Message}");
#endif
            }
#endif
        }
        
#if HOMA_BELLY_EDITOR_ANALYTICS_ENABLED
        private static async Task<EditorAnalyticsResponseModel> Post(string uri, Dictionary<string, object> body, EditorAnalyticsModelDeserializer deserializer)
        {
            using (HttpClient client = GetHttpClient())
            {
                string bodyAsJsonString = await Task.Run(() => Json.Serialize(body));
                StringContent data = new StringContent(bodyAsJsonString, Encoding.UTF8, "application/json");
                data.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response = await client.PostAsync(uri, data);
                if (response.IsSuccessStatusCode)
                {
                    string resultString = await response.Content.ReadAsStringAsync();
                    return deserializer.Deserialize(resultString);
                }
                else
                {
                    string errorString = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(errorString))
                    {
                        JsonObject responseObject = await Task.Run(() => Json.DeserializeObject(errorString));
                        // Detect any error
                        if (responseObject.TryGetString("status", out _) && responseObject.TryGetString("message", out var message))
                        {
                            throw new Exception(message);
                        }
                    }
                    else
                    {
                        throw new Exception(response.ReasonPhrase);
                    }
                }
            }

            return default;
        }
        
        private static HttpClient GetHttpClient()
        {
#if CHARLES_PROXY
            var httpClientHandler = new HttpClientHandler()
            {
                Proxy = new System.Net.WebProxy("http://localhost:8888", false),
                UseProxy = true
            };

            return new HttpClient(httpClientHandler);
#else
            return new HttpClient();
#endif // CHARLES_PROXY
        }
#endif // HOMA_BELLY_EDITOR_ANALYTICS_ENABLED
    }
}
