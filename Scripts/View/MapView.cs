using Godot;

/// <summary>Paints terrain and overlay tile layers from map data.</summary>
public partial class MapView : Node2D
{
    [Export] public TileMapLayer TerrainLayer;
    [Export] public TileMapLayer OverlayLayer;
    [Export] public TileMapLayer DesignationLayer;

    static readonly Vector2I StockpileAtlas = new(5, 0);

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

    /// <summary>Repaints a single terrain cell from its def's tile.</summary>
    /// <param name="c">The cell to repaint.</param>
    public void PaintCell(Vector2I c)
    {
        var terrain = _map.Terrain[c.X, c.Y];

        TerrainLayer.SetCell(c, 0, terrain.AtlasCoords);
    }

    /// <summary>Places the designation overlay icon on a cell.</summary>
    /// <param name="cell">The cell to mark.</param>
    public void MarkDesignation(DesignationType type, Vector2I cell)
    {
        var atlas = type switch
        {
            DesignationType.Harvest => new Vector2I(6, 0),
            DesignationType.Mine => new Vector2I(4, 0),
            _ => new Vector2I(4, 0)
        };
        DesignationLayer.SetCell(cell, 0, atlas);
    }

    /// <summary>Removes the designation overlay from a cell.</summary>
    /// <param name="cell">The cell to clear.</param>
    public void ClearDesignation(Vector2I cell)
    {
        DesignationLayer.EraseCell(cell);
    }

    /// <summary>Places the stockpile overlay on a cell. </summary>
    /// <param name="cell">The cell to mark.</param>
    public void MarkStockpile(Vector2I cell)
    {
        OverlayLayer.SetCell(cell, 0, StockpileAtlas);
    }

    /// <summary>Removes the stockpile overlay from a cell.</summary>
    /// <param name="cell">The cell to clear.</param>
    public void ClearStockpile(Vector2I cell)
    {
        OverlayLayer.EraseCell(cell);
    }
}