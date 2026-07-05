using System.Collections.Generic;
using Godot;

/// <summary>Footprint geometry</summary>
public static class Footprint
{
    /// <summary>Size after `rotation` quarter-turns (width/height swap on the odd turns).</summary>
    public static Vector2I Rotated(Vector2I size, int rotation) => rotation % 2 == 0 ? size : new Vector2I(size.Y, size.X);

    /// <summary>Cells a `size` footprint covers at `origin`, turned `rotation` quarter-turns.</summary>
    public static IEnumerable<Vector2I> Cells(Vector2I origin, Vector2I size, int rotation)
    {
        for (int i = 0; i < size.X; i++)
            for (int j = 0; j < size.Y; j++)
                yield return origin + RotateOffset(new Vector2I(i, j), rotation);
    }

    /// <summary>Top-left of the rotated footprint, as an offset from the origin cell.</summary>
    public static Vector2I MinOffset(Vector2I size, int rotation) => rotation switch
    {
        1 => new Vector2I(-(size.Y - 1), 0),
        2 => new Vector2I(-(size.X - 1), -(size.Y - 1)),
        3 => new Vector2I(0, -(size.X - 1)),
        _ => Vector2I.Zero,
    };

    /// <summary>A cell offset turned `rotation` quarter-turns clockwise about (0,0).</summary>
    static Vector2I RotateOffset(Vector2I o, int rotation) => rotation switch
    {
        1 => new Vector2I(-o.Y, o.X),
        2 => new Vector2I(-o.X, -o.Y),
        3 => new Vector2I(o.Y, -o.X),
        _ => o,
    };
}