using System.Collections.Generic;
using Godot;

/// <summary>A growing zone. Tracks which cells belong to it and which crop is sown there.</summary>
public class GrowZone
{
    public HashSet<Vector2I> Cells = new();

    public PlantDef Crop;

    /// <summary>Returns true if the cell belongs to this zone.</summary>
    /// <param name="cell">The cell to check.</param>
    public bool Contains(Vector2I cell) => Cells.Contains(cell);
}