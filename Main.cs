using Godot;

public partial class Main : Node2D
{
    [Export] public MapView MapView;

    public override void _Ready()
    {
        var map = new GameMap(64, 64);
        WorldGenerator.Generate(map, 12345);
        MapView.PaintAll(map);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
