using Godot;

public class Guy
{
    public int ID;
    public Vector2 Position;
    public Vector2I Cell => new((int)Mathf.Round(Position.X), (int)Mathf.Round(Position.Y));

    public float MoveSpeed = 0.05f;

    Vector2[] _path;
    int _pathIndex;

    public bool AtPathEnd => _path == null || _pathIndex >= _path.Length;

    public void StartPath(Vector2[] path)
    {
        _path = path;
        _pathIndex = 0;
    }

    public void MoveAlongPath()
    {
        if (AtPathEnd) return;
        Position = Position.MoveToward(_path[_pathIndex], MoveSpeed);
        if (Position.DistanceTo(_path[_pathIndex]) < 0.01f)
            _pathIndex++;
    }
}