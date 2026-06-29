using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class GuyTest
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

    // gives him a reachable mining job to be busy with
    static Guy BusyGuy()
    {
        Game.Map.Designations.Add(DesignationType.Mine, new Vector2I(5, 5));
        var guy = new Guy { Position = Vector2.Zero };
        guy.Tick(); // assigns and starts the mine job
        return guy;
    }

    [TestCase]
    public void CollapsesAndSleepsWhenExhausted()
    {
        var guy = BusyGuy();
        guy.Needs.Rest.Level = 0.01f; // below the collapse threshold

        guy.Tick(); // abandons the job, think tree picks Sleep

        AssertBool(guy.IsSleeping).IsTrue();
    }

    [TestCase]
    public void DoesNotCollapseWhenMerelyTired()
    {
        var guy = BusyGuy();
        guy.Needs.Rest.Level = 0.2f; // tired, but above collapse

        guy.Tick();// keeps mining, does not abandon mid-job

        AssertBool(guy.IsSleeping).IsFalse();
    }
}