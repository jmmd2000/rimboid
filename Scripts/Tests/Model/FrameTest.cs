using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class FrameTest
{
    static Frame NewFrame() => new()
    {
        Def = new BuildingDef { MaterialCost = 5, WorkToBuild = 120f },
        Cell = new Vector2I(1, 1),
    };

    [TestCase]
    public void MaterialsIncompleteWhenEmpty() =>
        AssertBool(NewFrame().MaterialsComplete).IsFalse();

    [TestCase]
    public void MaterialsCompleteAtCost()
    {
        var f = NewFrame();
        f.MaterialsDelivered = 5;
        AssertBool(f.MaterialsComplete).IsTrue();
    }

    [TestCase]
    public void WorkCompleteAtThreshold()
    {
        var f = NewFrame();
        f.WorkDone = 120f;
        AssertBool(f.WorkComplete).IsTrue();
    }
}