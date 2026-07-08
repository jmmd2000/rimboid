using System.Collections.Generic;
using Godot;

/// <summary>HUD panel showing the selected colonist's attributes: each one's level, its stat-chain factor, and the XP creeping toward the next level.</summary>
public partial class AttributesPanel : CanvasLayer
{
    [Export] public Container Box;
    [Export] public PackedScene RowScene;

    readonly Dictionary<AttributeDef, LabelledBar> _rows = new();

    public override void _Ready()
    {
        foreach (var def in DefDatabase<AttributeDef>.All)
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
            var attribute = guy.Attributes.Get(def);
            var progress = attribute.Level >= Attributes.MaxLevel ? "MAX" : $"{attribute.XP:0}/{Attributes.XPToLevel(attribute.Level):0}";
            row.Set($"{def.Label}  {attribute.Level} (×{guy.Attributes.Factor(def):0.00})  {progress}", attribute.XP / Attributes.XPToLevel(attribute.Level));
        }
    }
}