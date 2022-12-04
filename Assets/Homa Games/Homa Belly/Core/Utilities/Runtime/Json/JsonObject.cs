using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;

namespace HomaGames.HomaBelly.Utilities
{
    /// <summary>
    /// This object is a wrapper for dictionaries
    /// returned by <see cref="Json.Deserialize(string)"/>.
    /// </summary>
    public class JsonObject : JsonDataBase<string>
    {
        [NotNull]
        public static JsonObject Empty => new JsonObject(new Dictionary<string, object>());
        
        [NotNull]
        private readonly IDictionary<string, object> Data;

        public JsonObject([NotNull] IDictionary<string, object> data)
        {
            Data = data;
        }
        
        [PublicAPI, NotNull]
        public Dictionary<string, object> ToRawData() => 
            Data.ToDictionary(entry => entry.Key, entry => entry.Value);


        protected override bool TryGetCanBeNull<T>(string key, [CanBeNull] out T value)
        {
            if (Data.TryGetValue(key, out object objectDataValue))
            {
                return JsonConversionHelper.TryConvertTo(objectDataValue, out value);
            }

            value = default;
            return false;
        }
    }
    
    public abstract class JsonDataBase<TKey>
    {
        /// <summary>
        /// Try to obtain a <see cref="JsonList"/> identified by key. If the key
        /// exists but the value is null (or not a list) this method will return false.
        /// </summary>
        /// <param name="key">The key identifying the <see cref="JsonList"/></param>
        /// <param name="value">The <see cref="JsonList"/> output</param>
        /// <returns>True if the <see cref="JsonList"/> was successfully retrieved and not null, false otherwise</returns>
        [PublicAPI]
        public bool TryGetJsonList([NotNull] TKey key, [NotNull] out JsonList value)
        {
            if (TryGetNotNull(key, out List<object> valueData))
            {
                value = new JsonList(valueData);
                return true;
            }

            value = JsonList.Empty;
            return false;
        }
        
        /// <summary>
        /// Try to obtain a <see cref="JsonList"/> identified by key. If the key
        /// exists but the value is null (or not a list) this method will return false.
        /// </summary>
        /// <param name="key">The key identifying the <see cref="JsonList"/></param>
        /// <param name="value">Action to be invoked with the obtained <see cref="JsonList"/>. This will not
        /// be invoked if the <see cref="JsonList"/> could not be obtained or if the value is null</param>
        /// <returns>True if the <see cref="JsonList"/> was successfully retrieved and not null, false otherwise</returns>
        [PublicAPI]
        public bool TryGetJsonList([NotNull] TKey key, [InstantHandle] Action<JsonList> value)
        {
            if (TryGetNotNull(key, out List<object> valueData))
            {
                value.Invoke(new JsonList(valueData));
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Try to obtain a <see cref="JsonObject"/> identified by key. If the key
        /// exists but the value is null, this method will return true, and the
        /// value will be set to null. If the value is not a <see cref="JsonObject"/>, this method
        /// will return false.
        /// </summary>
        /// <param name="key">The key identifying the <see cref="JsonObject"/></param>
        /// <param name="value">The <see cref="JsonObject"/> output</param>
        /// <returns>True if the <see cref="JsonObject"/> was successfully retrieved, false otherwise</returns>
        [PublicAPI]
        public bool TryGetJsonObjectOrNull([NotNull] TKey key, [CanBeNull] out JsonObject value)
        {
            if (TryGetCanBeNull(key, out Dictionary<string, object> valueData))
            {
                value = valueData == null ? null : new JsonObject(valueData);
                return true;
            }

            value = JsonObject.Empty;
            return false;
        }
        
        /// <summary>
        /// Try to obtain a <see cref="JsonObject"/> identified by key. If the key
        /// exists but the value is null, this method will return true, and the
        /// delegate will be called with null. If the value is not a <see cref="JsonObject"/>, this method
        /// will return false.
        /// </summary>
        /// <param name="key">The key identifying the <see cref="JsonObject"/></param>
        /// <param name="value">Action to be invoked with the obtained <see cref="JsonObject"/>. This will not
        /// be invoked if the <see cref="JsonObject"/> could not be obtained</param>
        /// <returns>True if the <see cref="JsonObject"/> was successfully retrieved, false otherwise</returns>
        [PublicAPI]
        public bool TryGetJsonObjectOrNull([NotNull] TKey key, [InstantHandle] Action<JsonObject> value)
        {
            if (TryGetCanBeNull(key, out Dictionary<string, object> valueData))
            {
                value.Invoke(valueData == null ? null : new JsonObject(valueData));
                return true;
            }

            return false;
        }
        
                /// <summary>
        /// Try to obtain a <see cref="JsonList"/> identified by key. If the key
        /// exists but the value is null, this method will return true, and the
        /// value will be set to null. If the value is not a <see cref="JsonList"/>, this method
        /// will return false.
        /// </summary>
        /// <param name="key">The key identifying the <see cref="JsonList"/></param>
        /// <param name="value">The <see cref="JsonList"/> output</param>
        /// <returns>True if the <see cref="JsonList"/> was successfully retrieved, false otherwise</returns>
        [PublicAPI]
        public bool TryGetJsonListOrNull([NotNull] TKey key, [CanBeNull] out JsonList value)
        {
            if (TryGetCanBeNull(key, out List<object> valueData))
            {
                value = valueData == null ? null : new JsonList(valueData);
                return true;
            }

            value = JsonList.Empty;
            return false;
        }
        
        /// <summary>
        /// Try to obtain a <see cref="JsonList"/> identified by key. If the key
        /// exists but the value is null, this method will return true, and the
        /// value will be set to null. If the value is not a <see cref="JsonList"/>, this method
        /// will return false.
        /// </summary>
        /// <param name="key">The key identifying the <see cref="JsonList"/></param>
        /// <param name="value">Action to be invoked with the obtained <see cref="JsonList"/>. This will not
        /// be invoked if the <see cref="JsonList"/> could not be obtained</param>
        /// <returns>True if the <see cref="JsonList"/> was successfully retrieved, false otherwise</returns>
        [PublicAPI]
        public bool TryGetJsonListOrNull([NotNull] TKey key, [InstantHandle] Action<JsonList> value)
        {
            if (TryGetCanBeNull(key, out List<object> valueData))
            {
                value.Invoke(valueData == null ? null : new JsonList(valueData));
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Try to obtain a <see cref="JsonObject"/> identified by key. If the key
        /// exists but the value is null (or not an object) this method will return false.
        /// </summary>
        /// <param name="key">The key identifying the <see cref="JsonObject"/></param>
        /// <param name="value">The <see cref="JsonObject"/> output</param>
        /// <returns>True if the <see cref="JsonObject"/> was successfully retrieved and not null, false otherwise</returns>
        [PublicAPI]
        public bool TryGetJsonObject([NotNull] TKey key, [NotNull] out JsonObject value)
        {
            if (TryGetNotNull(key, out Dictionary<string, object> valueData))
            {
                value = new JsonObject(valueData);
                return true;
            }

            value = JsonObject.Empty;
            return false;
        }
        
        /// <summary>
        /// Try to obtain a <see cref="JsonObject"/> identified by key. If the key
        /// exists but the value is null (or not a list) this method will return false.
        /// </summary>
        /// <param name="key">The key identifying the <see cref="JsonObject"/></param>
        /// <param name="value">Action to be invoked with the obtained <see cref="JsonObject"/>. This will not
        /// be invoked if the <see cref="JsonObject"/> could not be obtained or if the value is null</param>
        /// <returns>True if the <see cref="JsonObject"/> was successfully retrieved and not null, false otherwise</returns>
        [PublicAPI]
        public bool TryGetJsonObject([NotNull] TKey key, [InstantHandle] Action<JsonObject> value)
        {
            if (TryGetNotNull(key, out Dictionary<string, object> valueData))
            {
                value.Invoke(new JsonObject(valueData));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Try to obtain a <see cref="string"/> identified by key. If the key
        /// exists but the value is null (or not a list) this method will return false.
        /// </summary>
        /// <param name="key">The key identifying the <see cref="string"/></param>
        /// <param name="value">The <see cref="string"/> output</param>
        /// <returns>True if the <see cref="string"/> was successfully retrieved, false otherwise</returns>
        [PublicAPI]
        public bool TryGetString([NotNull] TKey key, [NotNull] out string value)
        {
            if (TryGetNotNull(key, out value))
                return true;

            value = string.Empty;
            return false;
        }
        
        /// <summary>
        /// Try to obtain a <see cref="string"/> identified by key. If the key
        /// exists but the value is null (or not a string) this method will return false.
        /// </summary>
        /// <param name="key">The key identifying the <see cref="string"/></param>
        /// <param name="valueSetter">Action to be invoked with the obtained <see cref="string"/>. This will not
        /// be invoked if the <see cref="string"/> could not be obtained or if the value is null</param>
        /// <returns>True if the <see cref="string"/> was successfully retrieved, false otherwise</returns>
        [PublicAPI]
        public bool TryGetString([NotNull] TKey key, [InstantHandle] Action<string> valueSetter) =>
            TryGetNotNull(key, valueSetter);
        
        /// <summary>
        /// Try to obtain a <see cref="string"/> identified by key. If the key
        /// exists but the value is null, this method will return true, and the
        /// value will be set to null. If the value is not a <see cref="string"/>, this method
        /// will return false.
        /// </summary>
        /// <param name="key">The key identifying the <see cref="string"/></param>
        /// <param name="value">The <see cref="string"/> output</param>
        /// <returns>True if the <see cref="string"/> was successfully retrieved, false otherwise</returns>
        [PublicAPI]
        public bool TryGetStringOrNull([NotNull] TKey key, [CanBeNull] out string value) =>
            TryGetCanBeNull(key, out value);
        
        /// <summary>
        /// Try to obtain a <see cref="string"/> identified by key. If the key
        /// exists but the value is null, this method will return true, and
        /// value will be set to null. If the value is not a <see cref="string"/>, this method
        /// will return false.
        /// </summary>
        /// <param name="key">The key identifying the <see cref="string"/></param>
        /// <param name="valueSetter">Action to be invoked with the obtained <see cref="string"/>. This will not
        /// be invoked if the <see cref="string"/> could not be obtained</param>
        /// <returns>True if the <see cref="string"/> was successfully retrieved, false otherwise</returns>
        [PublicAPI]
        public bool TryGetStringOrNull([NotNull] TKey key, [InstantHandle] Action<string> valueSetter) =>
            TryGetCanBeNull(key, valueSetter);
        
        /// <summary>
        /// Try to obtain a <see cref="bool"/> identified by key. If the key
        /// exists but the value is null (or not a bool) this method will return false.
        /// </summary>
        /// <param name="key">The key identifying the <see cref="bool"/></param>
        /// <param name="value">The <see cref="bool"/> output</param>
        /// <returns>True if the <see cref="bool"/> was successfully retrieved, false otherwise</returns>
        [PublicAPI]
        public bool TryGetBool([NotNull] TKey key, out bool value) =>
            TryGetNotNull(key, out value);
        
        /// <summary>
        /// Try to obtain a <see cref="bool"/> identified by key. If the key
        /// exists but the value is null (or not a bool) this method will return false.
        /// </summary>
        /// <param name="key">The key identifying the <see cref="bool"/></param>
        /// <param name="valueSetter">Action to be invoked with the obtained <see cref="bool"/>. This will not
        /// be invoked if the <see cref="bool"/> could not be obtained or if the value is null</param>
        /// <returns>True if the <see cref="bool"/> was successfully retrieved, false otherwise</returns>
        [PublicAPI]
        public bool TryGetBool([NotNull] TKey key, [InstantHandle] Action<bool> valueSetter) =>
            TryGetNotNull(key, valueSetter);
        
        /// <summary>
        /// Try to obtain a <see cref="int"/> identified by key. If the key
        /// exists but the value is null (or not an int) this method will return false.
        /// </summary>
        /// <param name="key">The key identifying the <see cref="int"/></param>
        /// <param name="value">The <see cref="int"/> output</param>
        /// <returns>True if the <see cref="int"/> was successfully retrieved, false otherwise</returns>
        [PublicAPI]
        public bool TryGetInteger([NotNull] TKey key, out int value) =>
            TryGetNotNull(key, out value);
        
        /// <summary>
        /// Try to obtain a <see cref="int"/> identified by key. If the key
        /// exists but the value is null (or not an int) this method will return false.
        /// </summary>
        /// <param name="key">The key identifying the <see cref="int"/></param>
        /// <param name="valueSetter">Action to be invoked with the obtained <see cref="int"/>. This will not
        /// be invoked if the <see cref="int"/> could not be obtained or if the value is null</param>
        /// <returns>True if the <see cref="int"/> was successfully retrieved, false otherwise</returns>
        [PublicAPI]
        public bool TryGetInteger([NotNull] TKey key, [InstantHandle] Action<int> valueSetter) =>
            TryGetNotNull(key, valueSetter);
        
        /// <summary>
        /// Try to obtain a <see cref="double"/> identified by key. If the key
        /// exists but the value is null (or not a double) this method will return false.
        /// </summary>
        /// <param name="key">The key identifying the <see cref="double"/></param>
        /// <param name="value">The <see cref="double"/> output</param>
        /// <returns>True if the <see cref="double"/> was successfully retrieved, false otherwise</returns>
        [PublicAPI]
        public bool TryGetDouble([NotNull] TKey key, out double value) =>
            TryGetNotNull(key, out value);
        
        /// <summary>
        /// Try to obtain a <see cref="double"/> identified by key. If the key
        /// exists but the value is null (or not a double) this method will return false.
        /// </summary>
        /// <param name="key">The key identifying the <see cref="double"/></param>
        /// <param name="valueSetter">Action to be invoked with the obtained <see cref="double"/>. This will not
        /// be invoked if the <see cref="double"/> could not be obtained or if the value is null</param>
        /// <returns>True if the <see cref="double"/> was successfully retrieved, false otherwise</returns>
        [PublicAPI]
        public bool TryGetDouble([NotNull] TKey key, [InstantHandle] Action<double> valueSetter) =>
            TryGetNotNull(key, valueSetter);
        
        /// <summary>
        /// Try to obtain a <see cref="bool"/> identified by key. If the key
        /// exists but the value is null, this method will return true, and the
        /// value will be set to null. If the value is not a <see cref="bool"/>, this method
        /// will return false.
        /// </summary>
        /// <param name="key">The key identifying the <see cref="bool"/></param>
        /// <param name="value">The nullable <see cref="bool"/> output</param>
        /// <returns>True if the <see cref="bool"/> was successfully retrieved, false otherwise</returns>
        [PublicAPI]
        public bool TryGetBoolOrNull([NotNull] TKey key, out bool? value) =>
            TryGetCanBeNull(key, out value);
        
        /// <summary>
        /// Try to obtain a <see cref="bool"/> identified by key. If the key
        /// exists but the value is null, this method will return true, and
        /// the delegate will be called with null. If the value is not a <see cref="bool"/>, this method
        /// will return false.
        /// </summary>
        /// <param name="key">The key identifying the <see cref="bool"/></param>
        /// <param name="valueSetter">Action to be invoked with the obtained <see cref="bool"/>. This will not
        /// be invoked if the <see cref="bool"/> could not be obtained</param>
        /// <returns>True if the <see cref="bool"/> was successfully retrieved, false otherwise</returns>
        [PublicAPI]
        public bool TryGetBoolOrNull([NotNull] TKey key, [InstantHandle] Action<bool?> valueSetter) =>
            TryGetCanBeNull(key, valueSetter);
        
        /// <summary>
        /// Try to obtain a <see cref="int"/> identified by key. If the key
        /// exists but the value is null, this method will return true, and the
        /// value will be set to null. If the value is not a <see cref="int"/>, this method
        /// will return false.
        /// </summary>
        /// <param name="key">The key identifying the <see cref="int"/></param>
        /// <param name="value">The nullable <see cref="int"/> output</param>
        /// <returns>True if the <see cref="int"/> was successfully retrieved, false otherwise</returns>
        [PublicAPI]
        public bool TryGetIntegerOrNull([NotNull] TKey key, out int? value) =>
            TryGetCanBeNull(key, out value);
        
        /// <summary>
        /// Try to obtain a <see cref="int"/> identified by key. If the key
        /// exists but the value is null, this method will return true, and
        /// the delegate will be called with null. If the value is not a <see cref="int"/>, this method
        /// will return false.
        /// </summary>
        /// <param name="key">The key identifying the <see cref="int"/></param>
        /// <param name="valueSetter">Action to be invoked with the obtained <see cref="int"/>. This will not
        /// be invoked if the <see cref="int"/> could not be obtained</param>
        /// <returns>True if the <see cref="int"/> was successfully retrieved, false otherwise</returns>
        [PublicAPI]
        public bool TryGetIntegerOrNull([NotNull] TKey key, [InstantHandle] Action<int?> valueSetter) =>
            TryGetCanBeNull(key, valueSetter);
        
        /// <summary>
        /// Try to obtain a <see cref="double"/> identified by key. If the key
        /// exists but the value is null, this method will return true, and the
        /// value will be set to null. If the value is not a <see cref="double"/>, this method
        /// will return false.
        /// </summary>
        /// <param name="key">The key identifying the <see cref="double"/></param>
        /// <param name="value">The nullable <see cref="double"/> output</param>
        /// <returns>True if the <see cref="double"/> was successfully retrieved, false otherwise</returns>
        [PublicAPI]
        public bool TryGetDoubleOrNull([NotNull] TKey key, out double? value) =>
            TryGetCanBeNull(key, out value);
        
        /// <summary>
        /// Try to obtain a <see cref="double"/> identified by key. If the key
        /// exists but the value is null, this method will return true, and
        /// the delegate will be called with null. If the value is not a <see cref="double"/>, this method
        /// will return false.
        /// </summary>
        /// <param name="key">The key identifying the <see cref="double"/></param>
        /// <param name="valueSetter">Action to be invoked with the obtained <see cref="double"/>. This will not
        /// be invoked if the <see cref="double"/> could not be obtained</param>
        /// <returns>True if the <see cref="double"/> was successfully retrieved, false otherwise</returns>
        [PublicAPI]
        public bool TryGetDoubleOrNull([NotNull] TKey key, [InstantHandle] Action<double?> valueSetter) =>
            TryGetCanBeNull(key, valueSetter);


        protected abstract bool TryGetCanBeNull<T>([NotNull] TKey key, [CanBeNull] out T value);

        private bool TryGetCanBeNull<T>([NotNull] TKey key, [InstantHandle] Action<T> valueSetter)
        {
            if (TryGetCanBeNull(key, out T value))
            {
                valueSetter.Invoke(value);
                return true;
            }

            return false;
        }

        private bool TryGetNotNull<T>([NotNull] TKey key, out T value)
        {
            return TryGetCanBeNull(key, out value) && value != null;
        }
        
        private bool TryGetNotNull<T>([NotNull] TKey key, [InstantHandle] Action<T> valueSetter)
        {
            if (TryGetNotNull(key, out T value))
            {
                valueSetter.Invoke(value);
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// This object is a wrapper for lists
    /// returned by <see cref="Json.Deserialize(string)"/>.
    /// </summary>
    public class JsonList : JsonDataBase<int>
    {
        [NotNull]
        public static JsonList Empty => new JsonList(new List<object>());
        
        [NotNull]
        private readonly IList<object> Data;

        public JsonList([NotNull] IList<object> data)
        {
            Data = data;
        }

        public int Count => Data.Count;

        [PublicAPI, NotNull]
        public List<object> ToRawData() => Data.ToList();
        
        protected override bool TryGetCanBeNull<T>(int key, out T value)
        {
            if (key < 0 || key > Data.Count)
            {
                value = default;
                return false;
            }

            object objectValue = Data[key];

            return JsonConversionHelper.TryConvertTo(objectValue, out value);
        }
    }

    internal static class JsonConversionHelper
    {
        public static bool TryConvertTo<T>(object source, [CanBeNull] out T output)
        {
            try
            {
                if (source is T || CanBeAffectedNull(typeof(T)) && source == null)
                {
                    output = (T) source;
                    return true;
                }
                    
                T convertedValue = (T) Convert.ChangeType(source, typeof(T), CultureInfo.InvariantCulture);
                if (convertedValue != null)
                {
                    output = convertedValue;
                    return true;
                }
            }
            catch (Exception)
            {
                // Ignored
            }

            output = default;
            return false;
        }

        private static bool CanBeAffectedNull(Type type)
            => !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
    }
}

