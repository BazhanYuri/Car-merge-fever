using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace HomaGames.HomaBelly
{
    public static class UriHelper
    {
        [NotNull]
        public static string AddGetParameters([NotNull] string baseUri, [NotNull] Dictionary<string, string> parameters)
        {
            string firstDelimiter;
            
            if (baseUri.Contains("?"))
            {
                firstDelimiter = "&";
            }
            else
            {
                firstDelimiter = "?";
            }

            IEnumerable<string> parameterDefinitions = parameters.Select(pair => $"{pair.Key}={Uri.EscapeDataString(pair.Value ?? "null")}");

            return $"{baseUri}{firstDelimiter}{string.Join("&", parameterDefinitions)}";
        }
    }
}
