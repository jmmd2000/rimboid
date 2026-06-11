using System.Linq;
using Godot;

/// <summary>
/// Runtime colonist data. Holds position, movement, pathing state and job execution.
/// </summary>
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

    /// <summary>Returns the current path points array.</summary>
    public Vector2[] GetPathPoints() => _path;
    /// <summary>Returns the current index along the path.</summary>
    public int GetPathIndex() => _pathIndex;

    /// <summary>Sets the colonist on a new path.</summary>
    /// <param name="path">Array of world coordinates to follow.</param>
    public void StartPath(Vector2[] path)
    {
        _path = path;
        _pathIndex = 0;
    }

    /// <summary>
    /// Clears the current path so the colonist stops moving.
    /// </summary>
    public void ClearPath()
    {
        _path = null;
        _pathIndex = 0;
    }

    /// <summary>
    /// Moves the colonist one step along the current path each tick.
    /// </summary>
    public void MoveAlongPath()
    {
        if (AtPathEnd) return;
        Position = Position.MoveToward(_path[_pathIndex], MoveSpeed);
        if (Position.DistanceTo(_path[_pathIndex]) < 0.01f)
            _pathIndex++;
    }

    /// <summary>
    /// Per frame update. Runs the active job or picks up new designations.
    /// </summary>
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
            {
                if (status == JobStatus.Failed) ClearPath();
                _driver = null;
            }
        }
    }
}