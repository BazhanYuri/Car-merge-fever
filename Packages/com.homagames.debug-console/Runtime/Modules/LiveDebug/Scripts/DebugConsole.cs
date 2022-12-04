using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using HomaGames.HomaConsole.Core;
using HomaGames.HomaConsole.Core.Attributes;
using UnityEngine;

namespace HomaGames.HomaConsole
{
    public static class DebugConsole
    {
        public static event Action OnDebugItemChanged;

        [Serializable]
        public struct PropertyMeta
        {
            public string propertyName;
            public int order;
            public Type propertyType;
            public Type targetType;
            public bool isStatic;
            public System.Action<object, object> Set;
            public System.Func<object, object> Get;
            public System.Action<object> Invoke;
            public LayoutOption layoutOption;
            public float min;
            public float max;

            public bool IsReadOnly => Set == null && Get != null;

            public override int GetHashCode()
            {
                return propertyName.GetHashCode();
            }
        }

        // Full paths to properties
        internal static Dictionary<string, PropertyMeta> DebugPaths = new Dictionary<string, PropertyMeta>();

        public static void Set<T>(string fullPath,T newValue, object target)
        {
            if (DebugPaths.ContainsKey(fullPath) && DebugPaths[fullPath].Set != null)
            {
                if (target == null && !DebugPaths[fullPath].isStatic)
                    return;
                DebugPaths[fullPath].Set(target, newValue);
            }
        }

        public static T Get<T>(string fullPath, object target)
        {
            if (DebugPaths.ContainsKey(fullPath) && DebugPaths[fullPath].Get != null)
            {
                if (target == null && !DebugPaths[fullPath].isStatic)
                    return default;
                return (T) DebugPaths[fullPath].Get(target);
            }

            return default;
        }

        public static void Invoke(string fullPath, object target)
        {
            if (DebugPaths.ContainsKey(fullPath) && DebugPaths[fullPath].Invoke != null)
            {
                if (target == null && !DebugPaths[fullPath].isStatic)
                    return;
                DebugPaths[fullPath].Invoke(target);
            }
        }

        internal static Type GetTargetType(string fullPath)
        {
            if (DebugPaths.TryGetValue(fullPath,out PropertyMeta prop))
                return prop.targetType;
            return null;
        }

        public static void RegisterDebugProperty(string path, PropertyMeta property)
        {
            DebugPaths[path] = property;
            OnDebugItemChanged?.Invoke();
        }
    }
}