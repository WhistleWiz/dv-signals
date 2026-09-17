namespace Signals.Common
{
    public enum CrossingCheckMode
    {
        Ignore,
        WholeTrack,
        IntersectionOnly
    }

    public enum ControllerMode
    {
        Active,
        MostRestrictive
    }

    public enum OperationMode
    {
        EqualTo,
        DifferentFrom,
        LessThan,
        LessThanOrEqualTo,
        GreaterThan,
        GreaterThanOrEqualTo,
    }
}
