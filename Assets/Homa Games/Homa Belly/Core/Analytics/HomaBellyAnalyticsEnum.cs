namespace HomaGames.HomaBelly
{
    public enum ErrorSeverity
    {
        Undefined = 0,
        Debug = 1,
        Info = 2,
        Warning = 3,
        Error = 4,
        Critical = 5
    }

    public enum ProgressionStatus
    {
        //Undefined progression
        Undefined = 0,
        // User started progression
        Start = 1,
        // User succesfully ended a progression
        Complete = 2,
        // User failed a progression
        Fail = 3
    }

    public enum ResourceFlowType
    {
        //Undefined progression
        Undefined = 0,
        // Source: Used when adding resource to a user
        Source = 1,
        // Sink: Used when removing a resource from a user
        Sink = 2
    }

    public enum AdAction
    {
        Undefined = 0,
        Clicked = 1,
        Show = 2,
        FailedShow = 3,
        RewardReceived = 4,
        Request = 5,
        Loaded = 6
    }

    public enum AdType
    {
        Undefined = 0,
        Video = 1,
        RewardedVideo = 2,
        Playable = 3,
        Interstitial = 4,
        OfferWall = 5,
        Banner = 6
    }

    public enum AdError
    {
        Undefined = 0,
        Unknown = 1,
        Offline = 2,
        NoFill = 3,
        InternalError = 4,
        InvalidRequest = 5,
        UnableToPrecache = 6
    }
}