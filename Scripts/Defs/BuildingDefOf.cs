using Godot;

/// <summary>Static references to building definitions. Call Load() before use.</summary>
public static class BuildingDefOf
{
    public static BuildingDef WallStone;
    public static BuildingDef Stove;

    /// <summary>Loads all building definitions from .tres resource files.</summary>
    public static void Load()
    {
        WallStone = GD.Load<BuildingDef>("res://Defs/Buildings/WallStone.tres");
        Stove = GD.Load<BuildingDef>("res://Defs/Buildings/Stove.tres");
    }
}