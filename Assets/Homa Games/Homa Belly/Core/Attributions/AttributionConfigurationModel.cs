using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using HomaGames.HomaBelly.Utilities;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public class AttributionConfigurationModel
    {
        private const string DISABLE_PREF_KEY = "homagames.attribution.disable_singular";
        public bool DisableSingular
        {
            get => PlayerPrefs.GetInt(DISABLE_PREF_KEY, 0) == 1;
            set => PlayerPrefs.SetInt(DISABLE_PREF_KEY, value ? 1 : 0);
        }
    
        public static AttributionConfigurationModel FromRemoteConfigurationDictionary(JsonObject attributionObject)
        {
            AttributionConfigurationModel model = new AttributionConfigurationModel();

            if (attributionObject != null)
            {
                if (attributionObject.TryGetBool("b_disable_singular", out var disableSingular))
                {
                    model.DisableSingular = disableSingular;
                }
            }

            return model;
        }
    }
}