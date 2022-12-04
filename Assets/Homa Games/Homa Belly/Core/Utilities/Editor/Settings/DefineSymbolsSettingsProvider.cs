using System.Collections;
using System.Collections.Generic;
using HomaGames.HomaBelly;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public class DefineSymbolsSettingsProvider : ISettingsProvider
    {
        public int Order { get; }
        public string Name { get; }
        public string Version { get; }

        private readonly List<DefineSymbolSettingsElement> SettingsElements;

        public DefineSymbolsSettingsProvider(string name, int order, List<DefineSymbolSettingsElement> settingsElements) 
            : this(name, order, "", settingsElements)
        { }

        public DefineSymbolsSettingsProvider(string name, int order, string version, List<DefineSymbolSettingsElement> settingsElements)
        {
            Name = name;
            Order = order;
            Version = version;
            SettingsElements = settingsElements ?? new List<DefineSymbolSettingsElement>();

            foreach (DefineSymbolSettingsElement element in SettingsElements)
            {
                DefineSymbolsUtility.TrySetInitialValue(element.DefineSymbolName, element.DefaultValue, element.DefaultValuePrefKey);
            }
        }

        [PublicAPI]
        public void AddSettingsElement(DefineSymbolSettingsElement element) => SettingsElements.Add(element);
        [PublicAPI]
        public void RemoveSettingsElement(DefineSymbolSettingsElement element) => SettingsElements.Remove(element);
        
        public void Draw()
        {
            foreach (DefineSymbolSettingsElement element in SettingsElements)
            {
                bool value = DefineSymbolsUtility.GetDefineSymbolValue(element.DefineSymbolName);
                bool newValue = EditorGUILayout.Toggle(new GUIContent(element.SettingsName, element.SettingsTooltip), value);
                
                if (value != newValue)
                    DefineSymbolsUtility.SetDefineSymbolValue(element.DefineSymbolName, newValue);
            }
        }
    }

    public struct DefineSymbolSettingsElement
    {
        public string DefineSymbolName;

        public string SettingsName;
        [CanBeNull]
        public string SettingsTooltip;

        public bool DefaultValue;
        // For legacy purposes only
        [CanBeNull]
        public string DefaultValuePrefKey;
    }
}
