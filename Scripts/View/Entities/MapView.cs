using System.Collections.Generic;
using Godot;

/// <summary>Paints terrain and overlay tile layers from map data.</summary>
public partial class MapView : Node2D
{
    [Export] public TileMapLayer TerrainLayer;
    [Export] public TileMapLayer OverlayLayer;
    [Export] public TileMapLayer DesignationLayer;

    static readonly Vector2I StockpileAtlas = new(0, 0);
    static readonly Vector2I GrowZoneAtlas = new(0, 0);

    GameMap _map;

    // deconstruct marks are sprites centred on the building footprint, keyed by its origin cell
    readonly Dictionary<Vector2I, Sprite2D> _deconstructMarkers = new();

    /// <summary>Subscribes the view to the map's terrain and designation events. Call once, before the map is populated.</summary>
    /// <param name="map">The game map to observe.</param>
    public void Bind(GameMap map)
    {
        _map = map;
        map.TerrainChanged += PaintCell;
        map.Designations.Added += MarkDesignation;
        map.Designations.Removed += ClearDesignation;
    }

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
        // a building's mark is a sprite centred on its footprint, not a cell-locked tile
        if (type == DesignationType.Deconstruct) { MarkDeconstruct(cell); return; }

        var atlas = type switch
        {
            DesignationType.Mine => new Vector2I(0, 0),
            DesignationType.Harvest => new Vector2I(1, 0),
            DesignationType.Chop => new Vector2I(2, 0),
            _ => new Vector2I(0, 2)
        };
        DesignationLayer.SetCell(cell, 0, atlas);
    }

    /// <summary>Removes the designation overlay from a cell.</summary>
    /// <param name="cell">The cell to clear.</param>
    public void ClearDesignation(Vector2I cell)
    {
        if (_deconstructMarkers.TryGetValue(cell, out var marker))
        {
            marker.QueueFree();
            _deconstructMarkers.Remove(cell);
            return;
        }
        DesignationLayer.EraseCell(cell);
    }

    /// <summary>Marks a building for deconstruction with a sprite centred on its footprint, so a
    /// multi-cell building is marked in its middle rather than on its origin cell.</summary>
    void MarkDeconstruct(Vector2I origin)
    {
        var building = _map.BuildingAt(origin);
        if (building == null) return;

        var marker = new Sprite2D
        {
            Texture = ToolDefOf.Deconstruct.Icon,
            Position = building.FootprintCentre * Game.TileSize,
        };
        DesignationLayer.AddChild(marker);
        _deconstructMarkers[origin] = marker;
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

    /// <summary>Places the grow-zone overlay on a cell.</summary>
    /// <param name="cell">The cell to mark.</param>
    public void MarkGrowZone(Vector2I cell)
    {
        OverlayLayer.SetCell(cell, 1, GrowZoneAtlas);
    }

    /// <summary>Removes the grow-zone overlay from a cell.</summary>
    /// <param name="cell">The cell to clear.</param>
    public void ClearGrowZone(Vector2I cell)
    {
        OverlayLayer.EraseCell(cell);
    }
}