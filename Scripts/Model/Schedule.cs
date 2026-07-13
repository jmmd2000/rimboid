/// <summary>What a colonist is allowed to do during an hour: work, sleep, or the free catch-all</summary>
public enum ScheduleBlock { Anything, Work, Sleep }

/// <summary>A colonist's daily schedule, one block per hour. Defaults to Anything all day. Schedule.DayNight() gives a fresh colonist a night-sleep rhythm.</summary>
public class Schedule
{
    readonly ScheduleBlock[] _blocks = new ScheduleBlock[GameTime.HoursPerDay]; // Anything (enum 0) by default

    /// <summary>The block set for a given hour (0-23).</summary>
    public ScheduleBlock BlockAt(int hour) => _blocks[hour];

    /// <summary>The block for the current in-game hour.</summary>
    public ScheduleBlock BlockNow() => _blocks[GameTime.HourOfDay];

    /// <summary>Sets the block for one hour.</summary>
    public void Set(int hour, ScheduleBlock block) => _blocks[hour] = block;

    /// <summary>A sensible starting schedule: asleep 22:00-06:00, free the rest of the day.</summary>
    public static Schedule DayNight()
    {
        var s = new Schedule();
        for (int h = 0; h < GameTime.HoursPerDay; h++)
            if (h < 6 || h >= 22) s._blocks[h] = ScheduleBlock.Sleep;
        return s;
    }
}