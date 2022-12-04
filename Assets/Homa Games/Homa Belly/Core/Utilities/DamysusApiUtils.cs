using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public static class DamysusApiUtils
    {
        [CanBeNull]
        public static string GetMessageFromCode(string code)
        {
            switch (code)
            {
                case "CONF_VERSION_MISSING":
                    return "Missing configuration version in request";
                case "CONF_VERSION_WRONG":
                    return "Wrong configuration version format in request";
                case "CONF_VERSION_DEPRECATED":
                    return "Configuration version deprecated";
                case "SDK_VERSION_WRONG":
                    return "Wrong SDK version in request";
                case "APP_ID_MISSING":
                    return "Missing app ID in request";
                case "APP_VERSION_WRONG":
                    return "Wrong app version in request";
                default:
                    return null;
            }
        }
    }
}