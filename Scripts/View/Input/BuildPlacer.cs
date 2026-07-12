using Godot;

/// <summary>Placement of buildings</summary>
public class BuildPlacer
{
    int _rotation;

    /// <summary>Rotates the pending placement 90 degrees.</summary>
    public void Rotate() => _rotation = (_rotation + 1) % 4;

    /// <summary>Resets the orientation, e.g. when leaving build mode.</summary>
    public void Reset() => _rotation = 0;

    /// <summary>Places a frame on each valid cell: a wall fills the line, a multi-cell building goes at the cursor.</summary>
    public void Place(BuildingDef def, Vector2I a, Vector2I b)
    {
        if (def.Size == Vector2I.One && def.DragPlace)
        {
            foreach (var cell in Grid.CellsInRect(a, b)) TryPlaceFrame(def, cell);
            return;
        }
        TryPlaceFrame(def, b); // ghost placement, one at a time
    }

    /// <summary>Removes any blueprint frames in the rectangle.</summary>
    public void Cancel(Vector2I a, Vector2I b)
    {
        foreach (var cell in Grid.CellsInRect(a, b))
        {
            var frame = Game.Map.FrameAt(cell);
            if (frame == null) continue;
            Game.Map.RemoveFrame(frame);
        }
    }

    /// <summary>Draws the placement ghost onto the canvas, tinted by whether it can be placed.</summary>
    /// <param name="canvas">The canvas item to draw onto (call from its _Draw).</param>
    /// <param name="def">The building being placed.</param>
    /// <param name="origin">The origin cell under the cursor.</param>
    public void DrawGhost(CanvasItem canvas, BuildingDef def, Vector2I origin)
    {
        int t = Game.TileSize;
        var tint = CanPlace(def, origin)
            ? new Color(0.4f, 1f, 0.4f, 0.55f)
            : new Color(1f, 0.4f, 0.4f, 0.55f);

        var cellOrigin = new Vector2(origin.X * t, origin.Y * t);

        // a textureless door draws its leaf so the rotation reads (a plain square looks identical rotated)
        if (def.Texture == null && IsDoor(def))
        {
            canvas.DrawRect(new Rect2(cellOrigin, new Vector2(t, t)), tint);
            DoorView.DrawLeaf(canvas, cellOrigin, t, _rotation, 0f, new Color(def.Colour, 0.85f));
            return;
        }

        var pivot = cellOrigin + new Vector2(t / 2f, t / 2f);
        canvas.DrawSetTransform(pivot, _rotation * Mathf.Pi / 2f, Vector2.One);
        var rect = new Rect2(-t / 2f, -t / 2f, def.Size.X * t, def.Size.Y * t);
        if (def.Texture != null) canvas.DrawTextureRect(def.Texture, rect, tile: false, modulate: tint);
        else canvas.DrawRect(rect, tint);
        canvas.DrawSetTransform(Vector2.Zero, 0, Vector2.One);
    }

    static bool IsDoor(BuildingDef def)
    {
        foreach (var c in def.Components) if (c is ComponentProperties_Door) return true;
        return false;
    }

    /// <summary>True if a blueprint can be placed with its origin on the cell.</summary>
    bool CanPlace(BuildingDef def, Vector2I origin)
    {
        foreach (var cell in Footprint.Cells(origin, def.Size, _rotation))
        {
            if (!Game.Map.InBounds(cell)) return false;
            if (!Game.Map.Terrain[cell.X, cell.Y].Walkable) return false;
            if (Game.Map.HasPlant(cell) || Game.Map.HasFrame(cell)) return false;
            if (Game.Map.BuildingAt(cell) != null) return false;
        }
        return true;
    }

    void TryPlaceFrame(BuildingDef def, Vector2I origin)
    {
        if (!CanPlace(def, origin)) return;

        if (Game.CreativeMode)
        {
            var building = Game.Map.SpawnBuilding(def, origin, _rotation);
            foreach (var c in building.OccupiedCells) Game.Pathing.RefreshCell(Game.Map, c);
            return;
        }

        var frame = new Frame { Def = def, Cell = origin, Rotation = _rotation };
        Game.Map.AddFrame(frame);
    }
}