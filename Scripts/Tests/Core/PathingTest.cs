using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class PathingTest
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

    static void Wall(Vector2I cell)
    {
        Game.Map.SpawnBuilding(BuildingDefOf.WallStone, cell);
        Game.Pathing.RefreshCell(Game.Map, cell);
    }

    [TestCase]
    public void ReachableCellsCoversOpenMap()
    {
        var reachable = Game.Pathing.ReachableCells(new Vector2I(0, 0));
        AssertBool(reachable.Contains(new Vector2I(9, 9))).IsTrue();
    }

    [TestCase]
    public void SealedCellDropsOutAfterWalling()
    {
        var inside = new Vector2I(5, 5);
        AssertBool(Game.Pathing.ReachableCells(new Vector2I(0, 0)).Contains(inside)).IsTrue();

        foreach (var d in Grid.Adjacent8) Wall(inside + d);

        AssertBool(Game.Pathing.ReachableCells(new Vector2I(0, 0)).Contains(inside)).IsFalse();
    }
}