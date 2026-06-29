using System.Collections.Generic;
using Godot;

/// <summary>Wrapper around AStarGrid2D for map pathfinding.</summary>
public class Pathing
{
    AStarGrid2D _astar = new();
    bool[,] _solid;
    int _width, _height;

    // one cached set per connected region queried since the last grid change
    readonly List<HashSet<Vector2I>> _regions = new();
    bool _regionsValid;
    static readonly HashSet<Vector2I> _empty = new();

    /// <summary>Builds the pathfinding grid from the map's terrain data.</summary>
    /// <param name="map">The game map to read terrain from.</param>
    public void Init(GameMap map)
    {
        _width = map.Width;
        _height = map.Height;
        _solid = new bool[map.Width, map.Height];
        _astar.Region = new Rect2I(0, 0, map.Width, map.Height);
        _astar.DiagonalMode = AStarGrid2D.DiagonalModeEnum.OnlyIfNoObstacles;
        _astar.Update();

        for (int x = 0; x < map.Width; x++)
            for (int y = 0; y < map.Height; y++)
            {
                var terrain = map.Terrain[x, y];
                var cell = new Vector2I(x, y);
                bool solid = !terrain.Walkable || map.BlocksMovementAt(cell);
                _solid[x, y] = solid;
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
        using var _ = Prof.Sample("Pathing.GetPath");
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
        if (!_regionsValid) { _regions.Clear(); _regionsValid = true; }

        foreach (var region in _regions)
            if (region.Contains(from)) return region;   // cache hit, no flood

        if (!Free(from)) return _empty;
        var flooded = Flood(from);
        _regions.Add(flooded);
        return flooded;
    }

    /// <summary>Flood-fills the connected region of walkable cells containing 'from'.</summary>
    HashSet<Vector2I> Flood(Vector2I from)
    {
        using var _ = Prof.Sample("Pathing.Flood");
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
                if (cells.Contains(n) || !CanStep(c, n, d)) continue;
                cells.Add(n);
                queue.Enqueue(n);
            }
        }
        return cells;
    }

    /// <summary>Marks all cached regions stale, they re-flood on next use.</summary>
    void Invalidate() => _regionsValid = false;

    bool Free(Vector2I cell) => InBounds(cell) && !_solid[cell.X, cell.Y];

    /// <summary>Can a flood step from c to neighbour n (direction d)? Free, and no cutting a diagonal corner.</summary>
    bool CanStep(Vector2I c, Vector2I n, Vector2I d)
    {
        if (!Free(n)) return false;
        if (d.X != 0 && d.Y != 0 && (!Free(new Vector2I(n.X, c.Y)) || !Free(new Vector2I(c.X, n.Y)))) return false;
        return true;
    }

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
        _solid[cell.X, cell.Y] = solid;
        _astar.SetPointSolid(cell, solid);
        _astar.SetPointWeightScale(cell, terrain.PathCostMultiplier);

        Invalidate(); // grid changed, regions stale
    }

    /// <summary>Checks whether a cell is within the pathfinding grid bounds.</summary>
    bool InBounds(Vector2I cell) => cell.X >= 0 && cell.X < _width && cell.Y >= 0 && cell.Y < _height;

    /// <summary>
    /// Nearest of a cell's 8 neighbours that is walkable AND reachable from the origin,
    /// the cell a guy stands on to work it. Diagonals included, so corner/pocket targets
    /// stay workable. Returns null when the cell is sealed off.
    /// </summary>
    public Vector2I? NearestReachableWorkCell(Vector2I cell, Vector2I from)
    {
        var reachable = ReachableCells(from);
        Vector2I? best = null;
        float bestDist = float.MaxValue;
        foreach (var d in Grid.Adjacent8)
        {
            var n = cell + d;
            if (!reachable.Contains(n)) continue;
            float dist = from.DistanceTo(n);
            if (dist < bestDist) { best = n; bestDist = dist; }
        }
        return best;
    }

    /// <summary>
    /// Like NearestReachableWorkCell, but also rejects any cell that building frameCell
    /// would seal into an enclosed pocket, so the builder can't wall himself in.
    /// </summary>
    public Vector2I? NearestSafeWorkCell(Vector2I frameCell, Vector2I from)
    {
        var reachable = ReachableCells(from);
        Vector2I? best = null;
        float bestDist = float.MaxValue;
        foreach (var d in Grid.Adjacent8)
        {
            var n = frameCell + d;
            if (!reachable.Contains(n)) continue;
            if (WouldTrap(n, frameCell)) continue;
            float dist = from.DistanceTo(n);
            if (dist < bestDist) { best = n; bestDist = dist; }
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
        const int openEnough = 400;
        var visited = new HashSet<Vector2I> { start };
        var queue = new Queue<Vector2I>();
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            if (visited.Count > openEnough) return false;
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
        return cell.X == 0 || cell.Y == 0 || cell.X == _width - 1 || cell.Y == _height - 1;
    }
}