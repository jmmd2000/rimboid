using Godot;

/// <summary>Sprite for a plant, anchored at the base of its cell and scaled to the plant's
/// rolled draw size, so tall trees rise upward and overhang neighbouring cells visually.</summary>
public partial class PlantView : Sprite2D
{
    /// <summary>Positions and scales this view for a plant.</summary>
    /// <param name="plant">The plant to display.</param>
    /// <param name="tileSize">Pixel size of one tile.</param>
    public void Init(Plant plant, int tileSize)
    {
        // node sits on the bottom-centre of the footprint cell — this is its y-sort point
        Position = new Vector2(plant.Cell.X * tileSize + tileSize / 2f,
                               plant.Cell.Y * tileSize + tileSize);

        Texture = plant.Def.Texture;
        if (Texture == null) return;   // no art assigned yet, nothing to draw

        // offset the texture up and left so its bottom-centre lands on the node origin,
        // then scale it to the rolled draw size (in cells -> pixels)
        Centered = false;
        Offset = new Vector2(-Texture.GetWidth() / 2f, -Texture.GetHeight());
        Scale = new Vector2(plant.DrawWidth * tileSize / Texture.GetWidth(),
                            plant.DrawHeight * tileSize / Texture.GetHeight());
    }
}