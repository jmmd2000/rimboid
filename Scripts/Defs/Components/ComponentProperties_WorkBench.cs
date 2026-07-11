using Godot;

/// <summary>Marks a building as a workbench that hosts bills of the given recipes.</summary>
[GlobalClass]
public partial class ComponentProperties_WorkBench : ComponentProperties
{
    [Export] public Godot.Collections.Array<RecipeDef> Recipes { get; set; } = new();

    public override BuildingComponent MakeComponent() => new BuildingComponent_WorkBench();
}