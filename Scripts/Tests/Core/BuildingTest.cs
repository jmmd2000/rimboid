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
        DefLoader.LoadAll();

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

    [TestCase]
    public void FootprintCentreOfSingleCell()
    {
        var b = new Building { Def = new BuildingDef { Size = Vector2I.One }, Cell = new Vector2I(5, 5) };
        AssertBool(b.FootprintCentre.IsEqualApprox(new Vector2(5.5f, 5.5f))).IsTrue();
    }

    [TestCase]
    public void FootprintCentreOfMultiCellIsMidpoint()
    {
        // a 1x2 at (5,5) covers (5,5) and (5,6); the centre sits on their shared edge
        var b = new Building { Def = new BuildingDef { Size = new Vector2I(1, 2) }, Cell = new Vector2I(5, 5) };
        AssertBool(b.FootprintCentre.IsEqualApprox(new Vector2(5.5f, 6.0f))).IsTrue();
    }
}