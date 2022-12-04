using System;

namespace HomaGames.HomaBelly
{
    [Serializable]
    public class PluginManifest
    {
        //[JsonProperty("res")]
        public AvailablePackages Packages;
        //[JsonProperty("ti")]
        public string AppToken;

        private static PluginManifestDeserializer pluginManifestDeserializer = new PluginManifestDeserializer();

        public static PluginManifest LoadFromLocalFile()
        {
            return pluginManifestDeserializer.LoadFromCache();
        }

        #region Overrides

        public override string ToString()
        {
            string result = "Plugin Manifest:\n";

            result += Packages != null ? Packages.ToString() : "";

            return result;
        }

        public override bool Equals(object obj)
        {
            // Check for null and compare run-time types.
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                PluginManifest other = (PluginManifest) obj;
                bool equals = true;
                equals &= this.AppToken == other.AppToken;
                equals &= this.ToString() == other.ToString();
                return equals;
            }
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        #endregion
    }
}