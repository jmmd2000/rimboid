using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class JobDriverHaulToFrameTest
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
    public void DeliversMaterialsIntoTheFrame()
    {
        var frame = new Frame { Def = BuildingDefOf.WallStone, Cell = new Vector2I(5, 5) };
        Game.Map.AddFrame(frame);
        Game.Map.SpawnItem(ItemDefOf.Stone, new Vector2I(2, 2), 10);
        var guy = new Guy { Position = Vector2.Zero };
        var job = new WorkGiver_Construct().TryGiveJob(guy); // a HaulToFrame job

        var status = Run(new JobDriver_HaulToFrame(), guy, job);

        AssertBool(status == JobStatus.Completed).IsTrue();
        AssertInt(frame.MaterialsDelivered).IsEqual(frame.Def.MaterialCost); // frame fully stocked
        AssertObject(guy.Carrying).IsNull();                                 // the load was deposited
    }

    [TestCase]
    public void FailsAndKeepsFrameEmptyWhenTheFrameIsCancelledMidHaul()
    {
        var frame = new Frame { Def = BuildingDefOf.WallStone, Cell = new Vector2I(5, 5) };
        Game.Map.AddFrame(frame);
        Game.Map.SpawnItem(ItemDefOf.Stone, new Vector2I(2, 2), 10);
        var guy = new Guy { Position = Vector2.Zero };
        var job = new WorkGiver_Construct().TryGiveJob(guy);

        Game.Map.RemoveFrame(frame); // cancelled before the guy can deliver

        var status = Run(new JobDriver_HaulToFrame(), guy, job);

        AssertBool(status == JobStatus.Failed).IsTrue();       // job fails cleanly
        AssertInt(frame.MaterialsDelivered).IsEqual(0);        // nothing delivered into a dead frame
    }
}
