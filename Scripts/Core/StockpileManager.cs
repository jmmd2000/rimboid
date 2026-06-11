using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>Manages all stockpile zones on the map.</summary>
public class StockpileManager
{
    readonly List<Stockpile> _stockpiles = new();

    /// <summary>Creates a new stockpile and returns it.</summary>
    /// <returns>The new stockpile.</returns>
    public Stockpile Create()
    {
        var s = new Stockpile();
        _stockpiles.Add(s);
        return s;
    }

    /// <summary>Checks whether an item is already sitting in a valid stockpile.</summary>
    /// <param name="item">The item to check.</param>
    /// <returns>True if the item's cell belongs to a stockpile that accepts it.</returns>
    public bool IsInStockpile(Item item)
    {
        return _stockpiles.Any(s => s.Cells.Contains(item.Cell) && s.WouldAccept(item.Def));
    }

    /// <summary>Finds the best stockpile cell for an item type across all stockpiles.</summary>
    /// <param name="def">The item def to find storage for.</param>
    /// <returns>A cell that can hold the item, or null if no space.</returns>
    public Vector2I? BestCellFor(ItemDef def)
    {
        foreach (var s in _stockpiles)
        {
            if (!s.WouldAccept(def)) continue;
            var cell = s.FreeCellFor(def);
            if (cell != null) return cell;
        }
        return null;
    }
}