using System.Collections.Generic;

public enum JobStatus { Ongoing, Completed, Failed };

public abstract class JobDriver
{
    protected Guy guy;
    protected Job job;

    IEnumerator<Task> _tasks;
    Task _current;
    bool _started;

    public void Init(Guy guy, Job job)
    {
        this.guy = guy;
        this.job = job;
    }

    protected abstract IEnumerable<Task> MakeTasks();

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