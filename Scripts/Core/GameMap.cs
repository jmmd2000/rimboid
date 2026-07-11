using System;
using System.Collections.Generic;
using Godot;

/// <summary>Simulation model for the map. Holds terrain grid, designations, loose items, frames, buildings, plants.</summary>
public class GameMap
{
    public int Width;
    public int Height;
    public TerrainDef[,] Terrain;
    public DesignationManager Designations = new();
    public StockpileManager Stockpiles = new();
    public GrowZoneManager GrowZones = new();
    public Dictionary<Vector2I, Building> Buildings = new();
    public Dictionary<Vector2I, Plant> Plants = new();
    public List<Guy> Guys = new();
    public ReservationManager Reservations = new();

    // a flat list for iteration, plus a per-cell index for quicker lookup
    readonly List<Item> _looseItems = new();
    readonly Dictionary<Vector2I, List<Item>> _itemsByCell = new();
    public IReadOnlyList<Item> LooseItems => _looseItems;

    // components that opt into per-tick updates
    readonly List<BuildingComponent> _tickingComponents = new();

    readonly List<Frame> _frames = new();
    readonly Dictionary<Vector2I, Frame> _frameByCell = new();
    public IReadOnlyList<Frame> Frames => _frames;

    public event Action<Item> ItemSpawned;
    public event Action<Item> ItemRemoved;
    public event Action<Plant> PlantSpawned;
    public event Action<Plant> PlantRemoved;
    public event Action<Frame> FrameAdded;
    public event Action<Frame> FrameRemoved;
    public event Action<Building> BuildingSpawned;
    public event Action<Vector2I> TerrainChanged;
    public event Action<Guy> GuyAdded;


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
        using (Prof.Sample("Sim.Tick"))
            foreach (var guy in Guys) guy.Tick();

        using (Prof.Sample("Comp.Tick"))
            foreach (var comp in _tickingComponents) comp.Tick();

        Stats.Gauge("guys", Guys.Count);
        Stats.Gauge("loose items", _looseItems.Count);
        Stats.Gauge("frames", _frames.Count);
        Stats.Gauge("buildings", Buildings.Count);
    }

    // ---------- terrain ----------

    /// <summary>Swaps a cell's terrain, refreshes pathing, and raises TerrainChanged so the view repaints.</summary>
    /// <param name="cell">The cell to change.</param>
    /// <param name="def">The new terrain def.</param>
    public void SetTerrain(Vector2I cell, TerrainDef def)
    {
        Terrain[cell.X, cell.Y] = def;
        Game.Pathing.RefreshCell(this, cell);
        TerrainChanged?.Invoke(cell);
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

        // one stack per cell
        if (ItemAt(cell) != null) return (null, false, count);

        int initial = Mathf.Min(count, def.MaxStackSize);
        var item = new Item { Def = def, Cell = cell, Count = initial };
        _looseItems.Add(item);
        IndexItem(item);
        ItemSpawned?.Invoke(item);
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
        ItemRemoved?.Invoke(item);
    }

    /// <summary>Returns the item at a cell, or null if none. Optionally filters to one def.</summary>
    public Item ItemAt(Vector2I cell, ItemDef def = null)
    {
        if (!_itemsByCell.TryGetValue(cell, out var list)) return null;
        if (def == null) return list.Count > 0 ? list[0] : null;
        return list.Find(i => i.Def == def);
    }

    /// <summary>Total count of an item def in loose piles across the map.</summary>
    public int CountStored(ItemDef def)
    {
        int total = 0;
        foreach (var item in _looseItems)
            if (item.Def == def) total += item.Count;
        return total;
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

    /// <summary>A free cell beside the given one to drop a yield onto (preferring the cell below),
    /// so it isn't hidden under a plant sprite. Falls back to the cell itself if nothing's free.</summary>
    public Vector2I FreeDropCell(Vector2I cell)
    {
        bool Free(Vector2I cell) => InBounds(cell) && Terrain[cell.X, cell.Y].Walkable && !BlocksMovementAt(cell) && !HasPlant(cell);

        if (Free(cell + Vector2I.Down)) return cell + Vector2I.Down;

        foreach (var d in Grid.Adjacent8)
        {
            if (Free(cell + d)) return cell + d;
        }
        return cell;
    }

    // ---------- frames ----------

    /// <summary>Places a construction frame on the map. Assumes the cell has no frame already (callers check HasFrame).</summary>
    public void AddFrame(Frame frame)
    {
        _frames.Add(frame);
        foreach (var c in frame.OccupiedCells) _frameByCell[c] = frame;
        FrameAdded?.Invoke(frame);
    }

    /// <summary>Removes a construction frame (cancelled or finished).</summary>
    public void RemoveFrame(Frame frame)
    {
        _frames.Remove(frame);
        foreach (var c in frame.OccupiedCells) _frameByCell.Remove(c);
        FrameRemoved?.Invoke(frame);
    }

    /// <summary>Returns the construction frame at a cell, or null if none.</summary>
    public Frame FrameAt(Vector2I cell) => _frameByCell.GetValueOrDefault(cell);

    /// <summary>True if a construction frame already occupies the cell.</summary>
    public bool HasFrame(Vector2I cell) => _frameByCell.ContainsKey(cell);

    // ---------- buildings ----------

    /// <summary>Returns the building at a cell, or null if none.</summary>
    public Building BuildingAt(Vector2I cell) => Buildings.GetValueOrDefault(cell);

    /// <summary>True if a movement-blocking building occupies the cell.</summary>
    public bool BlocksMovementAt(Vector2I cell)
    {
        if (Buildings.TryGetValue(cell, out var b) && b.Def.BlocksMovement) return true;
        if (Plants.TryGetValue(cell, out var p) && p.Def.BlocksMovement) return true;
        return false;
    }

    /// <summary>Spawns a finished building at a cell and returns it.</summary>
    /// <param name="def">The building definition.</param>
    /// <param name="cell">The cell to place it on.</param>
    public Building SpawnBuilding(BuildingDef def, Vector2I cell, int rotation = 0)
    {
        var building = new Building { Def = def, Cell = cell, Rotation = rotation };
        building.InitComponents();
        foreach (var comp in building.Components)
            if (comp.Ticks) _tickingComponents.Add(comp);
        foreach (var c in building.OccupiedCells) Buildings[c] = building;
        BuildingSpawned?.Invoke(building);
        return building;
    }

    // ---------- plants ----------

    /// <summary>Spawns a plant at a cell (rolling its draw size) and returns it.</summary>
    /// <param name="drawWidth">Optional draw-width override (e.g. a stump inheriting a felled tree's girth), applied before the view is made.</param>
    public Plant SpawnPlant(PlantDef def, Vector2I cell, bool sown = false, float? drawWidth = null)
    {
        var plant = Plant.Spawn(def, cell);
        if (drawWidth.HasValue) plant.DrawWidth = drawWidth.Value;
        if (sown) plant.StartGrowing(def.GrowDays);
        Plants[cell] = plant;
        PlantSpawned?.Invoke(plant);
        return plant;
    }

    /// <summary>Removes a plant (harvested or cleared).</summary>
    public void RemovePlant(Plant plant)
    {
        Plants.Remove(plant.Cell);
        PlantRemoved?.Invoke(plant);
    }

    /// <summary>Returns the plant at a cell, or null if none.</summary>
    public Plant PlantAt(Vector2I cell) => Plants.GetValueOrDefault(cell);

    /// <summary>True if a plant occupies the cell.</summary>
    public bool HasPlant(Vector2I cell) => Plants.ContainsKey(cell);

    // ---------- guys ----------

    /// <summary>Adds a colonist to the map and raises GuyAdded so the view can spawn its nodes.</summary>
    /// <param name="guy">The guy to add.</param>
    public void AddGuy(Guy guy)
    {
        Guys.Add(guy);
        GuyAdded?.Invoke(guy);
    }
}