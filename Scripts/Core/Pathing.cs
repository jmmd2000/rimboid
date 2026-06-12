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
                _astar.SetPointSolid(cell, !terrain.Walkable);
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
    public bool IsReachable(Vector2I from, Vector2I to)
    {
        if (from == to) return true;
        var path = GetPath(from, to);
        return path != null && path.Length >= 2;
    }

    /// <summary>Updates a single cell's walkability and cost after terrain changes.</summary>
    /// <param name="map">The game map.</param>
    /// <param name="cell">The cell to refresh.</param>
    public void RefreshCell(GameMap map, Vector2I cell)
    {
        var terrain = map.Terrain[cell.X, cell.Y];
        _astar.SetPointSolid(cell, !terrain.Walkable);
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
}