using System;
using UnityEditor;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public class ScriptableObjectSettingsProvider : ISettingsProvider
    {
        private Editor editorInstance;
        private ScriptableObject scriptableObject;
        private Func<ScriptableObject> scriptableProvider;

        public ScriptableObjectSettingsProvider(string name, string version, Func<ScriptableObject> scriptableFunc,
            int order = 99)
        {
            Name = name;
            Version = version;
            scriptableProvider = scriptableFunc;
            scriptableObject = scriptableFunc();
            editorInstance = Editor.CreateEditor(scriptableObject, null);
            Order = order;
        }

        public string Name { get; }
        public string Version { get; }
        public int Order { get; }

        public void Draw()
        {
            if (!scriptableObject)
            {
                scriptableObject = scriptableProvider();
                editorInstance = Editor.CreateEditor(scriptableObject, null);
            }

            editorInstance.OnInspectorGUI();
        }
    }
}