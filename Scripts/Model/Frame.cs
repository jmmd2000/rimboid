using System.Collections.Generic;
using Godot;

/// <summary>An in-progress construction, a placed blueprint that accrues materials, then work.</summary>
public class Frame
{
    public BuildingDef Def { get; init; }
    public Vector2I Cell { get; init; } //origin top left
    public int Rotation { get; init; }
    public int MaterialsDelivered { get; set; }
    public float WorkDone { get; set; }

    public bool MaterialsComplete => MaterialsDelivered >= Def.MaterialCost;
    public bool WorkComplete => WorkDone >= Def.WorkToBuild;

    /// <summary>Every cell this frame covers, from its origin and the def's size.</summary>
    public IEnumerable<Vector2I> OccupiedCells => Footprint.Cells(Cell, Def.Size, Rotation);
}