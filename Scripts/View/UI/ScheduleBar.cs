using Godot;

/// <summary>A 24-hour strip for the selected colonist's schedule: one coloured cell per hour
/// (grey = Anything, gold = Work, blue = Sleep). Click a cell to cycle its block. Self-drawn so it
/// previews in the editor.</summary>
[Tool]
public partial class ScheduleBar : Control
{
    const int Hours = 24;

    static readonly Color AnythingColour = new(0.40f, 0.40f, 0.40f);
    static readonly Color WorkColour = new(0.85f, 0.70f, 0.25f);
    static readonly Color SleepColour = new(0.30f, 0.40f, 0.70f);

    int _lastHour = -1;

    public override void _Ready()
    {
        CustomMinimumSize = new Vector2(Hours * 12, 24);
        if (Engine.IsEditorHint()) return;
        Game.SelectedGuyChanged += OnSelectedGuyChanged;
        OnSelectedGuyChanged(Game.SelectedGuy);
    }

    public override void _ExitTree()
    {
        if (!Engine.IsEditorHint()) Game.SelectedGuyChanged -= OnSelectedGuyChanged;
    }

    void OnSelectedGuyChanged(Guy guy) { Visible = guy != null; QueueRedraw(); }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint() || Game.SelectedGuy == null || GameTime.HourOfDay == _lastHour) return;
        _lastHour = GameTime.HourOfDay; // redraw only when the hour marker moves
        QueueRedraw();
    }

    public override void _Draw()
    {
        float w = Size.X / Hours;
        var schedule = Game.SelectedGuy?.Schedule;
        for (int h = 0; h < Hours; h++)
            DrawRect(new Rect2(h * w, 0, w - 1, Size.Y), ColourOf(schedule?.BlockAt(h) ?? ScheduleBlock.Anything));

        if (!Engine.IsEditorHint()) // highlight the current hour
            DrawRect(new Rect2(GameTime.HourOfDay * w, 0, w - 1, Size.Y), new Color(1, 1, 1, 0.25f));
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is not InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } click) return;
        var guy = Game.SelectedGuy;
        if (guy == null) return;
        int hour = Mathf.Clamp((int)(click.Position.X / (Size.X / Hours)), 0, Hours - 1);
        guy.Schedule.Set(hour, Next(guy.Schedule.BlockAt(hour)));
        QueueRedraw();
    }

    static Color ColourOf(ScheduleBlock block) => block switch
    {
        ScheduleBlock.Work => WorkColour,
        ScheduleBlock.Sleep => SleepColour,
        _ => AnythingColour,
    };

    static ScheduleBlock Next(ScheduleBlock b) => b switch
    {
        ScheduleBlock.Anything => ScheduleBlock.Work,
        ScheduleBlock.Work => ScheduleBlock.Sleep,
        _ => ScheduleBlock.Anything,
    };
}