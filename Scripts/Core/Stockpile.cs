using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>A storage zone. Tracks which cells belong to it and which item types it accepts.</summary>
public class Stockpile
{
    public HashSet<Vector2I> Cells = new();
    public HashSet<ItemDef> Accepts = new();

    /// <summary>Returns true if this stockpile accepts the given item type.</summary>
    /// <param name="def">The item def to check.</param>
    /// <returns>True if accepted (empty filter means accept all)</returns>
    public bool WouldAccept(ItemDef def) => Accepts.Count == 0 || Accepts.Contains(def);

    /// <summary>Finds a cell in this stockpile with any room for the item, preferring a partial stack.</summary>
    /// <param name="def">The item def to find space for.</param>
    /// <returns>A cell with room, or null if every cell is full.</returns>
    public Vector2I? FreeCellFor(ItemDef def)
    {
        Vector2I? empty = null;
        foreach (var cell in Cells)
        {
            var here = Game.Map.ItemAt(cell);
            if (here == null) { empty ??= cell; continue; }
            if (here.Def == def && here.Count < def.MaxStackSize) return cell;
        }
        // no partial cell found, use an empty.
        return empty;
    }
}