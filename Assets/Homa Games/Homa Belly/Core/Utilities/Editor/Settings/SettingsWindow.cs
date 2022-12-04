using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public class SettingsWindow : EditorWindow
    {
        [MenuItem("Window/Homa Games/Homa Belly/Settings", false, 12)]
        static void Init()
        {
            SettingsWindow window = (SettingsWindow)GetWindow(typeof(SettingsWindow),false, "Homa Belly Settings");
            window.Show();
        }

        void OnGUI()
        {
            Settings.Draw();
        }
    }
}