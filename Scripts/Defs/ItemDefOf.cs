using Godot;

/// <summary>Static references to item definitions. Call Load() before use.</summary>
public static class ItemDefOf
{
    public static ItemDef Stone;
    public static ItemDef Berries;
    public static ItemDef Wheat;
    public static ItemDef SimpleMeal;

    /// <summary>Loads all item definitions from .tres resource files.</summary>
    public static void Load()
    {
        Stone = GD.Load<ItemDef>("res://Defs/Items/Stone.tres");
        Berries = GD.Load<ItemDef>("res://Defs/Items/Berries.tres");
        Wheat = GD.Load<ItemDef>("res://Defs/Items/Wheat.tres");
        SimpleMeal = GD.Load<ItemDef>("res://Defs/Items/SimpleMeal.tres");
    }
}