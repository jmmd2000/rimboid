using System.Collections.Generic;
using Godot;

/// <summary>Wrapper around AStarGrid2D for map pathfinding.</summary>
public class Pathing
{
    AStarGrid2D _astar = new();

    // every cell the colonist can currently walk to
    HashSet<Vector2I> _reachable = new();
    // false = grid changed, must re-flood
    bool _reachableValid;

    /// <summary>Builds the pathfinding grid from the map's terrain data.</summary>
    /// <param name="map">The game map to read terrain from.</param>
    public void Init(GameMap map)
    {
        _astar.Region = new Rect2I(0, 0, map.Width, map.Height);
        _astar.DiagonalMode = AStarGrid2D.DiagonalModeEnum.OnlyIfNoObstacles;
        _astar.Update();

        for (int x = 0; x < map.Width; x++)
            for (int y = 0; y < map.Height; y++)
            {
                var terrain = map.Terrain[x, y];
                var cell = new Vector2I(x, y);
                bool solid = !terrain.Walkable || map.BlocksMovementAt(cell);
                _astar.SetPointSolid(cell, solid);
                _astar.SetPointWeightScale(cell, terrain.PathCostMultiplier);
            }
        Invalidate();
    }

    /// <summary>Returns a path between two cells, or null if the target is solid.</summary>
    /// <param name="from">Start cell.</param>
    /// <param name="to">Destination cell.</param>
    /// <returns>Array of world positions, or null.</returns>
    public Vector2[] GetPath(Vector2I from, Vector2I to)
    {
        if (_astar.IsPointSolid(to)) return null;
        return _astar.GetPointPath(from, to);
    }

    /// <summary>
    /// All cells reachable from an origin, as a set you can test membership on in O(1).
    /// Cached: rebuilt only when the grid's walkability changes, reused otherwise.
    /// Treat the returned set as read-only.
    /// </summary>
    public HashSet<Vector2I> ReachableCells(Vector2I from)
    {
        if (_reachableValid && _reachable.Contains(from)) return _reachable;
        _reachable = Flood(from);
        _reachableValid = true;
        return _reachable;
    }

    /// <summary>Flood-fills the connected region of walkable cells containing 'from'.</summary>
    HashSet<Vector2I> Flood(Vector2I from)
    {
        var cells = new HashSet<Vector2I>();
        if (!Free(from)) return cells;
        cells.Add(from);
        var queue = new Queue<Vector2I>();
        queue.Enqueue(from);
        while (queue.Count > 0)
        {
            var c = queue.Dequeue();
            foreach (var d in Grid.Adjacent8)
            {
                var n = c + d;
                if (cells.Contains(n) || !Free(n)) continue;
                // no diagonal
                if (d.X != 0 && d.Y != 0 && (!Free(new Vector2I(n.X, c.Y)) || !Free(new Vector2I(c.X, n.Y)))) continue;
                cells.Add(n);
                queue.Enqueue(n);
            }
        }
        return cells;
    }

    /// <summary>Marks the cached reachable set stale, it re-floods on next use.</summary>
    void Invalidate() => _reachableValid = false;

    bool Free(Vector2I cell) => InBounds(cell) && !_astar.IsPointSolid(cell);

    /// <summary>Returns true if a path exists from one cell to another.</summary>
    /// <param name="from">Start cell.</param>
    /// <param name="to">Destination cell.</param>
    public bool IsReachable(Vector2I from, Vector2I to) => PathSteps(from, to) >= 0;

    /// <summary>Number of path steps from one cell to another, or -1 if unreachable.</summary>
    public int PathSteps(Vector2I from, Vector2I to)
    {
        if (from == to) return 0;
        var path = GetPath(from, to);
        return path == null || path.Length < 2 ? -1 : path.Length;
    }

    /// <summary>Updates a single cell's walkability and cost after terrain changes.</summary>
    /// <param name="map">The game map.</param>
    /// <param name="cell">The cell to refresh.</param>
    public void RefreshCell(GameMap map, Vector2I cell)
    {
        var terrain = map.Terrain[cell.X, cell.Y];
        bool solid = !terrain.Walkable || map.BlocksMovementAt(cell);
        _astar.SetPointSolid(cell, solid);
        _astar.SetPointWeightScale(cell, terrain.PathCostMultiplier);
        Invalidate();
    }

    /// <summary>Checks whether a cell is within the pathfinding grid bounds.</summary>
    bool InBounds(Vector2I cell)
    {
        var r = _astar.Region;
        return cell.X >= r.Position.X && cell.X < r.Position.X + r.Size.X
            && cell.Y >= r.Position.Y && cell.Y < r.Position.Y + r.Size.Y;
    }

    /// <summary>
    /// Nearest of a cell's 8 neighbours that is walkable AND reachable from the origin,
    /// the cell a guy stands on to work it. Diagonals included, so corner/pocket targets
    /// stay workable. Returns null when the cell is sealed off.
    /// </summary>
    public Vector2I? NearestReachableWorkCell(Vector2I cell, Vector2I from)
    {
        Vector2I? best = null;
        float bestSteps = int.MaxValue;
        foreach (var d in Grid.Adjacent8)
        {
            var n = cell + d;
            if (!InBounds(n) || _astar.IsPointSolid(n)) continue;
            int steps = PathSteps(from, n);
            if (steps < 0) continue;
            if (steps < bestSteps) { best = n; bestSteps = steps; }
        }
        return best;
    }

    /// <summary>
    /// Like NearestReachableWorkCell, but also rejects any cell that building frameCell
    /// would seal into an enclosed pocket, so the builder can't wall himself in.
    /// </summary>
    public Vector2I? NearestSafeWorkCell(Vector2I frameCell, Vector2I from)
    {
        Vector2I? best = null;
        int bestSteps = int.MaxValue;
        foreach (var d in Grid.Adjacent8)
        {
            var n = frameCell + d;
            if (!InBounds(n) || _astar.IsPointSolid(n)) continue;
            if (WouldTrap(n, frameCell)) continue;
            int steps = PathSteps(from, n);
            if (steps < 0) continue;
            if (steps < bestSteps) { best = n; bestSteps = steps; }
        }
        return best;
    }

    /// <summary>
    /// Nearest of a cell's four cardinal neighbours that the origin can reach, where a Guy
    /// stands to mine. Mining is orthogonal only (no reaching across a diagonal corner), so a
    /// rock whose only open neighbours are diagonal is correctly skipped. Null if none reachable.
    /// </summary>
    public Vector2I? NearestReachableCardinal(Vector2I cell, Vector2I from)
    {
        var reachable = ReachableCells(from);
        Vector2I? best = null;
        float bestDist = float.MaxValue;
        foreach (var d in Grid.Cardinal4)
        {
            var n = cell + d;
            if (!reachable.Contains(n)) continue;
            float dist = from.DistanceTo(n);
            if (dist < bestDist) { best = n; bestDist = dist; }
        }
        return best;
    }

    /// <summary>True if, once wall is solid, start can no longer reach the map edge i.e. it's enclosed.</summary>
    bool WouldTrap(Vector2I start, Vector2I wall)
    {
        var visited = new HashSet<Vector2I> { start };
        var queue = new Queue<Vector2I>();
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var c = queue.Dequeue();
            if (IsBorder(c)) return false;
            foreach (var d in Grid.Adjacent8)
            {
                var n = c + d;
                if (visited.Contains(n) || !Passable(n, wall)) continue;
                // no diagonal corner cutting
                if (d.X != 0 && d.Y != 0 && (!Passable(new Vector2I(n.X, c.Y), wall) || !Passable(new Vector2I(c.X, n.Y), wall))) continue;
                visited.Add(n);
                queue.Enqueue(n);
            }
        }
        // never reached the edge therefor trapped
        return true;
    }

    bool Passable(Vector2I cell, Vector2I wall) => cell != wall && Free(cell);

    bool IsBorder(Vector2I cell)
    {
        var region = _astar.Region;
        return cell.X == region.Position.X
            || cell.Y == region.Position.Y
            || cell.X == region.Position.X + region.Size.X - 1
            || cell.Y == region.Position.Y + region.Size.Y - 1;
    }
}