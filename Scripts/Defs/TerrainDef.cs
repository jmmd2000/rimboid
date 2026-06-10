using Godot;

[GlobalClass]
public partial class TerrainDef : Resource
{
    [Export] public string DefName { get; set; }
    [Export] public bool Walkable { get; set; } = true;
    [Export] public float PathCostMultiplier { get; set; }

}