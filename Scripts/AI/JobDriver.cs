using System;
using System.Collections.Generic;
using Godot;

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

    // set by WalkTo when a destination is missing or unreachable, fails the job
    protected bool pathFailed;

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

    /// <summary>
    /// A standard "walk to a cell" task: paths to the destination, steps along it each tick,
    /// and completes on arrival. Sets pathFailed (which fails the job) if the cell is unreachable.
    /// </summary>
    /// <param name="cell">The destination cell.</param>
    /// <param name="failIf">Optional extra per-tick failure check (e.g. the target became invalid).</param>
    protected Task WalkTo(Vector2I cell, Func<bool> failIf = null) => WalkTo(() => cell, failIf);

    /// <summary>
    /// As the cell overload, but the destination is computed when the walk starts i.e the nearest
    /// work cell beside a target, which may not be known until the guy gets there. A null
    /// destination fails the job.
    /// </summary>
    /// <param name="destination">Returns where to walk, evaluated at task start, null means fail.</param>
    /// <param name="failIf">Optional extra per-tick failure check.</param>
    protected Task WalkTo(Func<Vector2I?> destination, Func<bool> failIf = null)
    {
        return new Task
        {
            OnStart = () =>
            {
                var dest = destination();
                if (dest == null) { pathFailed = true; return; }
                if (guy.Cell == dest.Value) return; //already there
                var path = Game.Pathing.GetPath(guy.Cell, dest.Value);
                if (path == null || path.Length < 2) { pathFailed = true; return; }
                guy.StartPath(path);
            },
            OnTick = () => guy.MoveAlongPath(),
            IsComplete = () => guy.AtPathEnd,
            FailOn = () => pathFailed || (failIf != null && failIf()),
        };
    }

    ///<summary>
    /// Walks to each pile in turn and picks it up into a single carried stack, until `count` units
    /// are gathered. The piles must be already reserved by this guy. Used for multi pile hauls
    /// </summary>
    /// <param name="piles">Reserved source piles, all of the same item def.</param>
    /// <param name="count">How many units to gather in total.</param>
    /// <param name="failIf">Optional per-tick failure check (e.g. the destination became invalid).</param>
    protected IEnumerable<Task> GatherIntoCarrying(List<Item> piles, int count, Func<bool> failIf = null)
    {
        foreach (var pile in piles)
        {
            if ((guy.Carrying?.Count ?? 0) >= count) yield break; //already have enough
            var source = pile;

            yield return WalkTo(() => source.Cell, failIf);

            yield return new Task
            {
                OnStart = () =>
                {
                    if (!Game.Map.HasItem(source)) return;
                    int stillNeeded = count - (guy.Carrying?.Count ?? 0);
                    if (stillNeeded <= 0) return;
                    int take = Mathf.Min(stillNeeded, source.Count);

                    if (take >= source.Count)
                    {
                        Game.Map.RemoveItem(source);
                    }
                    else
                    {
                        source.Count -= take;
                    }

                    if (guy.Carrying == null)
                        guy.Carrying = new Item { Def = source.Def, Count = take };
                    else
                        guy.Carrying.Count += take;
                },
                IsComplete = () => true,
                FailOn = failIf,
            };
        }
    }

    /// <summary> One tick of skilled work, banks XP for the work done and returns how much work that is,
    /// scaled by the colonists skill. A null skill means unskilled, returns 1 and grants no xp.</summary>
    /// <param name="skill">The skill this work trains.</param>
    protected float SkilledWork(SkillDef skill)
    {
        float work = guy.WorkRate(skill);
        guy.Skills.Gain(skill, work * Skills.XPPerWork * guy.LearningRate);
        guy.Attributes.Gain(skill?.Attribute, Attributes.XPPerUse);
        return work;
    }

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