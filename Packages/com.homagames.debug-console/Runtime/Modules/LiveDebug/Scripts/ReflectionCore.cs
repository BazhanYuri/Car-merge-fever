using System;
using System.Reflection;
using HomaGames.HomaConsole.Core.Attributes;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace HomaGames.HomaConsole
{
    internal static class ReflectionCore
    {
#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void CacheDebuggableTypes()
        {
            var settings = HomaConsoleSettings.GetOrCreateSettings();
            settings.types.Clear();
            foreach (var type in TypeCache.GetTypesWithAttribute<DebuggableClassAttribute>())
                settings.types.Add(type.AssemblyQualifiedName);
            EditorUtility.SetDirty(settings);
        }
#endif

        public static void LoadDebuggableData()
        {
            var settings = HomaConsoleSettings.GetOrCreateSettings().types;
            Type[] types = new Type[settings.Count];
            for (int i = 0; i < settings.Count; i++)
            {
                types[i] = Type.GetType(settings[i]);
            }

            for (int j = 0; j < types.Length; j++)
            {
                if (types[j] == null)
                    continue;
                // Fields
                var fields = types[j].GetFields();
                for (int k = 0; k < fields.Length; k++)
                {
                    var attributes = fields[k].GetCustomAttributes<DebuggableFieldAttribute>();
                    foreach (var debuggableAttribute in attributes)
                    {
                        AddDebuggableItem(debuggableAttribute, new DebugConsole.PropertyMeta()
                        {
                            propertyName = String.IsNullOrEmpty(debuggableAttribute.CustomName)
                                ? fields[k].Name
                                : debuggableAttribute.CustomName,
                            order = debuggableAttribute.Order,
                            propertyType = fields[k].FieldType,
                            targetType = types[j],
                            isStatic = fields[k].IsStatic,
                            Get = fields[k].GetValue,
                            Set = fields[k].SetValue,
                            layoutOption = debuggableAttribute.LayoutOption,
                            min = debuggableAttribute.Min,
                            max = debuggableAttribute.Max
                        });
                    }
                }

                // Properties
                var properties = types[j].GetProperties();
                for (int k = 0; k < properties.Length; k++)
                {
                    var attributes = properties[k].GetCustomAttributes<DebuggableFieldAttribute>();
                    foreach (var debuggableAttribute in attributes)
                    {
                        var propertyMeta = new DebugConsole.PropertyMeta()
                        {
                            propertyName = String.IsNullOrEmpty(debuggableAttribute.CustomName)
                                ? properties[k].Name
                                : debuggableAttribute.CustomName,
                            order = debuggableAttribute.Order,
                            propertyType = properties[k].PropertyType,
                            targetType = types[j],
                            isStatic = properties[k].GetMethod != null && properties[k].GetMethod.IsStatic,
                            layoutOption = debuggableAttribute.LayoutOption,
                            min = debuggableAttribute.Min,
                            max = debuggableAttribute.Max
                        };

                        if (properties[k].GetMethod != null)
                            propertyMeta.Get = properties[k].GetValue;
                        if (properties[k].SetMethod != null)
                            propertyMeta.Set = properties[k].SetValue;

                        AddDebuggableItem(debuggableAttribute, propertyMeta);
                    }
                }

                // Methods
                var methods = types[j].GetMethods();
                for (int k = 0; k < methods.Length; k++)
                {
                    var attributes = methods[k].GetCustomAttributes<DebuggableActionAttribute>();
                    foreach (var debuggableAttribute in attributes)
                    {
                        var k1 = k;
                        AddDebuggableItem(debuggableAttribute, new DebugConsole.PropertyMeta()
                        {
                            propertyName = String.IsNullOrEmpty(debuggableAttribute.CustomName)
                                ? methods[k].Name
                                : debuggableAttribute.CustomName,
                            order = debuggableAttribute.Order,
                            propertyType = typeof(System.Action),
                            targetType = types[j],
                            isStatic = methods[k].IsStatic,
                            Invoke = (target) => methods[k1].Invoke(target, null)
                        });
                    }
                }
            }
        }

        private static void AddDebuggableItem(DebuggableAttribute attribute, DebugConsole.PropertyMeta propertyMeta)
        {
            if (attribute.Paths.Length > 0)
            {
                for (int l = 0; l < attribute.Paths.Length; l++)
                {
                    DebugConsole.RegisterDebugProperty($"{attribute.Paths[l]}/{propertyMeta.propertyName}",
                        propertyMeta);
                }
            }
            else
            {
                DebugConsole.RegisterDebugProperty(propertyMeta.propertyName, propertyMeta);
            }
        }
    }
}