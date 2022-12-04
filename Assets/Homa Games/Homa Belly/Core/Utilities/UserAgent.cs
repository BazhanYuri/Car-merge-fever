using System;

namespace HomaGames.HomaBelly
{
    public static class UserAgent
    {
        /// <summary>
        /// Obtain the User Agent to be sent within the requests
        /// </summary>
        /// <returns></returns>
        [Obsolete("Use SystemConstants.UserAgent instead.")]
        public static string GetUserAgent()
        {
            return SystemConstants.UserAgent;
        }
    }
}