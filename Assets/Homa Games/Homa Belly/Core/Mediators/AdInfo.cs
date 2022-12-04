namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Holds infos about an ad placement id
    /// </summary>
    public struct AdInfo
    {
        public AdInfo(string placementId,AdType adType,AdPlacementType adPlacementType)
        {
            PlacementId = placementId;
            AdPlacementType = adPlacementType;
            AdType = adType;
        }
        /// <summary>
        /// Placement ID, or Ad Unit, as a string, used by the mediator.
        /// </summary>
        public string PlacementId { get; }
        public AdType AdType { get; }
        public AdPlacementType AdPlacementType { get; }
        public static implicit operator string(AdInfo adInfo) => adInfo.PlacementId;
        public override string ToString()
        {
            return $"[{AdType} - {PlacementId} - {AdPlacementType}]";
        }
    }
    
    public enum AdPlacementType
    {
        /// <summary>
        /// A type of ad placement that has a higher value than the default ads.
        /// Use those where you want to reward the user more than usual with Rewarded Videos for instance.
        /// </summary>
        HighValue,
        /// <summary>
        /// Default Ad Placement Id loaded by Homa Belly for you.
        /// </summary>
        Default,
        /// <summary>
        /// User defined placement Ids are populated when you are using custom placement ids.
        /// </summary>
        User
    }
}