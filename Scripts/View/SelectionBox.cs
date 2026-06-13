using Godot;

/// <summary>Draws an outlined rectangle over a cell selection during a drag.</summary>
public partial class SelectionBox : Node2D
{
    int _tileSize;
    Vector2I _min, _max;
    bool _active;

    /// <summary>Sets the tile size and draw order</summary>
    public void Init(int tileSize)
    {
        _tileSize = tileSize;
        ZIndex = 1000;
    }

    /// <summary>Shows the outline covering the rectangle between two cells.</summary>
    public void SetSelection(Vector2I a, Vector2I b)
    {
        _min = new Vector2I(Mathf.Min(a.X, b.X), Mathf.Min(a.Y, b.Y));
        _max = new Vector2I(Mathf.Max(a.X, b.X), Mathf.Max(a.Y, b.Y));
        _active = true;
        QueueRedraw();
    }

    /// <summary>Hides the outline.</summary>
    public void Clear()
    {
        _active = false;
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (!_active) return;
        var pos = new Vector2(_min.X * _tileSize, _min.Y * _tileSize);
        var size = new Vector2((_max.X - _min.X + 1) * _tileSize, (_max.Y - _min.Y + 1) * _tileSize);
        DrawRect(new Rect2(pos, size), Colors.White, filled: false, width: 2f);
    }
}