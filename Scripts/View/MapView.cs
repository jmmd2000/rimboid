using Godot;

/// <summary>Paints terrain and overlay tile layers from map data.</summary>
public partial class MapView : Node2D
{
    [Export] public TileMapLayer TerrainLayer;
    [Export] public TileMapLayer OverlayLayer;

    GameMap _map;

    /// <summary>Repaints every cell on the map.</summary>
    /// <param name="map">The game map to read terrain from.</param>
    public void PaintAll(GameMap map)
    {
        _map = map;
        for (int x = 0; x < map.Width; x++)
            for (int y = 0; y < map.Height; y++)
                PaintCell(new Vector2I(x, y));
    }

    /// <summary>Repaints a single terrain cell.</summary>
    /// <param name="c">The cell to repaint.</param>
    public void PaintCell(Vector2I c)
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

    /// <summary>Places the designation overlay icon on a cell.</summary>
    /// <param name="cell">The cell to mark.</param>
    public void MarkDesignation(Vector2I cell)
    {
        OverlayLayer.SetCell(cell, 0, new Vector2I(4, 0));
    }

    /// <summary>Removes the designation overlay from a cell.</summary>
    /// <param name="cell">The cell to clear.</param>
    public void ClearDesignation(Vector2I cell)
    {
        OverlayLayer.EraseCell(cell);
    }

    /// <summary>Places the stockpile overlay on a cell. </summary>
    /// <param name="cell">The cell to mark.</param>
    public void MarkStockpile(Vector2I cell)
    {
        OverlayLayer.SetCell(cell, 0, new Vector2I(5, 0));
    }

    /// <summary>Removes the stockpile overlay from a cell.</summary>
    /// <param name="cell">The cell to clear.</param>
    public void ClearStockpile(Vector2I cell)
    {
        OverlayLayer.EraseCell(cell);
    }
}