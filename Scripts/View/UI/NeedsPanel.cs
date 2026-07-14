using Godot;

/// <summary>HUD panel showing a colonist's needs as bars.</summary>
public partial class NeedsPanel : Control
{
    [Export] public Container Box;
    [Export] public LabelledBar Rest;
    [Export] public LabelledBar Food;

    Guy _guy;

    public override void _Ready()
    {
        Game.SelectedGuyChanged += OnSelectedGuyChanged;
        OnSelectedGuyChanged(Game.SelectedGuy);
    }

    public override void _ExitTree() => Game.SelectedGuyChanged -= OnSelectedGuyChanged;

    void OnSelectedGuyChanged(Guy guy) { _guy = guy; Box.Visible = guy != null; }

    public override void _Process(double delta)
    {
        if (_guy == null) return;
        Rest.Set("Rest", _guy.Needs.Rest.Level);
        Food.Set("Food", _guy.Needs.Food.Level);
    }
}