using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class JobDriverDoBillTest
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

    static Building Stove(Vector2I cell)
    {
        var stove = Game.Map.SpawnBuilding(BuildingDefOf.Stove, cell);
        foreach (var c in stove.OccupiedCells) Game.Pathing.RefreshCell(Game.Map, c);
        return stove;
    }

    [TestCase]
    public void ConsumesIngredientsAndProducesOutput()
    {
        var stove = Stove(new Vector2I(5, 5));
        stove.WorkBench.Bills.Add(new Bill { Recipe = RecipeDefOf.CookSimpleMeal, RepeatMode = BillRepeatMode.DoForever });
        Game.Map.SpawnItem(ItemDefOf.Wheat, new Vector2I(2, 2), 4); // the whole recipe cost
        var guy = new Guy { Position = Vector2.Zero };
        var job = new WorkGiver_DoBill().TryGiveJob(guy);

        var status = Run(new JobDriver_DoBill(), guy, job);

        AssertBool(status == JobStatus.Completed).IsTrue();
        AssertInt(Game.Map.CountStored(ItemDefOf.Wheat)).IsEqual(0);// ingredients eaten by the bill
        AssertInt(Game.Map.CountStored(ItemDefOf.SimpleMeal)).IsEqual(1); // one meal dropped on the floor
    }
}
