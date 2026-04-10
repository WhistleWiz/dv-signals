namespace Signals.Game
{
    public enum TrackDirection
    {
        Out,
        In
    }

    public enum SignalType
    {
        NotSet,
        Mainline,
        IntoYard,
        Shunting,
        Distant,
        OutPax,
        Other
    }

    public enum SignalOperationMode
    {
        /// <summary>
        /// Fully automatic.
        /// </summary>
        Automatic,
        /// <summary>
        /// Retains the override until normal operation matches the aspect.
        /// </summary>
        TempOverride,
        /// <summary>
        /// Manual override unless the automatic aspect is more restrictive.
        /// </summary>
        SemiManual,
        /// <summary>
        /// Fully manual.
        /// </summary>
        FullManual
    }

    public enum PrefabType
    {
        NotSet,
        Mainline,
        MainlineJunction,
        IntoYard,
        Shunting,
        OutPax,
    }
}
