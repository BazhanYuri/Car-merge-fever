using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace HomaGames.HomaConsole
{
    static class HomaConsoleSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            var provider = new SettingsProvider("Project/Homa Console", SettingsScope.Project)
            {
                label = "Homa Console",
                guiHandler = DrawGUI,
                keywords = new HashSet<string>(new[] {"Homa", "Console"})
            };

            return provider;
        }

        private static void DrawGUI(string searchContext)
        {
            var editor = Editor.CreateEditor(HomaConsoleSettings.GetOrCreateSettings());
            editor.DrawDefaultInspector();
        }
    }
}