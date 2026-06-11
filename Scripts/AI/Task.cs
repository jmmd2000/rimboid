using System;

/// <summary>
/// Single unit of work within a job. Each task has optional start/tick callbacks,
/// a completion check, and a failure condition.
/// </summary>
public class Task
{
    public Action OnStart;
    public Action OnTick;
    public Func<bool> IsComplete;
    public Func<bool> FailOn;
}