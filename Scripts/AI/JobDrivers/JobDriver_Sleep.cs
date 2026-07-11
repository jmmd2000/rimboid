using System.Collections.Generic;
using Godot;

/// <summary>Job driver for sleeping. Walks to a claimed bed first if it has one, then lies down and
/// refills rest, faster on a bed, until rested.</summary>
public class JobDriver_Sleep : JobDriver
{
    // refills full in ~one fifth of a day on the floor, much faster than rest decays
    const float RefillPerTick = 1f / (GameTime.TicksPerDay * 0.2f);

    protected override IEnumerable<Task> MakeTasks()
    {
        // walk to the claimed bed, if this job has one
        if (job.ClaimsCell) yield return WalkTo(job.TargetCell);

        // lie down and sleep until rested, faster on a bed
        yield return new Task
        {
            OnStart = () => { guy.ClearPath(); guy.IsSleeping = true; },
            OnTick = () => guy.Needs.Rest.Add(RefillPerTick * RestFactorAt(guy.Cell)),
            IsComplete = () => guy.Needs.Rest.Level >= 0.99f,
        };
    }

    /// <summary>The rest-refill multiplier of the bed on a cell, or 1 when there's no bed there.</summary>
    static float RestFactorAt(Vector2I cell)
    {
        var bed = Game.Map.BuildingAt(cell)?.GetComponent<BuildingComponent_Bed>();
        return bed?.RestFactor ?? 1f;
    }

}