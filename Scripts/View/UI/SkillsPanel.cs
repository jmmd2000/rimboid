using System.Collections.Generic;
using Godot;

/// <summary>HUD panel showing the selected colonist's skills, each as a level with an XP bar.</summary>
public partial class SkillsPanel : CanvasLayer
{
    [Export] public Container Box;

    readonly Dictionary<SkillDef, LabelledBar> _rows = new();
    readonly Dictionary<SkillDef, (int level, float xp)> _shown = new(); // last-drawn value per row, to skip unchanged
    Guy _guy;

    public override void _Ready()
    {
        var rowScene = GD.Load<PackedScene>("res://Scenes/LabelledBar.tscn");
        foreach (var def in DefDatabase<SkillDef>.All)
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
            var skill = _guy.Skills.Get(def);
            var current = (skill.Level, skill.XP);
            if (_shown.TryGetValue(def, out var last) && last == current) continue;
            _shown[def] = current;

            var progress = skill.Level >= Skills.MaxLevel ? "MAX" : $"{skill.XP:0}/{Skills.XPToLevel(skill.Level):0}";
            row.Set($"{def.Label}  Lv {skill.Level}  ({progress})", skill.XP / Skills.XPToLevel(skill.Level));
        }
    }
}