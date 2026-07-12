using Godot;

/// <summary>Tints the whole world by time of day: each frame it samples the day gradient at the
/// clock's TimeOfDay and drives this CanvasModulate. The colour is a multiply, so white = full
/// daylight (untouched) and darker/cooler = night.</summary>
public partial class DayNightCycle : CanvasModulate
{
    [Export] public Gradient DayGradient;
    [Export] public Color VoidColour = new(0.09f, 0.10f, 0.12f); // the off-map void, before the day tint

    public override void _Process(double delta)
    {
        if (DayGradient == null) return;
        var tint = DayGradient.Sample(GameTime.TimeOfDay);
        Color = tint; // the world
        RenderingServer.SetDefaultClearColor(VoidColour * tint); // the void beyond the map, tinted to match
    }
}