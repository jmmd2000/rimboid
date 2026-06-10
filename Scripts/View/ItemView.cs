using Godot;

public partial class ItemView : Sprite2D
{
    public Item Item;
    int _tileSize;

    public void Init(Item item, int tileSize)
    {
        Item = item;
        _tileSize = tileSize;
        Position = new Vector2(item.Cell.X * tileSize + tileSize / 2f, item.Cell.Y * tileSize + tileSize / 2f);
    }
}