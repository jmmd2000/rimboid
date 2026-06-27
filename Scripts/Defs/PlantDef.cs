using Godot;

/// <summary>How a plant is gathered: chopped or harvested</summary>
public enum PlantWorkType { Chop, Harvest }

///<summary>Definition for a plant. Holds textures, harvest yield and draw size.</summary>
[GlobalClass]
public partial class PlantDef : Resource
{
    [Export] public string DefName { get; set; }
    [Export] public string Label { get; set; }
    [Export] public Texture2D[] GrowthStages { get; set; } = System.Array.Empty<Texture2D>();
    [Export] public bool BlocksMovement { get; set; }
    [Export] public PlantWorkType WorkType { get; set; }
    // the plant left behind when this one is chopped (e.g. a tree leaves a stump). Null = bare ground.
    [Export] public PlantDef LeavesBehind { get; set; }
    [Export] public bool Topples { get; set; }

    [ExportGroup("Growing")]
    // crops can be sown into a grow zone; wild plants (trees, bushes) cannot.
    [Export] public bool Sowable { get; set; }
    // days from sown (stage 0) to mature. Unused by wild plants, which spawn mature.
    [Export] public float GrowDays { get; set; } = 4f;
    [Export] public float WorkToSow { get; set; } = 100f;

    [ExportGroup("Gather")]
    [Export] public ItemDef HarvestItem { get; set; }
    [Export] public int HarvestYield { get; set; }
    [Export] public float WorkToHarvest { get; set; } = 100f;
    // 0 = destroyed when harvested. > 0 = harvested and regrows.
    [Export] public float RegrowDays { get; set; }

    [ExportGroup("Visual")]
    // sprite size in cells, fractional allowed (1.5 = 1 cell + 0.25 either side).
    // each plant rolls a size in these ranges when spawned, 0 means "match the footprint".
    // when DrawSizeGrowsWithStage is on, this is the size at the FINAL stage.
    [Export] public float MinDrawWidth { get; set; }
    [Export] public float MaxDrawWidth { get; set; }
    [Export] public float MinDrawHeight { get; set; }
    [Export] public float MaxDrawHeight { get; set; }
    // if true, draw size grows from the footprint (stage 0) up to the rolled size at the
    // final stage, so young plants sit inside their tile. if false, the rolled size is used
    // at every stage (default — trees, bushes).
    [Export] public bool DrawSizeGrowsWithStage { get; set; }

    [ExportGroup("Footprint")]
    // whole cells occupied on the grid (only blocks if BlocksMovement). Usually 1x1.
    [Export] public int FootprintWidth { get; set; } = 1;
    [Export] public int FootprintHeight { get; set; } = 1;

}