using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class JobDriverEatTest
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
    public void EatsFoodRaisesTheNeedAndConsumesTheItem()
    {
        var cell = new Vector2I(5, 5);
        Game.Map.SpawnItem(ItemDefOf.Berries, cell, 1);
        var guy = new Guy { Position = Vector2.Zero };
        guy.Needs.Food.Level = 0.1f; // hungry

        var job = new Job { Type = JobType.Eat, TargetCell = cell, TargetItem = Game.Map.ItemAt(cell) };
        var status = Run(new JobDriver_Eat(), guy, job);

        AssertBool(status == JobStatus.Completed).IsTrue();
        AssertBool(guy.Needs.Food.Level > 0.1f).IsTrue();  // the need rose
        AssertObject(Game.Map.ItemAt(cell)).IsNull();      // the one berry was eaten
    }

    [TestCase]
    public void FailsWhenTheFoodIsGoneBeforeArrival()
    {
        var cell = new Vector2I(5, 5);
        Game.Map.SpawnItem(ItemDefOf.Berries, cell, 1);
        var guy = new Guy { Position = Vector2.Zero };
        guy.Needs.Food.Level = 0.1f;
        var target = Game.Map.ItemAt(cell);
        var job = new Job { Type = JobType.Eat, TargetCell = cell, TargetItem = target };

        Game.Map.RemoveItem(target); // someone else grabbed it first

        var status = Run(new JobDriver_Eat(), guy, job);

        AssertBool(status == JobStatus.Failed).IsTrue(); // walk fails, no phantom eating
    }
}
