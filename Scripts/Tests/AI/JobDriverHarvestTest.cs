using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class JobDriverHarvestTest
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

    /// <summary>Runs a driver to a terminal status, or returns Ongoing if it never settles.</summary>
    static JobStatus Run(JobDriver driver, Guy guy, Job job, int maxTicks = 4000)
    {
        driver.Init(guy, job);
        for (int i = 0; i < maxTicks; i++)
        {
            var status = driver.Tick();
            if (status != JobStatus.Ongoing) return status;
        }
        return JobStatus.Ongoing;
    }

    static void Harvestable(PlantDef def, Vector2I cell)
    {
        Game.Map.SpawnPlant(def, cell); // wild plants spawn mature, so immediately harvestable
        Game.Map.Designations.Add(DesignationType.Harvest, cell);
    }

    [TestCase]
    public void HarvestsRegrowingPlantAndKeepsIt()
    {
        var cell = new Vector2I(5, 5);
        Harvestable(PlantDefOf.BerryBush, cell); // RegrowDays > 0
        var guy = new Guy { Position = Vector2.Zero };
        var job = new WorkGiver_Harvest().TryGiveJob(guy);

        var status = Run(new JobDriver_Harvest(), guy, job);

        AssertBool(status == JobStatus.Completed).IsTrue();
        AssertObject(Game.Map.PlantAt(cell)).IsNotNull();  // bush stays
        AssertBool(Game.Map.PlantAt(cell).IsHarvestable).IsFalse();// reset to growing
        AssertBool(Game.Map.Designations.Has(DesignationType.Harvest, cell)).IsFalse();// designation cleared
        AssertInt(Game.Map.CountStored(ItemDefOf.Berries)).IsEqual(8);// yield dropped
    }

    [TestCase]
    public void HarvestsCropAndRemovesIt()
    {
        var cell = new Vector2I(5, 5);
        Harvestable(PlantDefOf.Wheat, cell); // RegrowDays == 0, destroyed on harvest
        var guy = new Guy { Position = Vector2.Zero };
        var job = new WorkGiver_Harvest().TryGiveJob(guy);

        var status = Run(new JobDriver_Harvest(), guy, job);

        AssertBool(status == JobStatus.Completed).IsTrue();
        AssertObject(Game.Map.PlantAt(cell)).IsNull();// crop consumed
        AssertBool(Game.Map.Designations.Has(DesignationType.Harvest, cell)).IsFalse();// designation cleared
        AssertInt(Game.Map.CountStored(ItemDefOf.Wheat)).IsEqual(12);// yield dropped
    }
}
