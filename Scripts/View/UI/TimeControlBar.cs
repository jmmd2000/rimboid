using Godot;

///<summary>On-screen time controls and their keyboard shortcuts.</summary>
public partial class TimeControlBar : CanvasLayer
{
    [Export] public Button Pause;
    [Export] public Button Fast;
    [Export] public Button Faster;
    [Export] public Button Fastest;
    [Export] public Label Clock;

    TickManager _tick;

    /// <summary>Binds the bar to the tick manager it drives. Call before adding to the tree.</summary>
    /// <param name="tick">The tick manager to control.</param>
    public void Init(TickManager tick) => _tick = tick;

    public override void _Ready()
    {
        Pause.Pressed += () => _tick.TogglePause();
        Fast.Pressed += () => _tick.SetSpeed(1);
        Faster.Pressed += () => _tick.SetSpeed(3);
        Fastest.Pressed += () => _tick.SetSpeed(6);
    }

    public override void _Process(double delta)
    {
        if (_tick == null) return;

        Pause.Text = _tick.SpeedMultiplier == 0 ? "\u25B6" : "||";

        Highlight(Fast, _tick.SpeedMultiplier == 1);
        Highlight(Faster, _tick.SpeedMultiplier == 3);
        Highlight(Fastest, _tick.SpeedMultiplier == 6);

        Clock.Text = $"Day {GameTime.Day} - {GameTime.HourOfDay:00}:{GameTime.MinuteOfHour:00}";
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (_tick == null || e is not InputEventKey key || !key.Pressed || key.Echo) return;

        switch (key.Keycode)
        {
            case Key.Space: _tick.TogglePause(); break;
            case Key.Key1: _tick.SetSpeed(1); break;
            case Key.Key2: _tick.SetSpeed(3); break;
            case Key.Key3: _tick.SetSpeed(6); break;
            case Key.Key9: _tick.SetSpeed(50); break;
            case Key.Period: if (_tick.SpeedMultiplier == 0) _tick.DoSingleTick(); break;
        }
    }

    static void Highlight(Button b, bool active) => b.Modulate = active ? Colors.Yellow : Colors.White;
}