using Godot;

/// <summary>Main node. Connects world gen, pathing, colonists, player input, ui.</summary>
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
    Stockpile _stockpile;

    public override void _Ready()
    {
        _map = new GameMap(MapWidth, MapHeight);
        ItemDefOf.Load();
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

        _guy = new Guy { Position = FindWalkableCell() };

        _guyView = new GuyView();
        _guyView.Texture = GD.Load<Texture2D>("res://Assets/guy.png");
        _guyView.Init(_guy, Game.TileSize);
        AddChild(_guyView);

        var pathLine = new PathLine();
        pathLine.Init(_guy, Game.TileSize);
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
        zzz.Init(_guy, Game.TileSize);
        AddChild(zzz);

        var tools = new ToolController();
        tools.Init(_guy, _stockpile, TerrainLayer);
        AddChild(tools);

        // TEMP: spawn berries
        var (berries, _, _) = Game.Map.SpawnItem(ItemDefOf.Berries, new Vector2I(3, 3), 10);
        Game.Views.SpawnItemView(berries);
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
}