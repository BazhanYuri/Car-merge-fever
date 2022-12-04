using System;
using UnityEditor;

namespace HomaGames.HomaBelly
{
    public abstract class ReporterBase
    {
        private string LAST_PACKAGE_REPORT_KEY => $"homa_last_{GetType().Name}_date";
        
        protected long LastTimeReported
        {
            get =>
                long.Parse(EditorPrefs.GetString(LAST_PACKAGE_REPORT_KEY, "0"));
            set => EditorPrefs.SetString(LAST_PACKAGE_REPORT_KEY, value.ToString());
        }

        protected bool CanReport => DateTimeOffset.UtcNow.ToUnixTimeSeconds() - LastTimeReported >= MinTimeInSecondsBetweenReports;

        protected abstract long MinTimeInSecondsBetweenReports { get; }
    }
}