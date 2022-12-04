using System;
using System.IO;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Utils to manager proguard file
    /// </summary>
    public static class AndroidProguardUtils
    {
        private static string PROGUARD_FILE_PATH = Application.dataPath + "/Plugins/Android/proguard-user.txt";

        /// <summary>
        /// Adds proguard rules to main proguard file at `PROGUARD_FILE_PATH`
        /// </summary>
        public static void AddProguardRules(string rules)
        {
            AddProguardRules(rules, PROGUARD_FILE_PATH);
        }

        /// <summary>
        /// Adds proguard rules to the given proguard file path
        /// </summary>
        public static void AddProguardRules(string rules, string proguardFilePath)
        {
            if (File.Exists(proguardFilePath))
            {
                string proguardContents = File.ReadAllText(proguardFilePath);

                // Add proguard rules if not found
                if (!proguardContents.Contains(rules))
                {
                    HomaBellyEditorLog.Debug($"Proguard file detected. Adding proguard rules: {rules.Substring(0, Math.Min(rules.Length, 10))}...");
                    proguardContents += rules;
                    File.WriteAllText(proguardFilePath, proguardContents);
                }
            }
        }
    }
}
