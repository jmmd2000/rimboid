using Godot;

/// <summary>A finished, placed building. Blocks movement when its def says so.</summary>
public class Building
{
    public BuildingDef Def { get; init; }
    public Vector2I Cell { get; init; }
}