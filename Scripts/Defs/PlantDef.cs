using Godot;

/// <summary>How a plant is gathered: chopped or harvested</summary>
public enum PlantWorkType { Chop, Harvest }

///<summary>Definition for a plant. Holds textures, harvest yield and draw size.</summary>
[GlobalClass]
public partial class PlantDef : Resource
{
    [Export] public string DefName { get; set; }
    [Export] public string Label { get; set; }
    [Export] public Texture2D Texture { get; set; }
    // shown while a regrowing plant is depleted (e.g. a bush with no berries). Null for plants that don't regrow.
    [Export] public Texture2D DepletedTexture { get; set; }
    [Export] public bool BlocksMovement { get; set; }
    [Export] public PlantWorkType WorkType { get; set; }

    [ExportGroup("Gather")]
    [Export] public ItemDef HarvestItem { get; set; }
    [Export] public int HarvestYield { get; set; }
    [Export] public float WorkToHarvest { get; set; } = 100f;
    // 0 = destroyed when harvested. > 0 = harvested and regrows.
    [Export] public float RegrowDays { get; set; }

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