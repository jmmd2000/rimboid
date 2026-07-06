using System.Collections.Generic;
using Godot;

/// <summary>HUD panel showing the selected colonist's skills, each as a level with an XP bar.</summary>
public partial class SkillsPanel : CanvasLayer
{
    [Export] public Container Box;
    [Export] public PackedScene RowScene;

    readonly Dictionary<SkillDef, LabelledBar> _rows = new();

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
        if (guy == null) return;

        foreach (var (def, row) in _rows)
        {
            var skill = guy.Skills.Get(def);
            var progress = skill.Level >= Skills.MaxLevel ? "MAX" : $"{skill.XP:0}/{Skills.XPToLevel(skill.Level):0}";
            row.Set($"{def.Label}  Lv {skill.Level}  ({progress})", skill.XP / Skills.XPToLevel(skill.Level));
        }
    }
}