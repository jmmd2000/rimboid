using Godot;

[GlobalClass]
public partial class ItemDef : Resource
{
    [Export] public string DefName { get; set; }
    [Export] public string Label { get; set; }
    [Export] public int MaxStackSize { get; set; } = 75;
    [Export] public Texture2D Graphic { get; set; }
}