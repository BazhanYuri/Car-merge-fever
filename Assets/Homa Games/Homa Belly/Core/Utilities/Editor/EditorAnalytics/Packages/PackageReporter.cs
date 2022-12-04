#if HOMA_BELLY_EDITOR_ANALYTICS_ENABLED
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor.PackageManager;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Analyses both UPM packages and Homa Belly packages actually installed in project
    /// </summary>
    public class PackageReporter : ReporterBase, IReportingService
    {
        protected override long MinTimeInSecondsBetweenReports => 24 * 60 * 60;

        public PackageReporter()
        {
            SendData();
        }

        public event Action<EventApiQueryModel> OnDataReported;

        private void SendData()
        {
            if (CanReport)
            {
                GetData((data) =>
                {
                    OnDataReported?.Invoke(data);
                    LastTimeReported = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                });
            }
        }

        private void GetData(Action<PackageReportQueryModel> onDataFetched)
        {
            Dictionary<string, string> packageData = new Dictionary<string, string>();

            var bellyManifest = PluginManifest.LoadFromLocalFile();
            if (bellyManifest != null)
                foreach (var bellyPackage in bellyManifest.Packages.GetAllPackages())
                {
                    packageData.Add(bellyPackage.Id, bellyPackage.Version);
                }

            var installedPackages = Client.List(true);
            Task.Run(() =>
            {
                while (!installedPackages.IsCompleted)
                    Thread.Sleep(25);
            }).ContinueWith((r) =>
            {
                foreach (var packageInfo in installedPackages.Result)
                {
                    if (packageInfo.author.name == "Homa Games")
                        packageData.Add(packageInfo.name, packageInfo.version);
                }
                onDataFetched?.Invoke(new PackageReportQueryModel("packages_updated", packageData));
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}
#endif