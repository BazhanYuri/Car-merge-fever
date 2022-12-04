using System.Collections.Generic;
using UnityEditor;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
using UnityEngine;
#endif

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Utils to manager iOS PBX Project file
    /// </summary>
    public static class iOSPbxProjectUtils
    {
#if UNITY_IOS
        /// <summary>
        /// Loads the PBXProject file from the given build path
        /// </summary>
        /// <param name="buildPath">The build path for the iOS project</param>
        /// <returns></returns>
        public static PBXProject LoadPbxProject(string buildPath)
        {
            PBXProject project = new PBXProject();
            string projectPath = PBXProject.GetPBXProjectPath(buildPath);
            project.ReadFromFile(projectPath);
            return project;
        }

        /// <summary>
        /// Save the PBXProject file into the corresponding path within build path
        /// </summary>
        /// <param name="buildPath">The build path for the iOS project</param>
        /// <param name="project">The PBXProject object</param>
        public static void SavePbxProjectFile(string buildPath, PBXProject project)
        {
            // Write to file
            project.WriteToFile(PBXProject.GetPBXProjectPath(buildPath));
        }

        /// <summary>
        /// Adds a framework to the PBX Project file to be included in the build
        /// </summary>
        /// <param name="buildTarget"></param>
        /// <param name="buildPath"></param>
        /// <param name="frameworks"></param>
        public static void AddFrameworks(BuildTarget buildTarget, string buildPath, string[] frameworks)
        {
            if (buildTarget == BuildTarget.iOS)
            {
                PBXProject project = LoadPbxProject(buildPath);
                AddFrameworks(project, frameworks);
                SavePbxProjectFile(buildPath, project);
            }
        }

        /// <summary>
        /// Adds a framework to the given PBX Project
        /// </summary>
        /// <param name="project">The PBXProject instance</param>
        /// <param name="frameworks"></param>
        public static void AddFrameworks(PBXProject project, string[] frameworks)
        {
            if (project == null)
            {
                return;
            }

            string targetId;
#if UNITY_2019_3_OR_NEWER
            targetId = project.GetUnityFrameworkTargetGuid();
#else
            targetId = project.TargetGuidByName("Unity-iPhone");
#endif

            // Add required frameworks
            if (frameworks != null)
            {
                foreach (string framework in frameworks)
                {
                    project.AddFrameworkToProject(targetId, framework, false);
                }
            }
        }

        /// <summary>
        /// Adds a list of build properties to the iOS build
        /// </summary>
        /// <param name="buildTarget"></param>
        /// <param name="buildPath"></param>
        /// <param name="buildProperties"></param>
        public static void AddBuildProperties(BuildTarget buildTarget, string buildPath, KeyValuePair<string, string>[] buildProperties)
        {
            if (buildTarget == BuildTarget.iOS)
            {
                PBXProject project = LoadPbxProject(buildPath);
                AddBuildProperties(project, buildProperties);
                SavePbxProjectFile(buildPath, project);
            }
        }

        /// <summary>
        /// Adds a list of build properties to the given PBXProject instance
        /// </summary>
        /// <param name="project">The PBXProject instance</param>
        /// <param name="buildProperties"></param>
        public static void AddBuildProperties(PBXProject project, KeyValuePair<string, string>[] buildProperties)
        {
            if (project == null)
            {
                return;
            }

            string targetId;
#if UNITY_2019_3_OR_NEWER
            targetId = project.GetUnityFrameworkTargetGuid();
#else
            targetId = project.TargetGuidByName("Unity-iPhone");
#endif

            // Add required build properties
            if (buildProperties != null)
            {
                foreach (KeyValuePair<string, string> buildProperty in buildProperties)
                {
                    project.AddBuildProperty(targetId, buildProperty.Key, buildProperty.Value);
                }
            }
        }
#endif
    }
}
