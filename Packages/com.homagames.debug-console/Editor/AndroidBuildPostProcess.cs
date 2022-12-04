#if HOMA_DEVELOPMENT
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Callbacks;
using UnityEngine;

namespace HomaGames.HomaConsole
{
    public class AndroidBuildPostProcess : IPostGenerateGradleAndroidProject
    {
        private static XNamespace androidNamespace;

        public int callbackOrder { get; }

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            path = Path.Combine(path, "src/main/AndroidManifest.xml");
            AddAttributeToApplication(path, "debuggable", "true");
        }

        /// <summary>
        /// Adds the given attribute name/value to the `application` tag inside
        /// the specified manifest.
        /// </summary>
        /// <param name="manifestFilePath">The AndroidManifest file path</param>
        /// <param name="attributeName">The attribute local name. Do not specify namespace: `android` will be used</param>
        /// <param name="attributeValue">The attribute value</param>
        public static void AddAttributeToApplication(string manifestFilePath, string attributeName,
            string attributeValue)
        {
            XDocument manifest = LoadAndroidManifest(manifestFilePath);
            if (manifest != null)
            {
                // Sanity check: manifest
                var elementManifest = manifest.Element("manifest");
                if (elementManifest == null)
                {
                    return;
                }

                // Sanity check: application
                var elementApplication = elementManifest.Element("application");
                if (elementApplication == null)
                {
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
    }
}
#endif