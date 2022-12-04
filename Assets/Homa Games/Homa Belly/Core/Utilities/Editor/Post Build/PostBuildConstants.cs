using HomaGames.HomaBelly.Utilities;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public static class PostBuildConstants
    {
        public static string API_APP_POST_BUILD = HomaBellyConstants.API_HOST + "/appbuild?cv=" + HomaBellyConstants.API_VERSION
            + "&sv=" + HomaBellyConstants.PRODUCT_VERSION
            + "&av=" + Application.version
            + "&ti={0}"
            + "&ai=" + Application.identifier
            + "&ua={1}";
    }
}