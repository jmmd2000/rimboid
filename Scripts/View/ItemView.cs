using Godot;

/// <summary>Sprite positioned once at an item's map cell.</summary>
public partial class ItemView : Sprite2D
{
    public Item Item;
    int _tileSize;

    /// <summary>Positions this view at the item's cell.</summary>
    /// <param name="item">The item to display.</param>
    /// <param name="tileSize">Pixel size of one tile.</param>
    public void Init(Item item, int tileSize)
    {
        Item = item;
        _tileSize = tileSize;
        Position = new Vector2(item.Cell.X * tileSize + tileSize / 2f, item.Cell.Y * tileSize + tileSize / 2f);
    }
}