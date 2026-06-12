using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class TickManagerTest
{
    [TestCase]
    public void DefaultSpeedRunsOneTickPerFrame()
    {
        int ticks = 0;
        var tm = AutoFree(new TickManager());
        tm.Tick += () => ticks++;

        tm.RunFrame();

        AssertInt(ticks).IsEqual(1);
    }

    [TestCase]
    public void SpeedMultiplierRunsThatManyTicks()
    {
        int ticks = 0;
        var tm = AutoFree(new TickManager());
        tm.Tick += () => ticks++;
        tm.SetSpeed(3);

        tm.RunFrame();

        AssertInt(ticks).IsEqual(3);
    }

    [TestCase]
    public void PausedRunsNoTicks()
    {
        int ticks = 0;
        var tm = AutoFree(new TickManager());
        tm.Tick += () => ticks++;
        tm.TogglePause();

        tm.RunFrame();

        AssertInt(ticks).IsEqual(0);
    }

    [TestCase]
    public void TogglePauseRestoresLastSpeed()
    {
        var tm = AutoFree(new TickManager());
        tm.SetSpeed(3);

        tm.TogglePause();
        AssertInt(tm.SpeedMultiplier).IsEqual(0);

        tm.TogglePause();
        AssertInt(tm.SpeedMultiplier).IsEqual(3);
    }

    [TestCase]
    public void SetSpeedUnpauses()
    {
        var tm = AutoFree(new TickManager());
        tm.TogglePause();
        tm.SetSpeed(6);

        AssertInt(tm.SpeedMultiplier).IsEqual(6);
    }

    [TestCase]
    public void SingleTickRunsOnceWhilePaused()
    {
        int ticks = 0;
        var tm = AutoFree(new TickManager());
        tm.Tick += () => ticks++;
        tm.TogglePause();

        tm.DoSingleTick();

        AssertInt(ticks).IsEqual(1);
    }
}