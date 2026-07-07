using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class JobDriverBuildTest
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

    static Frame StockedFrame(Vector2I cell)
    {
        var frame = new Frame { Def = BuildingDefOf.WallStone, Cell = cell };
        frame.MaterialsDelivered = frame.Def.MaterialCost;
        Game.Map.AddFrame(frame);
        return frame;
    }

    [TestCase]
    public void RaisesBuildingFromStockedFrame()
    {
        var cell = new Vector2I(5, 5);
        StockedFrame(cell);
        var guy = new Guy { Position = Vector2.Zero };
        var job = new WorkGiver_Construct().TryGiveJob(guy);

        var status = Run(new JobDriver_Build(), guy, job);

        AssertBool(status == JobStatus.Completed).IsTrue();
        AssertObject(Game.Map.BuildingAt(cell)).IsNotNull();// building raised
        AssertBool(Game.Map.HasFrame(cell)).IsFalse();// frame consumed
        AssertBool(Game.Map.BlocksMovementAt(cell)).IsTrue();// wall now blocks the cell
    }

    [TestCase]
    public void FailsAndBuildsNothingWhenFrameCancelledMidJob()
    {
        var cell = new Vector2I(5, 5);
        var frame = StockedFrame(cell);
        var guy = new Guy { Position = Vector2.Zero };
        var job = new WorkGiver_Construct().TryGiveJob(guy);

        var driver = new JobDriver_Build();
        driver.Init(guy, job);
        driver.Tick();// first tick starts the walk
        Game.Map.RemoveFrame(frame);// player cancels the blueprint mid-walk

        var status = JobStatus.Ongoing;
        for (int i = 0; i < 4000 && status == JobStatus.Ongoing; i++) status = driver.Tick();

        AssertBool(status == JobStatus.Failed).IsTrue();
        AssertObject(Game.Map.BuildingAt(cell)).IsNull();// nothing built
    }
}
