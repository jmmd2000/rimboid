using System.Collections.Generic;
using Godot;

/// <summary>A finished, placed building. Blocks movement when its def says so.</summary>
public class Building
{
    public BuildingDef Def { get; init; }
    public Vector2I Cell { get; init; } // origin (top left cell) of the footprint
    public int Rotation { get; init; }

    /// <summary>Every cell this building covers, derived from its origin and the def's size.</summary>
    public IEnumerable<Vector2I> OccupiedCells => Footprint.Cells(Cell, Def.Size, Rotation);

    /// <summary>Runtime workbench state, present only when Def.WorkBench != null</summary>
    public WorkBench WorkBench { get; set; }
}