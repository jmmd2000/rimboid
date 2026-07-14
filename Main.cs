using System;
using System.Collections.Generic;
using Godot;

/// <summary>Main node. Connects world gen, pathing, colonists, player input, ui.</summary>
public partial class Main : Node2D
{
    [Export] public MapView MapView;
    [Export] public TileMapLayer TerrainLayer;

    [ExportGroup("World Gen")]
    [Export] public int Seed = 12345;
    [Export] public int MapWidth = 250;
    [Export] public int MapHeight = 250;
    [Export] public int StartingGuys = 3;

    [ExportGroup("Elevation Noise")]
    [Export] public float ElevationFrequency = 0.01f;
    [Export] public int ElevationOctaves = 3;

    [ExportGroup("Terrain Thresholds")]
    // elevation bands, low to high: water -> shore dirt -> grass -> foothill dirt -> stone
    [Export] public float WaterElevationThreshold = -0.4f;
    [Export] public float ShoreElevationThreshold = -0.3f;
    [Export] public float FoothillElevationThreshold = 0.3f;
    [Export] public float StoneElevationThreshold = 0.45f;

    [ExportGroup("Terrain Cleanup")]
    // terrain patches smaller than this (in cells) are dissolved into their surroundings
    [Export] public int MinTerrainRegionSize = 8;

    [ExportGroup("Terrain Shape")]
    // raises elevation to this power after normalising - pushes toward flat lowlands and sharp peaks.
    // 1.0 = no change, higher = flatter map with more dramatic mountains
    [Export] public float ElevationPower = 1.8f;
    // how strongly the warp noise displaces sample coordinates - breaks up blob edges.
    // 0 = no warp, low single digits = subtle, high values get stringy and torn
    [Export] public float WarpStrength = 5f;

    [ExportGroup("Plant Noise")]
    [Export] public float PlantFrequency = 0.04f;
    [Export] public int PlantOctaves = 3;
    // density above this carves out a clump where plants grow, below it stays bare
    [Export] public float PlantDensityThreshold = 0.2f;
    // fraction of cells inside a clump that get a plant (rest stay walkable gaps)
    [Export] public float PlantCoverage = 0.4f;

    GameMap _map;
    Pathing _pathing;
    TickManager _tick;
    Stockpile _stockpile;
    GrowZone _growZone;

    public override void _EnterTree()
    {
        // load defs before children _Ready: panels instanced in Main.tscn build their rows from the def
        // database in their own _Ready, and Godot runs child _Ready before this node's _Ready
        DefLoader.LoadAll();
    }

    public override void _Ready()
    {
        _map = new GameMap(MapWidth, MapHeight);

        // the view subscribes to the model before world-gen, so generated plants spawn their views via events
        var views = new ViewManager();
        AddChild(views);
        Game.Views = views;
        views.Bind(_map);
        MapView.Bind(_map);

        WorldGenerator.Generate(_map, this);
        MapView.PaintAll(_map);

        _pathing = new Pathing();
        _pathing.Init(_map);

        Game.Map = _map;
        Game.Pathing = _pathing;
        Game.MapView = MapView;

        _stockpile = Game.Map.Stockpiles.Create();

        _growZone = Game.Map.GrowZones.Create();
        _growZone.Crop = PlantDefOf.Wheat;

        var taken = new HashSet<Vector2I>();

        var stoveCell = FindFootprintCell(BuildingDefOf.Stove.Size, taken);
        var stove = Game.Map.SpawnBuilding(BuildingDefOf.Stove, stoveCell);
        foreach (var c in stove.OccupiedCells)
        {
            taken.Add(c);
            _pathing.RefreshCell(_map, c);
        }

        var rng = new Random(Seed); // seed-derived, same world seed = same starting colonists for now
        for (int i = 0; i < StartingGuys; i++)
        {
            var guy = new Guy { Position = FindWalkableCell(taken) };
            guy.Attributes.Roll(rng);
            guy.Schedule = Schedule.DayNight();
            taken.Add(guy.Cell);
            Game.Map.AddGuy(guy);
        }

        Game.SelectedGuy = Game.Map.Guys[0];

        GameTime.Reset();
        _tick = new TickManager();
        _tick.Tick += () => Game.Map.Tick();
        AddChild(_tick);
        Game.Tick = _tick;

        var timeBar = GD.Load<PackedScene>("res://Scenes/TimeControlBar.tscn").Instantiate<TimeControlBar>();
        AddChild(timeBar);

        AddChild(new DebugOverlay());

        var tools = new ToolController();
        tools.Init(_stockpile, _growZone, TerrainLayer);
        AddChild(tools);
    }

    /// <summary>Finds the first walkable cell on the map for the initial colonist placement.</summary>
    /// <returns>The first walkable cell</returns>
    Vector2 FindWalkableCell(HashSet<Vector2I> taken = null)
    {
        for (int x = 0; x < _map.Width; x++)
            for (int y = 0; y < _map.Height; y++)
            {
                var cell = new Vector2I(x, y);
                if (_map.Terrain[x, y].Walkable && !_map.BlocksMovementAt(cell) && (taken == null || !taken.Contains(cell)))
                    return new Vector2(x, y);
            }

        return Vector2.Zero;
    }

    /// <summary>Finds an origin cell where the whole size-footprint is walkable, unblocked and free.</summary>
    Vector2I FindFootprintCell(Vector2I size, HashSet<Vector2I> taken)
    {
        for (int x = 0; x < _map.Width - size.X; x++)
            for (int y = 0; y < _map.Height - size.Y; y++)
            {
                bool ok = true;
                for (int dx = 0; dx < size.X && ok; dx++)
                    for (int dy = 0; dy < size.Y && ok; dy++)
                    {
                        var c = new Vector2I(x + dx, y + dy);
                        if (!_map.Terrain[c.X, c.Y].Walkable || _map.BlocksMovementAt(c) || taken.Contains(c))
                            ok = false;
                    }
                if (ok) return new Vector2I(x, y);
            }
        return Vector2I.Zero;
    }
}