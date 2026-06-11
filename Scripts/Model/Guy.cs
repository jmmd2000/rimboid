using Godot;

/// <summary>Runtime colonist data. Holds position, movement, pathing state and job execution.</summary>
public class Guy
{
    public int ID;
    public Vector2 Position;
    public Vector2I Cell => new((int)Mathf.Round(Position.X), (int)Mathf.Round(Position.Y));

    public float MoveSpeed = 0.05f;
    /// <summary>The item the colonist is currently carrying, or null.</summary>
    public Item Carrying;

    Vector2[] _path;
    int _pathIndex;
    JobDriver _driver;
    readonly ThinkTree _think = new();

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

    /// <summary>Clears the current path so the colonist stops moving.</summary>
    public void ClearPath()
    {
        _path = null;
        _pathIndex = 0;
    }

    /// <summary>Moves the colonist one step along the current path each tick.</summary>
    public void MoveAlongPath()
    {
        if (AtPathEnd) return;
        Position = Position.MoveToward(_path[_pathIndex], MoveSpeed);
        if (Position.DistanceTo(_path[_pathIndex]) < 0.01f)
            _pathIndex++;
    }

    /// <summary>Per frame update. Runs the active job, or asks the think tree for a new one.</summary>
    public void Tick()
    {
        if (_driver == null)
        {
            var job = _think.FindJob(this);
            if (job != null)
            {
                _driver = MakeDriver(job.Type);
                _driver.Init(this, job);
            }
        }

        if (_driver != null)
        {
            var status = _driver.Tick();
            if (status != JobStatus.Ongoing)
            {
                if (status == JobStatus.Failed)
                {
                    ClearPath();
                    if (Carrying != null)
                    {
                        Game.Map.SpawnItem(Carrying.Def, Cell, Carrying.Count);
                        var dropped = Game.Map.ItemAt(Cell, Carrying.Def);
                        Game.Main.SpawnItemView(dropped);
                        Carrying = null;
                    }
                }
                _driver = null;
            }
        }
    }

    /// <summary>Builds the driver that executes a job of the given type.</summary>
    /// <param name="type">The job type to build a driver for.</param>
    /// <returns>A new, uninitialised job driver.</returns>
    static JobDriver MakeDriver(JobType type) => type switch
    {
        JobType.Mine => new JobDriver_Mine(),
        JobType.Haul => new JobDriver_Haul(),
        JobType.Wander => new JobDriver_Wander(),
        _ => throw new System.ArgumentOutOfRangeException(nameof(type)),
    };
}