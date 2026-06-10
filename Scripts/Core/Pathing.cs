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
}