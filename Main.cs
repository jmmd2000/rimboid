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
    Vector2I? _dragStart;
    MouseButton _dragButton;
    SelectionBox _selectionBox;


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

        var needsPanel = new NeedsPanel();
        needsPanel.Init(_guy);
        AddChild(needsPanel);

        var zzz = new SleepZZZ();
        zzz.Init(_guy, 16);
        AddChild(zzz);

        _selectionBox = new SelectionBox();
        _selectionBox.Init(16);
        AddChild(_selectionBox);

        // TEMP: spawn berries
        var (berries, _) = Game.Map.SpawnItem(ItemDefOf.Berries, new Vector2I(3, 3), 10);
        SpawnItemView(berries);
    }

    public override void _UnhandledInput(InputEvent e)
    {
        // toggle mine mode with M
        if (e is InputEventKey key && key.Pressed && key.Keycode == Key.M && !key.Echo)
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
        if (e is InputEventKey sKey && sKey.Pressed && sKey.Keycode == Key.S && !sKey.Echo)
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
                case Key.Key9: _tick.SetSpeed(25); break;
                case Key.Period: if (_tick.SpeedMultiplier == 0) _tick.DoSingleTick(); break;
            }
        }

        // drag select in mine or stockpile mode, otherwise left click walks
        if (_mineMode || _stockpileMode)
        {
            HandleDrag(e);
        }
        else if (e is InputEventMouseButton wmb && wmb.Pressed && wmb.ButtonIndex == MouseButton.Left)
        {
            Vector2I cell = CellUnderMouse();
            if (_map.InBounds(cell))
            {
                var path = _pathing.GetPath(_guy.Cell, cell);
                if (path != null)
                {
                    _guy.StartPath(path);
                }
            }
        }

    }

    /// <summary>Finds the first walkable cell on the map for the initial colonist placement.</summary>
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

    /// <summary>Creates a visual node for a loose item on the map.</summary>
    /// <param name="item">The runtime item instance to create a view for.</param>
    public void SpawnItemView(Item item)
    {
        var view = new ItemView();
        view.Texture = item.Def.Graphic;
        view.Init(item, 16);
        AddChild(view);
        _itemViews[item] = view;
    }

    /// <summary>Tracks a press/drag/release selection and updates the preview outline.</summary>
    /// <param name="e">The input event being handled.</param>
    void HandleDrag(InputEvent e)
    {
        if (e is InputEventMouseButton mb && (mb.ButtonIndex == MouseButton.Left || mb.ButtonIndex == MouseButton.Right))
        {
            Vector2I cell = CellUnderMouse();
            if (mb.Pressed)
            {
                _dragStart = _map.InBounds(cell) ? cell : (Vector2I?)null;
                _dragButton = mb.ButtonIndex;
                if (_dragStart != null) _selectionBox.SetSelection(cell, cell);
            }
            else if (_dragStart != null && mb.ButtonIndex == _dragButton)
            {
                if (_map.InBounds(cell))
                {
                    ApplyDrag(_dragStart.Value, cell, _dragButton);
                }
                _dragStart = null;
                _selectionBox.Clear();
            }
        }
        else if (e is InputEventMouseMotion && _dragStart != null)
        {
            Vector2I cell = CellUnderMouse();
            if (_map.InBounds(cell))
            {
                _selectionBox.SetSelection(_dragStart.Value, cell);
            }
        }
    }

    /// <summary>Applies a finished drag rectangle based on the active mode and button.</summary>
    /// <param name="a">The cell where the drag began.</param>
    /// <param name="b">The cell where the drag ended.</param>
    /// <param name="button">The mouse button used: left adds, right removes.</param>
    void ApplyDrag(Vector2I a, Vector2I b, MouseButton button)
    {
        if (_mineMode)
        {
            if (button == MouseButton.Left) DesignateMineRectangle(a, b);
            else CancelMineRectangle(a, b);
        }
        else // stockpile mode
        {
            if (button == MouseButton.Left) AddStockpileRectangle(a, b);
            else RemoveStockpileRectangle(a, b);
        }
    }

    /// <summary>Designates every reachable stone cell in the rectangle for mining.</summary>
    /// <param name="a">The cell where the drag began.</param>
    /// <param name="b">The cell where the drag ended.</param>
    void DesignateMineRectangle(Vector2I a, Vector2I b)
    {
        foreach (var cell in Grid.CellsInRect(a, b))
        {
            if (_map.Terrain[cell.X, cell.Y] == TerrainDefOf.Stone && !_map.Designations.Has(DesignationType.Mine, cell))
            {
                _map.Designations.Add(DesignationType.Mine, cell);
                MapView.MarkDesignation(cell);
            }
        }
        foreach (var orphan in _map.Designations.PruneUnreachable(_map))
        {
            MapView.MarkDesignation(orphan);
        }
    }

    /// <summary>Cancels every mine designation in the rectangle.</summary>
    /// <param name="a">The cell where the drag began.</param>
    /// <param name="b">The cell where the drag ended.</param>
    void CancelMineRectangle(Vector2I a, Vector2I b)
    {
        foreach (var cell in Grid.CellsInRect(a, b))
        {
            if (_map.Designations.Has(DesignationType.Mine, cell))
            {
                _map.Designations.Remove(DesignationType.Mine, cell);
                MapView.ClearDesignation(cell);
            }
        }

        // removing cells could strand others so prune the rest
        foreach (var orphan in _map.Designations.PruneUnreachable(_map))
        {
            MapView.ClearDesignation(orphan);
        }
    }

    /// <summary>Adds every walkable cell in the rectangle to the stockpile.</summary>
    /// <param name="a">The cell where the drag began.</param>
    /// <param name="b">The cell where the drag ended.</param>
    void AddStockpileRectangle(Vector2I a, Vector2I b)
    {
        foreach (var cell in Grid.CellsInRect(a, b))
        {
            if (_map.Terrain[cell.X, cell.Y].Walkable && _stockpile.Cells.Add(cell))
            {
                MapView.MarkStockpile(cell);
            }
        }
    }

    /// <summary>Removes every stockpile cell in the rectangle.</summary>
    /// <param name="a">The cell where the drag began.</param>
    /// <param name="b">The cell where the drag ended.</param>
    void RemoveStockpileRectangle(Vector2I a, Vector2I b)
    {
        foreach (var cell in Grid.CellsInRect(a, b))
        {
            if (_stockpile.Cells.Remove(cell))
            {
                MapView.ClearStockpile(cell);
            }
        }
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
