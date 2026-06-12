using System.Collections.Generic;

/// <summary>Possible outcomes of a job driver tick.</summary>
public enum JobStatus { Ongoing, Completed, Failed };

/// <summary>
/// Abstract base for job execution. Iterates through tasks yielded by MakeTasks,
/// calling OnStart/OnTick/IsComplete/FailOn each tick.
/// </summary>
public abstract class JobDriver
{
    protected Guy guy;
    protected Job job;

    IEnumerator<Task> _tasks;
    Task _current;

    /// <summary>Binds this driver to a colonist and job intent.</summary>
    /// <param name="guy">The colonist running the job.</param>
    /// <param name="job">The job intent with target data.</param>
    public void Init(Guy guy, Job job)
    {
        this.guy = guy;
        this.job = job;
    }

    /// <summary>The type of job this driver is executing.</summary>
    public JobType JobType => job.Type;

    /// <summary>Yields the sequence of tasks that make up this job.</summary>
    /// <returns>An enumerable of tasks to execute in order.</returns>
    protected abstract IEnumerable<Task> MakeTasks();

    /// <summary>Advances the job by one tick. Returns the current status.</summary>
    /// <returns>Ongoing, Completed, or Failed.</returns>
    public JobStatus Tick()
    {
        _tasks ??= MakeTasks().GetEnumerator();

        while (_current == null || _current.IsComplete())
        {
            if (!_tasks.MoveNext()) return JobStatus.Completed;
            _current = _tasks.Current;
            _current.OnStart?.Invoke();
            if (_current.FailOn?.Invoke() == true) return JobStatus.Failed;
        }

        if (_current.FailOn?.Invoke() == true) return JobStatus.Failed;
        _current.OnTick?.Invoke();
        return JobStatus.Ongoing;
    }
}