using System.Collections.Generic;
using Godot;

/// <summary>HUD panel showing the selected colonist's skills, each as a level with an XP bar.</summary>
public partial class SkillsPanel : CanvasLayer
{
    VBoxContainer _box;
    readonly Dictionary<SkillDef, (Label Label, ProgressBar Bar)> _rows = new();

    public override void _Ready()
    {
        _box = new VBoxContainer { Position = new Vector2(10, 100) };
        AddChild(_box);

        foreach (var def in DefDatabase<SkillDef>.All)
            _rows[def] = AddRow(_box, def.Label);
    }

    public override void _Process(double delta)
    {
        var guy = Game.SelectedGuy;
        _box.Visible = guy != null;
        if (guy == null) return;

        foreach (var (def, row) in _rows)
        {
            var skill = guy.Skills.Get(def);
            row.Label.Text = $"{def.Label} Lv {skill.Level} ({skill.XP:0}/{Skills.XPToLevel(skill.Level):0})";
            row.Bar.Value = skill.XP / Skills.XPToLevel(skill.Level);
        }
    }

    static (Label, ProgressBar) AddRow(VBoxContainer box, string label)
    {
        var text = new Label { Text = label };
        box.AddChild(text);
        var bar = new ProgressBar
        {
            MinValue = 0,
            MaxValue = 1,
            ShowPercentage = false,
            CustomMinimumSize = new Vector2(120, 12),
        };
        box.AddChild(bar);
        return (text, bar);
    }
}