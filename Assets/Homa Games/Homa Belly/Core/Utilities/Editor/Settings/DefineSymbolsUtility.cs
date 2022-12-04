using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public static class DefineSymbolsUtility
    {
        public static void TrySetInitialValue(string symbol, bool value, string editorPrefKey = null)
        {
            // We do not want to modify Define Symbols while in batch mode (headless builds)
            // as the Editor Player Prefs won't be defined and this potentially changes
            // the desired behavior (ie: release builds and HOMA_DEVELOPMENT)
            if (Application.isBatchMode)
            {
                return;
            }
            
            editorPrefKey = editorPrefKey ?? $"homagames.{symbol.ToLower()}_on_first_install";
            var enabledOnce = EditorPrefs.GetInt(editorPrefKey, 0) == 1;
            
            if (!enabledOnce)
            {
                SetDefineSymbolValue(symbol, value);
                EditorPrefs.SetInt(editorPrefKey, 1);
            }
        }

        public static bool GetDefineSymbolValue(string symbol) => ContainsDefineSymbolForBothPlatformsAndSync(symbol);
        public static void SetDefineSymbolValue(string symbol, bool value) => SetDefineSymbolForAllPlatforms(symbol, value);

        /// <summary>
        /// Return the define symbol state for both platforms and also sync define symbol for both platforms if they are in different states.
        /// </summary>
        /// <returns>True if the define symbol is enabled for both platforms.</returns>
        private static bool ContainsDefineSymbolForBothPlatformsAndSync(string symbol)
        {
            var isEnabledForCurrentPlatform = GetDefineSymbolForCurrentPlatform(symbol);
            SetDefineSymbolForAllPlatforms(symbol,isEnabledForCurrentPlatform);
            return isEnabledForCurrentPlatform;
        }
        
        private static bool ContainsDefineSymbol(BuildTargetGroup buildTargetGroup, string symbol)
        {
            var symbols = new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup).Split(';'));
            return symbols.Contains(symbol);
        }

        private static void SetDefineSymbolForAllPlatforms(string symbol, bool enabled)
        {
            // We do not want to modify Define Symbols while in batch mode (headless builds)
            // as the Editor Player Prefs won't be defined and this potentially changes
            // the desired behavior (ie: release builds and HOMA_DEVELOPMENT)
            if (Application.isBatchMode)
            {
                return;
            }
            SessionState.SetBool(symbol, enabled);
            SetDefineSymbol(BuildTargetGroup.Android, symbol, enabled);
            SetDefineSymbol(BuildTargetGroup.iOS, symbol, enabled);
        }

        private static bool GetDefineSymbolForCurrentPlatform(string symbol)
        {
            return ContainsDefineSymbol(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget),symbol);
        }
        
        private static void SetDefineSymbol(BuildTargetGroup buildTargetGroup, string symbol, bool enabled)
        {
            // We do not want to modify Define Symbols while in batch mode (headless builds)
            // as the Editor Player Prefs won't be defined and this potentially changes
            // the desired behavior (ie: release builds and HOMA_DEVELOPMENT)
            if (Application.isBatchMode)
            {
                return;
            }
            
            var symbols = new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup).Split(';'));
            var alreadySet = symbols.Contains(symbol);
            bool changed = false;
            if (enabled && !alreadySet)
            {
                symbols.Add(symbol);
                changed = true;
            }
            else if(!enabled && alreadySet)
            {
                symbols.Remove(symbol);
                changed = true;
            }

            if (changed)
            {
                var builtSymbols = String.Join(";", symbols);
                if (builtSymbols.StartsWith(";"))
                    builtSymbols = builtSymbols.Remove(0, 1);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup,builtSymbols);
                AssetDatabase.Refresh();
            }
        }
    }
}