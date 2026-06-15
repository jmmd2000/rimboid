using System.Collections.Generic;
using Godot;

/// <summary>
/// Builds a stocked frame into a finished building.
/// Tasks: walk adjacent -> construction work over time -> raise the building
/// (spawn it, drop the frame, refresh pathing so the cell blocks movement).
/// </summary>
public class JobDriver_Build : JobDriver
{
    protected override IEnumerable<Task> MakeTasks()
    {
        // walk to a cell adjacent to the frame
        yield return WalkTo(
            () => Game.Pathing.NearestSafeWorkCell(job.TargetCell, guy.Cell),
            failIf: () => !Game.Map.HasFrame(job.TargetCell)
        );

        // construction work over time
        yield return new Task
        {
            OnTick = () =>
            {
                var frame = Game.Map.FrameAt(job.TargetCell);
                if (frame != null) frame.WorkDone += 1f;
            },
            IsComplete = () =>
            {
                var frame = Game.Map.FrameAt(job.TargetCell);
                return frame == null || frame.WorkComplete;
            },
            FailOn = () => !Game.Map.HasFrame(job.TargetCell),
        };

        // frame -> real wall
        yield return new Task
        {
            OnStart = () =>
            {
                var frame = Game.Map.FrameAt(job.TargetCell);
                if (frame == null) return;

                var building = Game.Map.SpawnBuilding(frame.Def, frame.Cell);
                Game.Map.RemoveFrame(frame);
                Game.Views.RemoveFrameView(frame);
                Game.Views.SpawnBuildingView(building);
                // now blocks movement
                Game.Pathing.RefreshCell(Game.Map, frame.Cell);
            },
            IsComplete = () => true,
        };
    }
}