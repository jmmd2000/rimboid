using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>
/// Player world interaction, tool-mode selection (mine/stockpile/build), drag designation,
/// and click-to-move when no tool is active. Manages the drag selection box.
/// </summary>
public partial class ToolController : Node2D
{
    Stockpile _stockpile;
    TileMapLayer _terrainLayer;
    SelectionBox _selectionBox;

    Vector2I? _dragStart;
    MouseButton _dragButton;

    enum ToolMode { None, Mine, Stockpile, Build, Harvest, Chop }
    ToolMode _toolMode = ToolMode.None;

    static readonly Dictionary<Key, ToolMode> ModeKeys = new()
    {
        [Key.M] = ToolMode.Mine,
        [Key.S] = ToolMode.Stockpile,
        [Key.B] = ToolMode.Build,
        [Key.H] = ToolMode.Harvest,
        [Key.C] = ToolMode.Chop,
    };

    /// <summary>Binds the controller to what it acts on. Call before adding to the tree.</summary>
    /// <param name="guy">The guy that click-to-move drives.</param>
    /// <param name="stockpile">The stockpile the stockpile tool edits.</param>
    /// <param name="terrainLayer">The tilemap used to convert the mouse position to a cell.</param>
    public void Init(Stockpile stockpile, TileMapLayer terrainLayer)
    {
        _stockpile = stockpile;
        _terrainLayer = terrainLayer;

        _selectionBox = new SelectionBox();
        _selectionBox.Init(Game.TileSize);
        AddChild(_selectionBox);
    }

    public override void _UnhandledInput(InputEvent e)
    {
        // tool mode toggle
        if (e is InputEventKey key && key.Pressed && !key.Echo && ModeKeys.TryGetValue(key.Keycode, out var mode))
        {
            _toolMode = _toolMode == mode ? ToolMode.None : mode;
            GD.Print($"Tool mode: {_toolMode}");
        }

        // any tool mode drag selects, with no tool left-click walks
        if (_toolMode != ToolMode.None)
        {
            HandleDrag(e);
        }
        else if (e is InputEventMouseButton mb && mb.Pressed)
        {
            Vector2I cell = CellUnderMouse();
            if (!Game.Map.InBounds(cell)) return;

            if (mb.ButtonIndex == MouseButton.Left)
            {
                Game.SelectedGuy = Game.Map.Guys.FirstOrDefault(g => g.Cell == cell);

            }
            else if (mb.ButtonIndex == MouseButton.Right && Game.SelectedGuy != null)
            {
                var path = Game.Pathing.GetPath(Game.SelectedGuy.Cell, cell);
                if (path != null) Game.SelectedGuy.GoTo(path);
            }

        }
    }

    Vector2I CellUnderMouse() => _terrainLayer.LocalToMap(_terrainLayer.ToLocal(GetGlobalMousePosition()));

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
                if (_dragStart != null) _selectionBox.SetSelection(cell, cell);
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
            if (Game.Map.InBounds(cell))
            {
                _selectionBox.SetSelection(_dragStart.Value, cell);
            }
        }
    }

    /// <summary>Applies a finished drag rectangle based on the active mode and button.</summary>
    void ApplyDrag(Vector2I a, Vector2I b, MouseButton button)
    {
        switch (_toolMode)
        {
            case ToolMode.Mine:
                if (button == MouseButton.Left) DesignateMineRectangle(a, b);
                else CancelMineRectangle(a, b);
                break;
            case ToolMode.Stockpile:
                if (button == MouseButton.Left) AddStockpileRectangle(a, b);
                else RemoveStockpileRectangle(a, b);
                break;
            case ToolMode.Build:
                if (button == MouseButton.Left) PlaceWallRectangle(a, b);
                else CancelWallRectangle(a, b);
                break;
            case ToolMode.Harvest:
                if (button == MouseButton.Left) DesignatePlantRectangle(a, b, PlantWorkType.Harvest, DesignationType.Harvest);
                else CancelDesignationRectangle(a, b, DesignationType.Harvest);
                break;
            case ToolMode.Chop:
                if (button == MouseButton.Left) DesignatePlantRectangle(a, b, PlantWorkType.Chop, DesignationType.Chop);
                else CancelDesignationRectangle(a, b, DesignationType.Chop);
                break;
        }
    }

    /// <summary>Designates every reachable, mineable cell in the rectangle for mining.</summary>
    void DesignateMineRectangle(Vector2I a, Vector2I b)
    {
        foreach (var cell in Grid.CellsInRect(a, b))
        {
            if (Game.Map.Terrain[cell.X, cell.Y].Mineable && !Game.Map.Designations.Has(DesignationType.Mine, cell))
            {
                Game.Map.Designations.Add(DesignationType.Mine, cell);
                Game.MapView.MarkDesignation(DesignationType.Mine, cell);
            }
        }
        foreach (var orphan in Game.Map.Designations.PruneUnreachable(Game.Map))
        {
            Game.MapView.ClearDesignation(orphan);
        }
    }

    /// <summary>Cancels every mine designation in the rectangle.</summary>
    void CancelMineRectangle(Vector2I a, Vector2I b)
    {
        foreach (var cell in Grid.CellsInRect(a, b))
        {
            if (Game.Map.Designations.Has(DesignationType.Mine, cell))
            {
                Game.Map.Designations.Remove(DesignationType.Mine, cell);
                Game.MapView.ClearDesignation(cell);
            }
        }

        // removing cells could strand others so prune the rest
        foreach (var orphan in Game.Map.Designations.PruneUnreachable(Game.Map))
        {
            Game.MapView.ClearDesignation(orphan);
        }
    }

    /// <summary>Adds every walkable cell in the rectangle to the stockpile.</summary>
    void AddStockpileRectangle(Vector2I a, Vector2I b)
    {
        foreach (var cell in Grid.CellsInRect(a, b))
        {
            if (Game.Map.Terrain[cell.X, cell.Y].Walkable && _stockpile.Cells.Add(cell))
            {
                Game.MapView.MarkStockpile(cell);
            }
        }
    }

    /// <summary>Removes every stockpile cell in the rectangle.</summary>
    void RemoveStockpileRectangle(Vector2I a, Vector2I b)
    {
        foreach (var cell in Grid.CellsInRect(a, b))
        {
            if (_stockpile.Cells.Remove(cell))
            {
                Game.MapView.ClearStockpile(cell);
            }
        }
    }

    /// <summary>Places a wall blueprint frame on every valid cell in the rectangle.</summary>
    void PlaceWallRectangle(Vector2I a, Vector2I b)
    {
        foreach (var cell in Grid.CellsInRect(a, b))
        {
            if (!CanPlaceWall(cell)) continue;
            var frame = new Frame { Def = BuildingDefOf.WallStone, Cell = cell };
            Game.Map.AddFrame(frame);
            Game.Views.SpawnFrameView(frame);
        }
    }

    /// <summary>True if a wall blueprint can be placed on the cell.</summary>
    bool CanPlaceWall(Vector2I cell)
    {
        return Game.Map.Terrain[cell.X, cell.Y].Walkable && !Game.Map.HasFrame(cell) && Game.Map.BuildingAt(cell) == null;
    }

    /// <summary>Removes any wall blueprint frames in the rectangle.</summary>
    void CancelWallRectangle(Vector2I a, Vector2I b)
    {
        foreach (var cell in Grid.CellsInRect(a, b))
        {
            var frame = Game.Map.FrameAt(cell);
            if (frame == null) continue;
            Game.Map.RemoveFrame(frame);
            Game.Views.RemoveFrameView(frame);
        }
    }

    /// <summary>Designates every plant in the rectangle for harvest.</summary>
    void DesignateHarvestRectangle(Vector2I a, Vector2I b)
    {
        foreach (var cell in Grid.CellsInRect(a, b))
        {
            if (Game.Map.HasPlant(cell) && !Game.Map.Designations.Has(DesignationType.Harvest, cell))
            {
                Game.Map.Designations.Add(DesignationType.Harvest, cell);
                Game.MapView.MarkDesignation(DesignationType.Harvest, cell);
            }
        }
    }

    /// <summary>Cancels every harvest designation in the rectangle.</summary>
    void CancelHarvestRectangle(Vector2I a, Vector2I b)
    {
        foreach (var cell in Grid.CellsInRect(a, b))
        {
            if (Game.Map.Designations.Has(DesignationType.Harvest, cell))
            {
                Game.Map.Designations.Remove(DesignationType.Harvest, cell);
                Game.MapView.ClearDesignation(cell);
            }
        }
    }

    /// <summary>Designates plants of the given work type in the rectangle.</summary>
    void DesignatePlantRectangle(Vector2I a, Vector2I b, PlantWorkType workType, DesignationType desig)
    {
        foreach (var cell in Grid.CellsInRect(a, b))
        {
            var plant = Game.Map.PlantAt(cell);
            if (plant != null && plant.Def.WorkType == workType && !Game.Map.Designations.Has(desig, cell))
            {
                Game.Map.Designations.Add(desig, cell);
                Game.MapView.MarkDesignation(desig, cell);
            }
        }
    }

    /// <summary>Clears a designation type across the rectangle.</summary>
    void CancelDesignationRectangle(Vector2I a, Vector2I b, DesignationType desig)
    {
        foreach (var cell in Grid.CellsInRect(a, b))
        {
            if (Game.Map.Designations.Has(desig, cell))
            {
                Game.Map.Designations.Remove(desig, cell);
                Game.MapView.ClearDesignation(cell);
            }
        }
    }
}