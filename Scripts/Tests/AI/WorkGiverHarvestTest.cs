using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class WorkGiverHarvestTest
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
    public void ReturnsHarvestJobForDesignation()
    {
        Game.Map.Designations.Add(DesignationType.Harvest, new Vector2I(5, 5));
        var guy = new Guy { Position = Vector2.Zero };

        var job = new WorkGiver_Harvest().TryGiveJob(guy);

        AssertObject(job).IsNotNull();
        AssertBool(job.Type == JobType.Harvest).IsTrue();
        AssertBool(job.TargetCell == new Vector2I(5, 5)).IsTrue();
    }

    [TestCase]
    public void ReturnsNullWhenNoDesignations()
    {
        var guy = new Guy { Position = Vector2.Zero };
        AssertObject(new WorkGiver_Harvest().TryGiveJob(guy)).IsNull();
    }

    [TestCase]
    public void PicksNearestDesignation()
    {
        Game.Map.Designations.Add(DesignationType.Harvest, new Vector2I(8, 8));
        Game.Map.Designations.Add(DesignationType.Harvest, new Vector2I(2, 2));
        var guy = new Guy { Position = Vector2.Zero };

        var job = new WorkGiver_Harvest().TryGiveJob(guy);

        AssertBool(job.TargetCell == new Vector2I(2, 2)).IsTrue();
    }

    [TestCase]
    public void HarvestsViaDiagonalNeighbourOnly()
    {
        foreach (var d in Grid.Cardinal4)
        {
            var n = new Vector2I(5, 5) + d;
            Game.Map.Terrain[n.X, n.Y] = TerrainDefOf.Stone;
        }
        Game.Pathing.Init(Game.Map);
        Game.Map.Designations.Add(DesignationType.Harvest, new Vector2I(5, 5));

        AssertObject(new WorkGiver_Harvest().TryGiveJob(new Guy { Position = Vector2.Zero })).IsNotNull();
    }

    [TestCase]
    public void SkipsDesignationInSealedRegion()
    {
        for (int y = 0; y < Game.Map.Height; y++)
            Game.Map.Terrain[5, y] = TerrainDefOf.Stone;
        Game.Pathing.Init(Game.Map);

        Game.Map.Designations.Add(DesignationType.Harvest, new Vector2I(8, 8));
        var guy = new Guy { Position = Vector2.Zero };

        AssertObject(new WorkGiver_Harvest().TryGiveJob(guy)).IsNull();
    }
}