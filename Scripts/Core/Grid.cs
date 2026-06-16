using System.Collections.Generic;
using Godot;

/// <summary>Grid geometry helpers.</summary>
public static class Grid
{
    /// <summary>Yields every cell in the rectangle between two corners (inclusive).</summary>
    /// <param name="a">One corner cell.</param>
    /// <param name="b">The opposite corner cell.</param>
    public static IEnumerable<Vector2I> CellsInRect(Vector2I a, Vector2I b)
    {
        int minX = Mathf.Min(a.X, b.X), maxX = Mathf.Max(a.X, b.X);
        int minY = Mathf.Min(a.Y, b.Y), maxY = Mathf.Max(a.Y, b.Y);
        for (int x = minX; x <= maxX; x++)
            for (int y = minY; y <= maxY; y++)
                yield return new Vector2I(x, y);
    }

    /// <summary>
    /// Yields the cells forming the square ring at a given Chebyshev distance from a centre.
    /// Radius 0 yields just the centre, radius 1 the 8 surrounding cells, and so on outwards.
    /// </summary>
    /// <param name="centre">The centre cell.</param>
    /// <param name="radius">Ring distance from the centre.</param>
    public static IEnumerable<Vector2I> CellsInRing(Vector2I centre, int radius)
    {
        if (radius <= 0)
        {
            yield return centre;
            yield break;
        }
        for (int x = centre.X - radius; x <= centre.X + radius; x++)
            for (int y = centre.Y - radius; y <= centre.Y + radius; y++)
                if (Mathf.Abs(x - centre.X) == radius || Mathf.Abs(y - centre.Y) == radius)
                    yield return new Vector2I(x, y);
    }

    public static readonly Vector2I[] Adjacent8 =
    {
        // cardinals
        new(1, 0), new(-1, 0), new(0, 1), new(0, -1),
        // diagonals
        new(1, 1), new(1, -1), new(-1, 1), new(-1, -1),
    };

    public static readonly Vector2I[] Cardinal4 =
    {
        new(1, 0), new(-1, 0), new(0, 1), new(0, -1),
    };

    public static int DistanceSquared(Vector2I a, Vector2I b)
    {
        int dx = a.X - b.X;
        int dy = a.Y - b.Y;
        return dx * dx + dy * dy;
    }
}