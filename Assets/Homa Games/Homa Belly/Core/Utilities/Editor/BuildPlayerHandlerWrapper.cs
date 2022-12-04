using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

// ReSharper disable once CheckNamespace
namespace HomaGames.HomaBelly 
{
    public class BuildPlayerHandlerWrapper : IPreprocessBuildWithReport
    {
        public delegate bool FilterDelegate(BuildReport report);

        private static readonly List<FilterDelegate> m_filters = new List<FilterDelegate>();
        private static readonly List<FilterDelegate> m_interactiveFilters = new List<FilterDelegate>();
        
        #region Obsolete Code
        public delegate bool BuildPlayerHandlerFilter(BuildPlayerOptions options);
        private static readonly List<BuildPlayerHandlerFilter> m_legacy_filters = new List<BuildPlayerHandlerFilter>();
        
        [PublicAPI]
        [Obsolete("Use AddBuildFilter instead")]
        public static void AddBuildPlayerHandlerFilter(BuildPlayerHandlerFilter filter) {
            m_legacy_filters.Add(filter);
        }
        
        [PublicAPI]
        [Obsolete("Use RemoveBuildFilter instead")]
        public static void RemoveBuildPlayerHandlerFilter(BuildPlayerHandlerFilter filter) {
            m_legacy_filters.Remove(filter);
        }
        #endregion
        
        [PublicAPI]
        public static void AddBuildFilter(FilterDelegate filter) {
            m_filters.Add(filter);
        }

        [PublicAPI]
        public static void RemoveBuildFilter(FilterDelegate filter) {
            m_filters.Remove(filter);
        }
        
        [PublicAPI]
        public static void AddInteractiveFilter(FilterDelegate filter) {
            m_interactiveFilters.Add(filter);
        }

        [PublicAPI]
        public static void RemoveInteractiveFilter(FilterDelegate filter) {
            m_interactiveFilters.Remove(filter);
        }

        public int callbackOrder { get; } = 0;
        public void OnPreprocessBuild(BuildReport report)
        {
            var success = true;

            // Obsolete, I can't get build player options as we were doing before
            // so I will pass a default object. We can get rid off this after we change
            // the package that ios using the legacy method.
            var options = new BuildPlayerOptions();
            foreach (var filter in m_legacy_filters)
            {
                success &= filter.Invoke(options);
            }
            
            // Iterate over m_filters and return if some of them returns false
            foreach (var filter in m_filters)
            {
                success &= filter.Invoke(report);
            }

            if (!success)
            {
                throw new BuildFailedException("[ERROR] The build was cancelled because we detected some errors in HomaBelly. See the previous errors in the console to address the issue.");
            }
            
            for (var i = 0; i < m_interactiveFilters.Count; i++)
            {
                if(!m_interactiveFilters[i].Invoke(report))
                {
                    throw new BuildFailedException("[ERROR] Build cancelled by the user");
                }
            }
        }
    }
}
