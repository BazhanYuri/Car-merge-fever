using System;
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    [CustomEditor(typeof(HomaBellyManifestConfiguration))]
    [CanEditMultipleObjects]
    public class ConfigurationAssetEditor : Editor
    {
        private SerializedProperty manifestHash;
        private SerializedProperty configProperty;
        private bool pluginDataFoldout = false;

        private void OnEnable()
        {
            manifestHash = serializedObject.FindProperty("PluginManifestHash");
            configProperty = serializedObject.FindProperty("configData");
        }

        public override void OnInspectorGUI()
        {
            bool guiPreviouslyEnabled = GUI.enabled;
            GUI.enabled = false;
            if (manifestHash != null)
            {
                EditorGUILayout.PropertyField(manifestHash, new GUIContent("Plugin Manifest Hash"));
            }

            if (configProperty != null)
            {
                EditorGUILayout.PropertyField(configProperty, new GUIContent("Serialized Data"));
            }

            PluginManifest pluginManifest = PluginManifest.LoadFromLocalFile();
            if (pluginManifest != null)
            {
                pluginDataFoldout = EditorGUILayout.Foldout(pluginDataFoldout, "Plugin Data");
                if (pluginDataFoldout)
                {
                    foreach (var component in pluginManifest.Packages.GetAllPackages())
                    {
                        if (component.Data.Count > 0)
                        {
                            EditorGUILayout.LabelField(component.Id);
                            EditorGUI.indentLevel += 1;
                            foreach (var data in component.Data)
                            {
                                EditorGUILayout.TextField(data.Key, data.Value);
                            }

                            EditorGUI.indentLevel -= 1;
                        }
                    }
                }
            }
            
            GUI.enabled = guiPreviouslyEnabled;
        }
    }
}