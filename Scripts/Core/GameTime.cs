/// <summary>Global game clock. Advanced one tick at a time by the TickManager.</summary>
public static class GameTime
{
    /// <summary>Sim ticks elapsed since game start.</summary>
    public static long Ticks;


    public const int TicksPerHour = 2500;
    public const int HoursPerDay = 24;
    public const int TicksPerDay = TicksPerHour * HoursPerDay; // 60,000

    /// <summary>Whole days elapsed, the first day is Day 1.</summary>
    public static long Day => Ticks / TicksPerDay + 1;

    /// <summary>Hour of the current day, 0..23.</summary>
    public static int HourOfDay => (int)(Ticks / TicksPerHour % HoursPerDay);

    public static void Advance() => Ticks++;

    public static void Reset() => Ticks = 0;
}