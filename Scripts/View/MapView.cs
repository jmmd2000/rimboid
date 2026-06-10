using Godot;

public partial class MapView : Node2D
{
    [Export] public TileMapLayer TerrainLayer;

    GameMap _map;

    public void PaintAll(GameMap map)
    {
        _map = map;
        for (int x = 0; x < map.Width; x++)
            for (int y = 0; y < map.Height; y++)
                PaintCell(new Vector2I(x, y));
    }

    void PaintCell(Vector2I c)
    {
        var terrain = _map.Terrain[c.X, c.Y];
        Vector2I atlas = terrain.DefName switch
        {
            "Water" => new Vector2I(0, 0),
            "Stone" => new Vector2I(1, 0),
            "Grass" => new Vector2I(2, 0),
            "Dirt" => new Vector2I(3, 0),
            _ => new Vector2I(0, 0)
        };

        TerrainLayer.SetCell(c, 0, atlas);
    }
}