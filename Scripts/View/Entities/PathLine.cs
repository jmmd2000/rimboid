using Godot;

/// <summary>Line2D that draws the colonist's remaining path each frame.</summary>
public partial class PathLine : Line2D
{
    Guy _guy;
    int _tileSize;

    /// <summary>Binds this line to a colonist and configures appearance.</summary>
    /// <param name="guy">The colonist whose path to draw.</param>
    /// <param name="tileSize">Pixel size of one tile.</param>
    public void Init(Guy guy, int tileSize)
    {
        _guy = guy;
        _tileSize = tileSize;
        Width = 1f;
        DefaultColor = new Color(1, 1, 0, 0.5f);
    }

    public override void _Process(double delta)
    {
        ClearPoints();
        if (_guy == null || _guy != Game.SelectedGuy || _guy.AtPathEnd) return;

        AddPoint(_guy.Position * _tileSize + new Vector2(_tileSize / 2f, _tileSize / 2f));
        var path = _guy.GetPathPoints();
        if (path == null) return;
        var index = _guy.GetPathIndex();
        for (int i = index; i < path.Length; i++)
        {
            AddPoint(path[i] * _tileSize + new Vector2(_tileSize / 2f, _tileSize / 2f));
        }
    }
}