using Godot;

/// <summary>HUD panel showing a colonist's needs as bars.</summary>
public partial class NeedsPanel : CanvasLayer
{
    [Export] public Container Box;
    [Export] public LabelledBar Rest;
    [Export] public LabelledBar Food;

    public override void _Process(double delta)
    {
        var guy = Game.SelectedGuy;
        Box.Visible = guy != null;
        if (guy == null) return;

        Rest.Set("Rest", guy.Needs.Rest.Level);
        Food.Set("Food", guy.Needs.Food.Level);
    }
}