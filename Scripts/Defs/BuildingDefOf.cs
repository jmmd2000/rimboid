using Godot;

/// <summary>Static references to building definitions. Call Load() before use.</summary>
public static class BuildingDefOf
{
    public static BuildingDef WallStone;

    /// <summary>Loads all building definitions from .tres resource files.</summary>
    public static void Load()
    {
        WallStone = GD.Load<BuildingDef>("res://Defs/Buildings/WallStone.tres");
    }
}