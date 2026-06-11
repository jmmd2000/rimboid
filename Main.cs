using Godot;

/// <summary>
/// Main node. Connects world gen, pathing, colonists, player input.
/// </summary>
public partial class Main : Node2D
{
    [Export] public MapView MapView;
    [Export] public TileMapLayer TerrainLayer;

    [ExportGroup("World Gen")]
    [Export] public int Seed = 12345;
    [Export] public int MapWidth = 256;
    [Export] public int MapHeight = 256;

    [ExportGroup("Elevation Noise")]
    [Export] public float ElevationFrequency = 0.01f;
    [Export] public int ElevationOctaves = 3;

    [ExportGroup("Moisture Noise")]
    [Export] public float MoistureFrequency = 0.01f;
    [Export] public int MoistureOctaves = 3;

    [ExportGroup("Terrain Thresholds")]
    // How low the terrain elevation must be to allow for water, if wet enough
    [Export] public float WaterElevationThreshold = -0.1f;
    // How wet the low ground needs to be to be water
    [Export] public float WaterMoistureThreshold = 0.2f;
    // How high the terrain elevation must be to become stone
    [Export] public float StoneElevationThreshold = 0.3f;
    // How wet the midground must be to be grass
    [Export] public float GrassMoistureThreshold = -0.1f;

    GameMap _map;
    Guy _guy;
    Pathing _pathing;
    GuyView _guyView;
    bool _mineMode;


    public override void _Ready()
    {
        _map = new GameMap(MapWidth, MapHeight);
        ItemDefOf.Load();
        WorldGenerator.Generate(_map, this);
        MapView.PaintAll(_map);

        _pathing = new Pathing();
        _pathing.Init(_map);

        Game.Map = _map;
        Game.Pathing = _pathing;
        Game.MapView = MapView;
        Game.Main = this;

        _guy = new Guy { Position = FindWalkableCell() };

        _guyView = new GuyView();
        _guyView.Texture = GD.Load<Texture2D>("res://Assets/guy.png");
        _guyView.Init(_guy, 16);
        AddChild(_guyView);

        var pathLine = new PathLine();
        pathLine.Init(_guy, 16);
        AddChild(pathLine);
    }

    public override void _Process(double delta)
    {
        _guy.Tick();
    }

    public override void _UnhandledInput(InputEvent e)
    {
        // toggle mine mode with M
        if (e is InputEventKey key && key.Pressed && key.Keycode == Key.M)
        {
            _mineMode = !_mineMode;
            GD.Print(_mineMode ? "Mine mode ON" : "Mine mode OFF");
        }

        // add mining designation on left click
        if (e is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
        {
            Vector2I cell = TerrainLayer.LocalToMap(TerrainLayer.ToLocal(GetGlobalMousePosition()));

            if (_mineMode)
            {
                if (_map.Terrain[cell.X, cell.Y] == TerrainDefOf.Stone && _pathing.NearestWalkableNeighbour(cell, _guy.Cell) != null)
                {
                    _map.Designations.Add(DesignationType.Mine, cell);
                    GD.Print($"Designated mine at {cell}");
                    MapView.MarkDesignation(cell);
                }
            }
            else
            {
                var path = _pathing.GetPath(_guy.Cell, cell);
                if (path != null)
                    _guy.StartPath(path);
            }
        }

        if (_mineMode && e is InputEventMouseButton rmb && rmb.Pressed && rmb.ButtonIndex == MouseButton.Right)
        {
            Vector2I cell = TerrainLayer.LocalToMap(TerrainLayer.ToLocal(GetGlobalMousePosition()));
            if (_map.Designations.Has(DesignationType.Mine, cell))
            {
                _map.Designations.Remove(DesignationType.Mine, cell);
                MapView.ClearDesignation(cell);
                GD.Print($"Cancelled mine at {cell}");
            }
        }

    }

    /// <summary>
    /// Finds the first walkable cell on the map for the initial colonist placement.
    /// </summary>
    /// <returns>The first walkable cell</returns>
    Vector2 FindWalkableCell()
    {
        for (int x = 0; x < _map.Width; x++)
            for (int y = 0; y < _map.Height; y++)
                if (_map.Terrain[x, y].Walkable)
                    return new Vector2(x, y);
        return Vector2.Zero;
    }

    /// <summary>
    /// Creates a visual node for a loose item on the map.
    /// </summary>
    /// <param name="item">The runtime item instance to create a view for.</param>
    public void SpawnItemView(Item item)
    {
        var view = new ItemView();
        view.Texture = GD.Load<Texture2D>("res://Assets/stone.png");
        view.Init(item, 16);
        AddChild(view);
    }
}
