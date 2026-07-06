using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>Bottom-right tool bar</summary>
public partial class ToolBar : CanvasLayer
{
    [Export] public Container OrdersBar;
    [Export] public Container ZonesBar;

    readonly List<(Button button, ToolDef def)> _items = new();

    public override void _Ready()
    {
        Fill(OrdersBar, ToolCategory.Order);
        Fill(ZonesBar, ToolCategory.Zone);
    }

    public override void _Process(double delta)
    {
        // highlight the active tools button
        foreach (var (button, def) in _items)
            button.SetPressedNoSignal(def == Game.SelectedTool);
    }

    void Fill(Container bar, ToolCategory category)
    {
        foreach (var def in DefDatabase<ToolDef>.All.Where(d => d.Category == category).OrderBy(d => d.Order))
        {
            var button = MakeButton(def);
            _items.Add((button, def));
            bar.AddChild(button);
        }
    }

    static Button MakeButton(ToolDef def)
    {
        var button = new Button
        {
            ToggleMode = true,
            FocusMode = Control.FocusModeEnum.None, // dont let the button block shortcut keys
            CustomMinimumSize = new Vector2(48, 48),
            TooltipText = $"{def.Label} ({def.Shortcut})",
            Icon = def.Icon,
            ExpandIcon = true,
            Text = def.Icon == null ? def.Label : "",
        };

        button.Pressed += () =>
        {
            Game.SelectedTool = Game.SelectedTool == def ? null : def;
            Game.SelectedBuildable = null;
        };

        return button;
    }

}