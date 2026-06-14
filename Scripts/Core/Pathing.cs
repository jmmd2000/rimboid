using System.Collections.Generic;
using System.Security.Cryptography;
using Godot;

/// <summary>Wrapper around AStarGrid2D for map pathfinding.</summary>
public class Pathing
{
    AStarGrid2D _astar = new();

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
    }

    /// <summary>Finds the walkable cardinal neighbour of a cell closest to the given origin.</summary>
    /// <param name="cell">The target cell to find a neighbour of.</param>
    /// <param name="from">The origin to measure distance from.</param>
    /// <returns>The nearest walkable neighbour, or null if none exist.</returns>
    public Vector2I? NearestWalkableNeighbour(Vector2I cell, Vector2I from)
    {
        Vector2I[] neighbours =
        {
            cell + Vector2I.Up,
            cell + Vector2I.Down,
            cell + Vector2I.Left,
            cell + Vector2I.Right,
        };

        Vector2I? best = null;
        float bestDist = float.MaxValue;
        foreach (var n in neighbours)
        {
            if (!InBounds(n) || _astar.IsPointSolid(n)) continue;
            float dist = from.DistanceTo(n);
            if (dist < bestDist)
            {
                best = n;
                bestDist = dist;
            }
        }
        return best;
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

    bool Passable(Vector2I cell, Vector2I wall) => InBounds(cell) && cell != wall && !_astar.IsPointSolid(cell);

    bool IsBorder(Vector2I cell)
    {
        var region = _astar.Region;
        return cell.X == region.Position.X
            || cell.Y == region.Position.Y
            || cell.X == region.Position.X + region.Size.X - 1
            || cell.Y == region.Position.Y + region.Size.Y - 1;
    }
}