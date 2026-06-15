using Godot;

/// <summary>Runtime instance of an item on the map. References a def, has a cell and stack count.</summary>
public class Item
{
    public ItemDef Def { get; init; }
    // cell is immutable. items move by remove + respawn, 
    // never by reassigning cell
    public Vector2I Cell { get; init; }
    public int Count { get; set; }
}