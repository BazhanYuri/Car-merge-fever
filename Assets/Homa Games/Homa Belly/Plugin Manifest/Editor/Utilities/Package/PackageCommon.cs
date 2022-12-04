using System.IO;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    public static class PackageCommon
    {
        #region Private constants
        public const string FILE_PREFIX = "f/";
        public const string DIRECTORY_PREFIX = "d/";
        public const int PREFIX_LENGTH = 2;
        // Path to project root folder
        public static string BASE_PATH = Application.dataPath + "/../";
        #endregion

        /// <summary>
        /// Determines if the package component is installed by asserting
        /// all required files and directories are present within the project
        /// structure
        /// </summary>
        /// <param name="packageComponent"></param>
        /// <returns></returns>
        public static bool IsPackageInstalled(PackageComponent packageComponent)
        {
            if (packageComponent != null && packageComponent.Files != null)
            {
                bool installed = true;
                for (int i = 0; i < packageComponent.Files.Count && installed; i++)
                {
                    string assetPath = packageComponent.Files[i];
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        string assetWithoutPrefix = GetAssetWithoutPrefix(assetPath);
                        if (assetPath.StartsWith(FILE_PREFIX))
                        {
                            installed = installed && File.Exists(BASE_PATH + assetWithoutPrefix);
                        }
                        else if (assetPath.StartsWith(DIRECTORY_PREFIX))
                        {
                            installed = installed && Directory.Exists(BASE_PATH + assetWithoutPrefix);
                        }
                    }
                }

                return installed;
            }

            return false;
        }

        /// <summary>
        /// Determines if there is ANY file or folder from the given package
        /// component available within the project structure
        /// </summary>
        /// <param name="packageComponent"></param>
        /// <returns></returns>
        public static bool ShouldUninstallPackage(PackageComponent packageComponent)
        {
            if (packageComponent != null && packageComponent.Files != null)
            {
                bool shouldUninstall = false;
                for (int i = 0; i < packageComponent.Files.Count && !shouldUninstall; i++)
                {
                    string assetPath = packageComponent.Files[i];
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        string assetWithoutPrefix = GetAssetWithoutPrefix(assetPath);
                        if (assetPath.StartsWith(FILE_PREFIX))
                        {
                            shouldUninstall = File.Exists(BASE_PATH + assetWithoutPrefix);
                        }
                        else if (assetPath.StartsWith(DIRECTORY_PREFIX))
                        {
                            shouldUninstall = Directory.Exists(BASE_PATH + assetWithoutPrefix);
                        }
                    }
                }

                return shouldUninstall;
            }

            return false;
        }

        public static string GetAssetWithoutPrefix(string assetPath)
        {
            return assetPath.Substring(PREFIX_LENGTH);
        }
    }
}
