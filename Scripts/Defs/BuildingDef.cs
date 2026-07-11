using Godot;

/// <summary>Definition resource for a building type (wall, door). Static data shared by all instances.</summary>
[GlobalClass]
public partial class BuildingDef : Def
{
    [Export] public float WorkToBuild { get; set; } = 120f;
    [Export] public bool BlocksMovement { get; set; } = true;
    [Export] public ItemDef Materials { get; set; }
    [Export] public int MaterialCost { get; set; } = 5;
    [Export] public Color Colour { get; set; } = Colors.Gray;

    [Export] public Vector2I Size { get; set; } = Vector2I.One; // footprint in cells, origin is top left
    [Export] public bool DragPlace { get; set; } // place by dragging an area to fill or single place, like furniture
    [Export] public Texture2D Texture { get; set; } // drawn if set, otherwise fallback to colour
    [Export] public Godot.Collections.Array<ComponentProperties> Components { get; set; } = new();
}