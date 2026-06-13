using Godot;

/// <summary>HUD panel showing a colonist's needs as bars.</summary>
public partial class NeedsPanel : CanvasLayer
{
    Guy _guy;
    ProgressBar _rest;
    ProgressBar _food;

    /// <summary>Binds the panel to the colonist whose needs it shows.</summary>
    /// <param name="guy">The colonist to read needs from.</param>
    public void Init(Guy guy) => _guy = guy;

    public override void _Ready()
    {
        var box = new VBoxContainer { Position = new Vector2(10, 80) };
        AddChild(box);

        _rest = AddBar(box, "Rest");
        _food = AddBar(box, "Food");
    }

    public override void _Process(double delta)
    {
        if (_guy == null) return;
        UpdateBar(_rest, _guy.Needs.Rest.Level);
        UpdateBar(_food, _guy.Needs.Food.Level);
    }

    static ProgressBar AddBar(VBoxContainer box, string label)
    {
        box.AddChild(new Label { Text = label });
        var bar = new ProgressBar { MinValue = 0, MaxValue = 1, CustomMinimumSize = new Vector2(120, 16) };
        box.AddChild(bar);
        return bar;
    }

    static void UpdateBar(ProgressBar bar, float level)
    {
        bar.Value = level;
        bar.Modulate = level < 0.3f ? Colors.Red : Colors.White;
    }
}