using Godot;

/// <summary>An in-progress construction, a placed blueprint that accrues materials, then work.</summary>
public class Frame
{
    public BuildingDef Def;
    public Vector2I Cell;
    public int MaterialsDelivered;
    public float WorkDone;

    public bool MaterialsComplete => MaterialsDelivered >= Def.MaterialCost;

    public bool WorkComplete => WorkDone >= Def.WorkToBuild;
}