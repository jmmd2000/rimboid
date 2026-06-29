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
        DefLoader.LoadAll();

        Game.Map = new GameMap(10, 10);
        for (int x = 0; x < Game.Map.Width; x++)
            for (int y = 0; y < Game.Map.Height; y++)
                Game.Map.Terrain[x, y] = TerrainDefOf.Dirt;

        Game.Pathing = new Pathing();
        Game.Pathing.Init(Game.Map);
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
        AssertBool(job.Count == 3).IsTrue(); // empty stockpile has room for the whole pile
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

    [TestCase]
    public void CapsHaulAmountToStockpileRoom()
    {
        var sp = Game.Map.Stockpiles.Create();
        sp.Cells.Add(new Vector2I(0, 0));
        // only the one cell, filled to one below max -> room for exactly 1
        Game.Map.SpawnItem(ItemDefOf.Stone, new Vector2I(0, 0), ItemDefOf.Stone.MaxStackSize - 1);
        // a loose pile of 2 sitting outside
        Game.Map.SpawnItem(ItemDefOf.Stone, new Vector2I(5, 5), 2);
        var guy = new Guy { Position = Vector2.Zero };

        var job = new WorkGiver_Haul().TryGiveJob(guy);

        AssertObject(job).IsNotNull();
        AssertBool(job.TargetCell == new Vector2I(5, 5)).IsTrue();
        AssertInt(job.Count).IsEqual(1);// only 1 fits, so only 1 gets hauled
    }

    [TestCase]
    public void ReturnsNullWhenStockpileFull()
    {
        var sp = Game.Map.Stockpiles.Create();
        sp.Cells.Add(new Vector2I(0, 0));
        Game.Map.SpawnItem(ItemDefOf.Stone, new Vector2I(0, 0), ItemDefOf.Stone.MaxStackSize); // full
        Game.Map.SpawnItem(ItemDefOf.Stone, new Vector2I(5, 5), 2);// loose
        var guy = new Guy { Position = Vector2.Zero };

        // no room anywhere -> never pick it up
        AssertObject(new WorkGiver_Haul().TryGiveJob(guy)).IsNull();
    }

    [TestCase]
    public void SkipsUnreachableItem()
    {
        var sp = Game.Map.Stockpiles.Create();
        sp.Cells.Add(new Vector2I(0, 0));

        // wall off (5,5) so the guy can't path to it
        Game.Map.Terrain[5, 4] = TerrainDefOf.Stone;
        Game.Map.Terrain[5, 6] = TerrainDefOf.Stone;
        Game.Map.Terrain[4, 5] = TerrainDefOf.Stone;
        Game.Map.Terrain[6, 5] = TerrainDefOf.Stone;
        Game.Pathing.Init(Game.Map);

        Game.Map.SpawnItem(ItemDefOf.Stone, new Vector2I(5, 5), 3);
        var guy = new Guy { Position = Vector2.Zero };

        AssertObject(new WorkGiver_Haul().TryGiveJob(guy)).IsNull();
    }
}