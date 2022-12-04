using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace HomaGames.HomaBelly.Installer.Utilities
{
    public class EditorHttpCaller<T>
    {
        #region IHttpCaller implementation

        public async Task<T> Get(string uri, IModelDeserializer<T> deserializer)
        {
            try
            {
                using (HttpClient client = GetHttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(uri).ConfigureAwait(false);
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
                            Dictionary<string, object> dictionary = InstallerJson.Deserialize(errorString) as Dictionary<string, object>;
                            if (dictionary != null)
                            {
                                // Detect any error
                                if (dictionary.ContainsKey("status") && dictionary.ContainsKey("message"))
                                {
                                    throw new EditorHttpCallerException(Convert.ToString(dictionary["status"]), Convert.ToString(dictionary["message"]));
                                }
                            }
                        }
                        else
                        {
                            throw new EditorHttpCallerException(Convert.ToString(response.StatusCode), response.ReasonPhrase);
                        }
                    }
                }
            }
            catch (EditorHttpCallerException e)
            {
                throw e;
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
                    string bodyAsJsonString = await Task.Run(() => InstallerJson.Serialize(body));
                    StringContent data = new StringContent(bodyAsJsonString, Encoding.UTF8, "application/json");
                    data.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    HttpResponseMessage response = await client.PostAsync(uri, data).ConfigureAwait(false);
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
                            Dictionary<string, object> dictionary = InstallerJson.Deserialize(errorString) as Dictionary<string, object>;
                            if (dictionary != null)
                            {
                                // Detect any error
                                if (dictionary.ContainsKey("status") && dictionary.ContainsKey("message"))
                                {
                                    throw new EditorHttpCallerException(Convert.ToString(dictionary["status"]), Convert.ToString(dictionary["message"]));
                                }
                            }
                        }
                        else
                        {
                            throw new EditorHttpCallerException(Convert.ToString(response.StatusCode), response.ReasonPhrase);
                        }
                    }
                }
            }
            catch (EditorHttpCallerException e)
            {
                throw e;
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
                HttpResponseMessage response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
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