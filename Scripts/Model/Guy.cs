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
    public Skills Skills = new();
    public Social Social = new();
    public bool IsSleeping;

    Vector2[] _path;
    int _pathIndex;
    bool _commanded;
    JobDriver _driver;
    readonly ThinkTree _think = new();

    public bool AtPathEnd => _path == null || _pathIndex >= _path.Length;

    /// <summary>Returns the current path points array.</summary>
    public Vector2[] GetPathPoints() => _path;
    /// <summary>Returns the current index along the path.</summary>
    public int GetPathIndex() => _pathIndex;

    ///<summary>Player order, drop the current job and walk this path, ignoring the think tree until arrival.
    public void GoTo(Vector2[] path)
    {
        if (_driver != null) EndJob(dropCarry: true);
        _commanded = true;
        StartPath(path);
    }

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

        if (_commanded)
        {
            MoveAlongPath();
            if (AtPathEnd) _commanded = false;
            return;
        }

        using (Prof.Sample("Think.Urgent")) DropJobForUrgentNeed();
        using (Prof.Sample("Think.Start")) StartJobIfIdle();
        using (Prof.Sample("Job.Run")) RunCurrentJob();
    }

    void DropJobForUrgentNeed()
    {
        if (_driver == null) return;
        var urgent = _think.FindInterruptingJob(this);
        if (urgent != null && urgent.Type != _driver.JobType)
            EndJob(dropCarry: true);
    }

    void StartJobIfIdle()
    {
        if (_driver != null) return;
        var job = _think.FindJob(this);
        if (job == null) return;
        Reserve(job);
        _driver = MakeDriver(job.Type);
        _driver.Init(this, job);
    }

    void RunCurrentJob()
    {
        if (_driver == null) return;
        var status = _driver.Tick();
        if (status != JobStatus.Ongoing)
            EndJob(dropCarry: status == JobStatus.Failed);
    }

    /// <summary>Ends the current job, optionally dropping any carried item where the colonist stands.</summary>
    /// <param name="dropCarry">True to drop the carried item (job abandoned or failed).</param>
    void EndJob(bool dropCarry)
    {
        Game.Map.Reservations.ReleaseAll(this);
        ClearPath();
        IsSleeping = false;
        if (dropCarry && Carrying != null)
        {
            Game.Map.DropItems(Carrying.Def, Cell, Carrying.Count);
            Carrying = null;
        }
        _driver = null;
    }

    /// <summary>How hard the current job works the colonist, scaling need decay.</summary>
    float Exertion => _driver?.JobType switch
    {
        JobType.Mine => 2f,
        JobType.Chop => 1.6f,
        JobType.Harvest => 1.2f,
        JobType.Sow => 1.2f,
        JobType.Haul => 1.2f,
        JobType.HaulToFrame => 1.2f,
        JobType.Build => 1.6f,
        JobType.DoBill => 1.2f,
        _ => 1f,
    };

    /// <summary>Work-speed multiplier for a task using the given skill, 1.0 at level 0, rising with level.
    /// <param name="skill">The skill the task trains, or null for an unskilled task.</param>
    public float WorkRate(SkillDef skill)
    {
        if (skill == null) return 1f;
        return 1f + Skills.Get(skill).Level * Skills.WorkRatePerLevel;
    }

    /// <summary>Builds the driver that executes a job of the given type.</summary>
    /// <param name="type">The job type to build a driver for.</param>
    /// <returns>A new, uninitialised job driver.</returns>
    static JobDriver MakeDriver(JobType type) => type switch
    {
        JobType.Mine => new JobDriver_Mine(),
        JobType.Harvest => new JobDriver_Harvest(),
        JobType.Chop => new JobDriver_Chop(),
        JobType.Sow => new JobDriver_Sow(),
        JobType.Haul => new JobDriver_Haul(),
        JobType.HaulToFrame => new JobDriver_HaulToFrame(),
        JobType.Build => new JobDriver_Build(),
        JobType.DoBill => new JobDriver_DoBill(),
        JobType.Wander => new JobDriver_Wander(),
        JobType.Sleep => new JobDriver_Sleep(),
        JobType.Eat => new JobDriver_Eat(),
        _ => throw new System.ArgumentOutOfRangeException(nameof(type)),
    };

    /// <summary>Claims the targets a job will work, so other Guys skip them.</summary>
    void Reserve(Job job)
    {
        Game.Map.Reservations.ReserveItem(job.TargetItem, this);
        if (job.ClaimsCell) Game.Map.Reservations.ReserveCell(job.TargetCell, this);
        if (job.ReservedItems != null)
            foreach (var item in job.ReservedItems)
                Game.Map.Reservations.ReserveItem(item, this);
    }
}