using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class JobDriverHaulTest
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
    public void MovesLoosePileIntoStockpile()
    {
        var sp = Game.Map.Stockpiles.Create();
        sp.Cells.Add(new Vector2I(0, 0));
        Game.Map.SpawnItem(ItemDefOf.Stone, new Vector2I(5, 5), 3);
        var guy = new Guy { Position = Vector2.Zero };
        var job = new WorkGiver_Haul().TryGiveJob(guy);

        var status = Run(new JobDriver_Haul(), guy, job);

        AssertBool(status == JobStatus.Completed).IsTrue();
        AssertObject(Game.Map.ItemAt(new Vector2I(5, 5))).IsNull(); // source cell cleared
        AssertObject(Game.Map.ItemAt(new Vector2I(0, 0), ItemDefOf.Stone)).IsNotNull(); // now in the stockpile
        AssertInt(Game.Map.CountStored(ItemDefOf.Stone)).IsEqual(3); // nothing created or lost
    }

    [TestCase]
    public void SplitsAcrossStockpileCellsWhenOneIsNearlyFull()
    {
        int max = ItemDefOf.Stone.MaxStackSize;
        var sp = Game.Map.Stockpiles.Create();
        sp.Cells.Add(new Vector2I(0, 0)); // partial: room for exactly 1 more
        sp.Cells.Add(new Vector2I(1, 0)); // empty
        Game.Map.SpawnItem(ItemDefOf.Stone, new Vector2I(0, 0), max - 1);
        Game.Map.SpawnItem(ItemDefOf.Stone, new Vector2I(5, 5), 5); // loose pile to haul
        var guy = new Guy { Position = Vector2.Zero };
        var job = new WorkGiver_Haul().TryGiveJob(guy);

        var status = Run(new JobDriver_Haul(), guy, job);

        AssertBool(status == JobStatus.Completed).IsTrue();
        AssertObject(Game.Map.ItemAt(new Vector2I(5, 5))).IsNull();// source drained
        AssertInt(Game.Map.ItemAt(new Vector2I(0, 0), ItemDefOf.Stone).Count).IsEqual(max); // topped off first
        AssertInt(Game.Map.ItemAt(new Vector2I(1, 0), ItemDefOf.Stone).Count).IsEqual(4); // remainder spilled here
    }
}
