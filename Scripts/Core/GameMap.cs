using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>Simulation model for the map. Holds terrain grid, designations, and loose items.</summary>
public class GameMap
{
    public int Width;
    public int Height;
    public TerrainDef[,] Terrain;
    public DesignationManager Designations = new();
    public List<Item> LooseItems = new();
    public StockpileManager Stockpiles = new();

    /// <summary>Creates a new map with the given dimensions.</summary>
    /// <param name="width">Map width in cells.</param>
    /// <param name="height">Map height in cells.</param>
    public GameMap(int width, int height)
    {
        Width = width;
        Height = height;
        Terrain = new TerrainDef[width, height];
    }

    /// <summary>Spawns an item at a cell, stacking onto an existing pile if possible.</summary>
    /// <param name="def">The item definition.</param>
    /// <param name="cell">The map cell to spawn at.</param>
    /// <param name="count">Number of items to add.</param>
    /// <returns>The item, and true if a new pile was created (false if merged onto an existing one).</returns>
    public (Item item, bool isNew) SpawnItem(ItemDef def, Vector2I cell, int count)
    {
        var existing = LooseItems.FirstOrDefault(i => i.Def == def && i.Cell == cell);
        if (existing != null)
        {
            existing.Count += count;
            return (existing, false);
        }

        var item = new Item { Def = def, Cell = cell, Count = count };
        LooseItems.Add(item);
        return (item, true);
    }

    /// <summary>Returns the item at a cell, or null if empty.</summary>
    /// <param name="cell">The cell to check.</param>
    /// <param name="def">Optional: only match this item type.</param>
    /// <returns>The matching item, or null.</returns>
    public Item ItemAt(Vector2I cell, ItemDef def = null)
    {
        return LooseItems.FirstOrDefault(i => i.Cell == cell && (def == null || i.Def == def));
    }
}