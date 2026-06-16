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
        // wall the 4 cardinal neighbours; diagonals stay open, mining is orthogonal, so still unminable
        foreach (var d in Grid.Cardinal4)
        {
            var n = new Vector2I(5, 5) + d;
            Game.Map.Terrain[n.X, n.Y] = TerrainDefOf.Stone;
        }
        Game.Pathing.Init(Game.Map);
        Game.Map.Designations.Add(DesignationType.Mine, new Vector2I(5, 5));
        AssertObject(new WorkGiver_Mine().TryGiveJob(new Guy { Position = Vector2.Zero })).IsNull();
    }

    [TestCase]
    public void SkipsDesignationInSealedRegion()
    {
        // a solid stone wall down column 5 splits the map, the guy starts on the left
        for (int y = 0; y < Game.Map.Height; y++)
            Game.Map.Terrain[5, y] = TerrainDefOf.Stone;
        Game.Pathing.Init(Game.Map);

        // designation sits in the sealed-off right region, it HAS open floor
        // neighbours, but none the guy can actually path to
        Game.Map.Designations.Add(DesignationType.Mine, new Vector2I(8, 8));
        var guy = new Guy { Position = Vector2.Zero };

        AssertObject(new WorkGiver_Mine().TryGiveJob(guy)).IsNull();
    }
}