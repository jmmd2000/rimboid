using System.Collections.Generic;
using Godot;

/// <summary>HUD panel showing the selected colonist's attributes: each one's level, its stat-chain factor, and the XP creeping toward the next level.</summary>
public partial class AttributesPanel : Control
{
    [Export] public Container Box;

    readonly Dictionary<AttributeDef, LabelledBar> _rows = new();
    readonly Dictionary<AttributeDef, (int level, float xp)> _shown = new(); // last-drawn value per row, to skip unchanged
    Guy _guy;

    public override void _Ready()
    {
        var rowScene = GD.Load<PackedScene>("res://Scenes/LabelledBar.tscn");
        foreach (var def in DefDatabase<AttributeDef>.All)
        {
            var row = rowScene.Instantiate<LabelledBar>();
            Box.AddChild(row);
            _rows[def] = row;
        }
        Game.SelectedGuyChanged += OnSelectedGuyChanged;
        OnSelectedGuyChanged(Game.SelectedGuy);
    }

    public override void _ExitTree() => Game.SelectedGuyChanged -= OnSelectedGuyChanged;

    void OnSelectedGuyChanged(Guy guy)
    {
        _guy = guy;
        Box.Visible = guy != null;
        _shown.Clear(); // force a full re-render for the new colonist
    }

    public override void _Process(double delta)
    {
        if (_guy == null) return;
        foreach (var (def, row) in _rows)
        {
            var attribute = _guy.Attributes.Get(def);
            var current = (attribute.Level, attribute.XP);
            if (_shown.TryGetValue(def, out var last) && last == current) continue;
            _shown[def] = current;

            var progress = attribute.Level >= Attributes.MaxLevel ? "MAX" : $"{attribute.XP:0}/{Attributes.XPToLevel(attribute.Level):0}";
            row.Set($"{def.Label}  {attribute.Level} (×{_guy.Attributes.Factor(def):0.00})  {progress}", attribute.XP / Attributes.XPToLevel(attribute.Level));
        }
    }
}