using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HomaGames.HomaBelly.DataPrivacy
{
    public class SettingsEditorDrawer
    {
        #region Private properties
        private DataPrivacy.Settings Settings;
        
        private Material baseBackgroundMaterial;
        private Material fontMaterial;
        private Material textMeshProFontMaterial;
        private Material buttonFontMaterial;

        private bool configurationFoldout = true;
        private bool colorsFoldout = true;
        private bool localisationFoldout = true;
        
        private bool PluginManifestAvailable { get; }

        private GUIStyle FoldoutStyle => new GUIStyle(EditorStyles.foldoutHeader)
        {
            fontStyle = FontStyle.Bold
        };
        #endregion


        public SettingsEditorDrawer(DataPrivacy.Settings settings)
        {
            Settings = settings;
            LoadAssets();
            PluginManifestAvailable = PluginManifest.LoadFromLocalFile() != null;
        }

        public void SetFoldoutsState(bool state)
        {
            configurationFoldout = state;
            colorsFoldout = state;
            localisationFoldout = state;
        }

        public void OnInspectorGUI()
        {
            // Change properties
            configurationFoldout = EditorGUILayout.Foldout(configurationFoldout, "Configuration", FoldoutStyle);
            if (configurationFoldout)
            {
                bool originalWordWrap = GUI.skin.textField.wordWrap;
                GUI.skin.textField.wordWrap = true;
                float oldLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 200.0f; 

                Settings.GameName = EditorGUILayout.TextField(new GUIContent("Game Name",
                    "The game name to be visible in the DataPrivacy welcome page"), Settings.GameName);
                Settings.iOSIdfaPopupMessage = EditorGUILayout.TextField(new GUIContent("[iOS only] Privacy Popup Message",
                    "Customizable popup message to be displayed on the native popup"), Settings.iOSIdfaPopupMessage, GUILayout.Height(40));
                
                EditorGUIUtility.labelWidth = oldLabelWidth; 
                GUI.skin.textField.wordWrap = originalWordWrap;
                
                EditorGUILayout.Space(20);                

                Settings.ForceDisableGdpr =
                    EditorGUILayout.Toggle(
                        new GUIContent("Force Disable GDPR", "Disable GDPR for all platforms"),
                        Settings.ForceDisableGdpr);

                string disabledTooltip = string.Empty;
                bool previousGuiEnabled = GUI.enabled;
                if (PluginManifestAvailable)
                {
                    GUI.enabled = false;
                    disabledTooltip = "This option is editable through the Homa Belly manifest";
                }

                Settings.GdprEnabledForIos =
                    EditorGUILayout.Toggle(new GUIContent("GDPR Enabled for iOS", disabledTooltip),
                        Settings.GdprEnabledForIos);
                Settings.GdprEnabledForAndroid =
                    EditorGUILayout.Toggle(new GUIContent("GDPR Enabled for Android", disabledTooltip),
                        Settings.GdprEnabledForAndroid);
                Settings.ShowIdfa =
                    EditorGUILayout.Toggle(new GUIContent("[iOS only] IDFA Enabled", disabledTooltip),
                        Settings.ShowIdfa);
                Settings.ShowIdfaPrePopup =
                    EditorGUILayout.Toggle(new GUIContent("[iOS only] IDFA pre-popup Enabled", disabledTooltip),
                        Settings.ShowIdfaPrePopup);

                GUI.enabled = previousGuiEnabled;
            }
#if UNITY_2019_3_OR_NEWER
            EditorGUILayout.Space(10);
#else
            EditorGUILayout.Space();
#endif
            
            colorsFoldout = EditorGUILayout.Foldout(colorsFoldout, "Colors", FoldoutStyle);
            if (colorsFoldout)
            {
                Settings.BackgroundColor = EditorGUILayout.ColorField(new GUIContent("Background Color",
                    "Base background color for the DataPrivacy windows"), Settings.BackgroundColor);
                Settings.FontColor = EditorGUILayout.ColorField(new GUIContent("Font Color", "Base font color for the DataPrivacy text"), Settings.FontColor);
                Settings.SecondaryFontColor = EditorGUILayout.ColorField(new GUIContent("Secondary Font Color",
                    "Secondary font color for the DataPrivacy text"), Settings.SecondaryFontColor);
                Settings.ToggleColor = EditorGUILayout.ColorField(new GUIContent("Toggle Color", "Toggle color when it is ON"), Settings.ToggleColor);
                Settings.ButtonFontColor = EditorGUILayout.ColorField(new GUIContent("Button Font Color", "Font color for the DataPrivacy buttons"), Settings.ButtonFontColor);
            }
#if UNITY_2019_3_OR_NEWER
            EditorGUILayout.Space(10);
#else
            EditorGUILayout.Space();
#endif

            // Only enable localization settings if is not configured to fetch values from NTesting
            if (!Localization.FETCH_LITERALS_FROM_NTESTING)
            {
                localisationFoldout = EditorGUILayout.Foldout(localisationFoldout, "Localization", FoldoutStyle);
                if (localisationFoldout)
                {
                    Settings.UseDeviceLanguageWhenPossible = EditorGUILayout.Toggle(new GUIContent("Use Device Language",
                        "Flag informing if DataPrivacy should be displayed in device language in runtime or not"), Settings.UseDeviceLanguageWhenPossible);

                    // If user decided not to use device language
                    if (!Settings.UseDeviceLanguageWhenPossible)
                    {
                        var newLanguage = (DataPrivacy.Settings.SupportedLanguages) EditorGUILayout.EnumPopup(new GUIContent("Language",
                            "If `Use Device Language` is set to false, DataPrivacy will be always shown in this specific language"), Settings.Language); 

                        // Detect if language has changed. If so, load new one
                        if (Settings.Language != newLanguage)
                        {
                            Settings.Language = newLanguage;
                            Localization.CurrentLanguage = newLanguage;
                        }
                    }
                }
            }

            // Apply properties to material
            ApplySettingsToMaterials();

            // Reset to default
            if (GUILayout.Button("Reset to Defaults"))
            {
                Settings.ResetToDefaultValues();
                AssetDatabase.ForceReserializeAssets(new[] { Constants.DATA_PRIVACY_SETTINGS_ASSET_PATH });
            }
        }

        #region Private helpers
        private void LoadAssets()
        {
            if (baseBackgroundMaterial == null)
            {
                baseBackgroundMaterial = (Material)AssetDatabase.LoadAssetAtPath("Assets/Homa Games/Homa Belly/Core/DataPrivacy/Runtime/Materials/DataPrivacy_BaseBackground.mat", typeof(Material));
            }

            if (fontMaterial == null)
            {
                fontMaterial = (Material)AssetDatabase.LoadAssetAtPath("Assets/Homa Games/Homa Belly/Core/DataPrivacy/Runtime/Materials/DataPrivacy_Font.mat", typeof(Material));
            }

            if (buttonFontMaterial == null)
            {
                buttonFontMaterial = (Material)AssetDatabase.LoadAssetAtPath("Assets/Homa Games/Homa Belly/Core/DataPrivacy/Runtime/Materials/DataPrivacy_ButtonFont.mat", typeof(Material));
            }

            if (textMeshProFontMaterial == null)
            {
                textMeshProFontMaterial = (Material)AssetDatabase.LoadAssetAtPath("Assets/Homa Games/Homa Belly/Core/DataPrivacy/Runtime/Fonts/Poppins/Poppins-Regular SDF.mat", typeof(Material));
            }

            if (baseBackgroundMaterial == null
                || fontMaterial == null
                || buttonFontMaterial == null
                || textMeshProFontMaterial == null)
            {
                Debug.LogWarning("Some materials have not loaded properly. Applied " +
                                 "colours to DataPrivacy settings may not be saved properly.");
            }
        }

        private void ApplySettingsToMaterials()
        {
            if (Settings != null)
            {
                if (baseBackgroundMaterial != null) baseBackgroundMaterial.color = Settings.BackgroundColor;
                if (fontMaterial != null) fontMaterial.color = Settings.FontColor;
                if (textMeshProFontMaterial != null) textMeshProFontMaterial.SetColor("_FaceColor", Settings.FontColor);
                if (buttonFontMaterial != null) buttonFontMaterial.color = Settings.ButtonFontColor;
            }
        }
        #endregion
    }
}