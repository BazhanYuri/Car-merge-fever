using System;
using System.Collections.Generic;
using HomaGames.HomaBelly;
using UnityEditor;

namespace HomaGames.Geryon
{
    public class GeryonPostprocessor
    {
        private const string GERYON_ENABLED_DEFINE = "HOMA_BELLY_GERYON_ENABLED";
        
        [InitializeOnLoadMethod]
        static void Configure()
        {
            PluginManifest pluginManifest = PluginManifest.LoadFromLocalFile();

            if (pluginManifest != null)
            {
                PackageComponent packageComponent = pluginManifest.Packages
                    .GetPackageComponent(HomaBellyEditorConstants.PACKAGE_ID, HomaBellyEditorConstants.PACKAGE_TYPE);
                if (packageComponent != null)
                {
                    Dictionary<string, string> configurationData = packageComponent.Data;
                    
                    if (configurationData.TryGetValue("b_enable_geryon", out var value))
                    {
                        DefineSymbolsUtility.SetDefineSymbolValue(GERYON_ENABLED_DEFINE, Convert.ToBoolean(value));
                    }
                    
                }
            }
        }
    }
}
