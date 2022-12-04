using System;
using System.Collections.Generic;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    [System.Serializable]
    public class PackageComponent
    {
        //[JsonProperty("s_package_key")]
        public string Id;
        //[JsonProperty("s_version_number")]
        public string Version;
        //[JsonProperty("s_url")]
        public string Url;
        //[JsonProperty("s_adapter_url")]
        public string AdapterUrl;
        //[JsonProperty("as_files")]
        public List<string> Files;
        //[JsonProperty("o_data")]
        public Dictionary<string, string> Data;

        public string GetName()
        {
            return $"{Id}-{Version}";
        }

        public override string ToString()
        {
            string dataToString = Data != null ? $"\nData: {string.Join(System.Environment.NewLine, Data)}" : "";
            return $"{Id}:{Version}{dataToString}";
        }

        public static PackageComponent FromDictionary(Dictionary<string, object> dictionary)
        {
            PackageComponent package = new PackageComponent();

            // Base values
            package.Id = Convert.ToString(dictionary["s_package_key"]);
            package.Version = Convert.ToString(dictionary["s_version_number"]);
            package.Url = Convert.ToString(dictionary["s_url"]);
            package.AdapterUrl = dictionary.ContainsKey("s_adapter_url") ? Convert.ToString(dictionary["s_adapter_url"]) : "";

            // Files list
            package.Files = new List<string>();
            List<object> files = dictionary["as_files"] as List<object>;
            for (int i = 0; files != null && i < files.Count; i++)
            {
                package.Files.Add(files[i] as string);
            }

            // Optional - data
            if (dictionary.ContainsKey("o_data"))
            {
                package.Data = new Dictionary<string, string>();
                Dictionary<string, object> dataDict = dictionary["o_data"] as Dictionary<string, object>;
                if (dataDict != null)
                {
                    foreach (KeyValuePair<string, object> entry in dataDict)
                    {
                        // Ignore null values
                        if (entry.Value != null)
                        {
                            package.Data.Add(entry.Key, entry.Value.ToString());
                        }
                    }
                }
            }

            return package;
        }

        public string GetMainPackageLocalFilePath()
        {
            return string.Format("{0}/../Library/Homa Belly/{1}", Application.dataPath, Url.Substring(Url.LastIndexOf("/") + 1));
        }

        public string GetAdapterPackageLocalFilePath()
        {
            return string.Format("{0}/../Library/Homa Belly/{1}", Application.dataPath, AdapterUrl.Substring(AdapterUrl.LastIndexOf("/") + 1));
        }
    }
}
