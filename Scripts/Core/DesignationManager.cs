using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>Types of player-placed work designations.</summary>
public enum DesignationType { Mine };

/// <summary>Tracks active designations on the map. Add, remove, query by type and cell.</summary>
public class DesignationManager
{
    readonly List<Designation> _list = new();

    /// <summary>Adds a designation if one doesn't already exist at that cell.</summary>
    /// <param name="type">The designation type.</param>
    /// <param name="cell">The map cell to designate.</param>
    public void Add(DesignationType type, Vector2I cell)
    {
        if (Has(type, cell)) return;
        _list.Add(new Designation { Type = type, Cell = cell });
    }

    /// <summary>Removes all designations of the given type at the cell.</summary>
    /// <param name="type">The designation type.</param>
    /// <param name="cell">The map cell to clear.</param>
    public void Remove(DesignationType type, Vector2I cell) => _list.RemoveAll(d => d.Type == type && d.Cell == cell);

    /// <summary>Checks whether a designation of the given type exists at the cell.</summary>
    /// <param name="type">The designation type.</param>
    /// <param name="cell">The map cell to check.</param>
    /// <returns>True if a matching designation exists.</returns>
    public bool Has(DesignationType type, Vector2I cell) => _list.Any(d => d.Type == type && d.Cell == cell);

    /// <summary>Returns all cells that have a designation of the given type.</summary>
    /// <param name="type">The designation type to filter by.</param>
    /// <returns>Enumerable of designated cells.</returns>
    public IEnumerable<Vector2I> CellsOfType(DesignationType type) => _list.Where(d => d.Type == type).Select(d => d.Cell);

    static readonly Vector2I[] Cardinals = { Vector2I.Up, Vector2I.Down, Vector2I.Left, Vector2I.Right };

    /// <summary>True if any cardinal neighbour of the cell is walkable floor.</summary>
    static bool HasWalkableNeighbour(GameMap map, Vector2I cell)
    {
        foreach (var direction in Cardinals)
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
            foreach (var direction in Cardinals)
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
        foreach (var direction in Cardinals)
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

        _list.RemoveAll(d =>
        {
            if (d.Type != DesignationType.Mine || reachable.Contains(d.Cell)) return false;
            removed.Add(d.Cell);
            return true;
        });

        return removed;
    }
}

/// <summary>A single designation entry: a type paired with a map cell.</summary>
public class Designation
{
    public DesignationType Type;
    public Vector2I Cell;
}