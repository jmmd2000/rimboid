using System.Linq;
using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class GridTest
{
    [TestCase]
    public void SingleCellYieldsThatCell()
    {
        var cells = Grid.CellsInRect(new Vector2I(3, 4), new Vector2I(3, 4)).ToList();
        AssertInt(cells.Count).IsEqual(1);
        AssertBool(cells[0] == new Vector2I(3, 4)).IsTrue();
    }

    [TestCase]
    public void RectangleYieldsAllCellsInclusive()
    {
        var cells = Grid.CellsInRect(new Vector2I(1, 1), new Vector2I(3, 2)).ToList();
        AssertInt(cells.Count).IsEqual(6);
        AssertBool(cells.Contains(new Vector2I(1, 1))).IsTrue();
        AssertBool(cells.Contains(new Vector2I(3, 2))).IsTrue();
    }

    [TestCase]
    public void CornerOrderDoesNotMatter()
    {
        var forward = Grid.CellsInRect(new Vector2I(1, 1), new Vector2I(3, 2)).ToList();
        var reversed = Grid.CellsInRect(new Vector2I(3, 2), new Vector2I(1, 1)).ToList();

        AssertInt(reversed.Count).IsEqual(forward.Count);
        foreach (var c in forward)
            AssertBool(reversed.Contains(c)).IsTrue();
    }

    [TestCase]
    public void CellsInRingRadiusOneGivesEightNeighbours()
    {
        var ring = new System.Collections.Generic.List<Vector2I>(Grid.CellsInRing(new Vector2I(5, 5), 1));
        AssertInt(ring.Count).IsEqual(8);
        AssertBool(ring.Contains(new Vector2I(5, 6))).IsTrue();
    }

    [TestCase]
    public void CellsInRingRadiusZeroIsJustTheCentre()
    {
        var ring = Grid.CellsInRing(new Vector2I(5, 5), 0).ToList();
        AssertInt(ring.Count).IsEqual(1);
        AssertBool(ring[0] == new Vector2I(5, 5)).IsTrue();
    }

    [TestCase]
    public void CellsInRingRadiusTwoIsTheSixteenCellPerimeter()
    {
        // outer ring of a 5x5 block = 25 - inner 3x3 (9) = 16
        AssertInt(Grid.CellsInRing(new Vector2I(5, 5), 2).ToList().Count).IsEqual(16);
    }

    [TestCase]
    public void DistanceSquaredIsEuclideanSquared()
    {
        AssertInt(Grid.DistanceSquared(new Vector2I(0, 0), new Vector2I(3, 4))).IsEqual(25);
        AssertInt(Grid.DistanceSquared(new Vector2I(2, 2), new Vector2I(2, 2))).IsEqual(0);
    }
}