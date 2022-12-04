using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Utilities to manage AndroidManifest files
    /// </summary>
    public static class AndroidManifestUtils
    {
        private static XNamespace androidNamespace;

        /// <summary>
        /// Loads the given file path as an XDocument representing
        /// an AndroidManifest
        /// </summary>
        /// <param name="manifestFilePath">The path to the AndroidManifest file</param>
        /// <returns></returns>
        private static XDocument LoadAndroidManifest(string manifestFilePath)
        {
            XDocument manifest = null;
            if (File.Exists(manifestFilePath))
            {
                try
                {
                    manifest = XDocument.Load(manifestFilePath);
                    androidNamespace = manifest.Root.GetNamespaceOfPrefix("android");
                }
                catch (IOException exception)
                {
                    HomaBellyEditorLog.Error($"Could not load manifest file: {exception.Message}");
                }
            }

            return manifest;
        }

        /// <summary>
        /// Saves the given XDocument representing an AndroidManifest to the
        /// given file path
        /// </summary>
        /// <param name="manifest">The XDocument representing the AndroidManifest</param>
        /// <param name="manifestFilePath">The path to the AndroidManifest file</param>
        private static void SaveManifest(XDocument manifest, string manifestFilePath)
        {
            manifest.Save(manifestFilePath);
        }

        /// <summary>
        /// Adds the required AdMob meta-data element to the given AndroidManifest
        /// </summary>
        /// <param name="appId">The AdMob App ID</param>
        /// <param name="manifestFilePath">The AndroidManifest file path</param>
        public static void AddAdMobIdToManifest(string appId, string manifestFilePath)
        {
            AddMetaDataToManifest(manifestFilePath, "com.google.android.gms.ads.APPLICATION_ID", appId);
        }

        /// <summary>
        /// Adds a meta-data element into the given AndroidManifest
        /// </summary>
        /// <param name="manifestFilePath">The AndroidManifest file path</param>
        /// <param name="metadataName">The meta-data name. Do not specify namespace: `android` will be used</param>
        /// <param name="metadataValue">The meta-data value</param>
        public static void AddMetaDataToManifest(string manifestFilePath, string metadataName, string metadataValue)
        {
            XDocument manifest = LoadAndroidManifest(manifestFilePath);
            if (manifest != null)
            {
                // Sanity check: manifest
                var elementManifest = manifest.Element("manifest");
                if (elementManifest == null)
                {
                    HomaBellyEditorLog.Error($"Manifest does not have `manifest` element");
                    return;
                }

                // Sanity check: application
                var elementApplication = elementManifest.Element("application");
                if (elementApplication == null)
                {
                    HomaBellyEditorLog.Error($"Manifest does not have `application` element");
                    return;
                }

                // Get first meta-data element under application matching name attribute
                XElement adMobMetaData = elementApplication
                    .Descendants()
                    .FirstOrDefault(element => element.Name.LocalName.Equals("meta-data")
                                    && element.Attribute(androidNamespace + "name") != null
                                    && element.Attribute(androidNamespace + "name").Value.Equals(metadataName));

                if (adMobMetaData == null)
                {
                    // No metadata found, create it
                    elementApplication.Add(new XElement("meta-data",
                        new XAttribute(androidNamespace + "name", metadataName),
                        new XAttribute(androidNamespace + "value", metadataValue)));
                }
                else
                {
                    // Update value
                    adMobMetaData.Attribute(androidNamespace + "value").Value = metadataValue;
                }

                SaveManifest(manifest, manifestFilePath);
            }
        }

        /// <summary>
        /// Adds the given attribute name/value to the `application` tag inside
        /// the specified manifest.
        /// </summary>
        /// <param name="manifestFilePath">The AndroidManifest file path</param>
        /// <param name="attributeName">The attribute local name. Do not specify namespace: `android` will be used</param>
        /// <param name="attributeValue">The attribute value</param>
        public static void AddAttributeToApplication(string manifestFilePath, string attributeName, string attributeValue)
        {
            XDocument manifest = LoadAndroidManifest(manifestFilePath);
            if (manifest != null)
            {

                // Sanity check: manifest
                var elementManifest = manifest.Element("manifest");
                if (elementManifest == null)
                {
                    HomaBellyEditorLog.Error($"Manifest does not have `manifest` element");
                    return;
                }

                // Sanity check: application
                var elementApplication = elementManifest.Element("application");
                if (elementApplication == null)
                {
                    HomaBellyEditorLog.Error($"Manifest does not have `application` element");
                    return;
                }

                // Get `attributeName` attribute for `application` element
                XAttribute networkConfigAttribute = elementApplication.Attribute(androidNamespace + attributeName);
                if (networkConfigAttribute == null)
                {
                    // No attribute found, create it
                    elementApplication.SetAttributeValue(androidNamespace + attributeName, attributeValue);
                }

                SaveManifest(manifest, manifestFilePath);
            }
        }

        public static void UpdatePermissionInManifest(string manifestFilePath, string permission, bool add = true)
        {
            var manifest = LoadAndroidManifest(manifestFilePath);
            if (manifest == null)
            {
                Debug.LogWarning($"[WARNING] Can't modify AndroidManifest.xml. File not found in: {manifestFilePath}");
                return;
            }
            
            var manifestRoot = manifest.Element("manifest");
            if (manifestRoot == null)
            {
                Debug.LogWarning($"[WARNING] Manifest does not have `manifest` element");
                HomaBellyEditorLog.Error($"Manifest does not have `manifest` element");
                return;
            }

            bool manifestModified = false;
            bool permissionFound = false;
            // Check if permissions are already there.
            foreach (var node in manifestRoot.Descendants())
            {
                if (node.Name == "uses-permission")
                {
                    foreach (var attribute in node.Attributes())
                    {
                        if (attribute.ToString().Contains(permission))
                        {
                            permissionFound = true;
                            if (!add)
                            {
                                node.Remove();
                                manifestModified = true;
                                string msg = $"Manifest permission: {permission} removed from: {manifestFilePath}";
                                HomaBellyEditorLog.Debug(msg);
                                Debug.Log(msg);
                            }

                            break;
                        }
                    }
                    
                    if (permissionFound)
                    {
                        break;
                    }
                }
            }
            
            if (!permissionFound && add)
            {
                var newPermissionElement = new XElement("uses-permission",
                    new XAttribute(androidNamespace + "name", permission));
                
                // Try to add permission just before the `application` element
                var elementApplication = manifestRoot.Element("application");
                if (elementApplication != null)
                {
                    elementApplication.AddBeforeSelf(newPermissionElement);
                }
                else
                {
                    manifestRoot.Add(newPermissionElement);
                }

                manifestModified = true;
                string msg = $"Manifest permission: {permission} added to: {manifestFilePath}";
                HomaBellyEditorLog.Debug(msg);
                Debug.Log(msg);
            }

            if (manifestModified)
            {
                SaveManifest(manifest,manifestFilePath);
            }
        }
    }
}