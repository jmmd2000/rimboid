using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class WorkGiverHaulTest
{
    [BeforeTest]
    public void Setup()
    {
        ItemDefOf.Load();
        Game.Map = new GameMap(10, 10);
    }

    [TestCase]
    public void ReturnsHaulJobForLooseItem()
    {
        var sp = Game.Map.Stockpiles.Create();
        sp.Cells.Add(new Vector2I(0, 0));
        Game.Map.SpawnItem(ItemDefOf.Stone, new Vector2I(5, 5), 3);
        var guy = new Guy { Position = Vector2.Zero };

        var job = new WorkGiver_Haul().TryGiveJob(guy);

        AssertObject(job).IsNotNull();
        AssertBool(job.Type == JobType.Haul).IsTrue();
        AssertBool(job.TargetCell == new Vector2I(5, 5)).IsTrue();
        AssertBool(job.DestinationCell == new Vector2I(0, 0)).IsTrue();
    }

    [TestCase]
    public void ReturnsNullWhenItemAlreadyInStockpile()
    {
        var sp = Game.Map.Stockpiles.Create();
        sp.Cells.Add(new Vector2I(0, 0));
        Game.Map.SpawnItem(ItemDefOf.Stone, new Vector2I(0, 0), 3);
        var guy = new Guy { Position = Vector2.Zero };

        AssertObject(new WorkGiver_Haul().TryGiveJob(guy)).IsNull();
    }

    [TestCase]
    public void ReturnsNullWhenNoStockpileSpace()
    {
        Game.Map.SpawnItem(ItemDefOf.Stone, new Vector2I(5, 5), 3);
        var guy = new Guy { Position = Vector2.Zero };

        AssertObject(new WorkGiver_Haul().TryGiveJob(guy)).IsNull();
    }
}