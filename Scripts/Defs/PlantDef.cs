using Godot;

///<summary>Definition for a plant. Holds textures, harvest yield and draw size.</summary>
[GlobalClass]
public partial class PlantDef : Resource
{
    [Export] public string DefName { get; set; }
    [Export] public string Label { get; set; }
    [Export] public Texture2D Texture { get; set; }
    [Export] public bool BlocksMovement { get; set; }

    [ExportGroup("Harvest")]
    [Export] public ItemDef HarvestItem { get; set; }
    [Export] public int HarvestYield { get; set; }
    [Export] public float WorkToHarvest { get; set; } = 100f;

    [ExportGroup("Visual")]
    // sprite size in cells, fractional allowed (1.5 = 1 cell + 0.25 either side).
    // each plant rolls a size in these ranges when spawned, 0 means "match the footprint".
    [Export] public float MinDrawWidth { get; set; }
    [Export] public float MaxDrawWidth { get; set; }
    [Export] public float MinDrawHeight { get; set; }
    [Export] public float MaxDrawHeight { get; set; }

    [ExportGroup("Footprint")]
    // whole cells occupied on the grid (only blocks if BlocksMovement). Usually 1x1.
    [Export] public int FootprintWidth { get; set; } = 1;
    [Export] public int FootprintHeight { get; set; } = 1;

}