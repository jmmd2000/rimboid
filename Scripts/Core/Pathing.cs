using Godot;

public class Pathing
{
    AStarGrid2D _astar = new();

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

    public Vector2[] GetPath(Vector2I from, Vector2I to)
    {
        if (_astar.IsPointSolid(to)) return null;
        return _astar.GetPointPath(from, to);
    }

    public void RefreshCell(GameMap map, Vector2I cell)
    {
        var terrain = map.Terrain[cell.X, cell.Y];
        _astar.SetPointSolid(cell, !terrain.Walkable);
        _astar.SetPointWeightScale(cell, terrain.PathCostMultiplier);
    }

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

    bool InBounds(Vector2I cell)
    {
        var r = _astar.Region;
        return cell.X >= r.Position.X && cell.X < r.Position.X + r.Size.X
            && cell.Y >= r.Position.Y && cell.Y < r.Position.Y + r.Size.Y;
    }
}