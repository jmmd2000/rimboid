using System.Linq;
using Godot;

public class Guy
{
    public int ID;
    public Vector2 Position;
    public Vector2I Cell => new((int)Mathf.Round(Position.X), (int)Mathf.Round(Position.Y));

    public float MoveSpeed = 0.05f;

    Vector2[] _path;
    int _pathIndex;
    JobDriver _driver;

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

    public void Tick()
    {
        if (_driver == null)
        {
            MoveAlongPath();

            var target = Game.Map.Designations.CellsOfType(DesignationType.Mine).FirstOrDefault();
            if (target != default)
            {
                var job = new Job { TargetCell = target };
                _driver = new JobDriver_Mine();
                _driver.Init(this, job);
            }
        }

        if (_driver != null)
        {
            var status = _driver.Tick();
            if (status != JobStatus.Ongoing)
                _driver = null;
        }
    }
}