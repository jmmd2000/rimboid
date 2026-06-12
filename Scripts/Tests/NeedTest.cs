using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class NeedTest
{
    // concrete need with an exact fall rate so assertions need no float tolerance
    class TestNeed : Need
    {
        public override float FallPerTick => 0.25f;
    }

    [TestCase]
    public void StartsFull()
    {
        AssertFloat(new TestNeed().Level).IsEqual(1f);
    }

    [TestCase]
    public void TickDecaysByFallPerTick()
    {
        var need = new TestNeed();
        need.Tick();
        AssertFloat(need.Level).IsEqual(0.75f);
    }

    [TestCase]
    public void TickClampsAtZero()
    {
        var need = new TestNeed { Level = 0.1f };
        need.Tick();
        AssertFloat(need.Level).IsEqual(0f);
    }

    [TestCase]
    public void AddRaisesLevel()
    {
        var need = new TestNeed { Level = 0.5f };
        need.Add(0.25f);
        AssertFloat(need.Level).IsEqual(0.75f);
    }

    [TestCase]
    public void AddClampsAtOne()
    {
        var need = new TestNeed { Level = 0.9f };
        need.Add(0.25f);
        AssertFloat(need.Level).IsEqual(1f);
    }
}