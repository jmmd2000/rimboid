using System.Collections.Generic;
using Godot;

/// <summary>
/// Hauls one load of material to a construction frame, gathering it across one or more reserved
/// piles in a single trip. Fails (dropping the load) if the frame is cancelled mid-haul.
/// </summary>
public class JobDriver_HaulToFrame : JobDriver
{
    protected override IEnumerable<Task> MakeTasks()
    {

        bool FrameGone() => !Game.Map.HasFrame(job.TargetCell);

        // collect the reserved material piles into one carried load
        foreach (var t in GatherIntoCarrying(job.ReservedItems, job.Count, failIf: FrameGone))
            yield return t;

        // walk to a cell adjacent to the frame
        yield return WalkTo(
            () => Game.Pathing.NearestReachableWorkCell(job.TargetCell, guy.Cell),
            failIf: FrameGone
        );

        // deposit into the frame
        yield return new Task
        {
            OnStart = () =>
            {
                var frame = Game.Map.FrameAt(job.TargetCell);
                if (frame == null || guy.Carrying == null) return;
                frame.MaterialsDelivered += guy.Carrying.Count;
                guy.Carrying = null;
            },
            IsComplete = () => true,
            FailOn = FrameGone,
        };
    }
}