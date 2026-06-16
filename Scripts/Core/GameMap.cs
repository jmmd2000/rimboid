using System.Collections.Generic;
using Godot;

/// <summary>Simulation model for the map. Holds terrain grid, designations, loose items, frames and buildings.</summary>
public class GameMap
{
    public int Width;
    public int Height;
    public TerrainDef[,] Terrain;
    public DesignationManager Designations = new();
    public StockpileManager Stockpiles = new();
    public Dictionary<Vector2I, Building> Buildings = new();
    public List<Guy> Guys = new();
    public ReservationManager Reservations = new();

    // a flat list for iteration, plus a per-cell index for quicker lookup
    readonly List<Item> _looseItems = new();
    readonly Dictionary<Vector2I, List<Item>> _itemsByCell = new();
    public IReadOnlyList<Item> LooseItems => _looseItems;

    readonly List<Frame> _frames = new();
    readonly Dictionary<Vector2I, Frame> _frameByCell = new();
    public IReadOnlyList<Frame> Frames => _frames;


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
    public bool InBounds(Vector2I cell) => cell.X >= 0 && cell.X < Width && cell.Y >= 0 && cell.Y < Height;

    /// <summary>Advances every pawn on this map by one sim tick</summary>
    public void Tick()
    {
        foreach (var guy in Guys)
        {
            guy.Tick();
        }
    }

    // ---------- items ----------

    /// <summary>Spawns an item at a cell, merging onto an existing pile of the same def.
    /// The pile is capped at the def's max stack size, any amount that doesn't fit
    /// is returned as overflow for the caller to place elsewhere.</summary>
    /// <param name="def">The item def.</param>
    /// <param name="cell">The map cell to spawn at.</param>
    /// <param name="count">Number of items to add.</param>
    /// <returns>The pile, whether it was newly created, and the overflow that didn't fit.</returns>
    public (Item item, bool isNew, int overflow) SpawnItem(ItemDef def, Vector2I cell, int count)
    {
        var existing = ItemAt(cell, def);
        if (existing != null)
        {
            int space = Mathf.Max(0, def.MaxStackSize - existing.Count);
            int added = Mathf.Min(count, space);
            existing.Count += added;
            return (existing, false, count - added);
        }

        int initial = Mathf.Min(count, def.MaxStackSize);
        var item = new Item { Def = def, Cell = cell, Count = initial };
        _looseItems.Add(item);
        IndexItem(item);
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

    /// <summary>Removes a loose item from the map (picked up, eaten etc.)</summary>
    public void RemoveItem(Item item)
    {
        _looseItems.Remove(item);
        if (_itemsByCell.TryGetValue(item.Cell, out var list))
        {
            list.Remove(item);
            if (list.Count == 0) _itemsByCell.Remove(item.Cell);
        }
    }

    /// <summary>Returns the item at a cell, or null if none. Optionally filters to one def.</summary>
    public Item ItemAt(Vector2I cell, ItemDef def = null)
    {
        if (!_itemsByCell.TryGetValue(cell, out var list)) return null;
        if (def == null) return list.Count > 0 ? list[0] : null;
        return list.Find(i => i.Def == def);
    }

    /// <summary>True if this exact item is still on the map.</summary>
    public bool HasItem(Item item) => _itemsByCell.TryGetValue(item.Cell, out var list) && list.Contains(item);

    void IndexItem(Item item)
    {
        if (!_itemsByCell.TryGetValue(item.Cell, out var list))
        {
            _itemsByCell[item.Cell] = list = new List<Item>();
        }
        list.Add(item);
    }

    // ---------- frames ----------

    /// <summary>Places a construction frame on the map. Assumes the cell has no frame already (callers check HasFrame).</summary>
    public void AddFrame(Frame frame)
    {
        _frames.Add(frame);
        _frameByCell[frame.Cell] = frame;
    }

    /// <summary>Removes a construction frame (cancelled or finished).</summary>
    public void RemoveFrame(Frame frame)
    {
        _frames.Remove(frame);
        _frameByCell.Remove(frame.Cell);
    }

    /// <summary>Returns the construction frame at a cell, or null if none.</summary>
    public Frame FrameAt(Vector2I cell) => _frameByCell.GetValueOrDefault(cell);

    /// <summary>True if a construction frame already occupies the cell.</summary>
    public bool HasFrame(Vector2I cell) => _frameByCell.ContainsKey(cell);

    // ---------- buildings ----------

    /// <summary>Returns the building at a cell, or null if none.</summary>
    public Building BuildingAt(Vector2I cell) => Buildings.GetValueOrDefault(cell);

    /// <summary>True if a movement-blocking building occupies the cell.</summary>
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