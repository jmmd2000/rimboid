using System.Collections.Generic;
using Godot;

/// <summary>HUD panel showing the selected colonist's skills, each as a level with an XP bar.</summary>
public partial class SkillsPanel : CanvasLayer
{
    [Export] public Container Box;
    [Export] public PackedScene RowScene;

    readonly Dictionary<SkillDef, LabelledBar> _rows = new();
    readonly Dictionary<SkillDef, (int level, float xp)> _shown = new(); // last-drawn value per row, to skip unchanged
    Guy _lastGuy;

    public override void _Ready()
    {
        foreach (var def in DefDatabase<SkillDef>.All)
        {
            var row = RowScene.Instantiate<LabelledBar>();
            Box.AddChild(row);
            _rows[def] = row;
        }
    }

    public override void _Process(double delta)
    {
        var guy = Game.SelectedGuy;
        Box.Visible = guy != null;
        if (guy == null) { _lastGuy = null; return; }

        bool guyChanged = guy != _lastGuy;
        _lastGuy = guy;

        foreach (var (def, row) in _rows)
        {
            var skill = guy.Skills.Get(def);
            var current = (skill.Level, skill.XP);
            if (!guyChanged && _shown.TryGetValue(def, out var last) && last == current) continue; // nothing moved
            _shown[def] = current;

            var progress = skill.Level >= Skills.MaxLevel ? "MAX" : $"{skill.XP:0}/{Skills.XPToLevel(skill.Level):0}";
            row.Set($"{def.Label}  Lv {skill.Level}  ({progress})", skill.XP / Skills.XPToLevel(skill.Level));
        }
    }
}