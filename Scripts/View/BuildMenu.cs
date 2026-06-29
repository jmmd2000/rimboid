using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class BuildMenu : CanvasLayer
{
    [Export] public Container ButtonBar;

    readonly List<(Button button, BuildingDef def)> _items = new();

    public override void _Ready()
    {
        ButtonBar.AddThemeConstantOverride("separation", 8);

        foreach (var def in DefDatabase<BuildingDef>.All.OrderBy(d => d.Label))
        {
            var button = MakeButton(def);
            _items.Add((button, def));
            ButtonBar.AddChild(button);
        }
    }

    public override void _Process(double delta)
    {
        // highlight active button
        foreach (var (button, def) in _items)
            button.SetPressedNoSignal(def == Game.SelectedBuildable);
    }

    static Button MakeButton(BuildingDef def)
    {
        var button = new Button { ToggleMode = true, CustomMinimumSize = new Vector2(96, 96) };
        button.Pressed += () => Game.SelectedBuildable = def;

        var box = new VBoxContainer
        {
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Alignment = BoxContainer.AlignmentMode.Center,
        };

        box.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        box.AddThemeConstantOverride("separation", 6);

        Control icon = def.Texture != null ? new TextureRect
        {
            Texture = def.Texture,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            CustomMinimumSize = new Vector2(48, 48),
        } : new ColorRect { Color = def.Colour, CustomMinimumSize = new Vector2(48, 48) };

        icon.MouseFilter = Control.MouseFilterEnum.Ignore;
        box.AddChild(icon);

        box.AddChild(new Label
        {
            Text = def.Label,
            HorizontalAlignment = HorizontalAlignment.Center,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        });

        button.AddChild(box);
        return button;
    }
}