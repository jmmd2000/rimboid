using Godot;

/// <summary>Definition resource for a terrain type. Holds walkability and path cost.</summary>
[GlobalClass]
public partial class TerrainDef : Resource
{
    [Export] public string DefName { get; set; }
    [Export] public bool Walkable { get; set; } = true;
    [Export] public float PathCostMultiplier { get; set; } = 1f;

}