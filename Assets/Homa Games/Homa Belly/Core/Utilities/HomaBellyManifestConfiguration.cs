using System;
using System.Collections.Generic;
using System.Globalization;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Data structure holding generic runtime values for any Homa Belly package.
    /// </summary>
    public class HomaBellyManifestConfiguration : ScriptableObject
    {
        public static readonly string CONFIG_FILE_RESOURCES_PATH = "homa_data";
        public static readonly string[] MANIFEST_TOKEN_KEY = { "manifest", "token" };
        public static readonly string[] MANIFEST_VERSION_ID_KEY = { "manifest", "version_id"};
        public static readonly string CONFIG_FILE_PROJECT_PATH =
            $"Assets/Homa Games/Homa Belly/Preserved/Resources/{CONFIG_FILE_RESOURCES_PATH}.asset";

        private static HomaBellyManifestConfiguration _instance;

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
#endif
        public static void Initialise()
        {
            if (_instance) return;
            _instance = Resources.Load<HomaBellyManifestConfiguration>(CONFIG_FILE_RESOURCES_PATH);
            _instance.LoadRuntimeConfiguration();
        }


        [Serializable]
        private struct ConfigEntry
        {
            public int pathHash;
            public string value;
        }

        [SerializeField] private List<ConfigEntry> configData = new List<ConfigEntry>();
        public int PluginManifestHash;

        // Path hash to data
        private Dictionary<int, string> runtimeConfigData = new Dictionary<int, string>();

        public void SetEntry(string value, params string[] path)
        {
            int hash = GetPathHash(path);
            configData.RemoveAll(entry => entry.pathHash == hash);
            runtimeConfigData[hash] = value;
            configData.Add(new ConfigEntry() {pathHash = hash, value = value});
        }

        public static bool TryGetString(out string data, params string[] path)
        {
            data = "";
            if(!_instance)
                Initialise();
            return _instance && _instance.runtimeConfigData.TryGetValue(GetPathHash(path), out data);
        }

        public static bool TryGetBool(out bool data, params string[] path)
        {
            data = false;
            return TryGetString(out string stringData, path) &&
                   bool.TryParse(stringData, out data);
        }

        public static bool TryGetInt(out int data, params string[] path)
        {
            data = 0;
            return TryGetString(out string stringData, path) &&
                   int.TryParse(stringData, NumberStyles.Integer, CultureInfo.InvariantCulture, out data);
        }

        private static int GetPathHash(string[] path)
        {
            return String.Join(".", path, 0, path.Length).GetHashCode();
        }

        private void LoadRuntimeConfiguration()
        {
            runtimeConfigData.Clear();
            foreach (var key in configData)
            {
                runtimeConfigData[key.pathHash] = key.value;
            }
        }
    }
}