using System.Collections.Generic;

/// <summary>Job driver for idle wandering. Walks to a target cell, then pause briefly.</summary>
public class JobDriver_Wander : JobDriver
{
    bool _pathFailed;
    int _waitTicks;

    /// <summary>Yields two tasks: walk to the wander cell, then idle for a moment.</summary>
    /// <returns>Sequence of tasks for the job driver to execute.</returns>
    protected override IEnumerable<Task> MakeTasks()
    {
        // walk to the target cell
        yield return new Task
        {
            OnStart = () =>
            {
                var path = Game.Pathing.GetPath(guy.Cell, job.TargetCell);
                if (path == null || path.Length < 2) { _pathFailed = true; return; }
                guy.StartPath(path);
            },
            OnTick = () => guy.MoveAlongPath(),
            IsComplete = () => guy.AtPathEnd,
            FailOn = () => _pathFailed,
        };

        // wait for a bit
        yield return new Task
        {
            OnStart = () => _waitTicks = 0,
            OnTick = () => _waitTicks++,
            IsComplete = () => _waitTicks >= 30,
        };
    }
}