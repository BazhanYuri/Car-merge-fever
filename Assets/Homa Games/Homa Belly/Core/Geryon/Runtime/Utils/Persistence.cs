using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HomaGames.HomaBelly;
using HomaGames.HomaBelly.Utilities;
using UnityEngine;

namespace HomaGames.Geryon
{
    /// <summary>
    /// Persistence class to store any data that must persist between game runs. ie: first time app open configuration
    /// </summary>
    public static class Persistence
    {
        private static readonly string FIRST_TIME_REMOTE_CONFIGURATION_FILE = Path.Combine(Application.persistentDataPath,
            "homagames/ntesting/first_time_configuration.json");
        private static readonly string FIRST_TIME_EXTERNAL_TOKENS_FILE = Path.Combine(Application.persistentDataPath,
            "homagames/ntesting/first_time_external_tokens.json");

        /// <summary>
        /// Persists the First Time App Open configuration into a file
        /// </summary>
        /// <param name="firstTimeGeryonConfigurationModel">The response fetched from NTesting API</param>
        public static async Task PersistFirstTimeConfigurationAsync(GeryonConfigurationModel firstTimeGeryonConfigurationModel)
        {
            if (firstTimeGeryonConfigurationModel == null)
            {
                HomaGamesLog.Warning("Trying to persist an empty first time app open response. Skipping");
                return;
            }

            List<Task> writeTasks = new List<Task>();

            if (firstTimeGeryonConfigurationModel.Configuration != null)
            {
                var firstTimeRemoteConfigurationFile = FIRST_TIME_REMOTE_CONFIGURATION_FILE;
                
                // Serialize configuration and store it in FIRST_TIME_REMOTE_CONFIGURATION_FILE
                FileUtilities.CreateIntermediateDirectoriesIfNecessary(firstTimeRemoteConfigurationFile);
                
                writeTasks.Add(Task.Run(() =>
                    {
                        string serializedConfiguration = Json.Serialize(firstTimeGeryonConfigurationModel.Configuration);
                        File.WriteAllText(firstTimeRemoteConfigurationFile, serializedConfiguration);
                    }));
            }

            // Serialize configuration and store it in FIRST_TIME_EXTERNAL_TOKENS_FILE
            string[] externalTokensList = 
            {
                firstTimeGeryonConfigurationModel.ExternalToken0,
                firstTimeGeryonConfigurationModel.ExternalToken1,
                firstTimeGeryonConfigurationModel.ExternalToken2,
                firstTimeGeryonConfigurationModel.ExternalToken3,
                firstTimeGeryonConfigurationModel.ExternalToken4
            };

            FileUtilities.CreateIntermediateDirectoriesIfNecessary(FIRST_TIME_EXTERNAL_TOKENS_FILE);
            writeTasks.Add(Task.Run(() =>
            {
                string serializedExternalTokens = Json.Serialize(externalTokensList);
                File.WriteAllText(FIRST_TIME_EXTERNAL_TOKENS_FILE, serializedExternalTokens);
            }));

            await Task.WhenAll(writeTasks);
        }

        /// <summary>
        /// Loads the First Time App Open configuration from local file
        /// </summary>
        /// <returns>A fulfilled dictionary with the first time app open configuration loaded</returns>
        public static async Task<Dictionary<string, object>> LoadFirstTimeConfigurationFromPersistence()
        {
            Dictionary<string, object> firstTimeAppOpenDictionary = new Dictionary<string, object>();
            var firstTimeRemoteConfigurationFile = FIRST_TIME_REMOTE_CONFIGURATION_FILE;
            
            if (File.Exists(firstTimeRemoteConfigurationFile))
            {
                try
                {
                    var fileLoadingTask =
                        FileUtilities.LoadAndDeserializeJsonFromPersistentPath<Dictionary<string, object>>(
                            firstTimeRemoteConfigurationFile);

                    await fileLoadingTask;
                    return fileLoadingTask.Result;
                }
                catch (Exception e)
                {
                    HomaGamesLog.Warning($"Exception while reading first time app open from file: {e.Message}");
                }
            }

            return firstTimeAppOpenDictionary;
        }

        /// <summary>
        /// Loads the First Time App Open external tokens from local file
        /// </summary>
        /// <returns>A fulfilled list with first time app open external tokens</returns>
        public static async Task<string[]> LoadFirstTimeExternalTokensFromPersistence()
        {
            var firstTimeExternalTokensFile = FIRST_TIME_EXTERNAL_TOKENS_FILE;
            
            if (File.Exists(firstTimeExternalTokensFile))
            {
                try
                {
                    var fileLoadingTask =
                        FileUtilities.LoadAndDeserializeJsonFromPersistentPath<List<object>>(
                            firstTimeExternalTokensFile);

                    await fileLoadingTask;
                    return fileLoadingTask.Result.Select(s => (string) s).ToArray();
                }
                catch (Exception e)
                {
                    HomaGamesLog.Warning($"Exception while reading first time app open external tokens from file: {e.Message}");
                }
            }

            return new string[4];
        }
    }
}
