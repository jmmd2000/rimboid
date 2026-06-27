using Godot;

/// <summary>Static references to plant definitions. Call Load() before use.</summary>
public static class PlantDefOf
{
    public static PlantDef Pine;
    public static PlantDef Oak;
    public static PlantDef BerryBush;
    public static PlantDef Wheat;

    /// <summary>Loads all plant definitions from .tres resource files.</summary>
    public static void Load()
    {
        Pine = GD.Load<PlantDef>("res://Defs/Plants/Pine.tres");
        Oak = GD.Load<PlantDef>("res://Defs/Plants/Oak.tres");
        BerryBush = GD.Load<PlantDef>("res://Defs/Plants/BerryBush.tres");
        Wheat = GD.Load<PlantDef>("res://Defs/Plants/Wheat.tres");
    }
}