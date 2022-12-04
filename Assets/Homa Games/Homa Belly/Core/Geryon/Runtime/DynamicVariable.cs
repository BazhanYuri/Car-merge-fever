using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HomaGames.Geryon
{
    /// <summary>
    /// Class representing a dynamic variable. This class can only host bool, int, double or string values.
    /// </summary>
    /// <typeparam name="T">The data type of the variable</typeparam>
    public sealed class DynamicVariable<T>
    {
        

        static DynamicVariable()
        {
            List<Type> authorizedTypes = new List<Type>
            {
                typeof(string),
                typeof(int),
                typeof(double),
                typeof(bool)
            };
            
            if (!authorizedTypes.Contains(typeof(T)))
            {
                throw new InvalidTypeParameterException(
                    $"{typeof(DynamicVariable<>).Name} can only be used with the given types: " +
                    $"{string.Join(", ", authorizedTypes.Select(t => t.Name))}");
            }
        }
        
        /// <summary>
        /// Collection of all available DynamicVariables
        /// </summary>
        private static readonly Dictionary<string, T> innerDynamicVariablesDictionary = new Dictionary<string, T>();

        public static T Get(string key, T defaultValue)
        {
            if (Config.Initialized)
                return innerDynamicVariablesDictionary.TryGetValue(key, out var dvr) ? dvr : defaultValue;
            Debug.LogWarning($"You're trying to access {key} N-Testing value before N-Testing is initialised.");
            return defaultValue;
        }
        
        /// <summary>
        /// Try getting a N-Testing value from a key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>Returns true if the value is available.</returns>
        public static bool TryGet(string key, out T value)
        {
            value = default;
            return Config.Initialized && innerDynamicVariablesDictionary.TryGetValue(key, out value);
        }

        /// <summary>
        /// Updates the dynamic variable referenced by `key`, if it exists.
        /// If not, adds it to the dictionary
        /// </summary>
        /// <param name="key">The key referencing the dynamic variable</param>
        /// <param name="value">The new value</param>
        public static void Set(string key, T value)
        {
            innerDynamicVariablesDictionary[key] = value;
        }

        #region Unit Test helpers
#if UNITY_EDITOR
        /// <summary>
        /// Method to clear all stored dynamic variables. This method
        /// is only used for unit testing.
        /// </summary>
        public static void ClearDynamicVariables()
        {
            innerDynamicVariablesDictionary.Clear();
        }

        /// <summary>
        /// Getter to be used within Unit Tests
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T GetForUnitTests(string key)
        {
            return innerDynamicVariablesDictionary[key];
        }
#endif
        #endregion
    }

    public class InvalidTypeParameterException : Exception
    {
        public InvalidTypeParameterException()
        {
        }

        public InvalidTypeParameterException(string message) : base(message)
        {
        }
    }
}