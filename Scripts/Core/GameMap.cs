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
    public void SpawnItem(ItemDef def, Vector2I cell, int count)
    {
        var existing = LooseItems.FirstOrDefault(i => i.Def == def && i.Cell == cell);
        if (existing != null)
            existing.Count += count;
        else
            LooseItems.Add(new Item { Def = def, Cell = cell, Count = count });
    }
}