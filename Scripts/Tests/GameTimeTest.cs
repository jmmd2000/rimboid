using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class GameTimeTest
{
    [BeforeTest]
    public void Setup() => GameTime.Reset();

    [TestCase]
    public void StartsAtDayOneMidnight()
    {
        AssertInt((int)GameTime.Day).IsEqual(1);
        AssertInt(GameTime.HourOfDay).IsEqual(0);
        AssertInt(GameTime.MinuteOfHour).IsEqual(0);
    }

    [TestCase]
    public void AdvanceIncrementsTicks()
    {
        GameTime.Advance();
        AssertInt((int)GameTime.Ticks).IsEqual(1);
    }

    [TestCase]
    public void OneMinuteElapses()
    {
        GameTime.Ticks = GameTime.TicksPerMinute; // 42
        AssertInt(GameTime.MinuteOfHour).IsEqual(1);
        AssertInt(GameTime.HourOfDay).IsEqual(0);
    }

    [TestCase]
    public void LastMinuteOfHour()
    {
        GameTime.Ticks = GameTime.TicksPerHour - GameTime.TicksPerMinute; // 59 minutes in
        AssertInt(GameTime.MinuteOfHour).IsEqual(59);
        AssertInt(GameTime.HourOfDay).IsEqual(0);
    }

    [TestCase]
    public void MinuteWrapsIntoHour()
    {
        GameTime.Ticks = GameTime.TicksPerHour; // exactly 1 hour
        AssertInt(GameTime.MinuteOfHour).IsEqual(0);
        AssertInt(GameTime.HourOfDay).IsEqual(1);
    }

    [TestCase]
    public void HourWrapsIntoDay()
    {
        GameTime.Ticks = GameTime.TicksPerDay; // exactly 1 day
        AssertInt((int)GameTime.Day).IsEqual(2);
        AssertInt(GameTime.HourOfDay).IsEqual(0);
        AssertInt(GameTime.MinuteOfHour).IsEqual(0);
    }
}