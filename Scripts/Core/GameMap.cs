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
    public List<Frame> Frames = new();
    public Dictionary<Vector2I, Building> Buildings = new();

    /// <summary>Creates a new map with the given dimensions.</summary>
    /// <param name="width">Map width in cells.</param>
    /// <param name="height">Map height in cells.</param>
    public GameMap(int width, int height)
    {
        Width = width;
        Height = height;
        Terrain = new TerrainDef[width, height];
    }

    /// <summary>Returns true if the cell lies within the map grid.</summary>
    /// <param name="cell">The cell to test.</param>
    public bool InBounds(Vector2I cell) => cell.X >= 0 && cell.X < Width && cell.Y >= 0 && cell.Y < Height;

    /// <summary>Spawns an item at a cell, merging onto an existing pile of the same def.
    /// The pile is capped at the def's max stack size, any amount that doesn't fit
    /// is returned as overflow for the caller to place elsewhere.</summary>
    /// <param name="def">The item def.</param>
    /// <param name="cell">The map cell to spawn at.</param>
    /// <param name="count">Number of items to add.</param>
    /// <returns>The pile, whether it was newly created, and the overflow that didn't fit.</returns>
    public (Item item, bool isNew, int overflow) SpawnItem(ItemDef def, Vector2I cell, int count)
    {
        var existing = LooseItems.FirstOrDefault(i => i.Def == def && i.Cell == cell);
        if (existing != null)
        {
            int space = Mathf.Max(0, def.MaxStackSize - existing.Count);
            int added = Mathf.Min(count, space);
            existing.Count += added;
            return (existing, false, count - added);
        }

        int initial = Mathf.Min(count, def.MaxStackSize);
        var item = new Item { Def = def, Cell = cell, Count = initial };
        LooseItems.Add(item);
        return (item, true, count - initial);
    }

    /// <summary>
    /// Drops items on the map starting at a cell, capping each cell at the def's max stack size
    /// and spilling any remainder outwards to the nearest open floor cells. Returns the piles
    /// that were newly created so the caller can spawn views for them.
    /// </summary>
    /// <param name="def">The item def to drop.</param>
    /// <param name="origin">The preferred cell to drop on.</param>
    /// <param name="count">Total units to drop.</param>
    /// <returns>The newly created piles, in placement order.</returns>
    public List<Item> DropItems(ItemDef def, Vector2I origin, int count)
    {
        var newPiles = new List<Item>();
        int maxRadius = Mathf.Max(Width, Height);

        for (int radius = 0; count > 0 && radius <= maxRadius; radius++)
            foreach (var cell in Grid.CellsInRing(origin, radius))
            {
                if (count <= 0) break;
                if (!InBounds(cell) || !Terrain[cell.X, cell.Y].Walkable || BlocksMovementAt(cell)) continue;

                var (pile, isNew, overflow) = SpawnItem(def, cell, count);
                if (isNew) newPiles.Add(pile);
                count = overflow;
            }

        return newPiles;
    }

    /// <summary>Returns the item at a cell, or null if empty.</summary>
    /// <param name="cell">The cell to check.</param>
    /// <param name="def">Optional: only match this item type.</param>
    /// <returns>The matching item, or null.</returns>
    public Item ItemAt(Vector2I cell, ItemDef def = null)
    {
        return LooseItems.FirstOrDefault(i => i.Cell == cell && (def == null || i.Def == def));
    }

    /// <summary>Returns the construction frame at a cell, or null if none.</summary>
    /// <param name="cell">The cell to check.</param>
    public Frame FrameAt(Vector2I cell) => Frames.FirstOrDefault(f => f.Cell == cell);

    /// <summary>True if a construction frame already occupies the cell.</summary>
    /// <param name="cell">The cell to check.</param>
    public bool HasFrame(Vector2I cell) => Frames.Any(f => f.Cell == cell);

    /// <summary>Returns the building at a cell, or null if none.</summary>
    /// <param name="cell">The cell to check.</param>
    public Building BuildingAt(Vector2I cell) => Buildings.GetValueOrDefault(cell);

    /// <summary>True if a movement-blocking building occupies the cell.</summary>
    /// <param name="cell">The cell to check.</param>
    public bool BlocksMovementAt(Vector2I cell) => Buildings.TryGetValue(cell, out var b) && b.Def.BlocksMovement;

    /// <summary>Spawns a finished building at a cell and returns it.</summary>
    /// <param name="def">The building definition.</param>
    /// <param name="cell">The cell to place it on.</param>
    public Building SpawnBuilding(BuildingDef def, Vector2I cell)
    {
        var building = new Building { Def = def, Cell = cell };
        Buildings[cell] = building;
        return building;
    }
}