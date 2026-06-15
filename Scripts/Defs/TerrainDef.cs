using Godot;

/// <summary>Definition resource for a terrain type. Holds walkability, path cost and mining info.</summary>
[GlobalClass]
public partial class TerrainDef : Resource
{
    [Export] public string DefName { get; set; }
    [Export] public bool Walkable { get; set; } = true;
    [Export] public float PathCostMultiplier { get; set; } = 1f;

    [Export] public Vector2I AtlasCoords { get; set; }

    [ExportGroup("Mining")]
    [Export] public bool Mineable { get; set; }
    //optional
    [Export] public ItemDef MinedItem { get; set; }
    [Export] public int MineYield { get; set; } = 2;
    [Export] public float WorkToMine { get; set; } = 80f;
    // unset means dirt
    [Export] public TerrainDef TerrainAfterMined { get; set; }


}