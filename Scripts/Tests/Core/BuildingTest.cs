using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class BuildingTest
{
    [BeforeTest]
    public void Setup()
    {
        TerrainDefOf.Load();
        BuildingDefOf.Load();

        Game.Map = new GameMap(10, 10);
        for (int x = 0; x < Game.Map.Width; x++)
            for (int y = 0; y < Game.Map.Height; y++)
                Game.Map.Terrain[x, y] = TerrainDefOf.Dirt;

        Game.Pathing = new Pathing();
        Game.Pathing.Init(Game.Map);
    }

    [TestCase]
    public void BlocksMovementAtReflectsBuilding()
    {
        Game.Map.SpawnBuilding(BuildingDefOf.WallStone, new Vector2I(5, 5));
        AssertBool(Game.Map.BlocksMovementAt(new Vector2I(5, 5))).IsTrue();
        AssertBool(Game.Map.BlocksMovementAt(new Vector2I(4, 5))).IsFalse();
    }

    [TestCase]
    public void WallMakesCellUnpathableAfterRefresh()
    {
        var cell = new Vector2I(5, 5);
        AssertBool(Game.Pathing.IsReachable(new Vector2I(0, 0), cell)).IsTrue();

        Game.Map.SpawnBuilding(BuildingDefOf.WallStone, cell);
        Game.Pathing.RefreshCell(Game.Map, cell);

        AssertBool(Game.Pathing.IsReachable(new Vector2I(0, 0), cell)).IsFalse();
    }
}