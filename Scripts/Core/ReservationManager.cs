using System.Collections.Generic;
using Godot;

/// <summary> Tracks which Guy has claimed which work target, so two Guys don't grab the same job.</summary>
public class ReservationManager
{
    readonly Dictionary<Vector2I, Guy> _cells = new();
    readonly Dictionary<Item, Guy> _items = new();
    readonly List<Vector2I> _scratchCells = new();
    readonly List<Item> _scratchItems = new();

    public bool AvailableCell(Vector2I cell, Guy guy) => !_cells.TryGetValue(cell, out var o) || o == guy;
    public bool AvailableItem(Item item, Guy guy) => item == null || !_items.TryGetValue(item, out var o) || o == guy;

    public void ReserveCell(Vector2I cell, Guy guy) => _cells[cell] = guy;
    public void ReserveItem(Item item, Guy guy) { if (item != null) _items[item] = guy; }

    public void ReleaseAll(Guy guy)
    {
        _scratchCells.Clear();
        foreach (var kv in _cells) if (kv.Value == guy) _scratchCells.Add(kv.Key);
        foreach (var c in _scratchCells) _cells.Remove(c);

        _scratchItems.Clear();
        foreach (var kv in _items) if (kv.Value == guy) _scratchItems.Add(kv.Key);
        foreach (var i in _scratchItems) _items.Remove(i);
    }
}