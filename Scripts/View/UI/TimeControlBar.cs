using Godot;

///<summary>On-screen time controls and their keyboard shortcuts.</summary>
public partial class TimeControlBar : Control
{
    [Export] public Button Pause;
    [Export] public Button Fast;
    [Export] public Button Faster;
    [Export] public Button Fastest;
    [Export] public Label Clock;

    public override void _Ready()
    {
        Pause.Pressed += () => Game.Tick.TogglePause();
        Fast.Pressed += () => Game.Tick.SetSpeed(1);
        Faster.Pressed += () => Game.Tick.SetSpeed(3);
        Fastest.Pressed += () => Game.Tick.SetSpeed(6);
    }

    public override void _Process(double delta)
    {
        if (Game.Tick == null) return;

        Pause.Text = Game.Tick.SpeedMultiplier == 0 ? "\u25B6" : "||";

        Highlight(Fast, Game.Tick.SpeedMultiplier == 1);
        Highlight(Faster, Game.Tick.SpeedMultiplier == 3);
        Highlight(Fastest, Game.Tick.SpeedMultiplier == 6);

        Clock.Text = $"Day {GameTime.Day} - {GameTime.HourOfDay:00}:{GameTime.MinuteOfHour:00}";
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (Game.Tick == null || e is not InputEventKey key || !key.Pressed || key.Echo) return;

        switch (key.Keycode)
        {
            case Key.Space: Game.Tick.TogglePause(); break;
            case Key.Key1: Game.Tick.SetSpeed(1); break;
            case Key.Key2: Game.Tick.SetSpeed(3); break;
            case Key.Key3: Game.Tick.SetSpeed(6); break;
            case Key.Key9: Game.Tick.SetSpeed(50); break;
            case Key.Period: if (Game.Tick.SpeedMultiplier == 0) Game.Tick.DoSingleTick(); break;
        }
    }

    static void Highlight(Button b, bool active) => b.Modulate = active ? Colors.Yellow : Colors.White;
}