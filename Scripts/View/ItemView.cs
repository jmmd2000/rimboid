using Godot;

/// <summary>Sprite positioned once at an item's map cell.</summary>
public partial class ItemView : Sprite2D
{
    public Item Item;
    int _tileSize;
    Label _countLabel;

    /// <summary>Positions this view at the item's cell.</summary>
    /// <param name="item">The item to display.</param>
    /// <param name="tileSize">Pixel size of one tile.</param>
    public void Init(Item item, int tileSize)
    {
        Item = item;
        _tileSize = tileSize;
        Position = new Vector2(item.Cell.X * tileSize + tileSize / 2f, item.Cell.Y * tileSize + tileSize / 2f);

        _countLabel = new Label();
        _countLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _countLabel.Position = new Vector2(-tileSize / 2f, tileSize / 2f - 2);
        _countLabel.Size = new Vector2(tileSize, 12);
        _countLabel.AddThemeColorOverride("font_color", Colors.White);
        _countLabel.AddThemeFontSizeOverride("font_size", 8);
        _countLabel.Text = item.Count.ToString();
        AddChild(_countLabel);
    }

    public override void _Process(double delta)
    {
        if (Item == null) return;
        _countLabel.Text = Item.Count.ToString();
    }
}