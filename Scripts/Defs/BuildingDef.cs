using Godot;

/// <summary>Definition resource for a building type (wall, door). Static data shared by all instances.</summary>
[GlobalClass]
public partial class BuildingDef : Resource
{
    [Export] public string DefName { get; set; }
    [Export] public string Label { get; set; }
    [Export] public float WorkToBuild { get; set; } = 120f;
    [Export] public bool BlocksMovement { get; set; } = true;
    [Export] public ItemDef Materials { get; set; }
    [Export] public int MaterialCost { get; set; } = 5;
    [Export] public Color Colour { get; set; } = Colors.Gray;

    [Export] public Vector2I Size { get; set; } = Vector2I.One; // footprint in cells, origin is top left
    [Export] public Texture2D Texture { get; set; } // drawn if set, otherwise fallback to colour
    [Export] public WorkBenchDef WorkBench { get; set; } // non null means this building does bills
}