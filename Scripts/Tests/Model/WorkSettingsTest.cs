using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class WorkSettingsTest
{
    [TestCase]
    public void AllowsEverythingByDefault()
    {
        var ws = new WorkSettings();
        AssertBool(ws.Allows(WorkType.Mine)).IsTrue();
        AssertBool(ws.Allows(WorkType.Consolidate)).IsTrue();
    }

    [TestCase]
    public void SetTogglesOneTypeOnly()
    {
        var ws = new WorkSettings();
        ws.Set(WorkType.Haul, false);
        AssertBool(ws.Allows(WorkType.Haul)).IsFalse();
        AssertBool(ws.Allows(WorkType.Consolidate)).IsTrue();
        ws.Set(WorkType.Haul, true);
        AssertBool(ws.Allows(WorkType.Haul)).IsTrue();
    }
}