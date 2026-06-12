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
        TerrainDefOf.Load();

        Game.Map = new GameMap(10, 10);
        for (int x = 0; x < Game.Map.Width; x++)
            for (int y = 0; y < Game.Map.Height; y++)
                Game.Map.Terrain[x, y] = TerrainDefOf.Dirt;

        Game.Pathing = new Pathing();
        Game.Pathing.Init(Game.Map);
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

    [TestCase]
    public void SkipsUnreachableDesignation()
    {
        // wall off (5,5) — Stone is unwalkable, so it has no reachable neighbour
        Game.Map.Terrain[5, 4] = TerrainDefOf.Stone;
        Game.Map.Terrain[5, 6] = TerrainDefOf.Stone;
        Game.Map.Terrain[4, 5] = TerrainDefOf.Stone;
        Game.Map.Terrain[6, 5] = TerrainDefOf.Stone;
        Game.Pathing.Init(Game.Map); // rebuild the grid with the walls

        Game.Map.Designations.Add(DesignationType.Mine, new Vector2I(5, 5));
        var guy = new Guy { Position = Vector2.Zero };

        AssertObject(new WorkGiver_Mine().TryGiveJob(guy)).IsNull();
    }
}