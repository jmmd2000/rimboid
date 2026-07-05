using Godot;

/// <summary>Draws a finished building as a solid block in its def's colour.</summary>
public partial class BuildingView : Node2D
{
    public Building Building;
    int _tileSize;
    bool _selected;

    /// <summary>Positions this view at the building's cell.</summary>
    /// <param name="building">The building to display.</param>
    /// <param name="tileSize">Pixel size of one tile.</param>
    public void Init(Building building, int tileSize)
    {
        Building = building;
        _tileSize = tileSize;
        Position = new Vector2(building.Cell.X * tileSize, building.Cell.Y * tileSize);
        ZIndex = 1;
    }

    public override void _Process(double delta)
    {
        bool selected = Game.SelectedBuilding == Building;
        if (selected != _selected)
        {
            _selected = selected;
            QueueRedraw();
        }
    }

    public override void _Draw()
    {
        if (Building == null) return;
        var def = Building.Def;

        // pivot the sprite about the origin cell's centre so the footprint rotates around it
        DrawSetTransform(new Vector2(_tileSize / 2f, _tileSize / 2f), Building.Rotation * Mathf.Pi / 2f, Vector2.One);
        var rect = new Rect2(-_tileSize / 2f, -_tileSize / 2f, def.Size.X * _tileSize, def.Size.Y * _tileSize);
        if (def.Texture != null) DrawTextureRect(def.Texture, rect, tile: false);
        else DrawRect(rect, def.Colour);
        DrawSetTransform(Vector2.Zero, 0, Vector2.One);

        if (_selected)
        {
            var min = Footprint.MinOffset(def.Size, Building.Rotation);
            var size = Footprint.Rotated(def.Size, Building.Rotation);
            DrawRect(new Rect2(min.X * _tileSize, min.Y * _tileSize, size.X * _tileSize, size.Y * _tileSize), Colors.White, filled: false, width: 1f);
        }
    }
}