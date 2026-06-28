using Godot;

/// <summary>Capability data that marks a building as a workbench</summary>
[GlobalClass]
public partial class WorkBenchDef : Resource
{
    [Export] public Godot.Collections.Array<RecipeDef> Recipes { get; set; } = new();
}