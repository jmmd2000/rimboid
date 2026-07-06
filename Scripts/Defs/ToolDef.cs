using Godot;

/// <summary>Which bar section a tool belongs to: an Order acts on a target, a Zone reserves an area.</summary>
public enum ToolCategory { Order, Zone }

/// <summary>Definition resource for a player tool.</summary>
[GlobalClass]
public partial class ToolDef : Def
{
    [Export] public ToolCategory Category { get; set; }
    [Export] public Texture2D Icon { get; set; }
    [Export] public Key Shortcut { get; set; }
    [Export] public int Order { get; set; }
}