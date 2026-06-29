using System.Linq;
using System.Text;
using Godot;

/// <summary>Toggleable diagnostics overlay</summary>
public partial class DebugOverlay : CanvasLayer
{
    Label _label;
    double _accum;
    const double Interval = 0.5;

    public override void _Ready()
    {
        Layer = 128;

        _label = new Label();
        // pin to the top-right corner, grow leftward/downward as the text grows
        _label.SetAnchorsPreset(Control.LayoutPreset.TopRight);
        _label.GrowHorizontal = Control.GrowDirection.Begin;
        _label.GrowVertical = Control.GrowDirection.End;
        _label.OffsetRight = -10;
        _label.OffsetTop = 10;

        var font = new SystemFont { FontNames = ["Consolas", "Cascadia Mono", "Courier New", "monospace"] };
        _label.AddThemeFontOverride("font", font);
        _label.AddThemeFontSizeOverride("font_size", 13);
        _label.AddThemeColorOverride("font_color", new Color(0.85f, 0.92f, 1f));
        _label.AddThemeStyleboxOverride("normal", new StyleBoxFlat
        {
            BgColor = new Color(0.5f, 0.05f, 0.1f, 0.8f),
            CornerRadiusTopLeft = 5,
            CornerRadiusTopRight = 5,
            CornerRadiusBottomLeft = 5,
            CornerRadiusBottomRight = 5,
            ContentMarginLeft = 12,
            ContentMarginRight = 12,
            ContentMarginTop = 10,
            ContentMarginBottom = 10,
        });

        AddChild(_label);

        Visible = false;
        Prof.Enabled = false;
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (e is InputEventKey k && k.Pressed && !k.Echo && k.Keycode == Key.F3)
        {
            Visible = !Visible;
            Prof.Enabled = Visible;
            if (Visible) Prof.Snapshot();
        }
    }

    public override void _Process(double delta)
    {
        if (!Visible) return;
        _accum += delta;
        if (_accum < Interval) return;
        _accum = 0;

        var samples = Prof.Snapshot();
        var sb = new StringBuilder();

        double proc = Performance.GetMonitor(Performance.Monitor.TimeProcess) * 1000.0;
        double phys = Performance.GetMonitor(Performance.Monitor.TimePhysicsProcess) * 1000.0;
        sb.AppendLine($"FPS {(int)Engine.GetFramesPerSecond(),-4} proc {proc,5:F1} phys {phys,5:F1} ms");

        // average and worst sim tick this window
        if (samples.TryGetValue("Sim.Tick", out var sim) && sim.Calls > 0)
            sb.AppendLine($"sim  avg {sim.TotalMs / sim.Calls:F3}  max {sim.MaxMs:F2} ms  ({sim.Calls}/win)");

        sb.AppendLine();
        sb.AppendLine($"{"profile",-15}{"tot",7}{"calls",7}{"avg",8}{"max",8}");
        foreach (var kv in samples.OrderByDescending(s => s.Value.TotalMs))
        {
            var e = kv.Value;
            double avg = e.Calls > 0 ? e.TotalMs / e.Calls : 0;
            sb.AppendLine($"{kv.Key,-15}{e.TotalMs,7:F2}{e.Calls,7}{avg,8:F3}{e.MaxMs,8:F3}");
        }

        if (Stats.Gauges.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("counts");
            foreach (var kv in Stats.Gauges)
                sb.AppendLine($"{kv.Key,-15}{kv.Value,9:F0}");
        }

        _label.Text = sb.ToString();
    }
}
