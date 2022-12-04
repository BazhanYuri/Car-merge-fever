using UnityEditor;

namespace HomaGames.HomaBelly.DataPrivacy
{
    public class DataPrivacySettingsProvider : ISettingsProvider
    {
        [InitializeOnLoadMethod]
        private static void RegisterProvider()
        {
            HomaGames.HomaBelly.Settings.RegisterSettings(new DataPrivacySettingsProvider());
        }
    
        public int Order => 3;
        public string Name => "Data Privacy";
        public string Version => null;

        private DataPrivacy.Settings Settings;
        private SettingsEditorDrawer SettingsDrawer;

        private DataPrivacySettingsProvider()
        {
            Settings = DataPrivacy.Settings.EditorCreateOrLoadDataPrivacySettings();
            SettingsDrawer = new SettingsEditorDrawer(Settings);
            
            SettingsDrawer.SetFoldoutsState(false);
        }
        
        public void Draw()
        {
            EditorGUI.BeginChangeCheck();
            
            SettingsDrawer.OnInspectorGUI();
            
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(Settings);
        }
    }
}