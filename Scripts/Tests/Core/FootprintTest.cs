using System.Collections.Generic;
using System.Linq;
using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class FootprintTest
{
    static readonly Vector2I Origin = new(5, 5);
    static readonly Vector2I Wide = new(2, 1);   // 2 wide, 1 tall

    static HashSet<Vector2I> Cells(int rotation) => Footprint.Cells(Origin, Wide, rotation).ToHashSet();

    [TestCase]
    public void CellsRotateAroundOrigin()
    {
        // the origin cell is always occupied; the second cell walks E -> S -> W -> N
        AssertBool(Cells(0).SetEquals(new HashSet<Vector2I> { Origin, Origin + Vector2I.Right })).IsTrue();
        AssertBool(Cells(1).SetEquals(new HashSet<Vector2I> { Origin, Origin + Vector2I.Down })).IsTrue();
        AssertBool(Cells(2).SetEquals(new HashSet<Vector2I> { Origin, Origin + Vector2I.Left })).IsTrue();
        AssertBool(Cells(3).SetEquals(new HashSet<Vector2I> { Origin, Origin + Vector2I.Up })).IsTrue();
    }

    [TestCase]
    public void RotatedSwapsOnOddTurns()
    {
        AssertBool(Footprint.Rotated(Wide, 0) == new Vector2I(2, 1)).IsTrue();
        AssertBool(Footprint.Rotated(Wide, 1) == new Vector2I(1, 2)).IsTrue();
        AssertBool(Footprint.Rotated(Wide, 2) == new Vector2I(2, 1)).IsTrue();
        AssertBool(Footprint.Rotated(Wide, 3) == new Vector2I(1, 2)).IsTrue();
    }

    [TestCase]
    public void MinOffsetTracksTheRotatedTopLeft()
    {
        AssertBool(Footprint.MinOffset(Wide, 0) == new Vector2I(0, 0)).IsTrue();
        AssertBool(Footprint.MinOffset(Wide, 1) == new Vector2I(0, 0)).IsTrue();
        AssertBool(Footprint.MinOffset(Wide, 2) == new Vector2I(-1, 0)).IsTrue();
        AssertBool(Footprint.MinOffset(Wide, 3) == new Vector2I(0, -1)).IsTrue();
    }
}
