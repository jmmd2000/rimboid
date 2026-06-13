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
}