using System.Collections.Generic;
using Godot;

/// <summary>A finished, placed building. Blocks movement when its def says so.</summary>
public class Building
{
    public BuildingDef Def { get; init; }
    public Vector2I Cell { get; init; } // origin (top left cell) of the footprint

    // <summary>Every cell this building covers, derived from its origin and the def's size.</summary>
    public IEnumerable<Vector2I> OccupiedCells
    {
        get
        {
            for (int dx = 0; dx < Def.Size.X; dx++)
                for (int dy = 0; dy < Def.Size.Y; dy++)
                    yield return Cell + new Vector2I(dx, dy);
        }
    }
}