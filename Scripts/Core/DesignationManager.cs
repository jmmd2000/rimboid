using System;
using System.Collections.Generic;
using Godot;

/// <summary>Types of player-placed work designations.</summary>
public enum DesignationType { Mine, Harvest, Chop, Deconstruct };

/// <summary>Tracks active designations on the map. Add, remove, query by type and cell.</summary>
public class DesignationManager
{
    // designated cells indexed by type: O(1) Has/Add/Remove, and CellsOfType hands back the set directly
    readonly Dictionary<DesignationType, HashSet<Vector2I>> _byType = new();

    /// <summary>Raised when a designation is placed, so the view can mark the overlay.</summary>
    public event Action<DesignationType, Vector2I> Added;

    /// <summary>Raised when a designation is cleared at a cell, so the view can erase the overlay.</summary>
    public event Action<Vector2I> Removed;

    /// <summary>The cell set for a designation type, created on first use.</summary>
    HashSet<Vector2I> CellsFor(DesignationType type)
    {
        if (!_byType.TryGetValue(type, out var set)) _byType[type] = set = new HashSet<Vector2I>();
        return set;
    }

    /// <summary>Adds a designation if one doesn't already exist at that cell.</summary>
    /// <param name="type">The designation type.</param>
    /// <param name="cell">The map cell to designate.</param>
    public void Add(DesignationType type, Vector2I cell)
    {
        if (CellsFor(type).Add(cell)) Added?.Invoke(type, cell); // HashSet.Add is false if already present
    }

    /// <summary>Removes the designation of the given type at the cell.</summary>
    /// <param name="type">The designation type.</param>
    /// <param name="cell">The map cell to clear.</param>
    public void Remove(DesignationType type, Vector2I cell)
    {
        if (_byType.TryGetValue(type, out var set) && set.Remove(cell)) Removed?.Invoke(cell);
    }

    /// <summary>Checks whether a designation of the given type exists at the cell.</summary>
    /// <param name="type">The designation type.</param>
    /// <param name="cell">The map cell to check.</param>
    /// <returns>True if a matching designation exists.</returns>
    public bool Has(DesignationType type, Vector2I cell) => _byType.TryGetValue(type, out var set) && set.Contains(cell);

    /// <summary>Returns all cells that have a designation of the given type.</summary>
    /// <param name="type">The designation type to filter by.</param>
    /// <returns>The designated cells (empty if none).</returns>
    public IEnumerable<Vector2I> CellsOfType(DesignationType type) => _byType.TryGetValue(type, out var set) ? set : Array.Empty<Vector2I>();

    /// <summary>True if any cardinal neighbour of the cell is walkable floor.</summary>
    static bool HasWalkableNeighbour(GameMap map, Vector2I cell)
    {
        foreach (var direction in Grid.Cardinal4)
        {
            var n = cell + direction;
            if (map.InBounds(n) && map.Terrain[n.X, n.Y].Walkable) return true;
        }
        return false;
    }

    /// <summary>
    /// Flood-fills the set of mine designations that can eventually be reached.
    /// those touching floor now, plus any chained to them through other designations.
    /// </summary>
    HashSet<Vector2I> ReachableMines(GameMap map)
    {
        var designated = new HashSet<Vector2I>(CellsOfType(DesignationType.Mine));
        var reachable = new HashSet<Vector2I>();
        var frontier = new Queue<Vector2I>();

        // seed with designations already touching walkable spaces
        foreach (var cell in designated)
        {
            if (HasWalkableNeighbour(map, cell))
            {
                reachable.Add(cell);
                frontier.Enqueue(cell);
            }
        }

        // a designation next to a reachable one is exposed once that one is mined
        while (frontier.Count > 0)
        {
            var cell = frontier.Dequeue();
            foreach (var direction in Grid.Cardinal4)
            {
                var n = cell + direction;
                // Add returns false if already in
                if (designated.Contains(n) && reachable.Add(n)) frontier.Enqueue(n);
            }
        }
        return reachable;
    }

    /// <summary>True if a mine designation here would be reachable (touches floor, or chains to a reachable designation).</summary>
    /// <param name="map">The map, for terrain walkability.</param>
    /// <param name="cell">The candidate cell.</param>
    public bool WouldBeReachable(GameMap map, Vector2I cell)
    {
        if (HasWalkableNeighbour(map, cell)) return true;

        var reachable = ReachableMines(map);
        foreach (var direction in Grid.Cardinal4)
        {
            if (reachable.Contains(cell + direction)) return true;
        }
        return false;
    }

    /// <summary>Removes mine designations no longer reachable. Returns the cells removed.</summary>
    /// <param name="map">The map, for terrain walkability.</param>
    public List<Vector2I> PruneUnreachable(GameMap map)
    {
        var reachable = ReachableMines(map);
        var removed = new List<Vector2I>();

        CellsFor(DesignationType.Mine).RemoveWhere(cell =>
        {
            if (reachable.Contains(cell)) return false;
            removed.Add(cell);
            return true;
        });

        foreach (var cell in removed) Removed?.Invoke(cell);
        return removed;
    }
}