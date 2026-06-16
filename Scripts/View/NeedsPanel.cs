using Godot;

/// <summary>HUD panel showing a colonist's needs as bars.</summary>
public partial class NeedsPanel : CanvasLayer
{
    VBoxContainer _box;
    ProgressBar _rest;
    ProgressBar _food;

    public override void _Ready()
    {
        _box = new VBoxContainer { Position = new Vector2(10, 80) };
        AddChild(_box);

        _rest = AddBar(_box, "Rest");
        _food = AddBar(_box, "Food");
    }

    public override void _Process(double delta)
    {
        var guy = Game.SelectedGuy;
        _box.Visible = guy != null;
        if (guy == null) return;

        UpdateBar(_rest, guy.Needs.Rest.Level);
        UpdateBar(_food, guy.Needs.Food.Level);
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