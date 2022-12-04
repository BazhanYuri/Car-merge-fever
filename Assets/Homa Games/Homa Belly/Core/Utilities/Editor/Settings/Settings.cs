using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public static class Settings
    {
        private static HashSet<ISettingsProvider> _settingsList = new HashSet<ISettingsProvider>();
        public static IEnumerable<ISettingsProvider> AllSettings => _settingsList;
        
        public static void RegisterSettings(string name,string version,Func<ScriptableObject> scriptableObject)
        {
            _settingsList.Add(new ScriptableObjectSettingsProvider(name,version,scriptableObject));
        }

        public static void RegisterSettings(ISettingsProvider settingsProvider)
        {
            _settingsList.Add(settingsProvider);
        }

        private static Vector2 scroll;
        public static void Draw()
        {
            var titleStyle = new GUIStyle(EditorStyles.boldLabel);
            titleStyle.fontSize = 20;
            scroll = EditorGUILayout.BeginScrollView(scroll);
            foreach (var settingsProvider in Settings.AllSettings.OrderBy(provider => provider.Order))
            {
                EditorGUILayout.LabelField(settingsProvider.Name,titleStyle,GUILayout.Height(30));
                settingsProvider.Draw();
                if(!String.IsNullOrEmpty(settingsProvider.Version))
                    EditorGUILayout.LabelField($"v{settingsProvider.Version}",EditorStyles.centeredGreyMiniLabel);
                DrawUILine(Color.grey);
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndScrollView();
        }

        public static void OpenWindow()
        {
            EditorWindow.GetWindow<SettingsWindow>(false, "Homa Belly Settings",true);
        }

        private static void DrawUILine(Color color, int thickness = 2)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(thickness));
            r.height = thickness;
            r.x-=2;
            r.width +=6;
            EditorGUI.DrawRect(r, color);
        }
    }   
}