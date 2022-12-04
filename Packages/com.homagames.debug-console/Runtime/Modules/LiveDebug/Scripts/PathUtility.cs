using System.Collections.Generic;

namespace HomaGames.HomaConsole
{
    public static class PathUtility
    {
        public static string GetParentPath(string groupPath)
        {
            if (groupPath.IndexOf('/') < 0)
                return "";
            return groupPath.Substring(0, groupPath.LastIndexOf('/'));
        }

        public static string GetGroupNameFromFullPath(string groupPath)
        {
            if (groupPath.IndexOf('/') < 0)
                return groupPath;
            return groupPath.Substring(groupPath.LastIndexOf('/') + 1);
        }

        public static IEnumerable<string> GetGroupPaths(string fullPath)
        {
            if (fullPath == "")
                yield return "";
            else
                for (int i = 0; i < fullPath.Length; i++)
                {
                    if (i == fullPath.Length - 1)
                        yield return fullPath;
                    else if (fullPath[i] == '/')
                    {
                        yield return fullPath.Substring(0, i);
                    }
                }
        }
    }
}