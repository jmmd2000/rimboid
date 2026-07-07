using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class JobDriverMineTest
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
    public void MinesRockToFloorDroppingYieldAndClearingDesignation()
    {
        var cell = new Vector2I(5, 5);
        Game.Map.Terrain[cell.X, cell.Y] = TerrainDefOf.Stone; // wall to mine, dirt all around
        Game.Pathing.Init(Game.Map);
        Game.Map.Designations.Add(DesignationType.Mine, cell);
        var guy = new Guy { Position = Vector2.Zero };
        var job = new WorkGiver_Mine().TryGiveJob(guy);

        var status = Run(new JobDriver_Mine(), guy, job);

        AssertBool(status == JobStatus.Completed).IsTrue();
        AssertBool(Game.Map.Terrain[cell.X, cell.Y].Walkable).IsTrue(); // rock is now floor
        AssertBool(Game.Map.BlocksMovementAt(cell)).IsFalse(); // and no longer blocks
        AssertBool(Game.Map.Designations.Has(DesignationType.Mine, cell)).IsFalse();// designation cleared
        AssertInt(Game.Map.CountStored(ItemDefOf.Stone)).IsEqual(2);// MineYield dropped
    }
}
