using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>
///  A storage zone. Tracks which cells belong to it and which item types it accepts.
/// </summary>
public class Stockpile
{
    public HashSet<Vector2I> Cells = new();
    public HashSet<ItemDef> Accepts = new();

    /// <summary>
    /// Returns true if this stockpile accepts the given item type.
    /// </summary>
    /// <param name="def">The item def to check.</param>
    /// <returns>True if accepted (empty filter means accept all)</returns>
    public bool WouldAccept(ItemDef def) => Accepts.Count == 0 || Accepts.Contains(def);

    /// <summary>
    /// Finds a cell in this stockpile that has room for the given item.
    /// </summary>
    /// <param name="def">The item def to find space for.</param>
    /// <returns>A free cell, or null if full.</returns>
    public Vector2I? FreeCellFor(ItemDef def)
    {
        foreach (var cell in Cells)
        {
            var existing = Game.Map.ItemAt(cell, def);
            if (existing == null) return cell;
            if (existing.Count < def.MaxStackSize) return cell;
        }
        return null;
    }
}