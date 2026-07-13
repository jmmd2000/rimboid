using Godot;

/// <summary>Draws a finished building as a solid block in its def's colour.</summary>
public partial class BuildingView : Node2D
{
    public Building Building;
    protected int _tileSize;
    protected bool _selected;
    Vector2I _footprintMin;  // rotated footprint, cached at Init since Def/Rotation never change
    Vector2I _footprintSize;

    /// <summary>Positions this view at the building's cell.</summary>
    /// <param name="building">The building to display.</param>
    /// <param name="tileSize">Pixel size of one tile.</param>
    public void Init(Building building, int tileSize)
    {
        Building = building;
        _tileSize = tileSize;
        Position = new Vector2(building.Cell.X * tileSize, building.Cell.Y * tileSize);
        ZIndex = 1;
        _footprintMin = Footprint.MinOffset(building.Def.Size, building.Rotation);
        _footprintSize = Footprint.Rotated(building.Def.Size, building.Rotation);
        if (building.Def.OccludesLight) AddOccluder();
    }

    /// <summary>Adds a light occluder over the footprint so shadow-casting lights throw shadows behind
    /// this building. SDF collision stays on (default) so a future SDF lighting shader reads it for free.</summary>
    void AddOccluder()
    {
        var rect = new Rect2(_footprintMin.X * _tileSize, _footprintMin.Y * _tileSize, _footprintSize.X * _tileSize, _footprintSize.Y * _tileSize);

        AddChild(new LightOccluder2D
        {
            Occluder = new OccluderPolygon2D
            {
                Polygon = new[]
                {
                  rect.Position,
                  rect.Position + new Vector2(rect.Size.X, 0),
                  rect.End,
                  rect.Position + new Vector2(0, rect.Size.Y),
              }
            }
        });
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
            DrawRect(new Rect2(_footprintMin.X * _tileSize, _footprintMin.Y * _tileSize, _footprintSize.X * _tileSize, _footprintSize.Y * _tileSize), Colors.White, filled: false, width: 1f);
        }
    }
}