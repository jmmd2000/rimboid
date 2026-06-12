using Godot;

/// <summary>HUD panel showing a colonist's needs as bars.</summary>
public partial class NeedsPanel : CanvasLayer
{
    Guy _guy;
    ProgressBar _rest;

    /// <summary>Binds the panel to the colonist whose needs it shows.</summary>
    /// <param name="guy">The colonist to read needs from.</param>
    public void Init(Guy guy) => _guy = guy;

    public override void _Ready()
    {
        var box = new VBoxContainer { Position = new Vector2(10, 80) };
        AddChild(box);

        box.AddChild(new Label { Text = "Rest" });
        _rest = new ProgressBar
        {
            MinValue = 0,
            MaxValue = 1,
            CustomMinimumSize = new Vector2(120, 16),
        };
        box.AddChild(_rest);
    }

    public override void _Process(double delta)
    {
        if (_guy == null) return;
        _rest.Value = _guy.Needs.Rest.Level;
        _rest.Modulate = _guy.Needs.Rest.Level < 0.3f ? Colors.Red : Colors.White;
    }
}