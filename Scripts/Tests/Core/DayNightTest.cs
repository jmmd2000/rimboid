using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class DayNightTest
{
    [TestCase]
    public void FullNightAtMidnight() => AssertFloat(DayNight.NightFactor(0f)).IsEqual(1f);

    [TestCase]
    public void NoNightAtNoon() => AssertFloat(DayNight.NightFactor(0.5f)).IsEqual(0f);

    [TestCase]
    public void FullNightLateEvening() => AssertFloat(DayNight.NightFactor(0.95f)).IsEqual(1f);

    [TestCase]
    public void PartialThroughDusk()
    {
        float f = DayNight.NightFactor(0.80f);
        AssertFloat(f).IsGreater(0f);
        AssertFloat(f).IsLess(1f);
    }
}