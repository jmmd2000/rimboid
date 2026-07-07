using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class JobDriverChopTest
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

    [TestCase]
    public void FellsTreeLeavingStumpAndDroppingWood()
    {
        var cell = new Vector2I(5, 5);
        Game.Map.SpawnPlant(PlantDefOf.Pine, cell); // a tree: chops, topples, leaves a stump
        Game.Pathing.RefreshCell(Game.Map, cell);// the tree blocks its cell
        Game.Map.Designations.Add(DesignationType.Chop, cell);
        var guy = new Guy { Position = Vector2.Zero };
        var job = new WorkGiver_Chop().TryGiveJob(guy);

        var status = Run(new JobDriver_Chop(), guy, job);

        AssertBool(status == JobStatus.Completed).IsTrue();
        AssertObject(Game.Map.PlantAt(cell)).IsNotNull();// something remains
        AssertBool(Game.Map.PlantAt(cell).Def == PlantDefOf.Pine.LeavesBehind).IsTrue();// it's the stump
        AssertBool(Game.Map.Designations.Has(DesignationType.Chop, cell)).IsFalse();// designation cleared
        AssertInt(Game.Map.CountStored(PlantDefOf.Pine.HarvestItem)).IsEqual(10);// wood dropped
    }
}
