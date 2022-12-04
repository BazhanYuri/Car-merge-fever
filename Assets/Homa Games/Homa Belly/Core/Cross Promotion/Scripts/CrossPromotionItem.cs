using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Model holding a single cross promotion item
    /// </summary>
    public class CrossPromotionItem
    {
        private enum SafeConversionValue
        {
            String,
            Int,
            Double
        }

        //[JsonProperty("s_name")]
        public string Name;
        //[JsonProperty("s_label")]
        public string Label;
        //[JsonProperty("s_app_id")]
        public string AppId;
        //[JsonProperty("s_id")] || [JsonProperty("i_id")]
        public string ItemId;
        //[JsonProperty("s_creative_url")]
        public string CreativeUrl;
        //[JsonProperty("s_impression_url")]
        public string ImpressionUrl;
        //[JsonProperty("s_click_url")]
        public string ClickUrl;
        //[JsonProperty("i_creative_size_kb")]
        public int CreativeSizeKb;
        //[JsonProperty("f_weight")]
        public double Weight;
        //[JsonProperty("s_creative_mime_type")]
        public string MimeType;
        //[JsonProperty("i_creative_width")]
        public int CreativeWidth;
        //[JsonProperty("i_creative_height")]
        public int CreativeHeight;

        public string LocalVideoFilePath
        {
            get
            {
                // By the time being, we store the unique received creative as an MP4 format (design agreement)
                string videoFormat = !string.IsNullOrEmpty(MimeType) && MimeType.LastIndexOf('/') != -1 ? MimeType.Substring(MimeType.LastIndexOf('/') + 1) : "mp4";
                return Path.Combine(CrossPromotionManager.CrossPromotionVideoPath,
                    $"{UnityWebRequest.EscapeURL(ItemId)}.{videoFormat}");
            }
        }

        /// <summary>
        /// Given a dictionary representing the cross promotion items, deserializes
        /// and builds a CrossPromotionItem instance with its data
        /// </summary>
        /// <param name="dictionary">The json dictionary representing a cross promotion item</param>
        /// <returns>A CrossPromotionItem instance</returns>
        public static CrossPromotionItem FromDictionary(Dictionary<string, object> dictionary)
        {
            CrossPromotionItem item = new CrossPromotionItem();

            // Base values
            item.Name = (string) SafeDeserialization(dictionary, "s_name", SafeConversionValue.String);
            item.Label = (string)SafeDeserialization(dictionary, "s_label", SafeConversionValue.String);
            item.AppId = (string)SafeDeserialization(dictionary, "s_app_id", SafeConversionValue.String);
            item.CreativeUrl = (string)SafeDeserialization(dictionary, "s_creative_url", SafeConversionValue.String);
            item.ImpressionUrl = (string)SafeDeserialization(dictionary, "s_impression_url", SafeConversionValue.String);
            item.ClickUrl = (string)SafeDeserialization(dictionary, "s_click_url", SafeConversionValue.String);
            item.CreativeSizeKb = (int)SafeDeserialization(dictionary, "i_creative_size_kb", SafeConversionValue.Int);
            item.Weight = (double)SafeDeserialization(dictionary, "f_weight", SafeConversionValue.Double);
            item.MimeType = (string)SafeDeserialization(dictionary, "s_creative_mime_type", SafeConversionValue.String);
            item.CreativeWidth = (int)SafeDeserialization(dictionary, "i_creative_width", SafeConversionValue.Int);
            item.CreativeHeight = (int)SafeDeserialization(dictionary, "i_creative_height", SafeConversionValue.Int);

            // Item ID. Supporting int and string values. Fallback to app_id
            if (dictionary.ContainsKey("s_id"))
            {
                item.ItemId = (string) SafeDeserialization(dictionary, "s_id", SafeConversionValue.String);
            }
            else if (dictionary.ContainsKey("i_id"))
            {
                item.ItemId = SafeDeserialization(dictionary, "i_id", SafeConversionValue.Int).ToString();
            }
            else
            {
                item.ItemId = item.AppId.Replace('.', '_');
            }

            return item;
        }

        /// <summary>
        /// Safely deserializes a given key from the given dictionary.
        /// If key is not found, returns empty value
        /// </summary>
        /// <param name="dictionary">The dictionary where to obtain the value</param>
        /// <param name="key">The key representing the value inside the dictionary</param>
        /// <param name="type">The value type to be casted to</param>
        /// <returns></returns>
        private static object SafeDeserialization(Dictionary<string, object> dictionary, string key, SafeConversionValue type)
        {
            if (dictionary == null || dictionary.Count == 0 || string.IsNullOrEmpty(key))
            {
                return "";
            }

            object value = "";
            switch(type)
            {
                case SafeConversionValue.String:
                    value = dictionary.ContainsKey(key) ? Convert.ToString(dictionary[key]) : "";
                    break;
                case SafeConversionValue.Int:
                    value = dictionary.ContainsKey(key) ? Convert.ToInt32(dictionary[key]) : 0;
                    break;
                case SafeConversionValue.Double:
                    value = dictionary.ContainsKey(key) ? Convert.ToDouble(dictionary[key], System.Globalization.CultureInfo.InvariantCulture) : 0;
                    break;
            }

            return value;
        }

        /// <summary>
        /// Determines if the local video file is available or not
        /// </summary>
        /// <returns></returns>
        public bool IsLocalVideoFileAvailable()
        {
            return File.Exists(LocalVideoFilePath);
        }
    }
}
