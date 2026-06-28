using Godot;

/// <summary>Draws a finished building as a solid block in its def's colour.</summary>
public partial class BuildingView : Node2D
{
    public Building Building;
    int _tileSize;

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

    public override void _Draw()
    {
        if (Building == null) return;
        var def = Building.Def;
        var rect = new Rect2(Vector2.Zero, new Vector2(def.Size.X * _tileSize, def.Size.Y * _tileSize));

        if (def.Texture != null)
        {
            DrawTextureRect(def.Texture, rect, tile: false);
        }
        else
        {
            DrawRect(rect, Building.Def.Colour);
            DrawRect(rect, Building.Def.Colour.Darkened(0.3f), filled: false, width: 1f);
        }

    }
}