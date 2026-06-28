using Godot;

/// <summary>Base class for all definition resources.</summary>
public partial class Def : Resource
{
    [Export] public string DefName { get; set; }
    [Export] public string Label { get; set; }
}