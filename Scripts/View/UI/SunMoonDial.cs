using Godot;

/// <summary>A small clock-face dial: a marker circles once per day, top at noon, bottom at
/// midnight, drawn gold above the horizon (day) and pale below (night).</summary>
[Tool]
public partial class SunMoonDial : Control
{
    float _lastTimeOfDay = -1f;

    public override void _Ready() => CustomMinimumSize = new Vector2(40, 40);

    public override void _Process(double delta)
    {
        if (GameTime.TimeOfDay == _lastTimeOfDay) return; // same tick, nothing moved
        _lastTimeOfDay = GameTime.TimeOfDay;
        QueueRedraw();
    }

    public override void _Draw()
    {
        var centre = Size / 2f;
        float radius = Mathf.Min(Size.X, Size.Y) / 2f - 3f;
        float angle = GameTime.TimeOfDay * Mathf.Tau;

        DrawArc(centre, radius, 0f, Mathf.Tau, 32, new Color(1, 1, 1, 0.35f), 1.5f, antialiased: true); // the dial ring
        DrawLine(centre - new Vector2(radius, 0), centre + new Vector2(radius, 0), new Color(1, 1, 1, 0.2f)); // horizon

        var dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)); // noon=top, midnight=bottom
        bool day = Mathf.Cos(angle) < 0f;// marker above the horizon
        var marker = day ? new Color(1f, 0.82f, 0.3f) : new Color(0.82f, 0.88f, 1f); // gold sun / pale moon
        DrawCircle(centre + dir * radius, 3.5f, marker);
    }
}