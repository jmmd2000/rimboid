using Godot;

/// <summary>Runtime instance of an item on the map. References a def, has a cell and stack count.</summary>
public class Item
{
    public ItemDef Def;
    public Vector2I Cell;
    public int Count;
}