using System.Collections.Generic;
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
    TickManager _tick;
    bool _mineMode;
    bool _stockpileMode;
    Stockpile _stockpile;
    readonly Dictionary<Item, ItemView> _itemViews = new();


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
        _stockpile = Game.Map.Stockpiles.Create();

        _guy = new Guy { Position = FindWalkableCell() };

        _guyView = new GuyView();
        _guyView.Texture = GD.Load<Texture2D>("res://Assets/guy.png");
        _guyView.Init(_guy, 16);
        AddChild(_guyView);

        var pathLine = new PathLine();
        pathLine.Init(_guy, 16);
        AddChild(pathLine);

        GameTime.Reset();
        _tick = new TickManager();
        _tick.Tick += _guy.Tick;
        AddChild(_tick);

        var timeBar = new TimeControlBar();
        timeBar.Init(_tick);
        AddChild(timeBar);
    }

    public override void _UnhandledInput(InputEvent e)
    {
        // toggle mine mode with M
        if (e is InputEventKey key && key.Pressed && key.Keycode == Key.M)
        {
            _mineMode = !_mineMode;
            if (_mineMode)
            {
                _stockpileMode = false;
                GD.Print("Stockpile mode OFF");
            }
            GD.Print(_mineMode ? "Mine mode ON" : "Mine mode OFF");
        }

        // toggle stockpile mode with S
        if (e is InputEventKey sKey && sKey.Pressed && sKey.Keycode == Key.S)
        {
            _stockpileMode = !_stockpileMode;
            if (_stockpileMode)
            {
                _mineMode = false;
                GD.Print("Mining mode OFF");
            }
            GD.Print(_stockpileMode ? "Stockpile mode ON" : "Stockpile mode OFF");
        }

        // time controls
        if (e is InputEventKey tKey && tKey.Pressed && !tKey.Echo)
        {
            switch (tKey.Keycode)
            {
                case Key.Space: _tick.TogglePause(); break;
                case Key.Key1: _tick.SetSpeed(1); break;
                case Key.Key2: _tick.SetSpeed(3); break;
                case Key.Key3: _tick.SetSpeed(6); break;
                case Key.Period: if (_tick.SpeedMultiplier == 0) _tick.DoSingleTick(); break;
            }
        }

        // add mining designation on left click
        if (e is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
        {
            Vector2I cell = CellUnderMouse();
            if (!_map.InBounds(cell)) return;

            if (_mineMode)
            {
                if (_map.Terrain[cell.X, cell.Y] == TerrainDefOf.Stone && _pathing.NearestWalkableNeighbour(cell, _guy.Cell) != null)
                {
                    _map.Designations.Add(DesignationType.Mine, cell);
                    GD.Print($"Designated mine at {cell}");
                    MapView.MarkDesignation(cell);
                }
            }
            else if (!_stockpileMode)
            {
                var path = _pathing.GetPath(_guy.Cell, cell);
                if (path != null)
                    _guy.StartPath(path);
            }
        }

        // clear mining designation on right click
        if (_mineMode && e is InputEventMouseButton rmb && rmb.Pressed && rmb.ButtonIndex == MouseButton.Right)
        {
            Vector2I cell = CellUnderMouse();
            if (_map.Designations.Has(DesignationType.Mine, cell))
            {
                _map.Designations.Remove(DesignationType.Mine, cell);
                MapView.ClearDesignation(cell);
                GD.Print($"Cancelled mine at {cell}");
            }
        }

        if (_stockpileMode && e is InputEventMouseButton smb && smb.Pressed)
        {
            Vector2I cell = CellUnderMouse();
            if (!_map.InBounds(cell)) return;

            if (smb.ButtonIndex == MouseButton.Left && _map.Terrain[cell.X, cell.Y].Walkable)
            {
                _stockpile.Cells.Add(cell);
                MapView.MarkStockpile(cell);
                GD.Print($"Stockpile cell added at {cell}");
            }
            else if (smb.ButtonIndex == MouseButton.Right && _stockpile.Cells.Contains(cell))
            {
                _stockpile.Cells.Remove(cell);
                MapView.ClearStockpile(cell);
                GD.Print($"Stockpile cell removed at {cell}");
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

    Vector2I CellUnderMouse() => TerrainLayer.LocalToMap(TerrainLayer.ToLocal(GetGlobalMousePosition()));

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
        _itemViews[item] = view;
    }

    /// <summary>Removes the visual node for an item that's been picked up.</summary>
    /// <param name="item">The item whose view to remove.</param>
    public void RemoveItemView(Item item)
    {
        if (_itemViews.TryGetValue(item, out var view))
        {
            view.QueueFree();
            _itemViews.Remove(item);
        }
    }
}
