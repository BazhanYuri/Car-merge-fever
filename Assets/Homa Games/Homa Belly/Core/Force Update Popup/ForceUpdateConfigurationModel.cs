using System.Collections.Generic;
using HomaGames.HomaBelly.Utilities;

namespace HomaGames.HomaBelly
{
    public class ForceUpdateConfigurationModel
    {
        public bool Enabled;
        public bool ForceUpdate;
    
        public string MinRequiredVersion;
    
        public string StoreLink;
        
        private ForceUpdateConfigurationModel()
        { }

        public static ForceUpdateConfigurationModel FromServerResponse(JsonObject oForceUpdateData)
        {
            var output = new ForceUpdateConfigurationModel();

            if (oForceUpdateData == null)
                return output;

            oForceUpdateData.TryGetBool("b_enabled", b => output.Enabled = b);
            oForceUpdateData.TryGetBool("b_mandatory_update", b => output.ForceUpdate = b);
                
            oForceUpdateData.TryGetString("s_store_url",s => output.StoreLink = s);
            
            oForceUpdateData.TryGetString("s_min_required_version",s => output.MinRequiredVersion = s);

            return output;
        }
    }
}