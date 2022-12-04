using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Utils to manage iOS Plist file
    /// </summary>
    public static class iOSPlistUtils
    {
#if UNITY_IOS
        /// <summary>
        /// Loads the Plist file from the given build path
        /// </summary>
        /// <param name="buildPath">The build path for the iOS project</param>
        /// <returns></returns>
        private static PlistDocument LoadPlistDocument(string buildPath)
        {
            PlistDocument plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(buildPath + "/Info.plist"));
            return plist;
        }

        /// <summary>
        /// Saves the given PlistDocument into the corresponding file within build path
        /// </summary>
        /// <param name="buildPath">The build path for the iOS project</param>
        /// <param name="plist"></param>
        private static void SavePlist(string buildPath, PlistDocument plist)
        {
            // Write to file
            File.WriteAllText(buildPath + "/Info.plist", plist.WriteToString());
        }

        /// <summary>
        /// Set given collection of strings to the Root dict of the Plist file
        /// </summary>
        /// <param name="buildTarget">The project build target to avoid non-iOS builds</param>
        /// <param name="buildPath">The build path for the iOS project</param>
        /// <param name="pairs">Collection of key/value pairs to be added to the plist</param>
        public static void SetRootStrings(BuildTarget buildTarget, string buildPath, KeyValuePair<string, string>[] pairs)
        {
            if (buildTarget == BuildTarget.iOS)
            {
                PlistDocument plist = LoadPlistDocument(buildPath);
                if (plist != null)
                {
                    // Get root
                    PlistElementDict rootDict = plist.root;
                    if (pairs != null)
                    {
                        // Add strings
                        foreach (KeyValuePair<string, string> pair in pairs)
                        {
                            rootDict.SetString(pair.Key, pair.Value);
                        }
                    }

                    SavePlist(buildPath, plist);
                }
            }
        }

        /// <summary>
        /// Sets the NSAppTransportSecurity setting to the Plist file
        /// </summary>
        /// <param name="buildTarget">The project build target to avoid non-iOS builds</param>
        /// <param name="buildPath">The build path for the iOS project</param>
        public static void SetAppTransportSecurity(BuildTarget buildTarget, string buildPath)
        {
            if (buildTarget == BuildTarget.iOS)
            {
                PlistDocument plist = LoadPlistDocument(buildPath);
                if (plist != null)
                {
                    // Get root
                    PlistElementDict rootDict = plist.root;

                    // Add NSAppTransportSecurity dict with NSAllowsArbitraryLoads
                    PlistElementDict transportSecurityDict = rootDict.CreateDict("NSAppTransportSecurity");
                    transportSecurityDict.SetBoolean("NSAllowsArbitraryLoads", true);
                    transportSecurityDict.SetBoolean("NSAllowsLocalNetworking", true);
                    transportSecurityDict.SetBoolean("NSAllowsArbitraryLoadsInWebContent", true);

                    SavePlist(buildPath, plist);
                }
            }
        }

        /// <summary>
        /// Sets the LSApplicationQueriesSchemes array with the given URLs
        /// </summary>
        /// <param name="buildTarget">The project build target to avoid non-iOS builds</param>
        /// <param name="buildPath">The build path for the iOS project</param>
        /// <param name="urls">The list of URLs to support</param>
        public static void SetApplicationQueriesSchemes(BuildTarget buildTarget, string buildPath, string[] urls)
        {
            if (buildTarget == BuildTarget.iOS)
            {
                PlistDocument plist = LoadPlistDocument(buildPath);
                if (plist != null)
                {
                    // Get root
                    PlistElementDict rootDict = plist.root;

                    // Add LSApplicationQueriesSchemes dict with URL schemes
                    PlistElementArray applicationQueriesSchemes = rootDict.CreateArray("LSApplicationQueriesSchemes");
                    if (urls != null)
                    {
                        for (int i = 0; i < urls.Length; i++)
                        {
                            applicationQueriesSchemes.AddString(urls[i]);
                        }
                    }

                    SavePlist(buildPath, plist);
                }
            }
        }

        /// <summary>
        /// Appends the given AdNetworkIds to the Info.plist to be used within SkAdNetworl (iOS 14)
        /// </summary>
        /// <param name="buildTarget">The project build target to avoid non-iOS builds</param>
        /// <param name="buildPath">The build path for the iOS project</param>
        /// <param name="urls">The list of AdNetworkIds to be added for SkAdNetwork</param>
        public static void AppendAdNetworkIds(BuildTarget buildTarget, string buildPath, string[] adNetworkIds)
        {
            if (buildTarget == BuildTarget.iOS)
            {
                PlistDocument plist = LoadPlistDocument(buildPath);
                if (plist != null)
                {
                    // Get root
                    PlistElementDict rootDict = plist.root;

                    // Check if SKAdNetworkItems already exists
                    PlistElementArray SKAdNetworkItems = null;
                    if (rootDict.values.ContainsKey("SKAdNetworkItems"))
                    {
                        try
                        {
                            SKAdNetworkItems = rootDict.values["SKAdNetworkItems"] as PlistElementArray;
                        }
                        catch (Exception e)
                        {
                            HomaBellyEditorLog.Warning($"Could not obtain SKAdNetworkItems PlistElementArray: {e.Message}");
                        }
                    }

                    // If not exists, create it
                    if (SKAdNetworkItems == null)
                    {
                        SKAdNetworkItems = rootDict.CreateArray("SKAdNetworkItems");
                    }

                    if (adNetworkIds != null)
                    {
                        string plistContent = File.ReadAllText(buildPath + "/Info.plist");
                        for (int i = 0; i < adNetworkIds.Length; i++)
                        {
                            // If the SkAdNetworkId cannot be found within Info.plist, add it.
                            // Otherwise, we assume is already added, so do nothing
                            if (!plistContent.Contains(adNetworkIds[i]))
                            {
                                PlistElementDict SKAdNetworkIdentifierDict = SKAdNetworkItems.AddDict();
                                SKAdNetworkIdentifierDict.SetString("SKAdNetworkIdentifier", adNetworkIds[i]);
                            }
                        }
                    }

                    SavePlist(buildPath, plist);
                }
            }
        }
#endif
    }
}
