using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class ThinkNodeWorkTest
{
    [BeforeTest]
    public void Setup()
    {
        ItemDefOf.Load();
        Game.Map = new GameMap(10, 10);
    }

    [TestCase]
    public void PrioritisesMineOverHaul()
    {
        Game.Map.Designations.Add(DesignationType.Mine, new Vector2I(5, 5));
        var sp = Game.Map.Stockpiles.Create();
        sp.Cells.Add(new Vector2I(0, 0));
        Game.Map.SpawnItem(ItemDefOf.Stone, new Vector2I(7, 7), 3);
        var guy = new Guy { Position = Vector2.Zero };

        var job = new ThinkNode_Work().TryGiveJob(guy);

        AssertBool(job.Type == JobType.Mine).IsTrue();
    }

    [TestCase]
    public void ReturnsHaulWhenNoMine()
    {
        var sp = Game.Map.Stockpiles.Create();
        sp.Cells.Add(new Vector2I(0, 0));
        Game.Map.SpawnItem(ItemDefOf.Stone, new Vector2I(7, 7), 3);
        var guy = new Guy { Position = Vector2.Zero };

        var job = new ThinkNode_Work().TryGiveJob(guy);

        AssertBool(job.Type == JobType.Haul).IsTrue();
    }

    [TestCase]
    public void ReturnsNullWhenNoWork()
    {
        var guy = new Guy { Position = Vector2.Zero };
        AssertObject(new ThinkNode_Work().TryGiveJob(guy)).IsNull();
    }
}