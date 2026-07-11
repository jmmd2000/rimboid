using Godot;

/// <summary>Marks a building as a bed: colonists sleep in it, refilling rest RestFactor times as fast as the floor.</summary>
[GlobalClass]
public partial class ComponentProperties_Bed : ComponentProperties
{
    [Export] public float RestFactor { get; set; } = 2f;

    public override BuildingComponent MakeComponent() => new BuildingComponent_Bed();
}