using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class WorkGiverMineTest
{
    [BeforeTest]
    public void Setup()
    {
        ItemDefOf.Load();
        Game.Map = new GameMap(10, 10);
    }

    [TestCase]
    public void ReturnsMineJobForDesignation()
    {
        Game.Map.Designations.Add(DesignationType.Mine, new Vector2I(5, 5));
        var guy = new Guy { Position = Vector2.Zero };

        var job = new WorkGiver_Mine().TryGiveJob(guy);

        AssertObject(job).IsNotNull();
        AssertBool(job.Type == JobType.Mine).IsTrue();
        AssertBool(job.TargetCell == new Vector2I(5, 5)).IsTrue();
    }

    [TestCase]
    public void ReturnsNullWhenNoDesignations()
    {
        var guy = new Guy { Position = Vector2.Zero };
        AssertObject(new WorkGiver_Mine().TryGiveJob(guy)).IsNull();
    }

    [TestCase]
    public void PicksNearestDesignation()
    {
        Game.Map.Designations.Add(DesignationType.Mine, new Vector2I(8, 8));
        Game.Map.Designations.Add(DesignationType.Mine, new Vector2I(2, 2));
        var guy = new Guy { Position = Vector2.Zero };

        var job = new WorkGiver_Mine().TryGiveJob(guy);

        AssertBool(job.TargetCell == new Vector2I(2, 2)).IsTrue();
    }
}