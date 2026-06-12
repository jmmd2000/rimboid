using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class WorkGiverConsolidateTest
{
    [BeforeTest]
    public void Setup()
    {
        ItemDefOf.Load();
        TerrainDefOf.Load();

        Game.Map = new GameMap(10, 10);
        for (int x = 0; x < Game.Map.Width; x++)
            for (int y = 0; y < Game.Map.Height; y++)
                Game.Map.Terrain[x, y] = TerrainDefOf.Dirt;

        Game.Pathing = new Pathing();
        Game.Pathing.Init(Game.Map);
    }

    static void StockpileAt(params Vector2I[] cells)
    {
        var sp = Game.Map.Stockpiles.Create();
        foreach (var c in cells) sp.Cells.Add(c);
    }

    [TestCase]
    public void MergesTwoPartialPiles()
    {
        StockpileAt(new Vector2I(0, 0), new Vector2I(1, 0));
        Game.Map.SpawnItem(ItemDefOf.Stone, new Vector2I(0, 0), 20);
        Game.Map.SpawnItem(ItemDefOf.Stone, new Vector2I(1, 0), 30);
        var guy = new Guy { Position = Vector2.Zero };

        var job = new WorkGiver_Consolidate().TryGiveJob(guy);

        AssertObject(job).IsNotNull();
        AssertBool(job.Type == JobType.Haul).IsTrue();
        AssertBool(job.TargetCell == new Vector2I(0, 0)).IsTrue(); // moves the smaller (20) pile
        AssertInt(job.Count).IsEqual(20);
    }

    [TestCase]
    public void NullForSinglePile()
    {
        StockpileAt(new Vector2I(0, 0), new Vector2I(1, 0));
        Game.Map.SpawnItem(ItemDefOf.Stone, new Vector2I(0, 0), 20);
        var guy = new Guy { Position = Vector2.Zero };

        AssertObject(new WorkGiver_Consolidate().TryGiveJob(guy)).IsNull();
    }

    [TestCase]
    public void NullWhenItCannotEmptyACell()
    {
        // 70 + 70: neither fits in the others 5 of room, so no cell can be freed
        StockpileAt(new Vector2I(0, 0), new Vector2I(1, 0));
        Game.Map.SpawnItem(ItemDefOf.Stone, new Vector2I(0, 0), 70);
        Game.Map.SpawnItem(ItemDefOf.Stone, new Vector2I(1, 0), 70);
        var guy = new Guy { Position = Vector2.Zero };

        AssertObject(new WorkGiver_Consolidate().TryGiveJob(guy)).IsNull();
    }

    [TestCase]
    public void IgnoresFullPiles()
    {
        // one full + one partial = only one partial pile, nothing to merge
        StockpileAt(new Vector2I(0, 0), new Vector2I(1, 0));
        Game.Map.SpawnItem(ItemDefOf.Stone, new Vector2I(0, 0), ItemDefOf.Stone.MaxStackSize);
        Game.Map.SpawnItem(ItemDefOf.Stone, new Vector2I(1, 0), 20);
        var guy = new Guy { Position = Vector2.Zero };

        AssertObject(new WorkGiver_Consolidate().TryGiveJob(guy)).IsNull();
    }
}