using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>
/// Player world interaction, tool-mode selection (mine/stockpile/build), drag designation,
/// and click-to-move when no tool is active. Manages the drag selection box.
/// </summary>
public partial class ToolController : Node2D
{
    TileMapLayer _terrainLayer;
    SelectionBox _selectionBox;

    Vector2I? _dragStart;
    MouseButton _dragButton;

    Dictionary<ToolDef, Designator> _designators;  // the behaviour for each tool
    Dictionary<Key, ToolDef> _toolKeys;

    readonly BuildPlacer _build = new(); // building placement (rotation, ghost, place/cancel)

    /// <summary>Binds the controller to what it acts on. Call before adding to the tree.</summary>
    /// <param name="stockpile">The stockpile the stockpile tool edits.</param>
    /// <param name="growZone">The grow zone the grow tool edits.</param>
    /// <param name="terrainLayer">The tilemap used to convert the mouse position to a cell.</param>
    public void Init(Stockpile stockpile, GrowZone growZone, TileMapLayer terrainLayer)
    {
        _terrainLayer = terrainLayer;

        _designators = new()
        {
            [ToolDefOf.Mine] = new Designator_Mine(),
            [ToolDefOf.Stockpile] = new Designator_Stockpile(stockpile),
            [ToolDefOf.GrowZone] = new Designator_GrowZone(growZone),
            [ToolDefOf.Harvest] = new Designator_Harvest(),
            [ToolDefOf.Chop] = new Designator_Chop(),
            [ToolDefOf.Deconstruct] = new Designator_Deconstruct(),
        };

        _toolKeys = new();
        foreach (var def in DefDatabase<ToolDef>.All)
            if (def.Shortcut != Key.None) _toolKeys[def.Shortcut] = def;

        _selectionBox = new SelectionBox();
        _selectionBox.Init(Game.TileSize);
        AddChild(_selectionBox);
        ZIndex = 10;
    }

    bool _ghostShown;

    public override void _Process(double delta)
    {
        bool show = Game.SelectedBuildable != null;
        if (show || _ghostShown) QueueRedraw();
        if (!show && _ghostShown) _build.Reset();
        _ghostShown = show;
    }

    public override void _Draw()
    {
        var def = Game.SelectedBuildable;
        if (def == null) return;
        var origin = CellUnderMouse();
        if (!Game.Map.InBounds(origin)) return;

        _build.DrawGhost(this, def, origin);
    }

    public override void _UnhandledInput(InputEvent e)
    {
        // tool / build-mode keys
        if (e is InputEventKey key && key.Pressed && !key.Echo)
        {
            if (key.Keycode == Key.Escape)
            {
                Game.SelectedBuildable = null;   // exit build placement
                Game.SelectedTool = null;        // exit tool mode
            }
            else if (key.Keycode == Key.F2)
            {
                Game.CreativeMode = !Game.CreativeMode;
                GD.Print($"Creative mode {(Game.CreativeMode ? "on" : "off")}");
            }
            else if (key.Keycode == Key.R && Game.SelectedBuildable != null)
            {
                _build.Rotate();
            }
            else if (_toolKeys.TryGetValue(key.Keycode, out var tool))
            {
                Game.SelectedTool = Game.SelectedTool == tool ? null : tool;
                Game.SelectedBuildable = null; // switching to a tool exits build placement
            }
        }

        if (Game.SelectedBuildable != null) HandleDrag(e);
        else if (Game.SelectedTool != null) HandleDrag(e);
        else if (e is InputEventMouseButton mb && mb.Pressed)
        {
            Vector2I cell = CellUnderMouse();
            if (!Game.Map.InBounds(cell)) return;

            if (mb.ButtonIndex == MouseButton.Left)
            {
                var building = Game.Map.BuildingAt(cell);
                if (building != null)
                {
                    Game.SelectedBuilding = building;
                    Game.SelectedGuy = null;
                }
                else
                {
                    Game.SelectedGuy = GuyUnderMouse();
                    Game.SelectedBuilding = null;
                }

            }
            else if (mb.ButtonIndex == MouseButton.Right && Game.SelectedGuy != null)
            {
                var path = Game.Pathing.GetPath(Game.SelectedGuy.Cell, cell);
                if (path != null) Game.SelectedGuy.GoTo(path);
            }

        }
    }

    Vector2I CellUnderMouse() => _terrainLayer.LocalToMap(_terrainLayer.ToLocal(GetGlobalMousePosition()));

    /// <summary>The colonist whose drawn sprite is under the mouse, or null. Compares against each guy's
    /// visual centre (Position + half a tile) so guys stay clickable mid-move</summary>
    Guy GuyUnderMouse()
    {
        Vector2 mouseTiles = _terrainLayer.ToLocal(GetGlobalMousePosition()) / Game.TileSize;
        return Game.Map.Guys
            .Where(guy => (guy.Position + Vector2.One * 0.5f).DistanceTo(mouseTiles) <= 0.5f)
            .OrderBy(guy => (guy.Position + Vector2.One * 0.5f).DistanceTo(mouseTiles))
            .FirstOrDefault();
    }

    bool ShowSelectionBox => Game.SelectedBuildable == null || (Game.SelectedBuildable.Size == Vector2I.One && Game.SelectedBuildable.DragPlace);

    /// <summary>Tracks a press/drag/release selection and updates the preview outline.</summary>
    void HandleDrag(InputEvent e)
    {
        if (e is InputEventMouseButton mb && (mb.ButtonIndex == MouseButton.Left || mb.ButtonIndex == MouseButton.Right))
        {
            Vector2I cell = CellUnderMouse();
            if (mb.Pressed)
            {
                _dragStart = Game.Map.InBounds(cell) ? cell : (Vector2I?)null;
                _dragButton = mb.ButtonIndex;
                if (_dragStart != null && ShowSelectionBox) _selectionBox.SetSelection(cell, cell);
            }
            else if (_dragStart != null && mb.ButtonIndex == _dragButton)
            {
                if (Game.Map.InBounds(cell))
                {
                    ApplyDrag(_dragStart.Value, cell, _dragButton);
                }
                _dragStart = null;
                _selectionBox.Clear();
            }
        }
        else if (e is InputEventMouseMotion && _dragStart != null)
        {
            Vector2I cell = CellUnderMouse();
            if (Game.Map.InBounds(cell) && ShowSelectionBox)
            {
                _selectionBox.SetSelection(_dragStart.Value, cell);
            }
        }
    }

    /// <summary>Applies a finished drag rectangle: build placement, or the active tool's designator.</summary>
    void ApplyDrag(Vector2I a, Vector2I b, MouseButton button)
    {
        if (Game.SelectedBuildable != null)
        {
            if (button == MouseButton.Left) _build.Place(Game.SelectedBuildable, a, b);
            else _build.Cancel(a, b);
            return;
        }

        if (Game.SelectedTool == null || !_designators.TryGetValue(Game.SelectedTool, out var designator)) return;

        foreach (var cell in Grid.CellsInRect(a, b))
        {
            if (button == MouseButton.Left) designator.Apply(cell);
            else designator.Cancel(cell);
        }
        designator.OnDragFinished();
    }
}