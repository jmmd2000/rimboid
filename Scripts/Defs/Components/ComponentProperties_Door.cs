using Godot;

/// <summary>Marks a building as a door: passable, and it swings open when a colonist is near.</summary>
[GlobalClass]
public partial class ComponentProperties_Door : ComponentProperties
{
    public override BuildingComponent MakeComponent() => new BuildingComponent_Door();
}