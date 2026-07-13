using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class JobDriverWanderTest
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
    public void WalksToTargetThenCompletes()
    {
        var guy = new Guy { Position = Vector2.Zero };
        var job = new Job { Type = JobType.Wander, TargetCell = new Vector2I(3, 3) };

        var status = Run(new JobDriver_Wander(), guy, job);

        AssertBool(status == JobStatus.Completed).IsTrue();
        AssertBool(guy.Cell == new Vector2I(3, 3)).IsTrue(); // reached the wander cell
    }
}
