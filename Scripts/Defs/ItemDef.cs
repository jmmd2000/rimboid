using Godot;

/// <summary>Definition resource for an item type. Holds name, stack size, and graphic.</summary>
[GlobalClass]
public partial class ItemDef : Def
{
    [Export] public int MaxStackSize { get; set; } = 75;
    [Export] public Texture2D Texture { get; set; }

    [Export] public FoodProperties Food { get; set; }

    public bool IsFood => Food != null;
}