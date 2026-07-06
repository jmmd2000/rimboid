using Godot;

/// <summary>Sprite that follows a Guy's position each frame.</summary>
public partial class GuyView : Sprite2D
{
    Guy _guy;
    int _tileSize;
    bool _selected;

    /// <summary>Binds this view to a guy.</summary>
    /// <param name="guy">The guy to follow.</param>
    /// <param name="tileSize">Pixel size of one tile.</param>
    public void Init(Guy guy, int tileSize)
    {
        _guy = guy;
        _tileSize = tileSize;
    }

    public override void _Process(double delta)
    {
        if (_guy == null) return;
        Position = _guy.Position * _tileSize + new Vector2(_tileSize / 2f, _tileSize / 2f);
        Rotation = _guy.IsSleeping ? Mathf.Pi / 2f : 0f;

        bool selected = Game.SelectedGuy == _guy;
        if (selected != _selected)
        {
            _selected = selected;
            QueueRedraw();
        }
    }

    public override void _Draw()
    {
        if (!_selected) return;
        float h = _tileSize / 2f;
        DrawRect(new Rect2(-h, -h, _tileSize, _tileSize), Colors.White, filled: false, width: 1f);
    }


}