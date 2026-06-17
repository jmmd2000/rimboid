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

    public override void _Ready()
    {
        _map = new GameMap(MapWidth, MapHeight);
        ItemDefOf.Load();
        PlantDefOf.Load();
        BuildingDefOf.Load();
        WorldGenerator.Generate(_map, this);
        MapView.PaintAll(_map);

        _pathing = new Pathing();
        _pathing.Init(_map);

        Game.Map = _map;
        Game.Pathing = _pathing;
        Game.MapView = MapView;

        var views = new ViewManager();
        AddChild(views);
        Game.Views = views;

        _stockpile = Game.Map.Stockpiles.Create();

        var taken = new HashSet<Vector2I>();
        for (int i = 0; i < StartingGuys; i++)
        {
            var guy = new Guy { Position = FindWalkableCell(taken) };
            taken.Add(guy.Cell);
            Game.Map.Guys.Add(guy);
            Game.Views.SpawnGuyViews(guy);
        }

        Game.SelectedGuy = Game.Map.Guys[0];

        GameTime.Reset();
        _tick = new TickManager();
        _tick.Tick += () => Game.Map.Tick();
        AddChild(_tick);

        var timeBar = new TimeControlBar();
        timeBar.Init(_tick);
        AddChild(timeBar);

        var needsPanel = new NeedsPanel();
        AddChild(needsPanel);

        var tools = new ToolController();
        tools.Init(_stockpile, TerrainLayer);
        AddChild(tools);

        // TEMP: spawn berries
        var (berries, _, _) = Game.Map.SpawnItem(ItemDefOf.Berries, new Vector2I(3, 3), 10);
        Game.Views.SpawnItemView(berries);
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
}