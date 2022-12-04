using UnityEditor;

namespace HomaGames.HomaBelly
{
    public class HomaBellyDefineCheck : AssetPostprocessor
    {
        [InitializeOnLoadMethod]
        public static void Check()
        {
            DefineSymbolsUtility.SetDefineSymbolValue("HOMA_BELLY", true);
        }
        
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (var deletedAssetName in deletedAssets)
            {
                if (deletedAssetName.EndsWith(nameof(HomaBellyDefineCheck) + ".cs"))
                {
                    // Deleting Core package. Remove define symbol
                    DefineSymbolsUtility.SetDefineSymbolValue("HOMA_BELLY", false);
                }
            }
        }
    }
}

