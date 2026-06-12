using System.Collections.Generic;

/// <summary>Job driver for sleeping on the spot. Lies down, refills rest, then wakes.</summary>
public class JobDriver_Sleep : JobDriver
{
    // refills full in ~one fifth of a day, much faster than rest decays
    const float RefillPerTick = 1f / (GameTime.TicksPerDay * 0.2f);

    protected override IEnumerable<Task> MakeTasks()
    {
        // lie down and sleep until rested
        yield return new Task
        {
            OnStart = () => { guy.ClearPath(); guy.IsSleeping = true; },
            OnTick = () => guy.Needs.Rest.Add(RefillPerTick),
            IsComplete = () => guy.Needs.Rest.Level >= 0.99f,
        };

        // wake up
        yield return new Task
        {
            OnStart = () => guy.IsSleeping = false,
            IsComplete = () => true,
        };
    }
}