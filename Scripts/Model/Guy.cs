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

    public Needs Needs = new();
    public bool IsSleeping;

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

    /// <summary>Per sim tick update. Runs the active job, or asks the think tree for a new one.</summary>
    public void Tick()
    {
        Needs.Tick(Exertion);

        // a critical need pre-empts the current job, but only if it actually has a job to offer
        if (_driver != null)
        {
            var urgent = _think.FindInterruptingJob(this);
            if (urgent != null && urgent.Type != _driver.JobType)
                EndJob(dropCarry: true);
        }


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
                EndJob(dropCarry: status == JobStatus.Failed);
            }
        }
    }

    /// <summary>Ends the current job, optionally dropping any carried item where the colonist stands.</summary>
    /// <param name="dropCarry">True to drop the carried item (job abandoned or failed).</param>
    void EndJob(bool dropCarry)
    {
        ClearPath();
        IsSleeping = false;
        if (dropCarry && Carrying != null)
        {
            Game.Main.DropItems(Carrying.Def, Cell, Carrying.Count);
            Carrying = null;
        }
        _driver = null;
    }

    /// <summary>How hard the current job works the colonist, scaling need decay.</summary>
    float Exertion => _driver?.JobType switch
    {
        JobType.Mine => 2f,
        JobType.Haul => 1.2f,
        JobType.HaulToFrame => 1.2f,
        JobType.Build => 1.6f,
        _ => 1f,
    };

    /// <summary>Builds the driver that executes a job of the given type.</summary>
    /// <param name="type">The job type to build a driver for.</param>
    /// <returns>A new, uninitialised job driver.</returns>
    static JobDriver MakeDriver(JobType type) => type switch
    {
        JobType.Mine => new JobDriver_Mine(),
        JobType.Haul => new JobDriver_Haul(),
        JobType.HaulToFrame => new JobDriver_HaulToFrame(),
        JobType.Build => new JobDriver_Build(),
        JobType.Wander => new JobDriver_Wander(),
        JobType.Sleep => new JobDriver_Sleep(),
        JobType.Eat => new JobDriver_Eat(),
        _ => throw new System.ArgumentOutOfRangeException(nameof(type)),
    };
}