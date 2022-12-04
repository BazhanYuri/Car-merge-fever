using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HomaGames.HomaBelly.Utilities
{
    public class EditorHttpCaller<T>
    {
        #region IHttpCaller implementation

        private const int TIMEOUT_MS = 10000;
        
        /// <summary>
        /// Synchronous HTTP Get method. To be used in Unity callbacks/processes when we need to
        /// wait the HTTP result before continuing the process (ie: PostBuildProcess), so we can't
        /// await the request as the Unity method does not support async/await
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="deserializer"></param>
        /// <returns></returns>
        /// <exception cref="EditorHttpCallerException"></exception>
        public T GetSynchronous(string uri, IModelDeserializer<T> deserializer)
        {
            try
            {
                using (HttpClient client = GetHttpClient())
                {
                    var asyncTaskGet = client.GetAsync(uri);
                    if (!asyncTaskGet.Wait(TIMEOUT_MS))
                    {
                        throw new EditorHttpCallerException("408", $"EditorHttpCaller {TIMEOUT_MS}ms Timeout");
                    }
                    
                    HttpResponseMessage response = asyncTaskGet.Result;
                    if (response.IsSuccessStatusCode)
                    {
                        string resultString = response.Content.ReadAsStringAsync().Result;
                        return deserializer.Deserialize(resultString);
                    }
                    else
                    {
                        string errorString = response.Content.ReadAsStringAsync().Result;
                        if (!string.IsNullOrEmpty(errorString))
                        {
                            JsonObject responseObject = Json.DeserializeObject(errorString);
                            // Detect any error
                            if (responseObject.TryGetString("status", out var status) && responseObject.TryGetString("message", out var message))
                            {
                                throw new EditorHttpCallerException(status, message);
                            }
                        }
                        else
                        {
                            throw new EditorHttpCallerException(Convert.ToString(response.StatusCode), response.ReasonPhrase);
                        }
                    }
                }
            }
            catch (EditorHttpCallerException)
            {
                throw;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning($"[Editor Http Caller] Exception while querying API {uri}: {e.Message}");
            }

            return default;
        }
        
        public async Task<T> Get(string uri, IModelDeserializer<T> deserializer)
        {
            try
            {
                using (HttpClient client = GetHttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(uri);
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
                            if (responseObject.TryGetString("status", out var status) && responseObject.TryGetString("message", out var message))
                            {
                                throw new EditorHttpCallerException(status, message);
                            }
                        }
                        else
                        {
                            throw new EditorHttpCallerException(Convert.ToString(response.StatusCode), response.ReasonPhrase);
                        }
                    }
                }
            }
            catch (EditorHttpCallerException)
            {
                throw;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning($"[Editor Http Caller] Exception while querying API {uri}: {e.Message}");
            }

            return default;
        }
        
        public async Task<T> Post(string uri, Dictionary<string, object> body, IModelDeserializer<T> deserializer)
        {
            try
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
                            JsonObject resultObject = await Task.Run(() => Json.DeserializeObject(errorString));
                            // Detect any error
                            if (resultObject.TryGetString("status", out var status) && resultObject.TryGetString("message", out var message))
                            {
                                throw new EditorHttpCallerException(status, message);
                            }
                        }
                        else
                        {
                            throw new EditorHttpCallerException(Convert.ToString(response.StatusCode), response.ReasonPhrase);
                        }
                    }
                }
            }
            catch (EditorHttpCallerException)
            {
                throw;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning($"[Editor Http Caller] Exception while querying API {uri}: {e.Message}");
            }

            return default;
        }

        public async Task<string> DownloadFile(string uri, string outputFilePath)
        {
            using (HttpClient client = GetHttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
                if (response.IsSuccessStatusCode)
                {
                    using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
                    {
                        using (Stream streamToWriteTo = File.Open(outputFilePath, FileMode.Create))
                        {
                            await streamToReadFrom.CopyToAsync(streamToWriteTo);
                        }
                    }

                    HomaBellyEditorLog.Debug($"Done");
                    return outputFilePath;
                }
                else
                {
                    throw new FileNotFoundException(response.ReasonPhrase);
                }
            }
        }
        #endregion

        #region Private helpers

        private HttpClient GetHttpClient()
        {
#if CHARLES_PROXY
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

        #endregion
    }
}