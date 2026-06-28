using Godot;

/// <summary>Capability data for an edible item.</summary>
[GlobalClass]
public partial class FoodProperties : Resource
{
    [Export] public float Nutrition { get; set; }
}