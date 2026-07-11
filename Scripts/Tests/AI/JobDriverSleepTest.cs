using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class JobDriverSleepTest
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

    /// <summary>Rest gained by sleeping in place on a cell for a fixed number of ticks.</summary>
    static float SleepGain(Vector2I cell, int ticks)
    {
        var guy = new Guy { Position = new Vector2(cell.X, cell.Y) };
        guy.Needs.Rest.Level = 0.2f;

        var driver = new JobDriver_Sleep();
        driver.Init(guy, new Job { Type = JobType.Sleep }); // in place, so the refill reads the cell under him

        float before = guy.Needs.Rest.Level;
        for (int i = 0; i < ticks; i++) driver.Tick();
        return guy.Needs.Rest.Level - before;
    }

    [TestCase]
    public void RefillsFasterInABed()
    {
        Game.Map.SpawnBuilding(DefDatabase<BuildingDef>.Get("Bed"), new Vector2I(5, 5));

        float bedGain = SleepGain(new Vector2I(5, 5), 100); // lying on the bed
        float floorGain = SleepGain(new Vector2I(0, 0), 100); // lying on the floor

        AssertBool(bedGain > floorGain).IsTrue();
    }
}