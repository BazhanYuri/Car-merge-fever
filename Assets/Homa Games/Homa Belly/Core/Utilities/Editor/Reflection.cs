#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Utility class for Reflection features
    /// </summary>
    public static class Reflection
    {
        /// <summary>
        /// Obtain a list of available Types found within the currently
        /// loaded assemblies. Found classes wont be interfaces
        /// or abstract classes
        /// </summary>
        /// <param name="type">The Type to look for</param>
        /// <returns></returns>
        public static List<Type> GetHomaBellyAvailableClasses(Type type)
        {
            List<Type> types = new List<Type>();

            try
            {
                types = new List<Type>(TypeCache.GetTypesDerivedFrom(type)) {type};
                types.RemoveAll(t => t.IsAbstract || t.IsInterface);
            }
            catch (Exception e)
            {
                HomaGamesLog.Warning($"Exception searching for type {type}: {e.Message}");
            }

            return types;
        }
    }
}
#endif
