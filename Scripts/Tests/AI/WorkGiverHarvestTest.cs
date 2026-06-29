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
        DefLoader.LoadAll();

        Game.Map = new GameMap(10, 10);
        for (int x = 0; x < Game.Map.Width; x++)
            for (int y = 0; y < Game.Map.Height; y++)
                Game.Map.Terrain[x, y] = TerrainDefOf.Dirt;

        Game.Pathing = new Pathing();
        Game.Pathing.Init(Game.Map);
    }

    static void HarvestableAt(Vector2I cell)
    {
        Game.Map.SpawnPlant(PlantDefOf.BerryBush, cell);
        Game.Map.Designations.Add(DesignationType.Harvest, cell);
    }

    [TestCase]
    public void ReturnsHarvestJobForRipePlant()
    {
        HarvestableAt(new Vector2I(5, 5));
        var job = new WorkGiver_Harvest().TryGiveJob(new Guy { Position = Vector2.Zero });

        AssertObject(job).IsNotNull();
        AssertBool(job.Type == JobType.Harvest).IsTrue();
        AssertBool(job.TargetCell == new Vector2I(5, 5)).IsTrue();
    }

    [TestCase]
    public void ReturnsNullWhenNoDesignations()
    {
        AssertObject(new WorkGiver_Harvest().TryGiveJob(new Guy { Position = Vector2.Zero })).IsNull();
    }

    [TestCase]
    public void SkipsUnripePlant()
    {
        var plant = Game.Map.SpawnPlant(PlantDefOf.BerryBush, new Vector2I(5, 5));
        plant.MatureAtTick = GameTime.Ticks + GameTime.TicksPerDay;   // not mature yet
        Game.Map.Designations.Add(DesignationType.Harvest, new Vector2I(5, 5));

        AssertObject(new WorkGiver_Harvest().TryGiveJob(new Guy { Position = Vector2.Zero })).IsNull();
    }

    [TestCase]
    public void PicksNearestDesignation()
    {
        HarvestableAt(new Vector2I(8, 8));
        HarvestableAt(new Vector2I(2, 2));
        var job = new WorkGiver_Harvest().TryGiveJob(new Guy { Position = Vector2.Zero });

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
        HarvestableAt(new Vector2I(5, 5));

        AssertObject(new WorkGiver_Harvest().TryGiveJob(new Guy { Position = Vector2.Zero })).IsNotNull();
    }

    [TestCase]
    public void SkipsDesignationInSealedRegion()
    {
        for (int y = 0; y < Game.Map.Height; y++)
            Game.Map.Terrain[5, y] = TerrainDefOf.Stone;
        Game.Pathing.Init(Game.Map);
        HarvestableAt(new Vector2I(8, 8));

        AssertObject(new WorkGiver_Harvest().TryGiveJob(new Guy { Position = Vector2.Zero })).IsNull();
    }

    [TestCase]
    public void HarvestsMatureCropInGrowZoneWithoutDesignation()
    {
        var cell = new Vector2I(5, 5);
        Game.Map.SpawnPlant(PlantDefOf.BerryBush, cell);
        Game.Map.GrowZones.Create().Cells.Add(cell);

        var job = new WorkGiver_Harvest().TryGiveJob(new Guy { Position = Vector2.Zero });

        AssertObject(job).IsNotNull();
        AssertBool(job.TargetCell == cell).IsTrue();
    }

    [TestCase]
    public void SkipsMatureCropNeitherZonedNorDesignated()
    {
        Game.Map.SpawnPlant(PlantDefOf.BerryBush, new Vector2I(5, 5));
        AssertObject(new WorkGiver_Harvest().TryGiveJob(new Guy { Position = Vector2.Zero })).IsNull();
    }
}