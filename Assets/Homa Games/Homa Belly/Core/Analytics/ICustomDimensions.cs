using System;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Interface to allow setting custom dimensions for analytic systems
    /// </summary>
    public interface ICustomDimensions
    {
        void SetCustomDimension01(string customDimension);
        void SetCustomDimension02(string customDimension);
        void SetCustomDimension03(string customDimension);
    }
}
