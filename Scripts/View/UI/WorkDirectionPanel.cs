using System.Collections.Generic;
using Godot;

/// <summary>HUD panel to switch each kind of work on or off for the selected colonist.</summary>
[Tool]
public partial class WorkDirectionPanel : Control
{
    [Export] public Container Box;

    readonly Dictionary<WorkType, CheckBox> _rows = new();

    public override void _Ready()
    {
        if (Box.GetChildCount() == 0)
            foreach (var type in WorkTypes.All)
            {
                var row = new CheckBox { Text = type.Label() };
                row.Toggled += pressed =>
                {
                    if (Game.SelectedGuy != null) Game.SelectedGuy.WorkSettings.Set(type, pressed);
                };
                Box.AddChild(row);
                _rows[type] = row;
            }

        if (Engine.IsEditorHint()) return; // editor: preview rows only, no binding
        Game.SelectedGuyChanged += OnSelectedGuyChanged;
        OnSelectedGuyChanged(Game.SelectedGuy);
    }

    public override void _ExitTree()
    {
        if (!Engine.IsEditorHint()) Game.SelectedGuyChanged -= OnSelectedGuyChanged;
    }

    void OnSelectedGuyChanged(Guy guy)
    {
        Box.Visible = guy != null;
        if (guy == null) return;
        foreach (var (type, row) in _rows)
            row.SetPressedNoSignal(guy.WorkSettings.Allows(type));
    }
}