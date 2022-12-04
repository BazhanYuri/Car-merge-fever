using System;
using System.Collections.Generic;
using HomaGames.HomaBelly.Installer;

namespace HomaGames.HomaBelly
{
    [System.Serializable]
    public class AvailablePackages
    {
        //[JsonProperty("i_manifest_id")]
        public int Id;
        //[JsonProperty("s_manifest_name")]
        public string Name;
        //[JsonProperty("s_manifest_version_id")]
        public string ManifestVersionId;
        //[JsonProperty("ao_core_packages")]
        public List<PackageComponent> CorePackages;
        //[JsonProperty("ao_mediation_layers")]
        public List<PackageComponent> MediationLayers;
        //[JsonProperty("ao_attribution_platforms")]
        public List<PackageComponent> AttributionPlatforms;
        //[JsonProperty("ao_ad_networks")]
        public List<PackageComponent> AdNetworks;
        //[JsonProperty("ao_analytics_systems")]
        public List<PackageComponent> AnalyticsSystems;
        //[JsonProperty("ao_others")]
        public List<PackageComponent> Others;

        public PackageComponent GetPackageComponent(string id, string version)
        {
            PackageComponent component = null;

            if (component == null)
            {
                component = GetPackageComponent(id, version, CorePackages);
            }

            if (component == null)
            {
                component = GetPackageComponent(id, version, AnalyticsSystems);
            }

            if (component == null)
            {
                component = GetPackageComponent(id, version, AttributionPlatforms);
            }

            if (component == null)
            {
                component = GetPackageComponent(id, version, MediationLayers);
            }

            if (component == null)
            {
                component = GetPackageComponent(id, version, AdNetworks);
            }

            if (component == null)
            {
                component = GetPackageComponent(id, version, Others);
            }

            return component;
        }

        [Obsolete("Please use GetPackageComponent(string, string, EditorPackageType) instead")]
        public PackageComponent GetPackageComponent(string id, string version, PackageType type)
        {
            switch (type)
            {
                case PackageType.CORE_PACKAGE:
                    return GetPackageComponent(id, version, CorePackages);
                case PackageType.ANALYTICS_SYSTEM:
                    return GetPackageComponent(id, version, AnalyticsSystems);
                case PackageType.ATTRIBUTION_PLATFORM:
                    return GetPackageComponent(id, version, AttributionPlatforms);
                case PackageType.MEDIATION_LAYER:
                    return GetPackageComponent(id, version, MediationLayers);
                case PackageType.AD_NETWORK:
                    return GetPackageComponent(id, version, AdNetworks);
                case PackageType.OTHER:
                    return GetPackageComponent(id, version, Others);
            }

            return null;
        }
        
        public PackageComponent GetPackageComponent(string id, string version, EditorPackageType type)
        {
            switch (type)
            {
                case EditorPackageType.CORE_PACKAGE:
                    return GetPackageComponent(id, version, CorePackages);
                case EditorPackageType.ANALYTICS_SYSTEM:
                    return GetPackageComponent(id, version, AnalyticsSystems);
                case EditorPackageType.ATTRIBUTION_PLATFORM:
                    return GetPackageComponent(id, version, AttributionPlatforms);
                case EditorPackageType.MEDIATION_LAYER:
                    return GetPackageComponent(id, version, MediationLayers);
                case EditorPackageType.AD_NETWORK:
                    return GetPackageComponent(id, version, AdNetworks);
                case EditorPackageType.OTHER:
                    return GetPackageComponent(id, version, Others);
            }

            return null;
        }

        [Obsolete("Please use GetPackageComponent(string, EditorPackageType) instead")]
        public PackageComponent GetPackageComponent(string id, PackageType type)
        {
            switch (type)
            {
                case PackageType.CORE_PACKAGE:
                    return GetPackageComponent(id, CorePackages);
                case PackageType.ANALYTICS_SYSTEM:
                    return GetPackageComponent(id, AnalyticsSystems);
                case PackageType.ATTRIBUTION_PLATFORM:
                    return GetPackageComponent(id, AttributionPlatforms);
                case PackageType.MEDIATION_LAYER:
                    return GetPackageComponent(id, MediationLayers);
                case PackageType.AD_NETWORK:
                    return GetPackageComponent(id, AdNetworks);
                case PackageType.OTHER:
                    return GetPackageComponent(id, Others);
            }

            return null;
        }
        
        public PackageComponent GetPackageComponent(string id, EditorPackageType type)
        {
            switch (type)
            {
                case EditorPackageType.CORE_PACKAGE:
                    return GetPackageComponent(id, CorePackages);
                case EditorPackageType.ANALYTICS_SYSTEM:
                    return GetPackageComponent(id, AnalyticsSystems);
                case EditorPackageType.ATTRIBUTION_PLATFORM:
                    return GetPackageComponent(id, AttributionPlatforms);
                case EditorPackageType.MEDIATION_LAYER:
                    return GetPackageComponent(id, MediationLayers);
                case EditorPackageType.AD_NETWORK:
                    return GetPackageComponent(id, AdNetworks);
                case EditorPackageType.OTHER:
                    return GetPackageComponent(id, Others);
            }

            return null;
        }

        private PackageComponent GetPackageComponent(string id, List<PackageComponent> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Id == id)
                {
                    return list[i];
                }
            }

            return null;
        }

        private PackageComponent GetPackageComponent(string id, string version, List<PackageComponent> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Id == id && list[i].Version == version)
                {
                    return list[i];
                }
            }

            return null;
        }

        public string GetDependenciesAsTrackingString()
        {
            List<PackageComponent> allPackages = new List<PackageComponent>();
            allPackages.AddRange(CorePackages);
            allPackages.AddRange(MediationLayers);
            allPackages.AddRange(AttributionPlatforms);
            allPackages.AddRange(AdNetworks);
            allPackages.AddRange(AnalyticsSystems);
            allPackages.AddRange(Others);

            List<string> dependencies = new List<string>();
            for (int i = 0; allPackages != null && i < allPackages.Count; i++)
            {
                dependencies.Add($"{allPackages[i].Id}:{allPackages[i].Version}");
            }

            return string.Join(",", dependencies);
        }

        public List<PackageComponent> GetAllPackages()
        {
            List<PackageComponent> packages = new List<PackageComponent>();
            if (CorePackages != null) packages.AddRange(CorePackages);
            if (MediationLayers != null) packages.AddRange(MediationLayers);
            if (AttributionPlatforms != null) packages.AddRange(AttributionPlatforms);
            if (AdNetworks != null) packages.AddRange(AdNetworks);
            if (AnalyticsSystems != null) packages.AddRange(AnalyticsSystems);
            if (Others != null) packages.AddRange(Others);

            return packages;
        }
        
        public override string ToString()
        {
            string result = "\nAvailable Packages";

            result += "\n\nCore Packages\n----------";
            foreach (PackageComponent component in CorePackages)
            {
                result += ("\n" + component.ToString());
            }

            result += "\n\nMediation Layers\n----------";
            foreach (PackageComponent component in MediationLayers)
            {
                result += ("\n" + component.ToString());
            }

            result += "\n\nAttribution Systems\n----------";
            foreach (PackageComponent component in AttributionPlatforms)
            {
                result += ("\n" + component.ToString());
            }

            result += "\n\nAd Networks\n----------";
            foreach (PackageComponent component in AdNetworks)
            {
                result += ("\n" + component.ToString());
            }

            result += "\n\nAnalytics Systems\n----------";
            foreach (PackageComponent component in AnalyticsSystems)
            {
                result += ("\n" + component.ToString());
            }

            result += "\n\nOthers\n----------";
            foreach (PackageComponent component in Others)
            {
                result += ("\n" + component.ToString());
            }

            return result;
        }
    }
}
