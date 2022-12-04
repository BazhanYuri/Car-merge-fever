using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public static class SettingsReflection
    {
        private static bool reflectionCached = false;
        private static MethodInfo cachedMethodInfo;

        public static bool SettingsAPIExists()
        {
            return GetSettingsMethod() != null;
        }

        public static void OpenSettings()
        {
            GetSettingsMethod()?.Invoke(null, null);
        }

        private static MethodInfo GetSettingsMethod()
        {
            if (!reflectionCached)
            {
                Type t = Type.GetType("HomaGames.HomaBelly.Settings, Assembly-CSharp-Editor");
                cachedMethodInfo
                    = t?.GetMethod("OpenWindow", BindingFlags.Static | BindingFlags.Public);
                reflectionCached = true;
            }

            return cachedMethodInfo;
        }
    }
}
