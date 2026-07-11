using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class WorkGiverDeconstructTest
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

    static void DesignateWall(Vector2I cell)
    {
        Game.Map.SpawnBuilding(DefDatabase<BuildingDef>.Get("WallWood"), cell);
        Game.Pathing.Init(Game.Map);
        Game.Map.Designations.Add(DesignationType.Deconstruct, cell);
    }

    [TestCase]
    public void ReturnsDeconstructJobForDesignation()
    {
        DesignateWall(new Vector2I(5, 5));
        var guy = new Guy { Position = Vector2.Zero };

        var job = new WorkGiver_Deconstruct().TryGiveJob(guy);

        AssertObject(job).IsNotNull();
        AssertBool(job.Type == JobType.Deconstruct).IsTrue();
        AssertBool(job.TargetCell == new Vector2I(5, 5)).IsTrue();
    }

    [TestCase]
    public void ReturnsNullWhenNothingDesignated()
    {
        var guy = new Guy { Position = Vector2.Zero };
        AssertObject(new WorkGiver_Deconstruct().TryGiveJob(guy)).IsNull();
    }
}