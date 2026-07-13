/// <summary>One value per work-giver, the unit a colonist's WorkSettings toggles on or off.</summary>
public enum WorkType
{
    Mine,
    Chop,
    Harvest,
    Sow,
    Construct,
    Deconstruct,
    Bills,
    Haul,
    Consolidate,
}

/// <summary>Display helpers for WorkType (the labels the work panel shows).</summary>
public static class WorkTypes
{
    /// <summary>Every work type, in display order.</summary>
    public static readonly WorkType[] All = (WorkType[])System.Enum.GetValues(typeof(WorkType));

    /// <summary>The player-facing label for a work type.</summary>
    public static string Label(this WorkType type) => type switch
    {
        WorkType.Mine => "Mine",
        WorkType.Chop => "Chop",
        WorkType.Harvest => "Harvest",
        WorkType.Sow => "Sow",
        WorkType.Construct => "Build",
        WorkType.Deconstruct => "Deconstruct",
        WorkType.Bills => "Crafting",
        WorkType.Haul => "Haul",
        WorkType.Consolidate => "Tidy stockpiles",
        _ => type.ToString(),
    };
}