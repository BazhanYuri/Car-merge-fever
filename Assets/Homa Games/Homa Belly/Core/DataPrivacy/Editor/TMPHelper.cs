using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace HomaGames.HomaBelly.DataPrivacy
{
    public static class TMPHelper
    {
        public static event System.Action OnResourcesInstalled;

        [InitializeOnLoadMethod]
        public static void CheckOrInstallDependencies()
        {
            if (!Resources.Load<TMP_Settings>("TMP Settings"))
            {
                AssetDatabase.importPackageCompleted += OnPackageImported;
                AssetDatabase.ImportPackage(
                    GetPackageFullPath() + "/Package Resources/TMP Essential Resources.unitypackage", false);
            }
            else
            {
                OnResourcesInstalled?.Invoke();
            }
        }

        static void OnPackageImported(string packageName)
        {
            if (packageName == "TMP Essential Resources")
            {
                TMPro_EventManager.ON_RESOURCES_LOADED();

#if UNITY_2018_3_OR_NEWER
                SettingsService.NotifySettingsProviderChanged();
#endif
                OnResourcesInstalled?.Invoke();
            }

            AssetDatabase.importPackageCompleted -= OnPackageImported;
        }

        static string GetPackageFullPath()
        {
            // Check for potential UPM package
            string packagePath = Path.GetFullPath("Packages/com.unity.textmeshpro");
            if (Directory.Exists(packagePath))
            {
                return packagePath;
            }

            packagePath = Path.GetFullPath("Assets/..");
            if (Directory.Exists(packagePath))
            {
                // Search default location for development package
                if (Directory.Exists(packagePath + "/Assets/Packages/com.unity.TextMeshPro/Editor Resources"))
                {
                    return packagePath + "/Assets/Packages/com.unity.TextMeshPro";
                }

                // Search for default location of normal TextMesh Pro AssetStore package
                if (Directory.Exists(packagePath + "/Assets/TextMesh Pro/Editor Resources"))
                {
                    return packagePath + "/Assets/TextMesh Pro";
                }

                // Search for potential alternative locations in the user project
                string[] matchingPaths =
                    Directory.GetDirectories(packagePath, "TextMesh Pro", SearchOption.AllDirectories);
                string path = ValidateLocation(matchingPaths, packagePath);
                if (path != null) return packagePath + path;
            }

            return null;
        }

        static string ValidateLocation(string[] paths, string projectPath)
        {
            for (int i = 0; i < paths.Length; i++)
            {
                // Check if the Editor Resources folder exists.
                if (Directory.Exists(paths[i] + "/Editor Resources"))
                {
                    string folderPath = paths[i].Replace(projectPath, "");
                    folderPath = folderPath.TrimStart('\\', '/');
                    return folderPath;
                }
            }

            return null;
        }
    }
}