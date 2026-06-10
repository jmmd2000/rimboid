using System;

public class Task
{
    public Action OnStart;
    public Action OnTick;
    public Func<bool> IsComplete;
    public Func<bool> FailOn;
}