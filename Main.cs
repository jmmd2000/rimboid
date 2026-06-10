using System.CodeDom.Compiler;
using Godot;

public partial class Main : Node2D
{
    [Export] public MapView MapView;

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


    public override void _Ready()
    {
        Generate();
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (e.IsActionPressed("ui_accept"))
            Generate();
    }

    void Generate()
    {
        var map = new GameMap(MapWidth, MapHeight);
        WorldGenerator.Generate(map, this);
        MapView.PaintAll(map);
    }



}
