using System;
using Godot;

///<summary>On screen time controls.</summary>
public partial class TimeControlBar : CanvasLayer
{
    TickManager _tick;

    Button _pauseButton;
    Button _fast, _faster, _fastest;
    Label _clock;

    /// <summary>Binds the bar to the tick manager it drives. Call before adding to the tree.</summary>
    /// <param name="tick">The tick manager to control.</param>
    public void Init(TickManager tick) => _tick = tick;

    public override void _Ready()
    {
        var row = new HBoxContainer { Position = new Vector2(10, 10) };
        AddChild(row);

        _pauseButton = AddButton(row, "||", "Pause / play (Space)", () => _tick.TogglePause());
        _fast = AddButton(row, "\u25B6", "Fast (1)", () => _tick.SetSpeed(1));
        _faster = AddButton(row, "\u25B6\u25B6", "Faster (2)", () => _tick.SetSpeed(3));
        _fastest = AddButton(row, "\u25B6\u25B6\u25B6", "Fastest (3)", () => _tick.SetSpeed(6));

        _clock = new Label { Position = new Vector2(10, 44) };
        AddChild(_clock);
    }

    public override void _Process(double delta)
    {
        if (_tick == null) return;

        _pauseButton.Text = _tick.SpeedMultiplier == 0 ? "\u25B6" : "||";

        Highlight(_fast, _tick.SpeedMultiplier == 1);
        Highlight(_faster, _tick.SpeedMultiplier == 3);
        Highlight(_fastest, _tick.SpeedMultiplier == 6);

        _clock.Text = $"Day {GameTime.Day} - {GameTime.HourOfDay:00}:00";
    }

    Button AddButton(HBoxContainer row, string text, string tooltip, Action onPressed)
    {
        var b = new Button
        {
            Text = text,
            TooltipText = tooltip,
            // don't let the button capture Space/Enter
            FocusMode = Control.FocusModeEnum.None,
        };
        b.Pressed += () => onPressed();
        row.AddChild(b);
        return b;
    }

    static void Highlight(Button b, bool active) => b.Modulate = active ? Colors.Yellow : Colors.White;
}