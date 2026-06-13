using Godot;

/// <summary>Definition resource for an item type. Holds name, stack size, and graphic.</summary>
[GlobalClass]
public partial class ItemDef : Resource
{
    [Export] public string DefName { get; set; }
    [Export] public string Label { get; set; }
    [Export] public int MaxStackSize { get; set; } = 75;
    [Export] public Texture2D Graphic { get; set; }
    [Export] public float Nutrition { get; set; }

    public bool IsFood => Nutrition > 0f;
}