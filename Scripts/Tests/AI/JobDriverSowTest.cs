using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class JobDriverSowTest
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

    static JobStatus Run(JobDriver driver, Guy guy, Job job, int maxTicks = 20000)
    {
        driver.Init(guy, job);
        for (int i = 0; i < maxTicks; i++)
        {
            var status = driver.Tick();
            if (status != JobStatus.Ongoing) return status;
        }
        return JobStatus.Ongoing;
    }

    static GrowZone WheatZone(Vector2I cell)
    {
        var zone = Game.Map.GrowZones.Create();
        zone.Cells.Add(cell);
        zone.Crop = PlantDefOf.Wheat;
        return zone;
    }

    [TestCase]
    public void SowsTheZoneCropOnTheCell()
    {
        var cell = new Vector2I(5, 5);
        WheatZone(cell);
        var guy = new Guy { Position = Vector2.Zero };
        var job = new Job { Type = JobType.Sow, TargetCell = cell, ClaimsCell = true };

        var status = Run(new JobDriver_Sow(), guy, job);

        AssertBool(status == JobStatus.Completed).IsTrue();
        AssertObject(Game.Map.PlantAt(cell)).IsNotNull();                    // a crop was sown
        AssertBool(Game.Map.PlantAt(cell).Def == PlantDefOf.Wheat).IsTrue(); // the zone's crop
    }

    [TestCase]
    public void FailsWhenTheCellIsSownByAnotherFirst()
    {
        var cell = new Vector2I(5, 5);
        WheatZone(cell);
        var guy = new Guy { Position = Vector2.Zero };
        var job = new Job { Type = JobType.Sow, TargetCell = cell, ClaimsCell = true };

        Game.Map.SpawnPlant(PlantDefOf.Wheat, cell, sown: true); // beaten to it

        var status = Run(new JobDriver_Sow(), guy, job);

        AssertBool(status == JobStatus.Failed).IsTrue(); // the order no longer stands
    }
}
