using Godot;

/// <summary>Sprite that follows a Guy's position each frame.</summary>
public partial class GuyView : Sprite2D
{
    Guy _guy;
    int _tileSize;

    /// <summary>Binds this view to a colonist.</summary>
    /// <param name="guy">The colonist to follow.</param>
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
        // Lie down while sleeping
        Rotation = _guy.IsSleeping ? Mathf.Pi / 2f : 0f;
    }


}