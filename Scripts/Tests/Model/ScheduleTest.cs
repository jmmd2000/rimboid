using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class ScheduleTest
{
    [TestCase]
    public void DefaultsToAnythingAllDay()
    {
        var s = new Schedule();
        for (int h = 0; h < 24; h++) AssertBool(s.BlockAt(h) == ScheduleBlock.Anything).IsTrue();
    }

    [TestCase]
    public void SetChangesJustThatHour()
    {
        var s = new Schedule();
        s.Set(9, ScheduleBlock.Work);
        AssertBool(s.BlockAt(9) == ScheduleBlock.Work).IsTrue();
        AssertBool(s.BlockAt(10) == ScheduleBlock.Anything).IsTrue();
    }

    [TestCase]
    public void DayNightSleepsThroughTheNight()
    {
        var s = Schedule.DayNight();
        AssertBool(s.BlockAt(2) == ScheduleBlock.Sleep).IsTrue();
        AssertBool(s.BlockAt(23) == ScheduleBlock.Sleep).IsTrue();
        AssertBool(s.BlockAt(12) == ScheduleBlock.Anything).IsTrue();
    }
}