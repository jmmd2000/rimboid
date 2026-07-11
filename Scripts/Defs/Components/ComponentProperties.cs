using Godot;

/// <summary>Static config for a building capability.</summary>
[GlobalClass]
public abstract partial class ComponentProperties : Resource
{
    /// <summary>Creates the runtime component this config drives.</summary>
    public abstract BuildingComponent MakeComponent();
}