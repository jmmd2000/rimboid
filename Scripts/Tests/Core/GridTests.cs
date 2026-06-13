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
}