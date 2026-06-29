using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class DesignationManagerTest
{
    [TestCase]
    public void AddThenHasReturnsTrue()
    {
        var dm = new DesignationManager();
        dm.Add(DesignationType.Mine, new Vector2I(5, 5));
        AssertBool(dm.Has(DesignationType.Mine, new Vector2I(5, 5))).IsTrue();
    }

    [TestCase]
    public void HasOnWrongCellReturnsFalse()
    {
        var dm = new DesignationManager();
        dm.Add(DesignationType.Mine, new Vector2I(5, 5));
        AssertBool(dm.Has(DesignationType.Mine, new Vector2I(0, 0))).IsFalse();
    }

    [TestCase]
    public void AddDuplicateDoesNotCreateSecondEntry()
    {
        var dm = new DesignationManager();
        var cell = new Vector2I(5, 5);
        dm.Add(DesignationType.Mine, cell);
        dm.Add(DesignationType.Mine, cell);
        int count = 0;
        foreach (var c in dm.CellsOfType(DesignationType.Mine)) count++;
        AssertInt(count).IsEqual(1);
    }

    [TestCase]
    public void RemoveThenHasReturnsFalse()
    {
        var dm = new DesignationManager();
        var cell = new Vector2I(5, 5);
        dm.Add(DesignationType.Mine, cell);
        dm.Remove(DesignationType.Mine, cell);
        AssertBool(dm.Has(DesignationType.Mine, cell)).IsFalse();
    }

    [TestCase]
    public void CellsOfTypeEmptyAfterRemove()
    {
        var dm = new DesignationManager();
        var cell = new Vector2I(5, 5);
        dm.Add(DesignationType.Mine, cell);
        dm.Remove(DesignationType.Mine, cell);
        int count = 0;
        foreach (var c in dm.CellsOfType(DesignationType.Mine)) count++;
        AssertInt(count).IsEqual(0);
    }

    static GameMap MapWith(params Vector2I[] stoneCells)
    {
        DefLoader.LoadAll();
        var map = new GameMap(10, 10);
        for (int x = 0; x < map.Width; x++)
            for (int y = 0; y < map.Height; y++)
                map.Terrain[x, y] = TerrainDefOf.Dirt;
        foreach (var c in stoneCells)
            map.Terrain[c.X, c.Y] = TerrainDefOf.Stone;
        return map;
    }

    [TestCase]
    public void ReachableNextToFloor()
    {
        var map = MapWith(new Vector2I(5, 5)); // lone rock surrounded by dirt
        AssertBool(new DesignationManager().WouldBeReachable(map, new Vector2I(5, 5))).IsTrue();
    }

    [TestCase]
    public void NotReachableWhenEnclosed()
    {
        // (5,5) walled in by stone on all four sides, nothing designated yet
        var map = MapWith(new Vector2I(5, 4), new Vector2I(5, 6), new Vector2I(4, 5),
                          new Vector2I(6, 5), new Vector2I(5, 5));
        AssertBool(new DesignationManager().WouldBeReachable(map, new Vector2I(5, 5))).IsFalse();
    }

    [TestCase]
    public void ReachableThroughDesignatedNeighbour()
    {
        // (5,4) touches floor at (5,3); (5,5) is walled in behind it
        var map = MapWith(new Vector2I(5, 4), new Vector2I(5, 5),
                          new Vector2I(4, 5), new Vector2I(6, 5), new Vector2I(5, 6));
        var dm = new DesignationManager();

        AssertBool(dm.WouldBeReachable(map, new Vector2I(5, 5))).IsFalse();
        dm.Add(DesignationType.Mine, new Vector2I(5, 4));
        AssertBool(dm.WouldBeReachable(map, new Vector2I(5, 5))).IsTrue();
    }

    [TestCase]
    public void PruneRemovesOrphanedDesignations()
    {
        var map = MapWith(new Vector2I(5, 4), new Vector2I(5, 5),
                          new Vector2I(4, 5), new Vector2I(6, 5), new Vector2I(5, 6));
        var dm = new DesignationManager();
        dm.Add(DesignationType.Mine, new Vector2I(5, 4));
        dm.Add(DesignationType.Mine, new Vector2I(5, 5));

        dm.Remove(DesignationType.Mine, new Vector2I(5, 4));
        var removed = dm.PruneUnreachable(map);

        AssertBool(removed.Contains(new Vector2I(5, 5))).IsTrue();
        AssertBool(dm.Has(DesignationType.Mine, new Vector2I(5, 5))).IsFalse();
    }
}