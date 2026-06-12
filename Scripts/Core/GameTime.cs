/// <summary>Global game clock. Advanced one tick at a time by the TickManager.</summary>
public static class GameTime
{
    /// <summary>Sim ticks elapsed since game start.</summary>
    public static long Ticks;

    public const int TicksPerMinute = 42;
    public const int MinutesPerHour = 60;
    public const int HoursPerDay = 24;
    public const int TicksPerHour = TicksPerMinute * MinutesPerHour; // 2,520
    public const int TicksPerDay = TicksPerHour * HoursPerDay; // 60,480

    /// <summary>Whole days elapsed, the first day is Day 1.</summary>
    public static long Day => Ticks / TicksPerDay + 1;

    /// <summary>Hour of the current day, 0..23.</summary>
    public static int HourOfDay => (int)(Ticks / TicksPerHour % HoursPerDay);

    /// <summary>Minute of the current hour, 0..59.</summary>
    public static int MinuteOfHour => (int)(Ticks / TicksPerMinute % MinutesPerHour);

    public static void Advance() => Ticks++;

    public static void Reset() => Ticks = 0;
}