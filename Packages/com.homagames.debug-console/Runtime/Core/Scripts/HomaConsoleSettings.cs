using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace HomaGames.HomaConsole
{
    public class HomaConsoleSettings : ScriptableObject
    {
        public const string ResourceDirectory = "Homa Console/";
        public const string AssetName = "HomaConsoleSettings";
        public const string ResourcePath = ResourceDirectory + AssetName;
        public const string DirectoryPath = "Assets/Resources/" + ResourceDirectory;
        public const string AssetPath = DirectoryPath + AssetName + ".asset";
        public Activator.AnchorMode activatorMode; 
        public float activatorSize;
        [HideInInspector]
        public List<string> types = new List<string>();

        public static HomaConsoleSettings GetOrCreateSettings()
        {
            var settings = Resources.Load<HomaConsoleSettings>(ResourcePath);
#if UNITY_EDITOR
            if (settings == null)
            {
                var fullPath = Path.Combine(Application.dataPath,"..",DirectoryPath);
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }
                settings = ScriptableObject.CreateInstance<HomaConsoleSettings>();
                settings.activatorSize = 100;
                settings.activatorMode = Activator.AnchorMode.TopLeft;
                AssetDatabase.CreateAsset(settings, AssetPath);
                AssetDatabase.SaveAssets();
            }
#endif
            return settings;
        }
    }
}