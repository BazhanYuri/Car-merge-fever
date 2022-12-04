using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HomaGames.HomaBelly.Utilities;
using UnityEngine;
using UnityEngine.Networking;

namespace HomaGames.HomaBelly
{
    public static class FileUtilities
    {
        // We have to store it here to be sure to access it on the main thread
        private static readonly string PERSISTENT_DATA_PATH = Application.persistentDataPath;
        
        
        [Obsolete("This method will be removed in future versions. Use LoadAndDeserializeJsonFromResources instead")]
        public static string ReadAllText(string filePath)
        {
            if (filePath.Contains("://") || filePath.Contains(":///"))
            {
                UnityWebRequest www = UnityWebRequest.Get(filePath);
                www.SendWebRequest();
                // Wait until async operation has finished
                while(!www.isDone)
                {
                    continue;
                }
                return www.downloadHandler.text;
            }
            else
            {
                return File.ReadAllText(filePath);
            }
        }

        /// <summary>
        /// Async method to load and deserialize a text file in a Resources folder.
        /// </summary>
        /// <param name="filePath">A text file in a Resources folder</param>
        /// <param name="showErrorIfFileDontExist">Whether or not to log an error if the file does not exist</param>
        /// <typeparam name="T">Type to deserialize the json</typeparam>
        /// <returns>The deserialized object or null of something goes wrong.</returns>
        public static async Task<T> LoadAndDeserializeJsonFromResources<T>(string filePath,bool showErrorIfFileDontExist = true)
        {
            TextAsset asset = await ResourcesUtils.LoadAsync<TextAsset>(filePath);

            if (asset != null)
            {
                string text = asset.text;
                Resources.UnloadAsset(asset);
                return DeserializeJson<T>(text, filePath);
            }
            
            if (showErrorIfFileDontExist)
            {
                Debug.LogError(
                    $"[ERROR] No <b>TextAsset</b> found. Check the file exist and the type is TextAsset in path: {filePath}");
            }

            return default;
        }

        public static async Task<T> LoadAndDeserializeJsonFromPersistentPath<T>(string filePath, bool showErrorIfFileDontExist = true)
        {
            T output = default;
            
            await Task.Run(() =>
            {
                if (!filePath.StartsWith(PERSISTENT_DATA_PATH))
                    filePath = Path.Combine(PERSISTENT_DATA_PATH, filePath);

                if (!File.Exists(filePath))
                {
                    if (showErrorIfFileDontExist)
                        Debug.LogError($"No file found at path \"{filePath}\" in {nameof(LoadAndDeserializeJsonFromPersistentPath)}");
                    return;
                }

                output = DeserializeJson<T>(File.ReadAllText(filePath), filePath);
            });

            return output;
        }

        private static T DeserializeJson<T>(string json, string filePath)
        {
            T result = default;
            
            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogError($"[ERROR] The loaded file in path: {filePath} doesn't have a valid text");
                return default;
            }
				
            try
            {
                result = (T)Json.Deserialize(json);
            }
            catch (Exception deserializationException)
            {
                Debug.LogError($"[ERROR] Error deserializing the file: {filePath} Error: {deserializationException}");
            }

            if (result == null)
            {
                Debug.LogError($"[ERROR] Error deserializing the file: {filePath} Json: {json}");
            }

            return result;
        }

        /// <summary>
        /// Return true if the directory was created.
        /// </summary>
        public static bool CreateIntermediateDirectoriesIfNecessary(string filePath)
        {
            string parentPath = Directory.GetParent(filePath).ToString();
            if (!string.IsNullOrEmpty(parentPath) && !Directory.Exists(parentPath))
            {
                Directory.CreateDirectory(parentPath);
                return true;
            }

            return false;
        }
    }
}