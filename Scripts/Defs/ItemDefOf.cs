using Godot;

public static class ItemDefOf
{
    public static ItemDef Stone;

    public static void Load()
    {
        Stone = GD.Load<ItemDef>("res://Defs/Items/Stone.tres");
    }
}