using Godot;

public partial class GuyView : Sprite2D
{
    Guy _guy;
    int _tileSize;

    public void Init(Guy guy, int tileSize)
    {
        _guy = guy;
        _tileSize = tileSize;
    }

    public override void _Process(double delta)
    {
        if (_guy == null) return;
        Position = _guy.Position * _tileSize + new Vector2(_tileSize / 2f, _tileSize / 2f);
    }


}